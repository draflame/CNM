# 🎮 FIX: START NEW GAME (KHÔNG LOAD SAVE)

## ❌ VẤN ĐỀ TRƯỚC ĐÂY

- Nhấn nút **Start** → Vẫn load save cũ
- Không có cách xóa save file
- Game state không được reset

## ✅ ĐÃ FIX

### 1. Thêm vào `SaveGameManager.cs`:

```csharp
/// Xóa file save
public void DeleteSaveFile()

/// Reset tất cả game state (enemies, chests, checkpoints)
public void ResetGameState()
```

### 2. Update `SceneLoader.cs`:

```csharp
public void PlayGame()
{
    // 🎯 XÓA SAVE VÀ RESET GAME STATE
    SaveGameManager.Instance.DeleteSaveFile();
    SaveGameManager.Instance.ResetGameState();

    // Bắt đầu game mới
    LoadingManager.Instance.LoadMap("RuinedCastle", "default");
}
```

### 3. Update `CheckpointManager.cs`:

```csharp
public void ResetCheckpoints() // Xóa tất cả checkpoint data
```

## 🎯 CÁCH HOẠT ĐỘNG

### Nút **START / NEW GAME**:

1. ❌ **Xóa** file save (`savegame.json`)
2. 🔄 **Reset** EnemyManager (xóa dead enemies list)
3. 🔄 **Reset** ChestManager (xóa opened chests list)
4. 🔄 **Reset** CheckpointManager (xóa activated checkpoints)
5. ✅ **Load** map đầu tiên với spawn point default

### Nút **CONTINUE**:

1. ✅ **Load** file save (`savegame.json`)
2. ✅ **Restore** player stats, position
3. ✅ **Restore** enemies (dead enemies không spawn)
4. ✅ **Restore** chests (opened chests vẫn mở)
5. ✅ **Restore** checkpoint position

## 📋 CONSOLE LOG MẪU

Khi nhấn **Start**:

```
🗑️ Save file deleted successfully!
✅ EnemyManager reset
✅ ChestManager reset
✅ CheckpointManager reset
🔄 Game state reset complete!
Loading map: RuinedCastle with spawn: default
```

Khi nhấn **Continue**:

```
📦 Loading save data...
✅ Player positioned from save: (10.5, 5.2)
✅ Loaded enemy Map1_Enemy_15.2_8.3: isDead=true
✅ Loaded chest Map1_Chest_10.5_5.2: isOpened=true
✅ Checkpoint data restored: Map1_Checkpoint_20.0_10.0
```

## 🧪 TEST

1. **Start game mới**:

   - Nhấn **Start** → Chơi từ đầu
   - Mở chest, kill enemy
   - Save game
   - Quit

2. **Continue game**:

   - Nhấn **Continue** → Load save
   - Chest đã mở, enemy đã chết

3. **Start game mới lại**:
   - Nhấn **Start** → Chơi từ đầu
   - Chest chưa mở, enemy respawn
   - ✅ **KHÔNG** load save cũ

## ⚙️ API

```csharp
// SaveGameManager
SaveGameManager.Instance.DeleteSaveFile();        // Xóa save
SaveGameManager.Instance.ResetGameState();        // Reset tất cả managers

// EnemyManager
EnemyManager.Instance.ClearAllData();             // Xóa dead enemies

// ChestManager
ChestManager.Instance.ClearAllData();             // Xóa opened chests

// CheckpointManager
CheckpointManager.Instance.ResetCheckpoints();    // Xóa checkpoints
```

---

**🎉 HOÀN TẤT!** Giờ nút Start sẽ chơi game mới, Continue sẽ load save!
