# DadsEPI

DadsEPI is a clean-room extended inventory implementation for Valheim. It is built against Valheim's public game assembly and does not reuse another inventory mod's source code, assets, UI hierarchy, identifiers, or package contents.

## Design

- The character's normal `Inventory` remains the source of truth.
- Valheim's native variable-height inventory support provides additional rows.
- Reserved equipment and quick slots remain part of the normal serialized inventory.
- Automatic item placement uses general slots only.
- Equipment placement is type checked.
- Quick-slot input is isolated from movement controls and blocked while text, menu, map, console, or inventory input is active.

## Build

```powershell
.\build.ps1
```

Create the Thunderstore staging directory with:

```powershell
.\build.ps1 -Package
```

The project currently references the installed Valheim 1.0.12 managed assemblies and the `Default` Thunderstore profile's BepInEx core. Override `ValheimManagedPath` and `BepInExCorePath` MSBuild properties for other installations.

## Test rule

Do not load DadsEPI alongside another mod that owns extended player inventory rows or equipment slots.

