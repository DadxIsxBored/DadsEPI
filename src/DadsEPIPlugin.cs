using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace DadsEPI
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public sealed class DadsEPIPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.dadisbored.dadsepi";
        public const string PluginName = "DadsEPI";
        public const string PluginVersion = "0.2.0";

        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<int> ExtraRows;
        internal static ConfigEntry<bool> EquipmentRowEnabled;
        internal static ConfigEntry<bool> SeparateEquipmentPanel;
        internal static ConfigEntry<bool> AutoEquip;
        internal static ConfigEntry<int> QuickSlotCount;
        internal static ConfigEntry<bool> ShowQuickSlots;
        internal static ConfigEntry<bool> AlwaysShowQuickSlots;
        internal static ConfigEntry<int> QuickSlotsPerRow;
        internal static ConfigEntry<bool> WishboneSlot;
        internal static ConfigEntry<bool> DemisterSlot;
        internal static ConfigEntry<bool> OneUtilityAtATime;
        internal static ConfigEntry<string> RemovedEquipmentSlots;
        internal static ConfigEntry<string> CustomEquipmentSlots;
        internal static ConfigEntry<string> HeadLabel;
        internal static ConfigEntry<string> ChestLabel;
        internal static ConfigEntry<string> LegsLabel;
        internal static ConfigEntry<string> BackLabel;
        internal static ConfigEntry<string> UtilityLabel;
        internal static ConfigEntry<string> TrinketLabel;
        internal static ConfigEntry<float> HudScale;
        internal static ConfigEntry<Vector2> HudPosition;
        internal static ConfigEntry<KeyboardShortcut> HudDragKeys;
        internal static readonly ConfigEntry<KeyboardShortcut>[] QuickHotkeys = new ConfigEntry<KeyboardShortcut>[8];
        internal static readonly ConfigEntry<string>[] QuickHotkeyLabels = new ConfigEntry<string>[8];
        internal static ManualLogSource ModLogger;

        private Harmony _harmony;
        private float _nextLayoutCheck;

        private void Awake()
        {
            ModLogger = Logger;
            ModEnabled = Config.Bind("1 - Server & Sync", "Mod Enabled", true, "Enable DadsEPI.");
            ExtraRows = Config.Bind("2 - Inventory", "Extra Inventory Rows", 0,
                new ConfigDescription("Additional ordinary rows; zero retains the native inventory size.", new AcceptableValueRange<int>(0, 5)));
            EquipmentRowEnabled = Config.Bind("2 - Inventory", "Enable Equipment Row", true, "Add dedicated equipment and quick slots.");
            SeparateEquipmentPanel = Config.Bind("2 - Inventory", "Display Equipment in Separate Panel", true, "Render dedicated slots in a labeled side panel.");
            AutoEquip = Config.Bind("2 - Inventory", "Auto-Equip Items", true, "Equip items placed in equipment slots.");

            QuickSlotCount = Config.Bind("3 - Quick Slots", "Number of Quick Slots", 3,
                new ConfigDescription("Number of quick slots.", new AcceptableValueRange<int>(0, 8)));
            ShowQuickSlots = Config.Bind("3 - Quick Slots", "Show Quick Slots on HUD", true, "Display quick slots during gameplay.");
            AlwaysShowQuickSlots = Config.Bind("3 - Quick Slots", "Always Show All Slots", true, "Display empty quick slots on the HUD.");
            QuickSlotsPerRow = Config.Bind("3 - Quick Slots", "Slots Per Row", 8,
                new ConfigDescription("Maximum quick slots per HUD row.", new AcceptableValueRange<int>(1, 8)));

            WishboneSlot = Config.Bind("4 - Special Equipment Slots", "Enable Wishbone Slot", true, "Add a Wishbone-only slot.");
            DemisterSlot = Config.Bind("4 - Special Equipment Slots", "Enable Demister Slot", true, "Add a Demister-only slot.");
            OneUtilityAtATime = Config.Bind("4 - Special Equipment Slots", "One Utility Item At A Time", false, "Use vanilla utility-item exclusivity.");
            RemovedEquipmentSlots = Config.Bind("4.5 - Equipment Slot Management", "Removed Equipment Slots", "", "Comma or semicolon separated slot names.");
            CustomEquipmentSlots = Config.Bind("4.5 - Equipment Slot Management", "Custom Equipment Slots", "", "SlotName:Prefab1,Prefab2;SlotName2:Prefab3");

            HeadLabel = Config.Bind("6 - Equipment Slot Labels", "Head Slot Label", "Head", "Helmet slot label.");
            ChestLabel = Config.Bind("6 - Equipment Slot Labels", "Chest Slot Label", "Chest", "Chest slot label.");
            LegsLabel = Config.Bind("6 - Equipment Slot Labels", "Legs Slot Label", "Legs", "Leg slot label.");
            BackLabel = Config.Bind("6 - Equipment Slot Labels", "Back Slot Label", "Back", "Shoulder slot label.");
            UtilityLabel = Config.Bind("6 - Equipment Slot Labels", "Utility Slot Label", "Utility", "Utility slot label.");
            TrinketLabel = Config.Bind("6 - Equipment Slot Labels", "Trinket Slot Label", "Trinket", "Trinket slot label.");

            HudScale = Config.Bind("7 - Quick Slots Customization", "HUD Size", 1f, "Quick-slot HUD scale.");
            HudPosition = Config.Bind("7 - Quick Slots Customization", "HUD Position", new Vector2(47f, -115f), "Quick-slot HUD anchored position.");
            HudDragKeys = Config.Bind("7 - Quick Slots Customization", "Drag to Reposition Keys", new KeyboardShortcut(KeyCode.Mouse0, KeyCode.LeftControl), "Drag the quick-slot HUD.");

            KeyboardShortcut[] defaults =
            {
                new KeyboardShortcut(KeyCode.Z, KeyCode.LeftAlt), new KeyboardShortcut(KeyCode.X, KeyCode.LeftAlt),
                new KeyboardShortcut(KeyCode.C, KeyCode.LeftAlt), new KeyboardShortcut(KeyCode.V, KeyCode.LeftAlt),
                new KeyboardShortcut(KeyCode.B, KeyCode.LeftAlt), new KeyboardShortcut(KeyCode.N, KeyCode.LeftAlt),
                new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftAlt), new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftAlt)
            };
            for (int index = 0; index < 8; index++)
            {
                int slot = index + 1;
                QuickHotkeys[index] = Config.Bind("8 - Quick Slot Hotkeys", $"Hotkey {slot}", defaults[index], $"Shortcut for quick slot {slot}.");
                QuickHotkeyLabels[index] = Config.Bind("8 - Quick Slot Hotkeys", $"Hotkey {slot} Display Text", $"Alt + {defaults[index].MainKey.ToString().Replace("Alpha", "")}", $"HUD text for quick slot {slot}.");
            }

            Watch(ModEnabled); Watch(ExtraRows); Watch(EquipmentRowEnabled); Watch(SeparateEquipmentPanel); Watch(AutoEquip);
            Watch(QuickSlotCount); Watch(ShowQuickSlots); Watch(AlwaysShowQuickSlots); Watch(QuickSlotsPerRow);
            Watch(WishboneSlot); Watch(DemisterSlot); Watch(OneUtilityAtATime); Watch(RemovedEquipmentSlots); Watch(CustomEquipmentSlots);
            Watch(HeadLabel); Watch(ChestLabel); Watch(LegsLabel); Watch(BackLabel); Watch(UtilityLabel); Watch(TrinketLabel);
            for (int index = 0; index < 8; index++) { Watch(QuickHotkeys[index]); Watch(QuickHotkeyLabels[index]); }
            InventoryLayout.RebuildSlots();
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(DadsEPIPlugin).Assembly);
            Logger.LogInfo($"{PluginName} {PluginVersion} initialized with separate equipment and quick-slot compartments.");
        }

        private void OnDestroy() => _harmony?.UnpatchSelf();

        private static void Watch<T>(ConfigEntry<T> entry) => entry.SettingChanged += OnLayoutSettingChanged;

        private static void OnLayoutSettingChanged(object sender, EventArgs args)
        {
            InventoryLayout.RebuildSlots();
            if (Player.m_localPlayer != null) InventoryLayout.Apply(Player.m_localPlayer, true);
            SlotVisuals.Reset();
            QuickSlotHud.Reset();
        }

        private void Update()
        {
            if (ModEnabled?.Value != true || Player.m_localPlayer == null) return;
            if (Time.unscaledTime >= _nextLayoutCheck)
            {
                _nextLayoutCheck = Time.unscaledTime + 0.5f;
                InventoryLayout.Apply(Player.m_localPlayer, true);
            }
            if (InputBlocked()) return;
            for (int index = 0; index < InventoryLayout.EnabledQuickSlots; index++)
                if (QuickHotkeys[index].Value.IsDown()) InventoryLayout.UseQuickSlot(index);
        }

        private static bool InputBlocked()
        {
            return Console.IsVisible() || TextInput.IsVisible() || Menu.IsVisible() || InventoryGui.IsVisible() ||
                   Minimap.IsOpen() || (Chat.instance != null && Chat.instance.HasFocus());
        }
    }
}
