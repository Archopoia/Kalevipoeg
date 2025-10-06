# Estonian Folk Game Jam

**A 48-hour tower defense survival game inspired by Estonian folklore**

## 🎮 Game Overview

Survive ancient waves of Estonian folk monsters! Gather resources in an ever-generated terrain to build and upgrade defensive towers to protect your village from mythical creatures.

## 🎯 Gameplay

### Core Mechanics
- **Resource Gathering**: Chop trees for wood, mine rocks for stone
- **Tower Defense**: Build and upgrade towers to defend your village
- **Wave Survival**: Face increasingly difficult waves of Estonian folk monsters
- **Terrain Generation**: Explore procedurally generated landscapes

### Tower System
- **Level 1**: 6 wood → Basic tower
- **Level 2**: 6 wood + 6 stone → Upgraded tower
- **Level 3**: 18 stone → Advanced tower
- **Upgrade System**: Press "T" near towers to upgrade them

## 🎮 Controls

| Action | Key |
|--------|-----|
| **Movement** | WASD |
| **Look/Aim** | Mouse |
| **Gather Resources** | Left Click (trees/rocks) |
| **Upgrade Towers** | T (when near tower) |

**Note**: Resource gathering doesn't work in Build mode.

## 🏗️ Technical Features

### Core Systems
- **First-Person Controller**: Smooth WASD movement with mouse look
- **Grid-Based World**: Procedural terrain generation with tile system
- **Resource Management**: Dynamic inventory system for wood and stone
- **Tower Placement**: Strategic tower positioning with range indicators
- **Enemy AI**: Pathfinding with collision avoidance and town targeting
- **Wave Management**: Progressive difficulty with enhanced wave system

### Audio System
- **Dynamic Music**: Context-aware background music
- **3D Spatial Audio**: Positional sound effects
- **Ambient Audio**: Environmental and enemy audio cues
- **Audio Manager**: Centralized sound management system

### Visual Effects
- **Particle Systems**: Hit effects, fireballs, and destruction feedback
- **Range Indicators**: Visual tower range display when player is nearby
- **Damage Feedback**: Enemy flashing on damage with smooth animations
- **Camera Fade**: Smooth scene transitions

## 🏛️ Estonian Folklore Integration

The game features creatures and themes from Estonian mythology, creating an authentic folk experience during the Mythical Age setting.

## 📁 Project Structure

```
Assets/
├── Scripts/           # Core game logic
│   ├── Farming.cs     # Resource gathering system
│   ├── Farmables.cs   # Tree/rock interaction
│   ├── Tower.cs       # Tower mechanics and AI
│   ├── Enemy.cs       # Enemy AI and pathfinding
│   ├── GridManager.cs # World generation
│   ├── Inventory.cs   # Resource management
│   └── Audio/         # Sound system components
├── Prefabs/           # Game object templates
├── Materials/         # Visual materials
├── Sounds/            # Audio assets
├── Sprites/           # 2D graphics
└── Scenes/            # Game levels
```

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 or later
- Universal Render Pipeline (URP)

### Installation
1. Clone or download the project
2. Open in Unity Hub
3. Load the project
4. Open `MainScene.unity` to start playing

### Game Modes
- **Main Scene**: Primary gameplay experience
- **Main Menu**: Game launcher with music controller

## 🎵 Audio Features

- **Main Menu Music**: Dedicated music controller for menu experience
- **Game Music**: Dynamic background music during gameplay
- **Sound Effects**:
  - Resource gathering sounds
  - Tower firing audio
  - Enemy movement and attack sounds
  - Environmental audio cues

## 🔧 Development Notes

### Key Scripts
- `FirstPersonController.cs`: Player movement and camera control
- `GridManager.cs`: Procedural world generation and tile management
- `EnhancedWaveManager.cs`: Wave progression and difficulty scaling
- `TowerPlacementManager.cs`: Tower building mechanics
- `MusicManager.cs`: Audio system coordination

## 🏆 Game Objectives

1. **Survive**: Don't let your village fall to enemy attacks
2. **Gather**: Collect wood and stone from the environment
3. **Build**: Construct defensive towers strategically
4. **Upgrade**: Enhance your defenses as waves get stronger
5. **Defend**: Protect your village from Estonian folk monsters

## 🎨 Art Style

The game features an aesthetic with:
- Folk-inspired creature designs
- Atmospheric lighting
- Traditional Estonian visual elements
- Immersive particle effects

## 📝 Game Jam Context

Created during the Estonian Folk Game Jam (48 hours), this project showcases rapid prototyping skills and creative integration of cultural themes into engaging gameplay mechanics.

---

**Survive the ancient waves and defend your village!** 🏰⚔️
