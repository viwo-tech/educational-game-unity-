# Educational Puzzle Adventure Game - Unity

A mobile educational game for Android with puzzle and adventure mechanics across 10 progressively challenging levels.

## 🎮 Game Features

- **10 Progressive Levels** with increasing difficulty
- **Puzzle/Adventure Mechanics** for engaging gameplay
- **Mixed Educational Topics** (Math, Language, Science, History, Logic)
- **Score System** tracking player performance
- **Level Selector** to choose any level
- **UI/UX** with pause, restart, and main menu functions
- **Android APK Build** ready

## 📝 Single File Architecture

Semua game systems ada dalam **1 file saja**: `Assets/Scripts/GameController.cs`

Berisi:
- **GameManager** - Game state & level management
- **LevelManager** - Level setup & completion
- **UIManager** - UI control
- **LevelSelector** - Level selection
- **PuzzleGame** - Puzzle mechanics
- **LevelData** - Level configuration

## 🎯 How to Use

1. Drag `GameController.cs` ke Unity project Anda
2. Attach script ke GameObject di scene
3. Setup UI elements di Inspector
4. Create scenes dan level data
5. Build APK!

Lihat **MANUAL.md** untuk setup lengkap 📖
