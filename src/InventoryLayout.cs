using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DadsEPI
{
    internal sealed class DedicatedSlot
    {
        internal string Id;
        internal string Label;
        internal bool Quick;
        internal Func<ItemDrop.ItemData, bool> Accepts;
    }

    internal static class InventoryLayout
    {
        internal const int BaseVanillaRows = 4;
        internal const int MaximumVanillaRows = 9;
        internal static readonly List<DedicatedSlot> Slots = new List<DedicatedSlot>();
        internal static int VanillaRows { get; private set; } = BaseVanillaRows;
        private static bool _normalizing;
        private static ItemDrop.ItemData _pendingAutoItem;

        internal static int NormalRows => Mathf.Clamp(VanillaRows, BaseVanillaRows, MaximumVanillaRows) + Mathf.Clamp(DadsEPIPlugin.ExtraRows.Value, 0, 5);
        internal static int EquipmentSlotCount => Slots.Count(slot => !slot.Quick);
        internal static int EnabledQuickSlots => DadsEPIPlugin.EquipmentRowEnabled.Value ? Mathf.Clamp(DadsEPIPlugin.QuickSlotCount.Value, 0, 8) : 0;
        internal static int StorageRows(int width) => DadsEPIPlugin.EquipmentRowEnabled.Value ? Mathf.CeilToInt(Slots.Count / (float)Mathf.Max(1, width)) : 0;
        internal static int TotalRows(int width) => NormalRows + StorageRows(width);

        internal static void RebuildSlots()
        {
            Slots.Clear();
            if (DadsEPIPlugin.EquipmentRowEnabled?.Value != true) return;
            AddEquipment("Head", DadsEPIPlugin.HeadLabel.Value, item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet);
            AddEquipment("Chest", DadsEPIPlugin.ChestLabel.Value, item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest);
            AddEquipment("Legs", DadsEPIPlugin.LegsLabel.Value, item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs);
            AddEquipment("Back", DadsEPIPlugin.BackLabel.Value, item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder);
            if (DadsEPIPlugin.WisplightSlot.Value) AddEquipment("Wisplight", "Wisplight", IsWisplight);
            if (DadsEPIPlugin.WishboneSlot.Value) AddEquipment("Wishbone", "Wishbone", item => IsPrefab(item, "Wishbone"));
            if (DadsEPIPlugin.CryptKeySlot.Value) AddEquipment("Crypt Key", "Crypt Key", item => IsPrefab(item, "CryptKey"));
            if (DadsEPIPlugin.ArrowsSlot.Value) AddEquipment("Arrows", "Arrows", IsArrow);
            if (DadsEPIPlugin.ShieldSlot.Value) AddEquipment("Shield", "Shield", item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield);
            if (DadsEPIPlugin.UtilitySlot.Value) AddEquipment("Utility", DadsEPIPlugin.UtilityLabel.Value, item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility &&
                (!DadsEPIPlugin.WishboneSlot.Value || !IsPrefab(item, "Wishbone")) &&
                (!DadsEPIPlugin.WisplightSlot.Value || !IsWisplight(item)));

            for (int index = 0; index < DadsEPIPlugin.CustomSlotNames.Length; index++)
            {
                string name = (DadsEPIPlugin.CustomSlotNames[index].Value ?? string.Empty).Trim();
                var prefabs = new HashSet<string>((DadsEPIPlugin.CustomSlotItems[index].Value ?? string.Empty).Split(',').Select(value => value.Trim()).Where(value => value.Length > 0), StringComparer.Ordinal);
                if (name.Length > 0 && prefabs.Count > 0) AddEquipment(name, name, item => prefabs.Contains(PrefabName(item)));
            }

            for (int index = 0; index < Mathf.Clamp(DadsEPIPlugin.QuickSlotCount.Value, 0, 8); index++)
            {
                int captured = index;
                Slots.Add(new DedicatedSlot { Id = $"Quick{captured + 1}", Label = QuickLabel(captured), Quick = true, Accepts = item => true });
            }
        }

        internal static void SetVanillaRows(int rows)
        {
            VanillaRows = Mathf.Clamp(rows, BaseVanillaRows, MaximumVanillaRows);
        }

        internal static void ReadVanillaRows(Player player)
        {
            if (player != null && player.TryGetUniqueKeyValue(Player.InventoryRowsKey, out string value) && int.TryParse(value, out int rows)) SetVanillaRows(rows);
            else SetVanillaRows(BaseVanillaRows);
        }

        internal static void Apply(Player player, bool normalizeItems)
        {
            if (player == null || DadsEPIPlugin.ModEnabled?.Value != true) return;
            Inventory inventory = player.GetInventory();
            if (inventory == null) return;
            ReadVanillaRows(player);
            int target = TotalRows(inventory.GetWidth());
            if (inventory.GetHeight() < target) inventory.SetHeight(target);
            if (normalizeItems) Normalize(player, inventory);
            int occupiedHeight = inventory.GetAllItems().Count == 0 ? 0 : inventory.GetAllItems().Max(item => item.m_gridPos.y + 1);
            int safeHeight = Mathf.Max(target, occupiedHeight);
            if (inventory.GetHeight() != safeHeight) inventory.SetHeight(safeHeight);
            if (InventoryGui.instance != null)
                InventoryGui.instance.SetInventorySize(DadsEPIPlugin.SeparateEquipmentPanel.Value ? NormalRows : safeHeight);
            if (player.m_tombstone != null)
            {
                Container tombstone = player.m_tombstone.GetComponent<Container>();
                if (tombstone != null) tombstone.m_height = safeHeight;
            }
        }

        internal static bool IsPlayerInventory(Inventory inventory)
        {
            return inventory != null && Player.m_localPlayer != null && ReferenceEquals(inventory, Player.m_localPlayer.GetInventory());
        }

        internal static int SlotIndex(Vector2i position, int width)
        {
            int linear = position.y * width + position.x - NormalRows * width;
            return linear >= 0 && linear < Slots.Count ? linear : -1;
        }

        internal static Vector2i SlotPosition(int slotIndex, int width)
        {
            int linear = NormalRows * width + slotIndex;
            return new Vector2i(linear % width, linear / width);
        }

        internal static bool IsReserved(Vector2i position, int width) => SlotIndex(position, width) >= 0;

        internal static bool CanPlace(ItemDrop.ItemData item, Vector2i position, int width)
        {
            int index = SlotIndex(position, width);
            if (index < 0) return position.y < NormalRows;
            return item == null || Slots[index].Accepts(item);
        }

        internal static Vector2i FindGeneralEmpty(Inventory inventory, bool topFirst)
        {
            if (topFirst)
            {
                for (int y = 0; y < NormalRows; y++) for (int x = 0; x < inventory.GetWidth(); x++) if (inventory.GetItemAt(x, y) == null) return new Vector2i(x, y);
            }
            else
            {
                for (int y = NormalRows - 1; y >= 0; y--) for (int x = 0; x < inventory.GetWidth(); x++) if (inventory.GetItemAt(x, y) == null) return new Vector2i(x, y);
            }
            return new Vector2i(-1, -1);
        }

        internal static void BeginAutoAdd(Inventory inventory, ItemDrop.ItemData item)
        {
            _pendingAutoItem = DadsEPIPlugin.ModEnabled?.Value == true && DadsEPIPlugin.AutoEquip?.Value == true && IsPlayerInventory(inventory)
                ? item
                : null;
        }

        internal static void EndAutoAdd()
        {
            _pendingAutoItem = null;
        }

        internal static Vector2i FindPreferredEmpty(Inventory inventory, bool topFirst)
        {
            if (_pendingAutoItem != null)
            {
                int slot = FindEquipmentSlot(_pendingAutoItem);
                if (slot >= 0)
                {
                    Vector2i destination = SlotPosition(slot, inventory.GetWidth());
                    if (inventory.GetItemAt(destination.x, destination.y) == null) return destination;
                }
            }
            return FindGeneralEmpty(inventory, topFirst);
        }

        internal static int CountGeneralEmpty(Inventory inventory)
        {
            int count = 0;
            for (int y = 0; y < NormalRows; y++) for (int x = 0; x < inventory.GetWidth(); x++) if (inventory.GetItemAt(x, y) == null) count++;
            return count;
        }

        internal static void UseQuickSlot(int index)
        {
            Player player = Player.m_localPlayer;
            if (player == null || index < 0 || index >= EnabledQuickSlots) return;
            int slot = EquipmentSlotCount + index;
            ItemDrop.ItemData item = player.GetInventory().GetItemAt(SlotPosition(slot, player.GetInventory().GetWidth()).x, SlotPosition(slot, player.GetInventory().GetWidth()).y);
            if (item != null) player.UseItem(player.GetInventory(), item, false);
        }

        internal static void MoveEquippedItemToDedicatedSlot(Player player, ItemDrop.ItemData item)
        {
            if (_normalizing || player == null || item == null) return;
            int slot = FindEquipmentSlot(item);
            if (slot >= 0) MoveItem(player, player.GetInventory(), item, SlotPosition(slot, player.GetInventory().GetWidth()));
        }

        internal static void AutoPlaceNewItem(Player player, Inventory inventory, ItemDrop.ItemData item)
        {
            if (_normalizing || player == null || inventory == null || item == null || DadsEPIPlugin.AutoEquip?.Value != true) return;
            if (!ReferenceEquals(inventory, player.GetInventory())) return;
            int slot = FindEquipmentSlot(item);
            if (slot < 0) return;
            Vector2i destination = SlotPosition(slot, inventory.GetWidth());
            if (inventory.GetItemAt(destination.x, destination.y) != null) return;
            ItemDrop.ItemData storedItem = inventory.GetAllItems().Contains(item)
                ? item
                : inventory.GetAllItems().FirstOrDefault(candidate => Slots[slot].Accepts(candidate) && string.Equals(PrefabName(candidate), PrefabName(item), StringComparison.Ordinal));
            if (storedItem == null) return;
            MoveItem(player, inventory, storedItem, destination);
            player.EquipItem(storedItem);
        }

        internal static int FindEquipmentSlot(ItemDrop.ItemData item)
        {
            for (int index = 0; index < EquipmentSlotCount; index++) if (Slots[index].Accepts(item)) return index;
            return -1;
        }

        private static void Normalize(Player player, Inventory inventory)
        {
            if (_normalizing) return;
            _normalizing = true;
            try
            {
                foreach (ItemDrop.ItemData item in new List<ItemDrop.ItemData>(inventory.GetAllItems()))
                {
                    if (!item.m_equipped) continue;
                    int slot = FindEquipmentSlot(item);
                    if (slot >= 0) MoveItem(player, inventory, item, SlotPosition(slot, inventory.GetWidth()));
                }
                foreach (ItemDrop.ItemData item in new List<ItemDrop.ItemData>(inventory.GetAllItems()))
                {
                    if (item.m_gridPos.y < NormalRows || CanPlace(item, item.m_gridPos, inventory.GetWidth())) continue;
                    Vector2i empty = FindGeneralEmpty(inventory, true);
                    if (empty.x >= 0) MoveItem(player, inventory, item, empty);
                }
            }
            finally { _normalizing = false; }
        }

        private static void MoveItem(Player player, Inventory inventory, ItemDrop.ItemData item, Vector2i destination)
        {
            if (item.m_gridPos == destination) return;
            ItemDrop.ItemData occupant = inventory.GetItemAt(destination.x, destination.y);
            Vector2i source = item.m_gridPos;
            if (occupant != null && !ReferenceEquals(occupant, item))
            {
                Vector2i empty = FindGeneralEmpty(inventory, true);
                occupant.m_gridPos = empty.x >= 0 ? empty : source;
            }
            item.m_gridPos = destination;
            inventory.m_onChanged?.Invoke();
        }

        private static void AddEquipment(string id, string label, Func<ItemDrop.ItemData, bool> accepts)
        {
            var removed = new HashSet<string>((DadsEPIPlugin.RemovedEquipmentSlots.Value ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()), StringComparer.OrdinalIgnoreCase);
            if (!removed.Contains(id) && !removed.Contains(label)) Slots.Add(new DedicatedSlot { Id = id, Label = label, Accepts = accepts });
        }

        private static string QuickLabel(int index)
        {
            string configured = DadsEPIPlugin.QuickHotkeyLabels[index].Value;
            return string.IsNullOrWhiteSpace(configured) ? DadsEPIPlugin.QuickHotkeys[index].Value.ToString() : configured;
        }

        private static string PrefabName(ItemDrop.ItemData item) => item?.m_dropPrefab != null ? Utils.GetPrefabName(item.m_dropPrefab) : string.Empty;
        private static bool IsPrefab(ItemDrop.ItemData item, string prefab) => string.Equals(PrefabName(item), prefab, StringComparison.Ordinal);
        private static bool IsWisplight(ItemDrop.ItemData item) => IsPrefab(item, "Demister") || IsPrefab(item, "Wisplight");
        private static bool IsArrow(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null) return false;
            ItemDrop.ItemData.ItemType type = item.m_shared.m_itemType;
            return (type == ItemDrop.ItemData.ItemType.Ammo || type == ItemDrop.ItemData.ItemType.AmmoNonEquipable) && PrefabName(item).StartsWith("Arrow", StringComparison.Ordinal);
        }
    }
}
