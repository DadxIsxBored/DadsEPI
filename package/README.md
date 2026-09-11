# DadsEPI

DadsEPI is an independently implemented extended player inventory for Valheim 1.0.

## Features

- Configurable general inventory rows.
- Dedicated typed equipment slots.
- Dedicated quick-use slots with configurable hotkeys.
- Native Valheim character-inventory serialization.
- Reserved slots excluded from automatic pickup placement.
- Container and crafting compatibility through the standard `Inventory` API.

## Default controls

- Quick slot 1: `Z`
- Quick slot 2: `X`
- Quick slot 3: `C`

Controls and row counts can be changed in `BepInEx/config/com.dadisbored.dadsepi.cfg` after the first launch.

## Installation

Install through a Thunderstore-compatible mod manager. For manual installation, place `DadsEPI.dll` under `BepInEx/plugins/DadsEPI/`.

## Compatibility

DadsEPI is a client-side inventory owner. Do not run it alongside AzuExtendedPlayerInventory, EquipmentAndQuickSlots, or another mod that changes player inventory dimensions or owns equipment slots.

This prerelease targets Valheim 1.0.12 and BepInEx 5.4.2350. Back up the character before testing inventory-layout prereleases.

Source: [GitHub](https://github.com/DadxIsxBored/DadsEPI)
