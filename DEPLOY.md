# 🚀 Deployment Guide — Project Labyrinth

This document covers how to build a standalone Windows release of Project Labyrinth, how to package and distribute it, and how to automate the build process via the command line.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Manual Build (Unity Editor)](#manual-build-unity-editor)
3. [Automated Build (CLI / build.sh)](#automated-build-cli)
4. [Build Output Structure](#build-output-structure)
5. [Packaging for Distribution](#packaging-for-distribution)
6. [Publishing to GitHub Releases](#publishing-to-github-releases)
7. [Publishing to itch.io (Optional)](#publishing-to-itchio-optional)
8. [Build Targets Reference](#build-targets-reference)
9. [Rollback Procedure](#rollback-procedure)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

- Unity 6000.2.3f1 LTS installed with **Windows Build Support (IL2CPP)** module
- Git and Git LFS installed
- Repository cloned locally with all LFS assets pulled:
  ```bash
  git lfs pull
  ```
- For automated builds: Unity must be accessible on your system `PATH`  
  (default: `C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Unity.exe` on Windows)

---

## Manual Build (Unity Editor)

This is the standard approach for producing a release build.

### Steps

1. Open the project in Unity (see `README.md` → Getting Started)

2. Go to **File → Build Settings**

3. Set the following options:
   | Setting | Value |
   |---|---|
   | Target Platform | **Windows** |
   | Architecture | **x86_64** |
   | Scripting Backend | **IL2CPP** |
   | Development Build | ❌ Off (for release) |
   | Compression Method | **LZ4HC** |

4. Click **Build** (not "Build and Run")

5. When prompted, choose or create an output folder:
   ```
   Labyrinth/Builds/Windows/
   ```

6. Unity will produce:
   ```
   Builds/Windows/
   ├── Labyrinth.exe
   ├── Labyrinth_Data/
   ├── MonoBleedingEdge/
   └── UnityCrashHandler64.exe
   ```

7. Test the build by running `Labyrinth.exe` directly (not through the Unity Editor).

## Build Output Structure

A successful build produces the following structure in `Builds/Windows/`:

```
Builds/Windows/
├── Labyrinth.exe              ← Main executable (distribute this + everything below)
├── Labyrinth_Data/
│   ├── Managed/               ← C# assemblies
│   ├── Resources/             ← Packed game assets
│   ├── StreamingAssets/       ← Any streamed content
│   └── app.info               ← Version info
├── MonoBleedingEdge/          ← Mono runtime (required)
└── UnityCrashHandler64.exe    ← Unity crash reporter
```

> ⚠️ The entire `Builds/Windows/` folder is required to run the game. Distributing only `Labyrinth.exe` without `Labyrinth_Data/` will not work.

---

## Packaging for Distribution

Once a build is verified, package it into a `.zip` for distribution:

**Windows (PowerShell):**
```powershell
Compress-Archive -Path "Builds\Windows\*" -DestinationPath "Builds\Labyrinth-v2.0-windows.zip"
```

**Linux / macOS:**
```bash
cd Builds
zip -r Labyrinth-v2.0-windows.zip Windows/
```

**Naming convention:**
```
Labyrinth-v{MAJOR}.{MINOR}-windows.zip
```
Examples: `Labyrinth-v1.0-windows.zip`, `Labyrinth-v2.0-windows.zip`

---

## Build Targets Reference

| Target | Platform | Script Backend | Notes |
|---|---|---|---|
| Windows x64 | Windows | IL2CPP | Primary distribution target |
| Windows x64 (Dev) | Windows | Mono | For debugging; includes profiler |
| WebGL | Browser | IL2CPP | Experimental; not officially supported |

> WebGL builds require additional testing due to Unity's WebGL audio and pathfinding limitations.

---

## Rollback Procedure

If a release build introduces regressions:

1. Identify the last stable tag:
   ```bash
   git tag --list
   git log --oneline --tags
   ```

2. Check out the stable tag into a hotfix branch:
   ```bash
   git checkout -b hotfix/v2.0-fix v2.0
   ```

3. Apply the fix, commit, and re-build

4. Merge the hotfix back to `main` and `dev`, then re-tag:
   ```bash
   git tag -a v2.0.1 -m "Hotfix: boss AI stall fix"
   git push origin v2.0.1
   ```

5. Publish a new GitHub Release with the patched build, marking the previous release as **pre-release** if necessary.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| `Unity not found` in build script | Set `UNITY_PATH` env variable to your Unity executable |
| Build fails with IL2CPP errors | Ensure **Windows Build Support (IL2CPP)** module is installed via Unity Hub |
| Missing assets in build | Run `git lfs pull` to ensure all binary assets are downloaded |
| Game launches to black screen | Confirm all 3 scenes are in Build Settings in the correct order |
| `Labyrinth_Data/` missing | You must distribute the entire `Builds/Windows/` folder, not just the `.exe` |
| Build log shows script errors | Check `Builds/build.log` — fix all compiler errors in the Editor before building |
| Dungeon generation hangs | A seed produced an invalid layout; run `DungeonValidator` tests (see `docs/TESTING.md`) |
