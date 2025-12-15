# 📦 HƯỚNG DẪN SETUP HỆ THỐNG SAVE/LOAD CHEST

## ✅ CÁC FILE ĐÃ TẠO

1. **ISaveableChest.cs** - Interface cho chest có thể save
2. **ChestSaveData.cs** - Class chứa dữ liệu save của chest
3. **ChestManager.cs** - Singleton quản lý tất cả chests

## 🔧 SETUP TRONG UNITY

### Bước 1: Tạo ChestManager GameObject

1. Vào Scene **"01. PersistentManagers"**
2. Tạo Empty GameObject, đặt tên: **ChestManager**
3. Add Component → **ChestManager.cs**
4. ✅ Script sẽ tự động DontDestroyOnLoad

### Bước 2: Kiểm Tra Chest Prefab

Chest của bạn đã có:

- ✅ `chestID` (string) - tự động generate
- ✅ `isOpened` (bool)
- ✅ Animator với trigger "Open"
- ✅ worldUI (GameObject) - UI "Press E"
- ✅ AudioSource và OpenClip

**Không cần thay đổi gì thêm!**

### Bước 3: Test Save/Load

1. **Chạy game** → Mở 1-2 chest
2. **Save game** (từ nút Save)
3. **Quit** và **Continue Game**
4. **Kiểm tra**: Các chest đã mở phải vẫn ở trạng thái đã mở

## 📋 CONSOLE LOG MẪU

```
✅ ChestManager initialized
📦 Registered 3 chests in scene: Map1
💾 Saved chest: Map1_Chest_10.5_5.2 - Opened: false
📦 Chest opened and saved: Map1_Chest_10.5_5.2
✅ GetAllChestsSaveData via ChestManager: 3 chests
Saving 3 chests.
🔄 Applying chest save data for scene: Map1
✅ Loaded chest Map1_Chest_10.5_5.2: isOpened=true
```

## 🔍 TROUBLESHOOTING

### Chest không save được:

- Kiểm tra ChestManager có trong Persistent Scene không
- Xem Console có log "ChestManager initialized" không

### Chest đã mở vẫn hiện lại:

- Kiểm tra Animator có state "Open" không
- Xem Console log "Loaded chest ... isOpened=true"

### Chest ID bị trùng:

- ChestID được generate từ position (x, y)
- Nếu 2 chest cùng vị trí → ID trùng → lỗi
- **Giải pháp**: Di chuyển chest xa nhau hơn

## 🎯 CÁCH HOẠT ĐỘNG

1. **Start Game**:

   - ChestManager đăng ký tất cả chests trong scene
   - Mỗi chest có ID unique: `{sceneName}_Chest_{x}_{y}`

2. **Open Chest**:

   - Chest.OpenChest() → set isOpened = true
   - Gọi ChestManager.OnChestOpened() → lưu vào list

3. **Save Game**:

   - SaveGameManager gọi ChestManager.GetAllChestsSaveData()
   - Lấy data của tất cả chests (mở + chưa mở)
   - Save vào `savegame.json`

4. **Load Game**:
   - LoadingManager load scene
   - SaveGameManager.ApplyChestsSaveData() tìm chest theo ID
   - Gọi Chest.LoadFromSaveData() để restore trạng thái

## ⚙️ API REFERENCE

### ChestManager

```csharp
ChestManager.Instance.RegisterChest(Chest chest)
ChestManager.Instance.OnChestOpened(Chest chest)
ChestManager.Instance.GetAllChestsSaveData()
```

### Chest (ISaveableChest)

```csharp
string GetChestID()
bool IsOpened()
ChestSaveData GetSaveData()
void LoadFromSaveData(ChestSaveData data)
```

---

**🎉 HOÀN TẤT!** Hệ thống save/load chest đã sẵn sàng!
