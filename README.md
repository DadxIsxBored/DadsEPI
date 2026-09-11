# DadsEPI

DadsEPI is a clean-room extended inventory implementation for Valheim. It is built against Valheim's public game assembly and does not reuse another inventory mod's source code, assets, UI hierarchy, identifiers, or package contents.

## Design

- The character's normal `Inventory` remains the source of truth.
- The ordinary grid defaults to Valheim's current native row count; `Extra Inventory Rows` defaults to zero.
- Labeled equipment and quick slots render in a separate panel by default while remaining safely serialized in the player inventory.
- Equipment includes Head, Chest, Legs, Back, Utility, Trinket, Wishbone, Demister, and configurable custom slots.
- Quick slots support zero through eight entries, individual hotkeys, HUD labels, HUD scaling, row layout, and drag positioning.
- Automatic item placement uses general slots only.
- Equipment placement is type checked.
- Quick-slot input is isolated from movement controls and blocked while text, menu, map, console, or inventory input is active.

## Build

```powershell
.\build.ps1
```

Create and validate the Thunderstore upload ZIP with:

```powershell
.\build.ps1 -Package
```

The generated ZIP is written under `dist/`. Its required metadata, icon dimensions, version alignment, root layout, and bundled assemblies are validated before the path and SHA-256 hash are printed.

The project currently references the installed Valheim 1.0.12 managed assemblies and the `Default` Thunderstore profile's BepInEx core. Override `ValheimManagedPath` and `BepInExCorePath` MSBuild properties for other installations.

## Test rule

Do not load DadsEPI alongside another mod that owns extended player inventory rows or equipment slots.

See [THUNDERSTORE.md](THUNDERSTORE.md) for packaging and upload steps.
