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
        private static RectTransform _messageTextRect;
        private static RectTransform _messageIconRect;
        private static Vector2 _messageTextPosition;
        private static Vector2 _messageIconPosition;

        internal static void Reset()
        {
            RestorePickupMessagePosition();
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
            if (!visible)
            {
                PositionPickupMessage(false);
                return;
            }

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
            _root.anchoredPosition = DadsEPIPlugin.HudPosition.Value;
            PositionPickupMessage(count > 0);
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
                if (binding != null)
                {
                    binding.text = InventoryLayout.Slots[InventoryLayout.EquipmentSlotCount + index].Label;
                    binding.enableAutoSizing = false;
                    binding.textWrappingMode = TextWrappingModes.NoWrap;
                    binding.overflowMode = TextOverflowModes.Overflow;
                    binding.fontSize = 11f;
                    binding.alignment = TextAlignmentOptions.Center;

                    RectTransform bindingRect = binding.rectTransform;
                    bindingRect.anchorMin = new Vector2(0f, 1f);
                    bindingRect.anchorMax = new Vector2(1f, 1f);
                    bindingRect.pivot = new Vector2(0.5f, 1f);
                    bindingRect.offsetMin = new Vector2(2f, -18f);
                    bindingRect.offsetMax = new Vector2(-2f, -2f);
                }
            }
            foreach (string child in new[] { "equiped", "queued", "selected" })
            {
                Transform transform = element.transform.Find(child);
                if (transform != null) transform.gameObject.SetActive(false);
            }
            foreach (GuiBar durability in element.GetComponentsInChildren<GuiBar>(true))
                durability.gameObject.SetActive(false);
        }

        private static void PositionPickupMessage(bool belowSlots)
        {
            MessageHud messageHud = MessageHud.instance;
            RectTransform textRect = messageHud != null && messageHud.m_messageText != null ? messageHud.m_messageText.rectTransform : null;
            RectTransform iconRect = messageHud != null && messageHud.m_messageIcon != null ? messageHud.m_messageIcon.rectTransform : null;
            CapturePickupMessagePosition(textRect, iconRect);
            RestorePickupMessagePosition();
            if (!belowSlots || _root == null) return;

            float quickSlotBottom = WorldEdge(_root, false);
            float messageTop = Mathf.Max(WorldEdge(_messageTextRect, true), WorldEdge(_messageIconRect, true));
            float gap = Mathf.Abs(_root.TransformVector(new Vector3(0f, 10f, 0f)).y);
            float worldOffset = quickSlotBottom - gap - messageTop;
            if (worldOffset >= 0f) return;

            MoveVertically(_messageTextRect, _messageTextPosition, worldOffset);
            if (_messageIconRect != null && (_messageTextRect == null || !_messageIconRect.IsChildOf(_messageTextRect)))
                MoveVertically(_messageIconRect, _messageIconPosition, worldOffset);
        }

        private static void CapturePickupMessagePosition(RectTransform textRect, RectTransform iconRect)
        {
            if (_messageTextRect != textRect)
            {
                _messageTextRect = textRect;
                if (_messageTextRect != null) _messageTextPosition = _messageTextRect.anchoredPosition;
            }
            if (_messageIconRect != iconRect)
            {
                _messageIconRect = iconRect;
                if (_messageIconRect != null) _messageIconPosition = _messageIconRect.anchoredPosition;
            }
        }

        private static void RestorePickupMessagePosition()
        {
            if (_messageTextRect != null) _messageTextRect.anchoredPosition = _messageTextPosition;
            if (_messageIconRect != null) _messageIconRect.anchoredPosition = _messageIconPosition;
        }

        private static float WorldEdge(RectTransform rect, bool top)
        {
            if (rect == null) return top ? float.NegativeInfinity : float.PositiveInfinity;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float edge = corners[0].y;
            for (int index = 1; index < corners.Length; index++)
                edge = top ? Mathf.Max(edge, corners[index].y) : Mathf.Min(edge, corners[index].y);
            return edge;
        }

        private static void MoveVertically(RectTransform rect, Vector2 originalPosition, float worldOffset)
        {
            if (rect == null || rect.parent == null) return;
            float localOffset = rect.parent.InverseTransformVector(new Vector3(0f, worldOffset, 0f)).y;
            rect.anchoredPosition = originalPosition + new Vector2(0f, localOffset);
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
