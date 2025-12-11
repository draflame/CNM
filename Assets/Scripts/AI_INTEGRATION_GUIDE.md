# 🤖 HƯỚNG DẪN TÍCH HỢP AI VÀO ENEMY

## 📦 CÁC FILE ĐÃ TẠO

1. **AIDecisionMaker.cs** - Hệ thống ra quyết định AI
2. **SmartMovementAI.cs** - Di chuyển thông minh
3. **SmartAttackAI.cs** - Tấn công thông minh

---

## 🚀 CÁCH SỬ DỤNG - TỪNG BƯỚC

### BƯỚC 1: Thêm AI Components vào Enemy

#### Option A: Thêm vào Prefab có sẵn (Khuyến nghị)

1. Mở **Prefab** enemy trong Assets/PreFabs/ (ví dụ: `HellHound.prefab`)
2. Trong Inspector, click **Add Component**
3. Thêm 3 components theo thứ tự:
   - `AIDecisionMaker`
   - `SmartMovementAI`
   - `SmartAttackAI`

#### Option B: Thêm qua code

Thêm vào cuối hàm `Start()` trong `Enemy.cs`:

```csharp
protected virtual void Start()
{
    // ... code cũ ...
    
    // Thêm AI components nếu chưa có
    if (GetComponent<AIDecisionMaker>() == null)
        gameObject.AddComponent<AIDecisionMaker>();
    
    if (GetComponent<SmartMovementAI>() == null)
        gameObject.AddComponent<SmartMovementAI>();
    
    if (GetComponent<SmartAttackAI>() == null)
        gameObject.AddComponent<SmartAttackAI>();
}
```

---

### BƯỚC 2: Expose Health trong Enemy.cs

AI cần biết HP hiện tại để ra quyết định. Sửa `Enemy.cs`:

**Tìm dòng:**
```csharp
[SerializeField] private float health=4f;
```

**Đổi thành:**
```csharp
[SerializeField] protected float health=4f;
protected float maxHealth;
```

**Thêm vào hàm Start():**
```csharp
protected virtual void Start()
{
    maxHealth = health; // Lưu max health
    // ... code cũ ...
}
```

**Thêm getter method (cuối file):**
```csharp
public float GetHealthPercent()
{
    return health / maxHealth;
}

public float GetCurrentHealth()
{
    return health;
}
```

---

### BƯỚC 3: Cập nhật AIDecisionMaker để access Health

Sửa trong `AIDecisionMaker.cs`, hàm `GetHealthPercent()`:

**Tìm:**
```csharp
private float GetHealthPercent()
{
    // Sẽ cần modify Enemy.cs để expose health
    // Tạm thời return 1f
    return 1f;
}
```

**Đổi thành:**
```csharp
private float GetHealthPercent()
{
    if (enemyScript != null)
        return enemyScript.GetHealthPercent();
    return 1f;
}
```

---

### BƯỚC 4: Tích hợp AI vào HellHound (Ví dụ)

Sửa `HellHound.cs`:

**Thêm references (đầu class):**
```csharp
public class HellHound : Enemy
{
    // ... code cũ ...
    
    [Header("AI Components")]
    private AIDecisionMaker aiDecision;
    private SmartMovementAI aiMovement;
    private SmartAttackAI aiAttack;
```

**Sửa Start():**
```csharp
protected override void Start()
{
    base.Start();
    
    // Get AI components
    aiDecision = GetComponent<AIDecisionMaker>();
    aiMovement = GetComponent<SmartMovementAI>();
    aiAttack = GetComponent<SmartAttackAI>();
    
    // ... code cũ ...
}
```

**Sửa Update() để dùng AI:**
```csharp
protected override void Update()
{
    cooldownTimer += Time.deltaTime;
    UpdateAnimation();

    // === SỬ DỤNG AI ===
    if (aiDecision != null && aiMovement != null)
    {
        // AI ra quyết định
        aiDecision.MakeDecision();
        
        // Thực thi movement theo AI
        aiMovement.SmartMove();
        
        // Kiểm tra tấn công
        if (aiAttack != null && aiAttack.HasReadyAttack())
        {
            if (aiDecision.ShouldAttack())
            {
                var attack = aiAttack.DecideNextAttack();
                if (attack != null)
                {
                    aiAttack.ExecuteAttack(attack);
                    ApplyDamageToPlayer(); // Gọi damage
                }
            }
        }
        
        return; // Dừng logic cũ
    }
    
    // === LOGIC CŨ (fallback nếu không có AI) ===
    if (PlayerVisible())
    {
        // ... code cũ ...
    }
}
```

---

### BƯỚC 5: Config AI trong Inspector

Sau khi add components, config trong Unity Inspector:

#### **AIDecisionMaker Settings:**
- ✅ Show Debug Info: `true` (để xem AI đang làm gì)
- Decision Interval: `0.5` (cập nhật mỗi 0.5s)
- Low Health Threshold: `0.3` (30% HP)
- Aggressive Health Threshold: `0.7` (70% HP)
- Close Distance: `3`
- Medium Distance: `6`
- Far Distance: `10`

#### **SmartMovementAI Settings:**
- Move Speed: `3` (điều chỉnh theo enemy)
- Use Predictive Movement: `true`
- Prediction Time: `0.3`
- Dodging Cooldown: `2`

