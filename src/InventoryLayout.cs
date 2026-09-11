using System.Collections.Generic;
using UnityEngine;

namespace DadsEPI
{
    internal static class InventoryLayout
    {
        internal const int VanillaRows = 4;
        private static bool _normalizing;

        internal static int TotalRows
        {
            get
            {
                int reservedRows = (DadsEPIPlugin.EquipmentRowEnabled.Value ? 1 : 0)
                                   + (DadsEPIPlugin.QuickRowEnabled.Value ? 1 : 0);
                return Mathf.Clamp(VanillaRows + Mathf.Clamp(DadsEPIPlugin.GeneralExtraRows.Value, 0, 3) + reservedRows, VanillaRows, 9);
            }
        }

        internal static int FirstReservedRow => TotalRows
                                                - (DadsEPIPlugin.EquipmentRowEnabled.Value ? 1 : 0)
                                                - (DadsEPIPlugin.QuickRowEnabled.Value ? 1 : 0);

        internal static int EquipmentRow => DadsEPIPlugin.EquipmentRowEnabled.Value ? FirstReservedRow : -1;

        internal static int QuickRow => DadsEPIPlugin.QuickRowEnabled.Value ? TotalRows - 1 : -1;

        internal static int EnabledQuickSlots => Mathf.Clamp(DadsEPIPlugin.QuickSlotCount.Value, 1, 8);

        internal static void Apply(Player player, bool normalizeItems)
        {
            if (player == null || DadsEPIPlugin.ModEnabled?.Value != true)
            {
                return;
            }

            Inventory inventory = player.GetInventory();
            if (inventory == null)
            {
                return;
            }

            if (inventory.GetHeight() != TotalRows)
            {
                inventory.SetHeight(TotalRows);
            }

            if (InventoryGui.instance != null)
            {
                InventoryGui.instance.SetInventorySize(TotalRows);
            }

            if (normalizeItems)
            {
                NormalizeReservedRows(player, inventory);
            }
        }

        internal static bool IsPlayerInventory(Inventory inventory)
        {
            return inventory != null
                   && Player.m_localPlayer != null
                   && ReferenceEquals(inventory, Player.m_localPlayer.GetInventory());
        }

        internal static bool IsReserved(Vector2i position)
        {
            return position.y >= FirstReservedRow && position.y < TotalRows;
        }

        internal static bool CanPlace(ItemDrop.ItemData item, Vector2i position)
        {
            if (!IsReserved(position))
            {
                return true;
            }

            if (item == null)
            {
                return false;
            }

            if (position.y == QuickRow)
            {
                return position.x >= 0 && position.x < EnabledQuickSlots;
            }

            if (position.y == EquipmentRow)
            {
                return EquipmentColumn(item.m_shared.m_itemType) == position.x;
            }

            return false;
        }

        internal static int EquipmentColumn(ItemDrop.ItemData.ItemType itemType)
        {
            switch (itemType)
            {
                case ItemDrop.ItemData.ItemType.Helmet: return 0;
                case ItemDrop.ItemData.ItemType.Chest: return 1;
                case ItemDrop.ItemData.ItemType.Legs: return 2;
                case ItemDrop.ItemData.ItemType.Shoulder: return 3;
                case ItemDrop.ItemData.ItemType.Utility: return 4;
                case ItemDrop.ItemData.ItemType.Hands: return 5;
                case ItemDrop.ItemData.ItemType.Trinket: return 6;
                default: return -1;
            }
        }

        internal static Vector2i FindGeneralEmpty(Inventory inventory, bool topFirst)
        {
            int width = inventory.GetWidth();
            if (topFirst)
            {
                for (int y = 0; y < FirstReservedRow; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        if (inventory.GetItemAt(x, y) == null) return new Vector2i(x, y);
                    }
                }
            }
            else
            {
                for (int y = FirstReservedRow - 1; y >= 0; --y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        if (inventory.GetItemAt(x, y) == null) return new Vector2i(x, y);
                    }
                }
            }

            return new Vector2i(-1, -1);
        }

        internal static int CountGeneralEmpty(Inventory inventory)
        {
            int count = 0;
            for (int y = 0; y < FirstReservedRow; ++y)
            {
                for (int x = 0; x < inventory.GetWidth(); ++x)
                {
                    if (inventory.GetItemAt(x, y) == null) ++count;
                }
            }

            return count;
        }

        internal static void UseQuickSlot(int index)
        {
            Player player = Player.m_localPlayer;
            if (player == null || index < 0 || index >= EnabledQuickSlots || QuickRow < 0)
            {
                return;
            }

            Inventory inventory = player.GetInventory();
            ItemDrop.ItemData item = inventory.GetItemAt(index, QuickRow);
            if (item != null)
            {
                player.UseItem(inventory, item, fromInventoryGui: false);
            }
        }

        internal static void MoveEquippedItemToReservedSlot(Player player, ItemDrop.ItemData item)
        {
            if (_normalizing || player == null || item == null || EquipmentRow < 0)
            {
                return;
            }

            int column = EquipmentColumn(item.m_shared.m_itemType);
            if (column < 0 || column >= player.GetInventory().GetWidth())
            {
                return;
            }

            MoveItem(player, player.GetInventory(), item, new Vector2i(column, EquipmentRow));
        }

        private static void NormalizeReservedRows(Player player, Inventory inventory)
        {
            if (_normalizing)
            {
                return;
            }

            _normalizing = true;
            try
            {
                List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>(inventory.GetAllItems());

                if (EquipmentRow >= 0)
                {
                    foreach (ItemDrop.ItemData item in items)
                    {
                        if (item.m_equipped)
                        {
                            int column = EquipmentColumn(item.m_shared.m_itemType);
                            if (column >= 0 && column < inventory.GetWidth())
                            {
                                MoveItem(player, inventory, item, new Vector2i(column, EquipmentRow));
                            }
                        }
                    }
                }

                items = new List<ItemDrop.ItemData>(inventory.GetAllItems());
                foreach (ItemDrop.ItemData item in items)
                {
                    if (!IsReserved(item.m_gridPos) || CanPlace(item, item.m_gridPos))
                    {
                        continue;
                    }

                    Vector2i empty = FindGeneralEmpty(inventory, topFirst: true);
                    if (empty.x >= 0)
                    {
                        MoveItem(player, inventory, item, empty);
                    }
                }
            }
            finally
            {
                _normalizing = false;
            }
        }

        private static void MoveItem(Player player, Inventory inventory, ItemDrop.ItemData item, Vector2i destination)
        {
            if (item.m_gridPos == destination)
            {
                return;
            }

            ItemDrop.ItemData occupant = inventory.GetItemAt(destination.x, destination.y);
            Vector2i source = item.m_gridPos;
            if (occupant != null && !ReferenceEquals(occupant, item))
            {
                if (occupant.m_equipped)
                {
                    player.UnequipItem(occupant, triggerEquipEffects: false);
                }
                occupant.m_gridPos = source;
            }

            item.m_gridPos = destination;
            inventory.m_onChanged?.Invoke();
        }
    }
}

