# DadsEPI

DadsEPI is an independently implemented extended player inventory for Valheim 1.0.

## Features

- Native Valheim inventory size by default, with zero-to-five optional ordinary rows.
- A separate right-side equipment grid for Head, Chest, Legs, Back, Wisplight, Wishbone, Crypt Key, Arrows, Shield, and Utility.
- Zero-to-eight quick-use slots with individual hotkeys, HUD labels, scaling, row layout, and drag positioning.
- Toggleable built-in special slots, including Utility.
- Ten custom slot pairs with a visible name and comma-separated exact in-game prefab names.
- Native Valheim character-inventory serialization.
- Automatic routing and equipping of newly acquired and upgraded accepted equipment.
- Accepted items already occupying dedicated slots are equipped automatically; occupied slots remain in place during pickup placement.
- Simultaneous active utilities in separate accepted equipment slots, including Wishbone and Wisplight/Demister.
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

Version 1.2.0 targets Valheim 1.0.12 and BepInEx 5.4.2350. Back up the character before changing inventory-owner mods.

Source: [GitHub](https://github.com/DadxIsxBored/DadsEPI)