#### **SmartAttackAI Settings:**
- Use Smart Patterns: `true`
- Pattern Length: `3`
- **Available Attacks**: Click `+` để thêm attacks
  - Attack 1:
    - Name: `QuickSlash`
    - Animation Trigger: `Attack`
    - Damage: `10`
    - Range: `2`
    - Cooldown: `1`
    - Type: `Quick`
  - Attack 2:
    - Name: `HeavyBite`
    - Animation Trigger: `HeavyAttack`
    - Damage: `25`
    - Range: `2.5`
    - Cooldown: `2.5`
    - Type: `Heavy`

---

## 🎮 TEST AI

### 1. Chạy game và quan sát Console

Bạn sẽ thấy debug logs như:
```
[HellHound] Tactic: Aggressive | Threat: 0.35 | Dist: 4.2m
[HellHound] Executing: QuickSlash (Type: Quick)
[HellHound] Tactic: Defensive | Threat: 0.65 | Dist: 2.1m
```

### 2. Xem trong Scene View (khi chọn enemy)

- **Vòng tròn vàng**: Preferred distance
- **Đường đến player**: Xanh (safe) / Đỏ (dangerous)
- **Vòng tròn cyan**: Predicted player position
- **Vòng tròn màu sắc**: Attack ranges

### 3. Test các scenarios:

**Scenario 1: Player HP cao, enemy HP cao**
- ✅ Enemy nên ở mode **Aggressive** hoặc **Patrol**

**Scenario 2: Player HP thấp (<40%)**
- ✅ Enemy nên **rush** và tấn công mạnh

**Scenario 3: Enemy HP thấp (<30%)**
- ✅ Enemy nên **Retreat** (chạy lui)

**Scenario 4: Player đang tấn công**
- ✅ Enemy nên **Defensive** (giữ khoảng cách)

**Scenario 5: Nhiều enemy gần nhau**
- ✅ Một vài enemy nên **Flank** (đi vòng)

---

## ⚙️ TUNING AI

### Điều chỉnh độ khó:

**Dễ hơn:**
- Tăng `Decision Interval` → 1.0s (AI chậm hơn)
- Tăng `Attack Cooldown` → 2-3s
- Giảm `Move Speed` → 2-2.5
- Tắt `Use Predictive Movement`

**Khó hơn:**
- Giảm `Decision Interval` → 0.2s (AI nhanh hơn)
- Giảm `Attack Cooldown` → 0.5-1s
- Tăng `Move Speed` → 4-5
- Bật `Use Predictive Movement`
- Tăng `Prediction Time` → 0.5s

---

## 🐛 TROUBLESHOOTING

### Problem: AI không hoạt động

**Solution:**
1. Check console có lỗi không
2. Đảm bảo đã thêm cả 3 components
3. Đảm bảo `Enemy.cs` đã có `GetHealthPercent()`
4. Check enemy có Rigidbody2D không

### Problem: Enemy đứng im

**Solution:**
1. Check `SmartMovementAI.moveSpeed` > 0
2. Check enemy có `Rigidbody2D` và `Gravity Scale` hợp lý
3. Bật `Show Debug Info` xem tactic hiện tại

### Problem: Enemy không tấn công

**Solution:**
1. Check `Available Attacks` có được setup chưa
2. Check animation triggers có đúng tên không
3. Check `Attack Range` có đủ lớn không

### Problem: AI quá dễ/khó

**Solution:**
- Xem phần **TUNING AI** ở trên
- Điều chỉnh thresholds trong `AIDecisionMaker`

---

## 🎯 NEXT STEPS - Nâng cao hơn

### 1. **Boss AI với Phases**

Tạo `BossAI.cs` kế thừa `AIDecisionMaker`:
```csharp
public class BossAI : AIDecisionMaker
{
    private int currentPhase = 1;
    
    private void CheckPhase()
    {
        float hp = GetHealthPercent();
        
        if (hp < 0.5f && currentPhase == 1)
        {
            EnterPhase2();
        }
    }
    
    private void EnterPhase2()
    {
        currentPhase = 2;
        // Unlock new attacks, increase speed, etc.
    }
}
```

### 2. **Group Coordination**

Enemy giao tiếp với nhau:
```csharp
public class GroupCoordinator : MonoBehaviour
{
    private static List<Enemy> allEnemies = new List<Enemy>();
    
    public void AssignRoles()
    {
        // Enemy 1: Tank
        // Enemy 2: Flanker
        // Enemy 3: Ranged
    }
}
```

### 3. **Learning from Player**

Track player patterns và adapt:
```csharp
public class PlayerPatternLearner : MonoBehaviour
{
    // Track xem player hay dùng skill gì
    // Adapt tactics để counter
}
```

---

## 📚 TÀI LIỆU THAM KHẢO

- Unity AI Navigation: https://docs.unity3d.com/Packages/com.unity.ai.navigation@latest
- Behavior Trees: https://www.gamedeveloper.com/programming/behavior-trees-for-ai-how-they-work
- Unity ML-Agents (nâng cao): https://github.com/Unity-Technologies/ml-agents

---

## ✅ CHECKLIST HOÀN THÀNH

- [ ] Thêm 3 AI components vào enemy prefab
- [ ] Sửa Enemy.cs để expose health
- [ ] Cập nhật AIDecisionMaker để đọc health
- [ ] Tích hợp AI vào Update() của enemy
- [ ] Config AI settings trong Inspector
- [ ] Test các scenarios
- [ ] Tune difficulty phù hợp
- [ ] Vô hiệu hóa hoặc xóa logic cũ nếu cần

---

**Chúc bạn thành công! 🎮🤖**

Nếu gặp vấn đề, hãy:
1. Bật `Show Debug Info` trong AIDecisionMaker
2. Xem Console logs
3. Xem Gizmos trong Scene view
