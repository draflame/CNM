# 🎯 Hướng dẫn Setup EnemyManager

## ⚠️ VẤN ĐỀ ĐÃ FIX

**Vấn đề cũ**: Khi giết quái → destroy → save game → thông tin quái đã chết không được lưu → khi load game quái spawn lại

**Giải pháp**: Tạo `EnemyManager` để tracking tất cả enemies (cả sống và chết) trước khi destroy.

## 📋 BƯỚC SETUP (BẮT BUỘC)

### 1. Tạo EnemyManager GameObject trong Persistent Scene

1. Mở scene `Persistent Scene` (hoặc scene DontDestroyOnLoad của bạn)
2. Tạo Empty GameObject mới, đặt tên: `EnemyManager`
3. Add component: `EnemyManager` script
4. **QUAN TRỌNG**: GameObject này phải DontDestroyOnLoad

### 2. Thêm EnemyManager vào PersistentManager (Khuyến nghị)

Nếu bạn có PersistentManager:

1. Mở GameObject có component `PersistentManager`
2. Trong Inspector, tìm array `Persistent Objects`
3. Thêm `EnemyManager` GameObject vào array này

Hoặc attach trực tiếp vào Persistent Scene để nó tự động DontDestroyOnLoad.

### 3. Verify Setup

Chạy game và check Console:

- Phải thấy log: `✅ EnemyManager initialized`
- Khi enemies spawn: `✅ Registered enemy: [ID] in scene [SceneName]`

## 🔄 CƠ CHẾ HOẠT ĐỘNG

### Khi Enemy Spawn:

```
Enemy.Start()
  → Auto-generate ID (persistent dựa trên position)
  → EnemyManager.RegisterEnemy(this)
```

### Khi Enemy Chết:

```
Enemy.Die()
  → Set isDead = true
  → EnemyManager.ReportEnemyDeath(this) 👈 LƯU THÔNG TIN TRƯỚC KHI DESTROY
  → Destroy(gameObject, 1f)
```

### Khi Save Game:

```
SaveGameManager.SaveGame()
  → GetAllEnemiesSaveData()
    → EnemyManager.GetAllEnemiesSaveData()
      → Lấy enemies còn sống từ scene
      → Lấy enemies đã chết từ deadEnemies list ✅
  → Save tất cả vào file JSON
```

### Khi Load Game:

```
LoadingManager loads scene
  → SaveGameManager.ApplyEnemiesSaveData()
    → EnemyManager.LoadEnemiesState() (sync dead list)
    → Tìm enemies trong scene theo ID
    → Nếu enemy.isDead == true → Destroy()
    → Nếu enemy còn sống → Restore health/position
```

## ✅ KIỂM TRA HOẠT ĐỘNG

### Test 1: Giết quái và save

```
1. Play game
2. Giết 1-2 enemies
3. Nhấn F9 → Check console:
   💀 Enemy death reported: [ID]
4. Save game
5. Check console phải thấy:
   💀 Saved dead enemy: [ID]
```

### Test 2: Load game sau khi giết quái

```
1. Continue từ save đã có quái chết
2. Check console:
   💀 Loaded dead enemy info: [ID]
   💀 Destroyed dead enemy: [ID]
3. Enemies đã chết KHÔNG spawn lại ✅
```

### Test 3: Debug Info

```
Nhấn F9 trong game:
- Hiển thị số enemies sống
- Hiển thị danh sách enemies đã chết
- Verify với save file
```

## 🐛 TROUBLESHOOTING

### "EnemyManager not found" warning

**Nguyên nhân**: Chưa tạo EnemyManager GameObject
**Giải pháp**: Tạo EnemyManager theo bước 1

### Quái vẫn spawn lại sau khi chết

**Kiểm tra**:

1. Console có log "💀 Enemy death reported" không?
2. Save file có enemies với "isDead": true không?
3. Load game có log "💀 Destroyed dead enemy" không?

**Nếu không có log "Enemy death reported"**:

- Check Enemy.Die() có gọi EnemyManager.ReportEnemyDeath() không
- Verify EnemyManager.Instance != null

### Enemy ID thay đổi mỗi lần load

**Nguyên nhân**: Enemy position không cố định
**Giải pháp**:

- Set enemy position cố định trong scene
- Hoặc set enemyID thủ công trong Inspector

## 📊 KIẾN TRÚC MỚI

```
┌─────────────────────────────────────────┐
│         EnemyManager (Singleton)         │
│  - Track tất cả enemies by scene        │
│  - Lưu list enemies đã chết             │
│  - Provide save/load API                │
└─────────────────────────────────────────┘
                ▲
                │ Register/Report
                │
┌───────────────┴──────────────────┐
│         Enemy (MonoBehaviour)     │
│  - Start(): Register với Manager  │
│  - Die(): Report death trước destroy │
│  - Implement ISaveableEnemy       │
└──────────────────────────────────┘
                ▲
                │ Query
                │
┌───────────────┴──────────────────┐
│       SaveGameManager            │
│  - GetAllEnemiesSaveData()       │
│    → Gọi EnemyManager            │
│  - ApplyEnemiesSaveData()        │
│    → Load state vào EnemyManager │
└──────────────────────────────────┘
```

## 🎮 API MỚI

### EnemyManager Public Methods:

- `RegisterEnemy(Enemy enemy)` - Đăng ký enemy
- `ReportEnemyDeath(Enemy enemy)` - Báo cáo death
- `GetAllEnemiesSaveData()` - Lấy data để save
- `LoadEnemiesState(List<EnemySaveData>)` - Load state
- `IsEnemyDead(string id, string scene)` - Check dead
- `ClearAllData()` - Clear khi new game
- `PrintDebugInfo()` - Debug helper

### Không cần thay đổi Enemy code cũ:

- Tất cả enemy classes kế thừa từ `Enemy` tự động support
- Không cần modify logic gameplay
- Chỉ cần ensure EnemyManager tồn tại trong scene

## 💡 LƯU Ý

1. **EnemyManager phải được khởi tạo trước enemies**

   - Đặt trong Persistent Scene
   - DontDestroyOnLoad

2. **Mỗi scene có list enemies riêng**

   - Dead enemies được track theo scene
   - Chỉ áp dụng cho scene tương ứng

3. **New Game**

   - Gọi `EnemyManager.Instance.ClearAllData()` khi start new game
   - Xóa hết dead enemies list

4. **Performance**
   - Dictionary lookup O(1) rất nhanh
   - Chỉ track enemies thực sự cần thiết
   - Auto cleanup khi scene unload

## ✨ TÍNH NĂNG MỚI

- ✅ Lưu được cả enemies đã destroy
- ✅ Track enemies đã chết persistent
- ✅ Không spawn lại enemies đã giết
- ✅ Support multiple scenes
- ✅ Debug tools đầy đủ
- ✅ Fallback nếu EnemyManager không có

---

**Hoàn thành!** Hệ thống giờ đã lưu đúng tất cả enemies kể cả khi đã destroy. 🎉
