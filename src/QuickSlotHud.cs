using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DadsEPI
{
    internal static class QuickSlotHud
    {
        private const string RootName = "DadsEPI_QuickSlots";
        private static RectTransform _root;
        private static readonly List<GameObject> Elements = new List<GameObject>();
        private static Vector3 _lastMouse;
        private static bool _dragging;

        internal static void Reset()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            _root = null;
            Elements.Clear();
            _dragging = false;
        }

        internal static void Refresh(Hud hud)
        {
            if (hud == null || Player.m_localPlayer == null || DadsEPIPlugin.ModEnabled?.Value != true) return;
            Ensure(hud);
            if (_root == null) return;
            bool visible = DadsEPIPlugin.ShowQuickSlots.Value && InventoryLayout.EnabledQuickSlots > 0;
            _root.gameObject.SetActive(visible);
            if (!visible) return;

            int count = VisibleSlotCount();
            EnsureElementCount(hud, count);
            int perRow = Mathf.Clamp(DadsEPIPlugin.QuickSlotsPerRow.Value, 1, 8);
            float spacing = 70f;
            Inventory inventory = Player.m_localPlayer.GetInventory();
            for (int index = 0; index < Elements.Count; index++)
            {
                GameObject element = Elements[index];
                bool active = index < count;
                element.SetActive(active);
                if (!active) continue;
                RectTransform rect = element.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2((index % perRow) * spacing, -(index / perRow) * spacing);
                int slotIndex = InventoryLayout.EquipmentSlotCount + index;
                Vector2i position = InventoryLayout.SlotPosition(slotIndex, inventory.GetWidth());
                ItemDrop.ItemData item = inventory.GetItemAt(position.x, position.y);
                SetElement(element, item, index);
            }
            _root.sizeDelta = new Vector2(Mathf.Min(count, perRow) * spacing, Mathf.CeilToInt(count / (float)perRow) * spacing);
            _root.localScale = Vector3.one * Mathf.Clamp(DadsEPIPlugin.HudScale.Value, 0.25f, 3f);
            _root.anchoredPosition = DadsEPIPlugin.HudPosition.Value;
            UpdateDrag();
        }

        private static int VisibleSlotCount()
        {
            int configured = InventoryLayout.EnabledQuickSlots;
            if (DadsEPIPlugin.AlwaysShowQuickSlots.Value) return configured;
            Inventory inventory = Player.m_localPlayer.GetInventory();
            int highest = 0;
            for (int index = 0; index < configured; index++)
            {
                Vector2i position = InventoryLayout.SlotPosition(InventoryLayout.EquipmentSlotCount + index, inventory.GetWidth());
                if (inventory.GetItemAt(position.x, position.y) != null) highest = index + 1;
            }
            return highest;
        }

        private static void Ensure(Hud hud)
        {
            if (_root != null) return;
            GameObject root = new GameObject(RootName, typeof(RectTransform));
            root.transform.SetParent(hud.m_rootObject.transform, false);
            _root = root.GetComponent<RectTransform>();
            _root.anchorMin = new Vector2(0f, 1f);
            _root.anchorMax = new Vector2(0f, 1f);
            _root.pivot = new Vector2(0f, 1f);
        }

        private static void EnsureElementCount(Hud hud, int count)
        {
            HotkeyBar sourceBar = hud.m_rootObject.GetComponentInChildren<HotkeyBar>(true);
            if (sourceBar == null || sourceBar.m_elementPrefab == null) return;
            while (Elements.Count < count)
            {
                GameObject element = Object.Instantiate(sourceBar.m_elementPrefab, _root, false);
                element.name = $"DadsEPI_QuickSlot_{Elements.Count + 1}";
                Elements.Add(element);
            }
        }

        private static void SetElement(GameObject element, ItemDrop.ItemData item, int index)
        {
            Transform iconTransform = element.transform.Find("icon");
            if (iconTransform != null)
            {
                Image icon = iconTransform.GetComponent<Image>();
                icon.gameObject.SetActive(item != null);
                if (item != null) icon.sprite = item.GetIcon();
            }
            Transform amountTransform = element.transform.Find("amount");
            if (amountTransform != null)
            {
                TMP_Text amount = amountTransform.GetComponent<TMP_Text>();
                amount.gameObject.SetActive(item != null && item.m_shared.m_maxStackSize > 1);
                if (item != null) amount.text = item.m_stack.ToString();
            }
            Transform bindingTransform = element.transform.Find("binding");
            if (bindingTransform != null)
            {
                TMP_Text binding = bindingTransform.GetComponent<TMP_Text>();
                if (binding != null) binding.text = InventoryLayout.Slots[InventoryLayout.EquipmentSlotCount + index].Label;
            }
            foreach (string child in new[] { "equiped", "queued", "selected" })
            {
                Transform transform = element.transform.Find(child);
                if (transform != null) transform.gameObject.SetActive(false);
            }
        }

        private static void UpdateDrag()
        {
            Vector3 mouse = Input.mousePosition;
            if (DadsEPIPlugin.HudDragKeys.Value.IsPressed())
            {
                if (!_dragging) _dragging = RectTransformUtility.RectangleContainsScreenPoint(_root, mouse);
                if (_dragging && _lastMouse != Vector3.zero)
                {
                    Vector3 delta = mouse - _lastMouse;
                    DadsEPIPlugin.HudPosition.Value += new Vector2(delta.x, delta.y);
                }
            }
            else _dragging = false;
            _lastMouse = mouse;
        }
    }
}
