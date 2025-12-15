# Hướng dẫn Debug Enemy Save/Load System

## ✅ Đã fix những vấn đề sau:

### 1. **FindObjectsOfType không tìm được enemies đúng cách**

- **Vấn đề**: Dùng LINQ `.OfType<ISaveableEnemy>()` không hoạt động tốt
- **Giải pháp**: Dùng `FindObjectsOfType<Enemy>()` trực tiếp

### 2. **Enemy ID không persistent giữa các lần load**

- **Vấn đề**: Dùng GUID random khiến ID thay đổi mỗi lần load scene
- **Giải pháp**: Tạo ID dựa trên `sceneName + enemyType + position` để ID giống nhau

### 3. **Timing issue khi load game**

- **Vấn đề**: ApplyEnemiesSaveData được gọi trước khi enemies chạy Start()
- **Giải pháp**: Thêm delay 0.2s trước khi apply data

## 🧪 Cách test hệ thống:

### Bước 1: Thêm Debugger vào scene

1. Tạo một Empty GameObject trong scene có enemies
2. Add component `EnemySaveDebugger`
3. Check "Show Debug Info" trong Inspector

### Bước 2: Test Save

1. Chơi game, để enemies bị damage (giảm máu)
2. Nhấn **F9** để xem thông tin enemies hiện tại
3. Nhấn nút **Save Game**
4. Kiểm tra Console - phải thấy logs như:
   ```
   🔍 Found X enemies in scene to save
   💾 Saved enemy: SceneName_EnemyType_X_Y - Health: XX, Dead: false
   ✅ Total enemies saved: X
   ```

### Bước 3: Test Load

1. Quit game hoặc chuyển scene khác
2. Load lại game từ save file
3. Kiểm tra Console - phải thấy logs:
   ```
   🔄 Applying enemy save data for scene: [SceneName]
   🔍 Found X enemies in current scene
   📋 Registered enemy for restore: [EnemyID]
   ♻️ Restored enemy: [EnemyID] - Health: XX
   ```
4. Nhấn **F9** để verify enemies có đúng máu/trạng thái

### Bước 4: Test Enemy đã chết

1. Chơi game, giết một vài enemies
2. Save game
3. Load lại
4. Kiểm tra - enemies đã chết phải **không spawn lại**
5. Console phải có logs:
   ```
   💀 Destroyed dead enemy: [EnemyID]
   ```

## 🐛 Troubleshooting:

### Vấn đề: "Found 0 enemies in scene to save"

**Nguyên nhân**: Enemy class không được tìm thấy
**Giải pháp**:

- Kiểm tra enemy có inherit từ `Enemy` class không
- Đảm bảo enemy GameObject active trong scene
- Check Console có lỗi compile không

### Vấn đề: Enemy ID thay đổi mỗi lần load

**Nguyên nhân**: Enemy vị trí spawn thay đổi
**Giải pháp**:

- Đảm bảo enemy spawn ở vị trí cố định trong scene
- Hoặc set `enemyID` thủ công trong Inspector

### Vấn đề: Enemies không restore health đúng

**Nguyên nhân**: Hearts không được update
**Giải pháp**:

- Kiểm tra `LoadFromSaveData()` có chạy không
- Verify hearts list có đủ không
- Check logs "♻️ Restored enemy"

### Vấn đề: Enemy đã chết vẫn spawn lại

**Nguyên nhân**: Save data không có isDead = true
**Giải pháp**:

- Kiểm tra `Die()` method có set `isDead = true` không
- Verify save file có field "isDead": true

## 📝 Notes:

### Enemy ID Format:

```
[SceneName]_[EnemyType]_[X]_[Y]
Ví dụ: Map1_Goblin_15.3_8.7
```

### Save File Location:

```
Windows: C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[GameName]\savegame.json
```

### Debug Commands:

- **F9**: In thông tin tất cả enemies + save file
- **GUI Button**: Hiển thị số lượng enemies trong scene

## 🎮 Lưu ý khi sử dụng:

1. **Mỗi enemy type nên có tên riêng** (set trong Inspector)
2. **Không đặt 2 enemies cùng type ở cùng vị trí** (sẽ có cùng ID)
3. **Nếu muốn control ID thủ công**, set trong Inspector trước khi play
4. **Enemies cần có BoxCollider2D** để detection hoạt động

## ✨ Tính năng đã implement:

- ✅ Auto-generate persistent ID cho enemies
- ✅ Save: health, position, direction, dead state
- ✅ Load: restore all states
- ✅ Remove dead enemies khi load
- ✅ Support multiple enemy types
- ✅ Debug tools để kiểm tra

Good luck! 🎯
