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
        private const int EquipmentColumns = 3;
        private static readonly Dictionary<int, Transform> OriginalParents = new Dictionary<int, Transform>();
        private static readonly Dictionary<int, Vector2> OriginalPositions = new Dictionary<int, Vector2>();
        private static RectTransform _panel;

        internal static void Reset()
        {
            var elements = new List<InventoryElement>();
            if (InventoryGui.instance != null && InventoryGui.instance.m_playerGrid != null)
                elements.AddRange(InventoryGui.instance.m_playerGrid.GetComponentsInChildren<InventoryElement>(true));
            if (_panel != null)
                elements.AddRange(_panel.GetComponentsInChildren<InventoryElement>(true));
            foreach (InventoryElement element in elements)
            {
                if (element == null) continue;
                int id = element.GetInstanceID();
                RectTransform rect = element.transform as RectTransform;
                if (rect != null && OriginalParents.TryGetValue(id, out Transform original) && original != null)
                {
                    rect.SetParent(original, false);
                    if (OriginalPositions.TryGetValue(id, out Vector2 position)) rect.anchoredPosition = position;
                }
                element.gameObject.SetActive(true);
                Transform label = element.transform.Find(LabelName);
                if (label != null) label.gameObject.SetActive(false);
            }
            if (_panel != null) Object.Destroy(_panel.gameObject);
            _panel = null;
            OriginalParents.Clear();
            OriginalPositions.Clear();
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
                RectTransform rect = element.transform as RectTransform;
                if (rect == null) continue;
                int id = element.GetInstanceID();
                if (!OriginalParents.ContainsKey(id))
                {
                    OriginalParents[id] = rect.parent;
                    OriginalPositions[id] = rect.anchoredPosition;
                }
                int slotIndex = InventoryLayout.SlotIndex(element.Position, grid.GetInventory().GetWidth());
                bool unusedStorageCell = slotIndex < 0 && element.Position.y >= InventoryLayout.NormalRows;
                if (unusedStorageCell)
                {
                    rect.gameObject.SetActive(false);
                    continue;
                }
                rect.gameObject.SetActive(true);
                TMP_Text label = GetOrCreateLabel(element);
                if (slotIndex < 0 || !DadsEPIPlugin.SeparateEquipmentPanel.Value)
                {
                    if (OriginalParents.TryGetValue(id, out Transform original) && rect.parent != original)
                    {
                        rect.SetParent(original, false);
                        if (OriginalPositions.TryGetValue(id, out Vector2 originalPosition)) rect.anchoredPosition = originalPosition;
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
                label.transform.SetAsLastSibling();
            }
            if (_panel != null)
            {
                UpdatePanelLayout(grid);
                _panel.gameObject.SetActive(DadsEPIPlugin.SeparateEquipmentPanel.Value && InventoryLayout.Slots.Count > 0);
            }
        }

        private static Vector2 PanelPosition(int slotIndex, float space)
        {
            if (slotIndex < InventoryLayout.EquipmentSlotCount)
            {
                int column = slotIndex % EquipmentColumns;
                int row = slotIndex / EquipmentColumns;
                return new Vector2(10f + column * space, -10f - row * space);
            }
            int quick = slotIndex - InventoryLayout.EquipmentSlotCount;
            int perRow = Mathf.Min(EquipmentColumns, Mathf.Max(1, InventoryLayout.EnabledQuickSlots));
            int rowIndex = quick / perRow;
            int columnIndex = quick % perRow;
            int equipmentRows = Mathf.CeilToInt(InventoryLayout.EquipmentSlotCount / (float)EquipmentColumns);
            return new Vector2(10f + columnIndex * space, -10f - (equipmentRows + 0.35f) * space - rowIndex * space);
        }

        private static void EnsurePanel(InventoryGrid grid)
        {
            if (_panel != null) return;
            RectTransform playerPanel = InventoryGui.instance != null ? InventoryGui.instance.m_player : null;
            Transform parent = playerPanel != null && playerPanel.parent != null ? playerPanel.parent : grid.transform.parent;
            Transform existing = parent.Find(PanelName);
            if (existing != null)
            {
                _panel = existing.GetComponent<RectTransform>();
                return;
            }
            GameObject panelObject = new GameObject(PanelName, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            _panel = panelObject.GetComponent<RectTransform>();
            _panel.anchorMin = playerPanel != null ? playerPanel.anchorMin : new Vector2(0f, 1f);
            _panel.anchorMax = _panel.anchorMin;
            _panel.pivot = new Vector2(0f, 1f);
            Image panelImage = panelObject.GetComponent<Image>();
            Image sourceImage = FindNativePanelImage(playerPanel);
            if (sourceImage != null)
            {
                panelImage.sprite = sourceImage.sprite;
                panelImage.overrideSprite = sourceImage.overrideSprite;
                panelImage.type = sourceImage.type;
                panelImage.material = sourceImage.material;
                panelImage.color = sourceImage.color;
                panelImage.fillCenter = sourceImage.fillCenter;
                panelImage.preserveAspect = sourceImage.preserveAspect;
                panelImage.pixelsPerUnitMultiplier = sourceImage.pixelsPerUnitMultiplier;
            }
            panelImage.raycastTarget = false;
            _panel.SetAsLastSibling();
            UpdatePanelLayout(grid);
        }

        private static Image FindNativePanelImage(RectTransform playerPanel)
        {
            if (playerPanel == null) return null;

            Image best = null;
            float largestArea = -1f;
            foreach (Image image in playerPanel.GetComponentsInChildren<Image>(true))
            {
                if (image == null || image.sprite == null) continue;
                RectTransform rect = image.rectTransform;
                float area = Mathf.Abs(rect.rect.width * rect.rect.height);
                if (area <= largestArea) continue;
                best = image;
                largestArea = area;
            }
            return best;
        }

        private static void UpdatePanelLayout(InventoryGrid grid)
        {
            if (_panel == null) return;
            RectTransform playerPanel = InventoryGui.instance != null ? InventoryGui.instance.m_player : null;
            if (playerPanel != null)
            {
                _panel.anchorMin = playerPanel.anchorMin;
                _panel.anchorMax = playerPanel.anchorMin;
                var corners = new Vector3[4];
                playerPanel.GetWorldCorners(corners);
                float rightEdge = Mathf.Max(corners[2].x, corners[3].x);
                float topEdge = Mathf.Max(corners[1].y, corners[2].y);
                IncludeRightEdge(InventoryGui.instance.m_weight != null ? InventoryGui.instance.m_weight.rectTransform : null, playerPanel, ref rightEdge);
                IncludeRightEdge(InventoryGui.instance.m_armor != null ? InventoryGui.instance.m_armor.rectTransform : null, playerPanel, ref rightEdge);
                float gap = Mathf.Abs(playerPanel.TransformVector(new Vector3(12f, 0f, 0f)).x);
                _panel.position = new Vector3(rightEdge + gap, topEdge, playerPanel.position.z);
            }
            int equipmentRows = Mathf.CeilToInt(InventoryLayout.EquipmentSlotCount / (float)EquipmentColumns);
            int quickColumns = Mathf.Min(EquipmentColumns, Mathf.Max(1, InventoryLayout.EnabledQuickSlots));
            int quickRows = Mathf.CeilToInt(InventoryLayout.EnabledQuickSlots / (float)quickColumns);
            int columns = Mathf.Max(EquipmentColumns, quickColumns);
            float rows = equipmentRows + (quickRows > 0 ? 0.35f + quickRows : 0f);
            _panel.sizeDelta = new Vector2(columns * grid.m_elementSpace + 20f, rows * grid.m_elementSpace + 20f);
        }

        private static void IncludeRightEdge(RectTransform rect, RectTransform playerPanel, ref float rightEdge)
        {
            if (rect == null) return;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            rightEdge = Mathf.Max(rightEdge, Mathf.Max(corners[2].x, corners[3].x));

            RectTransform parent = rect.parent as RectTransform;
            if (parent == null || parent == playerPanel || parent == _panel.parent) return;
            parent.GetWorldCorners(corners);
            rightEdge = Mathf.Max(rightEdge, Mathf.Max(corners[2].x, corners[3].x));
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
