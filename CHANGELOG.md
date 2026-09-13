# Changelog

## 1.1.0

- Moved top-left item pickup messages below the visible quick-slot HUD.
- Removed durability bars from the quick-slot HUD while retaining item stack counts.
- Moved the separate equipment panel beyond the inventory weight and armor displays.

## 1.0.2

- Rebuilt against BepInEx `5.4.23.5` from BepInExPack Valheim `5.4.2350`.

## 1.0.1

- Reduced the in-game quick-slot HUD label size to 11 pixels.
- Disabled quick-slot label wrapping and widened the label bounds so shortcut text remains horizontal.

## 1.0.0

- Published the first stable DadsEPI release for Valheim 1.0.12.
- Includes the separate labeled equipment grid, configurable accepted-prefab lists, optional equipment compartments, configurable quick slots, movement-compatible quick-slot hotkeys, and native-sized inventory defaults developed through versions 0.1.0 through 0.3.2.
- Automatically routes newly acquired accepted equipment into its matching empty dedicated slot and equips it when the Auto-Equip setting is enabled; occupied dedicated slots are left unchanged.

## 0.3.2

- Changed quick-slot shortcut detection so held movement, sprint, crouch, and other unrelated keys do not block activation.

## 0.3.1

- Hid unused padding cells in the final internal equipment-storage row so they cannot appear outside the inventory or equipment panel.
- Restored hidden cells and removed slot labels when the visual layout resets.

## 0.3.0

- Moved the equipment grid outside the inventory hierarchy and positioned it directly to the inventory's right side.
- Added the default Head, Chest, Legs, Back, Wisplight, Wishbone, Crypt Key, Arrows, Shield, and Utility slots in that order.
- Added individual toggles for Wisplight, Wishbone, Crypt Key, Arrows, Shield, and Utility slots.
- Replaced the combined custom-slot definition with ten paired Name and Items settings using exact comma-separated prefab names.
- Preserved original inventory-cell parents and positions when switching panel modes.

## 0.2.0

- Changed the default ordinary inventory expansion from two rows to zero.
- Preserved Valheim 1.0's native `invrows` value separately from DadsEPI storage rows.
- Replaced the combined generic grid with a labeled separate equipment and quick-slot panel.
- Added Head, Chest, Legs, Back, Utility, Trinket, Wishbone, and Demister slots.
- Added zero-to-eight configurable quick slots with eight configurable hotkeys and HUD labels.
- Added configurable custom equipment slots, removed slots, panel mode, HUD layout, scale, position, and drag controls.
- Added a dedicated quick-slot HUD and retained standard inventory serialization.

## 0.1.0

- Added configurable general inventory rows.
- Added typed equipment slots.
- Added configurable quick-use slots.
- Added reserved-slot placement validation.
- Added native inventory serialization and load-time sizing.
