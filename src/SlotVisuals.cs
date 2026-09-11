using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DadsEPI
{
    internal static class SlotVisuals
    {
        private const string PanelName = "DadsEPI_EquipmentPanel";
        private const string LabelName = "DadsEPI_SlotLabel";
        private static readonly Dictionary<int, Transform> OriginalParents = new Dictionary<int, Transform>();
        private static RectTransform _panel;

        internal static void Reset()
        {
            if (_panel != null)
            {
                foreach (InventoryElement element in _panel.GetComponentsInChildren<InventoryElement>(true))
                {
                    if (OriginalParents.TryGetValue(element.GetInstanceID(), out Transform original) && original != null)
                        element.transform.SetParent(original, false);
                }
                Object.Destroy(_panel.gameObject);
            }
            _panel = null;
            OriginalParents.Clear();
        }

        internal static void Refresh(InventoryGrid grid)
        {
            if (grid == null || !InventoryLayout.IsPlayerInventory(grid.GetInventory())) return;
            EnsurePanel(grid);
            var elements = new List<InventoryElement>(grid.GetComponentsInChildren<InventoryElement>(true));
            if (_panel != null) elements.AddRange(_panel.GetComponentsInChildren<InventoryElement>(true));
            foreach (InventoryElement element in elements)
            {
                if (element == null) continue;
                RectTransform rect = element.GetElementRectTransform();
                int id = element.GetInstanceID();
                if (!OriginalParents.ContainsKey(id)) OriginalParents[id] = rect.parent;
                int slotIndex = InventoryLayout.SlotIndex(element.Position, grid.GetInventory().GetWidth());
                TMP_Text label = GetOrCreateLabel(element);
                if (slotIndex < 0 || !DadsEPIPlugin.SeparateEquipmentPanel.Value)
                {
                    if (OriginalParents.TryGetValue(id, out Transform original) && rect.parent != original)
                    {
                        rect.SetParent(original, false);
                        rect.anchoredPosition = new Vector2(element.Position.x * grid.m_elementSpace, -element.Position.y * grid.m_elementSpace);
                    }
                    label.gameObject.SetActive(slotIndex >= 0);
                    if (slotIndex >= 0) label.text = InventoryLayout.Slots[slotIndex].Label;
                    continue;
                }

                rect.SetParent(_panel, false);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = PanelPosition(slotIndex, grid.m_elementSpace);
                label.text = InventoryLayout.Slots[slotIndex].Label;
                label.gameObject.SetActive(true);
            }
            if (_panel != null) _panel.gameObject.SetActive(DadsEPIPlugin.SeparateEquipmentPanel.Value && InventoryLayout.Slots.Count > 0);
        }

        private static Vector2 PanelPosition(int slotIndex, float space)
        {
            if (slotIndex < InventoryLayout.EquipmentSlotCount)
            {
                int column = slotIndex / 4;
                int row = slotIndex % 4;
                return new Vector2(10f + column * space, -10f - row * space);
            }
            int quick = slotIndex - InventoryLayout.EquipmentSlotCount;
            int perRow = Mathf.Min(4, Mathf.Max(1, InventoryLayout.EnabledQuickSlots));
            int rowIndex = quick / perRow;
            int columnIndex = quick % perRow;
            return new Vector2(10f + columnIndex * space, -10f - 4.35f * space - rowIndex * space);
        }

        private static void EnsurePanel(InventoryGrid grid)
        {
            if (_panel != null) return;
            Transform parent = InventoryGui.instance != null ? InventoryGui.instance.m_player : grid.transform.parent;
            Transform existing = parent.Find(PanelName);
            if (existing != null)
            {
                _panel = existing.GetComponent<RectTransform>();
                return;
            }
            GameObject panelObject = new GameObject(PanelName, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            _panel = panelObject.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0f, 1f);
            _panel.anchorMax = new Vector2(0f, 1f);
            _panel.pivot = new Vector2(1f, 1f);
            _panel.anchoredPosition = new Vector2(-12f, -12f);
            int columns = Mathf.Max(2, Mathf.Min(4, InventoryLayout.EnabledQuickSlots));
            int quickRows = Mathf.CeilToInt(InventoryLayout.EnabledQuickSlots / (float)columns);
            _panel.sizeDelta = new Vector2(Mathf.Max(2, columns) * grid.m_elementSpace + 20f, (4.4f + quickRows) * grid.m_elementSpace + 20f);
            Image panelImage = panelObject.GetComponent<Image>();
            Image sourceImage = parent.GetComponent<Image>();
            if (sourceImage != null)
            {
                panelImage.sprite = sourceImage.sprite;
                panelImage.type = sourceImage.type;
                panelImage.material = sourceImage.material;
            }
            panelImage.color = new Color(0.12f, 0.09f, 0.07f, 0.94f);
            panelImage.raycastTarget = false;
            _panel.SetAsFirstSibling();
        }

        private static TMP_Text GetOrCreateLabel(InventoryElement element)
        {
            Transform existing = element.transform.Find(LabelName);
            if (existing != null) return existing.GetComponent<TMP_Text>();
            GameObject labelObject = new GameObject(LabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(element.transform, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            if (element.m_amount != null)
            {
                label.font = element.m_amount.font;
                label.fontSharedMaterial = element.m_amount.fontSharedMaterial;
            }
            label.enabled = true;
            label.alignment = TextAlignmentOptions.Top;
            label.fontSize = 11f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(0.95f, 0.84f, 0.60f, 1f);
            label.raycastTarget = false;
            return label;
        }
    }
}
