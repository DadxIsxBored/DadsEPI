using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DadsEPI
{
    internal static class SlotVisuals
    {
        private const string LabelName = "DadsEPI_SlotLabel";
        private static readonly Dictionary<int, ColorBlock> OriginalColors = new Dictionary<int, ColorBlock>();
        private static readonly Color EquipmentColor = new Color(0.48f, 0.30f, 0.12f, 0.92f);
        private static readonly Color QuickColor = new Color(0.10f, 0.32f, 0.48f, 0.92f);
        private static readonly Color DisabledColor = new Color(0.12f, 0.12f, 0.12f, 0.80f);

        internal static void Refresh(InventoryGrid grid)
        {
            InventoryElement[] elements = grid.GetComponentsInChildren<InventoryElement>(includeInactive: true);
            foreach (InventoryElement element in elements)
            {
                if (element == null || element.m_button == null)
                {
                    continue;
                }

                int id = element.GetInstanceID();
                if (!OriginalColors.TryGetValue(id, out ColorBlock original))
                {
                    original = element.m_button.colors;
                    OriginalColors[id] = original;
                }

                Vector2i position = element.Position;
                bool reserved = InventoryLayout.IsReserved(position);
                TMP_Text label = GetOrCreateLabel(element);
                if (!reserved)
                {
                    element.m_button.colors = original;
                    label.gameObject.SetActive(false);
                    continue;
                }

                string labelText = GetLabel(position);
                ColorBlock colors = original;
                colors.normalColor = GetColor(position);
                colors.selectedColor = colors.normalColor;
                element.m_button.colors = colors;
                label.text = labelText;
                label.gameObject.SetActive(true);
            }
        }

        private static TMP_Text GetOrCreateLabel(InventoryElement element)
        {
            Transform existing = element.transform.Find(LabelName);
            if (existing != null)
            {
                return existing.GetComponent<TMP_Text>();
            }

            GameObject labelObject = new GameObject(LabelName, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(element.transform, worldPositionStays: false);
            RectTransform rect = (RectTransform)labelObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(3f, 2f);
            rect.offsetMax = new Vector2(-3f, -2f);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.TopLeft;
            label.fontSize = 9f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(0.95f, 0.86f, 0.62f, 0.90f);
            label.raycastTarget = false;
            return label;
        }

        private static string GetLabel(Vector2i position)
        {
            if (position.y == InventoryLayout.QuickRow)
            {
                return position.x < InventoryLayout.EnabledQuickSlots ? $"Q{position.x + 1}" : "";
            }

            if (position.y != InventoryLayout.EquipmentRow)
            {
                return "";
            }

            switch (position.x)
            {
                case 0: return "HEAD";
                case 1: return "BODY";
                case 2: return "LEGS";
                case 3: return "BACK";
                case 4: return "UTIL";
                case 5: return "HANDS";
                case 6: return "TRINKET";
                default: return "";
            }
        }

        private static Color GetColor(Vector2i position)
        {
            if (position.y == InventoryLayout.QuickRow)
            {
                return position.x < InventoryLayout.EnabledQuickSlots ? QuickColor : DisabledColor;
            }

            return position.y == InventoryLayout.EquipmentRow && position.x <= 6 ? EquipmentColor : DisabledColor;
        }
    }
}

