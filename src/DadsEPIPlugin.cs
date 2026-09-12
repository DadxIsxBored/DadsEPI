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
        public const string PluginVersion = "1.0.2";

        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<int> ExtraRows;
        internal static ConfigEntry<bool> EquipmentRowEnabled;
        internal static ConfigEntry<bool> SeparateEquipmentPanel;
        internal static ConfigEntry<bool> AutoEquip;
        internal static ConfigEntry<int> QuickSlotCount;
        internal static ConfigEntry<bool> ShowQuickSlots;
        internal static ConfigEntry<bool> AlwaysShowQuickSlots;
        internal static ConfigEntry<int> QuickSlotsPerRow;
        internal static ConfigEntry<bool> WisplightSlot;
        internal static ConfigEntry<bool> WishboneSlot;
        internal static ConfigEntry<bool> CryptKeySlot;
        internal static ConfigEntry<bool> ArrowsSlot;
        internal static ConfigEntry<bool> ShieldSlot;
        internal static ConfigEntry<bool> UtilitySlot;
        internal static ConfigEntry<string> RemovedEquipmentSlots;
        internal static readonly ConfigEntry<string>[] CustomSlotNames = new ConfigEntry<string>[10];
        internal static readonly ConfigEntry<string>[] CustomSlotItems = new ConfigEntry<string>[10];
        internal static ConfigEntry<string> HeadLabel;
        internal static ConfigEntry<string> ChestLabel;
        internal static ConfigEntry<string> LegsLabel;
        internal static ConfigEntry<string> BackLabel;
        internal static ConfigEntry<string> UtilityLabel;
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

            WisplightSlot = Config.Bind("4 - Special Equipment Slots", "Enable Wisplight Slot", true, "Add a Wisplight-only slot.");
            WishboneSlot = Config.Bind("4 - Special Equipment Slots", "Enable Wishbone Slot", true, "Add a Wishbone-only slot.");
            CryptKeySlot = Config.Bind("4 - Special Equipment Slots", "Enable Crypt Key Slot", true, "Add a Crypt Key-only slot.");
            ArrowsSlot = Config.Bind("4 - Special Equipment Slots", "Enable Arrows Slot", true, "Add an arrow-ammunition slot.");
            ShieldSlot = Config.Bind("4 - Special Equipment Slots", "Enable Shield Slot", true, "Add a shield-only slot.");
            UtilitySlot = Config.Bind("4 - Special Equipment Slots", "Enable Utility Slot", true, "Add a utility-item slot.");
            RemovedEquipmentSlots = Config.Bind("4.5 - Equipment Slot Management", "Removed Equipment Slots", "", "Comma or semicolon separated slot names.");
            for (int index = 0; index < CustomSlotNames.Length; index++)
            {
                int slot = index + 1;
                CustomSlotNames[index] = Config.Bind("4.5 - Equipment Slot Management", $"Custom Slot {slot} Name", "", "Visible name for this custom equipment slot. Leave blank to disable it.");
                CustomSlotItems[index] = Config.Bind("4.5 - Equipment Slot Management", $"Custom Slot {slot} Items", "", "Comma-separated exact in-game prefab names accepted by this slot.");
            }

            HeadLabel = Config.Bind("6 - Equipment Slot Labels", "Head Slot Label", "Head", "Helmet slot label.");
            ChestLabel = Config.Bind("6 - Equipment Slot Labels", "Chest Slot Label", "Chest", "Chest slot label.");
            LegsLabel = Config.Bind("6 - Equipment Slot Labels", "Legs Slot Label", "Legs", "Leg slot label.");
            BackLabel = Config.Bind("6 - Equipment Slot Labels", "Back Slot Label", "Back", "Shoulder slot label.");
            UtilityLabel = Config.Bind("6 - Equipment Slot Labels", "Utility Slot Label", "Utility", "Utility slot label.");

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
            Watch(WisplightSlot); Watch(WishboneSlot); Watch(CryptKeySlot); Watch(ArrowsSlot); Watch(ShieldSlot); Watch(UtilitySlot); Watch(RemovedEquipmentSlots);
            for (int index = 0; index < CustomSlotNames.Length; index++) { Watch(CustomSlotNames[index]); Watch(CustomSlotItems[index]); }
            Watch(HeadLabel); Watch(ChestLabel); Watch(LegsLabel); Watch(BackLabel); Watch(UtilityLabel);
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
                if (ShortcutPressed(QuickHotkeys[index].Value)) InventoryLayout.UseQuickSlot(index);
        }

        private static bool ShortcutPressed(KeyboardShortcut shortcut)
        {
            if (shortcut.MainKey == KeyCode.None || !Input.GetKeyDown(shortcut.MainKey)) return false;
            foreach (KeyCode modifier in shortcut.Modifiers)
                if (!Input.GetKey(modifier)) return false;
            return true;
        }

        private static bool InputBlocked()
        {
            return Console.IsVisible() || TextInput.IsVisible() || Menu.IsVisible() || InventoryGui.IsVisible() ||
                   Minimap.IsOpen() || (Chat.instance != null && Chat.instance.HasFocus());
        }
    }
}
