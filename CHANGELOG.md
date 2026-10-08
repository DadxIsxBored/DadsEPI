# Changelog

## 1.4.6

- Keep the separate equipment panel directly after the player inventory in UI order so the Achievements window can cover it.

## 1.4.5

- Restore the prior display-setting UI rebuild while retaining the inventory item placement changes.

## 1.4.4

- Keep quick-slot and equipment-slot items in their assigned slots when changing settings in the mod manager.
- Move items by slot identity only when the inventory layout changes; refresh display settings without rearranging inventory items.
- Preserve equipped items already in valid quick or equipment slots when loading a character.

## 1.4.3

- general optimisations

## 1.4.2

- Use the completed inventory-grid drop to identify the item Valheim moved, including moves that create a new item record.
- Keep gear dragged out of a dedicated slot unequipped and allow replacement gear to equip in that slot.
- Leave a right-clicked, unequipped item in its dedicated slot until it is dragged elsewhere.

## 1.4.1

- Equip items placed directly into an empty dedicated slot by inventory pickup.
- Keep equipment unequipped when dragged into ordinary inventory, including when Valheim clones the item during the move.
- Equip replacement gear dropped into a dedicated slot and reject swaps that would place an incompatible item there.
- Right-click equipping from ordinary inventory continues to move the item into its dedicated slot.

## 1.4.0

- Fixed equipment swaps so an equipped item in a dedicated slot is unequipped before its replacement is dropped into the slot.
- Stopped inventory additions from re-equipping an item left in an occupied equipment slot.
- Thanks to Jigglefrizz and Ribas1337 for reporting the equipment swap bug.

## 1.3.5

- Initialize slot labels while inactive and assign the native game font before activation, avoiding the missing LiberationSans default-font warnings.
- When the amount label has no font, select an existing inventory text component with a valid font.

## 1.3.4

- Removed equipment normalization and automatic equip calls from the recurring half-second inventory-size check.
- Limited automatic equip calls to items Valheim identifies as equipable, excluding dedicated storage items such as the Crypt Key.
- Assigned slot labels an existing Valheim TMP font when the inventory amount label has no font reference.

## 1.3.3

- Removed automatic-pickup capacity handling from DadsEPI; that behavior is owned by DadsQoL.

## 1.3.2

- Kept the quick-slot HUD within the screen viewport and aligned each slot from its top-left corner.
- Added a tombstone-recovery pass that routes recovered equipment into its dedicated slots and equips it.

## 1.3.1

- Replaced the separate-slot backdrop with a resized clone of Valheim's native crafting/inventory wood-and-leather background object.
- Kept the cloned background behind the equipment and quick-slot cells and disabled its input raycast.

## 1.3.0

- Replaced the separate equipment panel's black fill with Valheim's native inventory background sprite, material, tint, and image settings.
- Added a compact custom-slot editor through DadsBepInExModManager that lists available slots and uses one name field, one prefab-list field, and an Add Slot button.
- Routed accepted items from ordinary inventory into empty dedicated slots during layout normalization, including Wishbone, Wisplight/Demister, Crypt Key, and configured custom-slot items.

## 1.2.1

- Prevented equipment-slot layout movement from firing inventory callbacks during Valheim's character-load equipment restore.
- Isolated load-time equipment restoration per item so an invalid equipment record is left unequipped instead of aborting player spawn.

## 1.2.0

- Auto-equipped accepted gear after pickup when its dedicated slot was empty or already occupied by that item.
- Re-equipped upgraded gear returned to its dedicated slot by Valheim's crafting process.
- Allowed multiple utility items in separate accepted equipment slots to remain equipped simultaneously.
- Applied every equipped utility status effect so Wishbone and Wisplight/Demister can operate together.
- Preserved multiple utility equipment state across inventory normalization, character loading, and unequip-all operations.

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
