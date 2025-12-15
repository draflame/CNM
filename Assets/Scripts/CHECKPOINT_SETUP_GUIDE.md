# 🎯 Hướng dẫn Setup Checkpoint System (Respawn)

## ✅ ĐÃ IMPLEMENT

Hệ thống checkpoint hybrid với:

- ✅ Checkpoint giống Chest (Press E để interact)
- ✅ Save full game tại checkpoint
- ✅ Hồi máu và stamina khi activate checkpoint
- ✅ Respawn tại checkpoint cuối khi chết
- ✅ Fallback về default spawn nếu chưa có checkpoint

## 📋 BƯỚC SETUP

### Bước 1: Tạo CheckpointManager

1. **Mở Persistent Scene**
2. **Tạo Empty GameObject** tên: `CheckpointManager`
3. **Add Component**: `CheckpointManager` script
4. **Configure Inspector**:
   - Default Spawn Position: Vị trí spawn mặc định (VD: 0, 0, 0)
   - Default Spawn Scene: Tên scene spawn mặc định (VD: "Map1")

### Bước 2: Tạo Checkpoint Prefab

#### 2.1 Tạo GameObject

1. Tạo GameObject mới tên: `Checkpoint`
2. Add Components:
   - `SpriteRenderer` (hình ảnh checkpoint - VD: bonfire, statue...)
   - `BoxCollider2D` (set IsTrigger = ✅)
   - `Animator` (optional - animation khi activate)
   - `AudioSource` (optional - âm thanh khi activate)
   - **`Checkpoint`** script

#### 2.2 Tạo UI "Press E"

1. **Trong Checkpoint GameObject**, tạo child:

   - Tên: `WorldUI`
   - Add `Canvas` component:
     - Render Mode: **World Space**
     - Sorting Layer: UI
   - Adjust scale: (0.01, 0.01, 0.01)

2. **Trong WorldUI**, tạo child Text:

   - Tên: `PressEText`
   - Add `TextMeshPro - Text` (hoặc UI Text)
   - Nội dung: "Press E to Rest"
   - Font size: 24
   - Alignment: Center

3. **Tùy chỉnh vị trí**:
   - Đặt WorldUI phía trên Checkpoint (Y offset ~1-2 units)

#### 2.3 Configure Checkpoint Script

Trong Inspector của Checkpoint:

- **Checkpoint ID**: Để trống (sẽ auto-generate) hoặc đặt tên custom
- **World UI**: Kéo WorldUI GameObject vào đây
- **Activated Effect**: (Optional) Effect khi đã activate
- **Activate Particle**: (Optional) Particle khi activate
- **Audio Source**: Kéo AudioSource vào
- **Activate Clip**: Âm thanh khi activate

#### 2.4 Tạo Prefab

1. Kéo Checkpoint GameObject vào folder Prefabs
2. Delete khỏi scene
3. Ready to use!

### Bước 3: Đặt Checkpoints vào Map

1. **Kéo Checkpoint prefab** vào các vị trí trong map
2. **Naming**: Đặt tên rõ ràng (VD: "Checkpoint_Map1_Start", "Checkpoint_BeforeBoss")
3. **Vị trí gợi ý**:
   - Đầu map
   - Trước boss arena
   - Sau khu vực khó
   - Gần cửa hàng/NPC

### Bước 4: Test

#### Test Checkpoint Activation:

1. Play game
2. Đi đến checkpoint → Hiển thị "Press E"
3. Nhấn E → Check Console:
   ```
   ✅ Checkpoint activated: [ID]
   💾 Game saved at checkpoint: [ID]
   💚 Player health and stamina restored
   ```
4. Verify: Máu và stamina đầy ✅

#### Test Respawn:

1. Activate một checkpoint
2. Đi xa → Để bị quái giết
3. Khi chết → Check Console:
   ```
   💀 Player died!
   🔄 Loading from last checkpoint...
   ```
