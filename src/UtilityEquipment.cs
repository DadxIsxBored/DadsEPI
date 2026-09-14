using System.Collections.Generic;
using HarmonyLib;

namespace DadsEPI
{
    internal static class UtilityEquipment
    {
        private static readonly AccessTools.FieldRef<Humanoid, ItemDrop.ItemData> UtilityItem =
            AccessTools.FieldRefAccess<Humanoid, ItemDrop.ItemData>("m_utilityItem");
        private static bool _restoringInventoryEquipment;

        internal static bool IsRestoringInventoryEquipment => _restoringInventoryEquipment;

        internal static void BeginInventoryRestore()
        {
            _restoringInventoryEquipment = true;
        }

        internal static void EndInventoryRestore()
        {
            _restoringInventoryEquipment = false;
        }

        internal static ItemDrop.ItemData BeginEquip(Humanoid humanoid, ItemDrop.ItemData item)
        {
            if (!IsLocalUtilityItem(humanoid, item) || (item.m_equipped && !_restoringInventoryEquipment)) return null;

            ItemDrop.ItemData previous = UtilityItem(humanoid);
            if (previous == null || ReferenceEquals(previous, item) || !previous.m_equipped) return null;

            Player player = (Player)humanoid;
            if (!InventoryLayout.IsInEquipmentSlot(player.GetInventory(), previous)) return null;
            if (!InventoryLayout.UsesSeparateEquipmentSlot(player.GetInventory(), item, previous)) return null;

            UtilityItem(humanoid) = null;
            return previous;
        }

        internal static void EndEquip(Humanoid humanoid, ItemDrop.ItemData previous, bool equipped)
        {
            if (previous == null) return;
            previous.m_equipped = true;
            if (!equipped && UtilityItem(humanoid) == null) UtilityItem(humanoid) = previous;
        }

        internal static bool IsManagedUtility(Humanoid humanoid, ItemDrop.ItemData item)
        {
            if (_restoringInventoryEquipment || !IsLocalUtilityItem(humanoid, item) || !item.m_equipped) return false;
            Player player = (Player)humanoid;
            return InventoryLayout.IsInEquipmentSlot(player.GetInventory(), item);
        }

        internal static bool HasEquippedUtilityType(Humanoid humanoid, ItemDrop.ItemData item)
        {
            if (!IsLocalUtilityItem(humanoid, item)) return false;
            foreach (ItemDrop.ItemData equipped in GetManagedUtilities((Player)humanoid))
            {
                if (equipped.m_shared.m_name == item.m_shared.m_name) return true;
            }
            return false;
        }

        internal static void AddStatusEffects(Humanoid humanoid, HashSet<StatusEffect> effects)
        {
            if (!(humanoid is Player player) || player != Player.m_localPlayer || DadsEPIPlugin.ModEnabled?.Value != true) return;
            foreach (ItemDrop.ItemData item in GetManagedUtilities(player))
            {
                StatusEffect effect = item.m_shared.m_equipStatusEffect;
                if (effect != null) effects.Add(effect);
            }
        }

        internal static void UnequipAdditionalUtilities(Humanoid humanoid)
        {
            if (!(humanoid is Player player) || player != Player.m_localPlayer || DadsEPIPlugin.ModEnabled?.Value != true) return;
            foreach (ItemDrop.ItemData item in new List<ItemDrop.ItemData>(GetManagedUtilities(player)))
            {
                if (item.m_equipped) humanoid.UnequipItem(item, false);
            }
        }

        private static IEnumerable<ItemDrop.ItemData> GetManagedUtilities(Player player)
        {
            Inventory inventory = player.GetInventory();
            if (inventory == null) yield break;
            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (item?.m_shared == null || !item.m_equipped || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Utility) continue;
                if (InventoryLayout.IsInEquipmentSlot(inventory, item)) yield return item;
            }
        }

        private static bool IsLocalUtilityItem(Humanoid humanoid, ItemDrop.ItemData item)
        {
            if (DadsEPIPlugin.ModEnabled?.Value != true || item?.m_shared == null || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Utility) return false;
            return humanoid is Player player && player == Player.m_localPlayer;
        }
    }
}
