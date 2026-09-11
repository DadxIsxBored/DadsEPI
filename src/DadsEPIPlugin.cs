using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace DadsEPI
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class DadsEPIPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.dadisbored.dadsepi";
        public const string PluginName = "DadsEPI";
        public const string PluginVersion = "0.1.0";

        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<int> GeneralExtraRows;
        internal static ConfigEntry<bool> EquipmentRowEnabled;
        internal static ConfigEntry<bool> QuickRowEnabled;
        internal static ConfigEntry<int> QuickSlotCount;
        internal static ConfigEntry<KeyboardShortcut> QuickSlotOne;
        internal static ConfigEntry<KeyboardShortcut> QuickSlotTwo;
        internal static ConfigEntry<KeyboardShortcut> QuickSlotThree;
        internal static ManualLogSource ModLogger;

        private Harmony _harmony;
        private float _nextLayoutCheck;

        private void Awake()
        {
            ModLogger = Logger;
            ModEnabled = Config.Bind("General", "Enabled", true, "Enable DadsEPI.");
            GeneralExtraRows = Config.Bind("Inventory", "GeneralExtraRows", 2, "Additional general inventory rows. Total inventory height is capped at nine rows by Valheim.");
            EquipmentRowEnabled = Config.Bind("Inventory", "EquipmentRow", true, "Reserve one row for equipped armor, utility, hand, and trinket items.");
            QuickRowEnabled = Config.Bind("Inventory", "QuickRow", true, "Reserve one row for configurable quick-use slots.");
            QuickSlotCount = Config.Bind("Inventory", "QuickSlotCount", 3, "Number of enabled quick slots, from one through eight.");
            QuickSlotOne = Config.Bind("Hotkeys", "QuickSlot1", new KeyboardShortcut(KeyCode.Z), "Use quick slot one.");
            QuickSlotTwo = Config.Bind("Hotkeys", "QuickSlot2", new KeyboardShortcut(KeyCode.X), "Use quick slot two.");
            QuickSlotThree = Config.Bind("Hotkeys", "QuickSlot3", new KeyboardShortcut(KeyCode.C), "Use quick slot three.");

            GeneralExtraRows.SettingChanged += OnLayoutSettingChanged;
            EquipmentRowEnabled.SettingChanged += OnLayoutSettingChanged;
            QuickRowEnabled.SettingChanged += OnLayoutSettingChanged;
            QuickSlotCount.SettingChanged += OnLayoutSettingChanged;
            ModEnabled.SettingChanged += OnLayoutSettingChanged;

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(DadsEPIPlugin).Assembly);
            Logger.LogInfo($"{PluginName} {PluginVersion} initialized for Valheim's native inventory format.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private static void OnLayoutSettingChanged(object sender, System.EventArgs args)
        {
            if (Player.m_localPlayer != null)
            {
                InventoryLayout.Apply(Player.m_localPlayer, normalizeItems: true);
            }
        }

        private void Update()
        {
            if (ModEnabled?.Value != true || Player.m_localPlayer == null)
            {
                return;
            }

            if (Time.unscaledTime >= _nextLayoutCheck)
            {
                _nextLayoutCheck = Time.unscaledTime + 1f;
                InventoryLayout.Apply(Player.m_localPlayer, normalizeItems: true);
            }

            if (!QuickRowEnabled.Value || InputBlocked())
            {
                return;
            }

            if (QuickSlotOne.Value.IsDown()) InventoryLayout.UseQuickSlot(0);
            if (QuickSlotTwo.Value.IsDown()) InventoryLayout.UseQuickSlot(1);
            if (QuickSlotThree.Value.IsDown()) InventoryLayout.UseQuickSlot(2);
        }

        private static bool InputBlocked()
        {
            return Console.IsVisible()
                   || TextInput.IsVisible()
                   || Menu.IsVisible()
                   || InventoryGui.IsVisible()
                   || Minimap.IsOpen()
                   || (Chat.instance != null && Chat.instance.HasFocus());
        }
    }
}