4. Player respawn tại checkpoint ✅
5. Inventory, quái đã giết được giữ nguyên ✅

#### Test Default Spawn (không có checkpoint):

1. Start new game (chưa activate checkpoint nào)
2. Để chết
3. Player spawn tại default position ✅

## 🎨 CUSTOMIZATION

### Thay đổi Default Spawn từ code:

```csharp
// Gọi khi load map mới
CheckpointManager.Instance.SetDefaultSpawn(
    new Vector3(10f, 5f, 0f),
    "Map2"
);
```

### Custom Checkpoint ID:

Trong Inspector của Checkpoint, set `Checkpoint ID` thủ công:

- "BonfireStart"
- "SavePoint_Boss1"
- "Checkpoint_Area3"

### Thêm Visual Effect:

1. Tạo particle system (VD: light glow, sparkles)
2. Gán vào `Activate Particle` trong Checkpoint
3. Khi activate → Particle sẽ play

### Thêm Animation:

1. Tạo Animator với 2 states:
   - Idle (không activate)
   - Activated (đã activate - VD: light on)
2. Trigger: "Activate"
3. Gán Animator vào Checkpoint GameObject

## 📊 KIẾN TRÚC

```
Player chết
    ↓
CheckpointManager.RespawnPlayer()
    ↓
Có SaveData? → YES → LoadingManager.LoadMapFromSave()
    ↓                     ↓
   NO                  Load checkpoint cuối
    ↓                     ↓
SpawnAtDefault()      Restore: health, stamina, position,
    ↓                        inventory, quái đã giết
Reset scene
```

## 🎮 GAMEPLAY FLOW

### First Playthrough:

1. Player start game → No checkpoint
2. Explore → Tìm checkpoint → Activate
3. Continue → Quái đã giết không respawn
4. Chết → Respawn tại checkpoint → Tiếp tục

### Death Penalty:

Hiện tại: Respawn xa → Phải chạy lại

**Có thể thêm penalty**:

- Mất % vàng/exp
- Giảm 50% health khi respawn
- Bloodstain mechanic (như Dark Souls)

## 🐛 TROUBLESHOOTING

### "CheckpointManager not found"

- Verify CheckpointManager GameObject tồn tại
- Check script attached đúng
- Verify DontDestroyOnLoad

### Checkpoint không hiển thị UI

- Check WorldUI active = true trong prefab
- Verify Canvas Render Mode = World Space
- Check trigger collider đúng size

### Respawn không hoạt động

- Check Console có log "Player died"?
- Verify CheckpointManager.Instance != null
- Check SaveGameManager có save được không

### Player spawn sai vị trí

- Check checkpoint position trong save file
- Verify scene name khớp
- Check LoadingManager hoạt động

## 💡 TIPS

1. **Checkpoint placement**: Đặt checkpoint trước khu vực khó
2. **Visual feedback**: Thêm light/glow khi checkpoint active
3. **Sound design**: Âm thanh bonfire/save point tạo atmosphere
4. **UI polish**: Animation fade in/out cho "Press E"

## 🎯 NEXT STEPS (Optional)

Có thể mở rộng:

- ✨ Fast travel giữa các checkpoint
- 💰 Checkpoint shop (mua items tại checkpoint)
- 📊 Checkpoint stats (số lần rest, time spent)
- 🎨 Checkpoint themes khác nhau theo khu vực

---

**Setup xong!** Hệ thống respawn giờ đã hoạt động với checkpoint system. 🎉

## 📝 SUMMARY

**Files created:**

- ✅ Checkpoint.cs - Script cho checkpoint prefab
- ✅ CheckpointManager.cs - Manager quản lý checkpoints
- ✅ Updated knight.cs - Respawn logic
- ✅ Updated GameData.cs - Checkpoint save data
- ✅ Updated SaveGameManager.cs - Save/load checkpoint

**Ready to use!** 🚀
