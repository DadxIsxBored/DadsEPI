# Changelog

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
