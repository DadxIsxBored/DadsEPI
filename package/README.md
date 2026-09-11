# DadsEPI

DadsEPI is an independently implemented extended player inventory for Valheim 1.0.

## Features

- Native Valheim inventory size by default, with zero-to-five optional ordinary rows.
- A separate labeled equipment panel for Head, Chest, Legs, Back, Utility, Trinket, Wishbone, and Demister.
- Zero-to-eight quick-use slots with individual hotkeys, HUD labels, scaling, row layout, and drag positioning.
- Configurable custom equipment slots and removable built-in slots.
- Native Valheim character-inventory serialization.
- Reserved slots excluded from automatic pickup placement.
- Container and crafting compatibility through the standard `Inventory` API.

## Default controls

- Quick slot 1: `LeftAlt + Z`
- Quick slot 2: `LeftAlt + X`
- Quick slot 3: `LeftAlt + C`

Controls and row counts can be changed in `BepInEx/config/com.dadisbored.dadsepi.cfg` after the first launch.

## Installation

Install through a Thunderstore-compatible mod manager. For manual installation, place `DadsEPI.dll` under `BepInEx/plugins/DadsEPI/`.

## Compatibility

DadsEPI is a client-side inventory owner. Do not run it alongside AzuExtendedPlayerInventory, EquipmentAndQuickSlots, or another mod that changes player inventory dimensions or owns equipment slots.

This prerelease targets Valheim 1.0.12 and BepInEx 5.4.2350. Back up the character before testing inventory-layout prereleases.

Source: [GitHub](https://github.com/DadxIsxBored/DadsEPI)
