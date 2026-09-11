using System;
using HarmonyLib;

namespace DadsEPI
{
    [HarmonyPatch(typeof(Player), nameof(Player.Load), new[] { typeof(ZPackage) })]
    internal static class PlayerLoadPatch
    {
        private static void Prefix(Player __instance)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true)
            {
                __instance.GetInventory()?.SetHeight(InventoryLayout.TotalRows);
            }
        }

        private static void Postfix(Player __instance)
        {
            InventoryLayout.Apply(__instance, normalizeItems: true);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    internal static class PlayerSpawnedPatch
    {
        private static void Postfix(Player __instance)
        {
            InventoryLayout.Apply(__instance, normalizeItems: true);
        }
    }

    [HarmonyPatch(typeof(Inventory), "FindEmptySlot")]
    internal static class FindEmptySlotPatch
    {
        private static bool Prefix(Inventory __instance, bool __0, ref Vector2i __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value != true || !InventoryLayout.IsPlayerInventory(__instance))
            {
                return true;
            }

            __result = InventoryLayout.FindGeneralEmpty(__instance, __0);
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetEmptySlots))]
    internal static class GetEmptySlotsPatch
    {
        private static void Postfix(Inventory __instance, ref int __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true && InventoryLayout.IsPlayerInventory(__instance))
            {
                __result = InventoryLayout.CountGeneralEmpty(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot))]
    internal static class HaveEmptySlotPatch
    {
        private static void Postfix(Inventory __instance, ref bool __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true && InventoryLayout.IsPlayerInventory(__instance))
            {
                __result = InventoryLayout.CountGeneralEmpty(__instance) > 0;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem))]
    internal static class InventoryGridDropPatch
    {
        private static bool Prefix(InventoryGrid __instance, ItemDrop.ItemData item, Vector2i pos, ref bool __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value != true || !InventoryLayout.IsPlayerInventory(__instance.GetInventory()))
            {
                return true;
            }

            if (!InventoryLayout.CanPlace(item, pos))
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "DadsEPI: that item cannot use this reserved slot.");
                __result = false;
                return false;
            }

            if (item != null && item.m_equipped && item.m_gridPos.y == InventoryLayout.EquipmentRow && pos.y != InventoryLayout.EquipmentRow)
            {
                Player.m_localPlayer?.UnequipItem(item);
            }

            return true;
        }

        private static void Postfix(InventoryGrid __instance, ItemDrop.ItemData item, Vector2i pos, bool __result)
        {
            if (!__result || item == null || Player.m_localPlayer == null || !InventoryLayout.IsPlayerInventory(__instance.GetInventory()))
            {
                return;
            }

            if (pos.y == InventoryLayout.EquipmentRow && InventoryLayout.EquipmentColumn(item.m_shared.m_itemType) == pos.x)
            {
                Player.m_localPlayer.EquipItem(item);
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem), new[] { typeof(ItemDrop.ItemData), typeof(bool) })]
    internal static class EquipItemPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, bool __result)
        {
            if (__result && DadsEPIPlugin.ModEnabled?.Value == true && __instance is Player player && player == Player.m_localPlayer)
            {
                InventoryLayout.MoveEquippedItemToReservedSlot(player, item);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateInventory))]
    internal static class InventoryGridUpdatePatch
    {
        private static void Postfix(InventoryGrid __instance)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true && InventoryLayout.IsPlayerInventory(__instance.GetInventory()))
            {
                SlotVisuals.Refresh(__instance);
            }
        }
    }
}

