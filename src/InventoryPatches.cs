using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace DadsEPI
{
    [HarmonyPatch(typeof(Player), nameof(Player.Load), new[] { typeof(ZPackage) })]
    internal static class PlayerLoadPatch
    {
        private static void Prefix(Player __instance)
        {
            if (DadsEPIPlugin.ModEnabled?.Value != true) return;
            Inventory inventory = __instance.GetInventory();
            inventory?.SetHeight(Math.Max(inventory.GetHeight(), InventoryLayout.TotalRows(inventory.GetWidth())));
        }

        private static void Postfix(Player __instance) => InventoryLayout.Apply(__instance, true);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned), new[] { typeof(bool) })]
    internal static class PlayerSpawnedPatch
    {
        private static void Postfix(Player __instance) => InventoryLayout.Apply(__instance, true);
    }

    [HarmonyPatch(typeof(Player), "EquipInventoryItems")]
    internal static class EquipInventoryItemsPatch
    {
        private static void Prefix()
        {
            UtilityEquipment.BeginInventoryRestore();
        }

        private static Exception Finalizer(Exception __exception)
        {
            UtilityEquipment.EndInventoryRestore();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetInventorySize))]
    internal static class NativeInventorySizePatch
    {
        private static bool Prefix(Player __instance, int rows)
        {
            if (DadsEPIPlugin.ModEnabled?.Value != true) return true;
            rows = UnityEngine.Mathf.Clamp(rows, InventoryLayout.BaseVanillaRows, InventoryLayout.MaximumVanillaRows);
            InventoryLayout.SetVanillaRows(rows);
            __instance.AddUniqueKeyValue(Player.InventoryRowsKey, rows.ToString());
            Inventory inventory = __instance.GetInventory();
            inventory.SetHeight(InventoryLayout.TotalRows(inventory.GetWidth()));
            InventoryLayout.Apply(__instance, true);
            __instance.DropInvalidItems();
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), "FindEmptySlot")]
    internal static class FindEmptySlotPatch
    {
        private static bool Prefix(Inventory __instance, bool __0, ref Vector2i __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value != true || !InventoryLayout.IsPlayerInventory(__instance)) return true;
            __result = InventoryLayout.FindPreferredEmpty(__instance, __0);
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetEmptySlots))]
    internal static class GetEmptySlotsPatch
    {
        private static void Postfix(Inventory __instance, ref int __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true && InventoryLayout.IsPlayerInventory(__instance))
                __result = InventoryLayout.CountGeneralEmpty(__instance);
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot))]
    internal static class HaveEmptySlotPatch
    {
        private static void Postfix(Inventory __instance, ref bool __result)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true && InventoryLayout.IsPlayerInventory(__instance))
                __result = InventoryLayout.CountGeneralEmpty(__instance) > 0;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
    internal static class AddItemAutoEquipmentPatch
    {
        private static void Prefix(Inventory __instance, ItemDrop.ItemData item)
        {
            InventoryLayout.BeginAutoAdd(__instance, item);
        }

        private static void Postfix(Inventory __instance, ItemDrop.ItemData item, bool __result)
        {
            InventoryLayout.EndAutoAdd();
            if (!__result || DadsEPIPlugin.ModEnabled?.Value != true || Player.m_localPlayer == null) return;
            InventoryLayout.AutoPlaceNewItem(Player.m_localPlayer, __instance, item);
        }

        private static Exception Finalizer(Exception __exception)
        {
            InventoryLayout.EndAutoAdd();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem), new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(Vector2i) })]
    internal static class InventoryGridDropPatch
    {
        private static bool Prefix(InventoryGrid __instance, ItemDrop.ItemData item, Vector2i pos, ref bool __result)
        {
            Inventory inventory = __instance.GetInventory();
            if (DadsEPIPlugin.ModEnabled?.Value != true || !InventoryLayout.IsPlayerInventory(inventory)) return true;
            if (!InventoryLayout.CanPlace(item, pos, inventory.GetWidth()))
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "This item does not match that dedicated slot.");
                __result = false;
                return false;
            }
            int oldSlot = InventoryLayout.SlotIndex(item?.m_gridPos ?? new Vector2i(-1, -1), inventory.GetWidth());
            int newSlot = InventoryLayout.SlotIndex(pos, inventory.GetWidth());
            if (item != null && item.m_equipped && oldSlot >= 0 && newSlot < 0) Player.m_localPlayer?.UnequipItem(item);
            return true;
        }

        private static void Postfix(InventoryGrid __instance, ItemDrop.ItemData item, Vector2i pos, bool __result)
        {
            if (!__result || item == null || Player.m_localPlayer == null) return;
            Inventory inventory = __instance.GetInventory();
            if (!InventoryLayout.IsPlayerInventory(inventory)) return;
            int slot = InventoryLayout.SlotIndex(pos, inventory.GetWidth());
            if (slot >= 0 && slot < InventoryLayout.EquipmentSlotCount && DadsEPIPlugin.AutoEquip.Value)
                Player.m_localPlayer.EquipItem(item);
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem), new[] { typeof(ItemDrop.ItemData), typeof(bool) })]
    internal static class EquipItemPatch
    {
        private static void Prefix(Humanoid __instance, ItemDrop.ItemData item, out ItemDrop.ItemData __state)
        {
            __state = UtilityEquipment.BeginEquip(__instance, item);
        }

        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, bool __result, ItemDrop.ItemData __state)
        {
            UtilityEquipment.EndEquip(__instance, __state, __result);
            if (__result && DadsEPIPlugin.ModEnabled?.Value == true && __instance is Player player && player == Player.m_localPlayer)
                InventoryLayout.MoveEquippedItemToDedicatedSlot(player, item);
        }

        private static Exception Finalizer(Exception __exception, Humanoid __instance, ItemDrop.ItemData __state)
        {
            if (__exception != null) UtilityEquipment.EndEquip(__instance, __state, false);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.IsItemEquiped))]
    internal static class IsItemEquippedPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!__result && UtilityEquipment.IsManagedUtility(__instance, item)) __result = true;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.IsItemTypeEquiped))]
    internal static class IsItemTypeEquippedPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!__result && UtilityEquipment.HasEquippedUtilityType(__instance, item)) __result = true;
        }
    }

    [HarmonyPatch(typeof(Humanoid), "UpdateEquipmentStatusEffects")]
    internal static class EquipmentStatusEffectsPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var statusEffectsField = AccessTools.Field(typeof(Humanoid), "m_equipmentStatusEffects");
            var addUtilityEffects = AccessTools.Method(typeof(UtilityEquipment), nameof(UtilityEquipment.AddStatusEffects));

            for (int index = 1; index < codes.Count; index++)
            {
                if (codes[index].opcode != OpCodes.Ldfld || !Equals(codes[index].operand, statusEffectsField) || codes[index - 1].opcode != OpCodes.Ldarg_0) continue;

                int insertion = index - 1;
                var loadHumanoid = new CodeInstruction(OpCodes.Ldarg_0);
                loadHumanoid.labels.AddRange(codes[insertion].labels);
                loadHumanoid.blocks.AddRange(codes[insertion].blocks);
                codes[insertion].labels.Clear();
                codes[insertion].blocks.Clear();
                codes.InsertRange(insertion, new[]
                {
                    loadHumanoid,
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, addUtilityEffects)
                });
                return codes;
            }

            DadsEPIPlugin.ModLogger?.LogError("Could not patch utility status-effect collection.");
            return codes;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipAllItems))]
    internal static class UnequipAllItemsPatch
    {
        private static void Postfix(Humanoid __instance)
        {
            UtilityEquipment.UnequipAdditionalUtilities(__instance);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
    internal static class CraftingAutoEquipPatch
    {
        private static void Postfix(Player __0)
        {
            InventoryLayout.AutoEquipDedicatedItems(__0);
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateInventory))]
    internal static class InventoryGridUpdatePatch
    {
        private static void Postfix(InventoryGrid __instance)
        {
            if (DadsEPIPlugin.ModEnabled?.Value == true && InventoryLayout.IsPlayerInventory(__instance.GetInventory()))
                SlotVisuals.Refresh(__instance);
        }
    }

    [HarmonyPatch(typeof(Hud), "Update")]
    internal static class HudUpdatePatch
    {
        private static void Postfix(Hud __instance) => QuickSlotHud.Refresh(__instance);
    }

    [HarmonyPatch(typeof(Hud), "OnDestroy")]
    internal static class HudDestroyPatch
    {
        private static void Prefix() => QuickSlotHud.Reset();
    }
}
