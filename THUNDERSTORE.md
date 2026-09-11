# Thunderstore release process

## Build and validate

```powershell
.\build.ps1 -Package
```

The command produces a versioned ZIP under `dist/`. Open the ZIP and confirm these files are directly at its root:

- `manifest.json`
- `README.md`
- `CHANGELOG.md`
- `icon.png`
- `DadsEPI.dll`
- `LICENSE`

## Test before upload

Import the generated ZIP as a local mod in Thunderstore Mod Manager and test it in an isolated profile with other extended-inventory owners disabled. Test character load/save, inventory reopening, crafting, upgrades, containers, death recovery, equipment placement, quick-slot controls, and a second game launch using the same character.

## Upload

1. Open the Valheim community on Thunderstore and use **Upload package**.
2. Select the team that will permanently own DadsEPI.
3. Upload the generated ZIP from `dist/`.
4. Select the applicable mod and client-side categories shown by the Valheim upload form.
5. Review the rendered README, dependency, icon, and version before submission.

Thunderstore versions are immutable. Keep the manifest name `DadsEPI` unchanged after the first upload and increment `version_number` for every later upload.
