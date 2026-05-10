# 🗡️ Project Labyrinth
> A top-down 2D roguelike dungeon crawler built in Unity

**COMP 490 / COMP 491 — Senior Capstone Project**  
California State University, Northridge | Fall 2025 - Spring 2026

| | |
|---|---|
| **Engine** | Unity 2022.3 LTS (C#) |
| **Platform** | Windows PC (Standalone) |
| **Genre** | Top-down 2D Roguelike |
| **Status** | Active Development — Labyrinth 2.0 |

---

## 📖 About

Project Labyrinth is a dungeon-crawling roguelike inspired by *The Binding of Isaac* and *Enter the Gungeon*. Every run generates a brand-new dungeon using seed-based procedural generation — no two runs are the same. Fight through waves of enemies, collect powerful loot, and face a boss that stands between you and victory. Die, and you start over from scratch.

**Key Features:**
- Procedurally generated dungeons (seed-based BSP layout algorithm)
- Two playable classes: **Melee Warrior** and **Ranged Archer**
- 5 enemy types with state-machine AI and A\* pathfinding
- Item and loot drop system with unique power-up effects
- Boss encounters with multi-phase attack patterns
- Original pixel art and music composed by the team

**Built by:** Aidan Fox · Austen Bernal · Derek Newton · Justin Ly

---

## 🎮 Controls

| Action | Input |
|---|---|
| Move | `W A S D` |
| Attack | `Spacebar` |
| Aim | `Arrow Keys` |
| Use Bomb | `E` |
| Return to Menu | `Escape` (hold down) |
| Restart Run | `R` (hold down) |

---

## 🛠️ Prerequisites

Before opening or building the project, make sure you have the following installed:

- **Unity Hub** — [Download here](https://unity.com/download)
- **Unity 2022.3.x LTS** (exact version: see `ProjectSettings/ProjectVersion.txt`)
  - Install via Unity Hub → Installs → Add → select `2022.3.x LTS`
  - Required modules: **Windows Build Support (IL2CPP)**
- **Git** — [Download here](https://git-scm.com/)
- **Git LFS** — Required for binary assets (art, audio)
  ```bash
  git lfs install
  ```

> ⚠️ **Unity version matters.** Opening the project in a different Unity version may cause warnings or broken references. Always use the version listed in `ProjectSettings/ProjectVersion.txt`.

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/aidanfoxcsun/project-labyrinth.git
cd ProjectLabyrinth
```

If Git LFS is installed, assets will download automatically. If not:

```bash
git lfs pull
```

### 2. Open in Unity

1. Open **Unity Hub**
2. Click **Open → Add project from disk**
3. Navigate to the cloned `ProjectLabyrinth/` folder and select it
4. Unity Hub will detect the project version from `ProjectSettings/ProjectVersion.txt`
5. If prompted to upgrade Unity version — **do not upgrade.** Install the exact matching version instead.

### 3. Open the Main Scene

Once the project loads in the Unity Editor:

1. In the **Project** panel, navigate to `Assets/Scenes/`
2. Double-click **`MainMenu.unity`** to open the entry point scene
3. Press **Play (▶)** in the Unity Editor toolbar to run the game in the editor

> The game will start at the main menu. Select start new game to begin a run.

---

## 📦 Dependencies

All Unity package dependencies are version-pinned in `Packages/manifest.json` and installed automatically when you open the project. Key packages:

| Package | Version | Purpose |
|---|---|---|
| `com.unity.2d.tilemap` | 1.0.0 | Dungeon tilemap rendering |
| `com.unity.inputsystem` | 1.7.0 | Player input handling |
| `com.unity.test-framework` | 1.3.9 | Unity Test Runner |

No additional `npm install`, `pip install`, or external package managers are required.

---

## 🔧 Configuration

Unity project configuration is stored in `ProjectSettings/` (committed to the repo). Notable settings:

| File | Purpose |
|---|---|
| `ProjectSettings/ProjectVersion.txt` | Authoritative Unity version for this project |
| `ProjectSettings/InputManager.asset` | Keybindings and input axes |
| `ProjectSettings/QualitySettings.asset` | Graphics quality tiers |
| `ProjectSettings/AudioManager.asset` | Master volume defaults |

There are no runtime environment variables or `.env` files — this is an offline desktop game with no network dependencies.

---

## 🐛 Known Issues

- Minimap is not yet implemented (deferred to post-launch)
- Run statistics (kills, time, items collected) are not tracked across sessions
- Some minor collision bugs due to high velocities or forces acting on Rigidbody2D components

---

## 📄 License

This project is developed for academic purposes under the COMP 490/491 Senior Project sequence. All original art and music assets are created by the project team and are not licensed for redistribution without permission.

---

## 🔗 Repository

The full project source code, build instructions, and deployment artifacts are available at:  
**https://github.com/aidanfoxcsun/ProjectLabyrinth**

Release builds (Windows standalone `.exe`) are available under [GitHub Releases](https://github.com/aidanfoxcsun/ProjectLabyrinth/releases).
