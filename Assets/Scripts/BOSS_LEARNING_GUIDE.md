# 🧠 BOSS LEARNING AI - HƯỚNG DẪN TÍCH HỢP

## ✅ ĐÃ TẠO

1. **BossLearningAI.cs** - Boss học và thích nghi với player
2. **PlayerObserver.cs** - Ghi nhận hành động player

---

## 🚀 CÁCH SỬ DỤNG

### BƯỚC 1: Add vào Boss (1 phút)

#### Trong Unity Editor:

1. **Chọn Bringer of Death** GameObject
2. **Add Component** → `BossLearningAI`
3. **Config trong Inspector**:
   - ✅ Enable Learning: `true`
   - Learning Speed: `1` (1 = normal, 2 = fast)
   - Min Observations To Learn: `3`
   - ✅ Show Learning Debug: `true`

---

### BƯỚC 2: Add vào Player (1 phút)

1. **Chọn Player** GameObject
2. **Add Component** → `PlayerObserver`
3. **Config**:
   - ✅ Enable Observation: `true`

---

### BƯỚC 3: Hook Player Actions (3 phút)

#### Trong SkillBase.cs hoặc SkillManager.cs:

```csharp
// Khi player dùng skill
public void UseSkill()
{
    // ... code hiện tại ...
    
    // Notify observer
    PlayerObserver observer = GetComponent<PlayerObserver>();
    if (observer != null)
    {
        observer.OnSkillUsed(skillName); // VD: "Tornado Slash"
    }
}
```

#### Trong Player.cs (nếu có dodge):

```csharp
void Dash()
{
    // ... code dash hiện tại ...
    
    // Notify observer
    PlayerObserver observer = GetComponent<PlayerObserver>();
    if (observer != null)
    {
        Vector2 dodgeDirection = dashDirection; // Direction of dodge
        observer.OnDodge(dodgeDirection);
    }
}
```

#### Trong HealPotion hoặc player heal:

```csharp
void UsePotion()
{
    // ... code heal ...
    
    PlayerObserver observer = player.GetComponent<PlayerObserver>();
    if (observer != null)
    {
        observer.OnHeal();
    }
}
```

---

### BƯỚC 4: Tích hợp vào BringerOfDeath.cs (5 phút)

#### Thêm reference:

```csharp
public class BringerOfDeath : Enemy
{
    // ... code hiện tại ...
    
    [Header("Learning AI")]
    private BossLearningAI learningAI;
    
    protected override void Start()
    {
        base.Start();
        
        // Get learning AI
        learningAI = GetComponent<BossLearningAI>();
        
        // ... code hiện tại ...
    }
}
```

#### Sử dụng learning AI trong Update():

```csharp
protected override void Update()
{
    cooldownTimer += Time.deltaTime;
    spellTimer += Time.deltaTime;
    deathClawTimer += Time.deltaTime;

    if (player == null) return;

    float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
    
    // === USE LEARNING AI ===
    
    // 1. Check if should interrupt player
    if (learningAI != null && learningAI.ShouldInterruptPlayer())
    {
        if (distanceToPlayer <= attackRange)
        {
            // Quick interrupt attack!
            StartCoroutine(QuickInterrupt());
            return;
        }
    }
    
    // 2. Adjust distance based on learning
    float preferredDistance = learningAI != null 
        ? learningAI.GetPreferredDistance() 
        : spellRange;
    
    // 3. Use predicted position for spell cast
    if (distanceToPlayer <= preferredDistance && spellTimer >= spellCooldown)
    {
        StartCoroutine(CastSpellWithPrediction());
        return;
    }
    
    // ... rest của code hiện tại ...
}
```

#### Cast spell với prediction:

```csharp
private IEnumerator CastSpellWithPrediction()
{
    if (isAttacking) yield break;

    isAttacking = true;
    spellTimer = 0f;
    rb.linearVelocity = Vector2.zero;
    animator.SetTrigger("Spell");

    yield return new WaitForSeconds(1f);

    if (spellPrefab != null && player != null)
    {
        // Use predicted position!
        Vector3 targetPos = learningAI != null 
            ? learningAI.GetPredictedPlayerPosition()
            : player.transform.position;
        
        Vector3 spawnPos = targetPos + new Vector3(0, spellHeightOffset+1, 0);
        GameObject spellInstance = Instantiate(spellPrefab, spawnPos, Quaternion.identity);

        SpellProjectile sp = spellInstance.GetComponent<SpellProjectile>();
    }

    yield return new WaitForSeconds(0.2f);

    isAttacking = false;
}
```

---

## 🎮 BOSS SẼ HỌC ĐƯỢC GÌ?

### 1️⃣ **Dodge Patterns**

```
[Boss Learning] Player used: dodgeLeft
[Boss Learning] Player used: dodgeLeft
[Boss Learning] Player used: dodgeLeft
🧠 [Boss Learned] Player favors LEFT dodge → Aim RIGHT!
```

**Kết quả:**
- Boss aim spell về bên PHẢI thay vì vị trí hiện tại
- Catch player khi dodge!

---

### 2️⃣ **Skill Spam Detection**

```
[Boss Learning] Player used: Tornado Slash
[Boss Learning] Player used: Tornado Slash
[Boss Learning] Player used: Tornado Slash
🧠 [Boss Learned] Player spams Tornado → KEEP DISTANCE!
```

**Kết quả:**
- Boss giữ khoảng cách 8m
- Dùng ranged attacks
- Không rush vào

---

### 3️⃣ **Heal Threshold**

```
[Boss Learning] Player healed at 35% HP
[Boss Learning] Player healed at 32% HP
🧠 [Boss Learned] Player heals at 33% HP → PRESSURE BEFORE HEAL!
```

**Kết quả:**
- Boss aggressive khi player HP ~35%
- Interrupt heal attempts
- Rush in để prevent heal

---

### 4️⃣ **Defensive Playstyle**

```
[Boss Learning] Player shields/retreats often
🧠 [Boss Learned] Player plays defensive → AGGRESSIVE MODE!
```

**Kết quả:**
- Boss tăng aggression 1.5x
- Push player constantly
- Break defensive rhythm

---

## 📊 TEST AI LEARNING

### Trong Console sẽ thấy:

```
[Player Observer] Found 1 learning bosses
[Boss Learning] Player used: Tornado Slash
[Boss Learning] Player used: Tornado Slash
[Boss Learning] Player used: Tornado Slash
🧠 [Boss Learned] Player spams Tornado → KEEP DISTANCE!
🧠 [Boss Adapting] Player near heal threshold → RUSH!
```

### Trong Scene View:

- Chọn Boss
- Thấy **vòng tròn vàng** = Predicted player position
- Thấy **vòng tròn cyan** = Preferred distance
- Boss aim về predicted position thay vì current!

---

## 🎯 BOSS PHASES + LEARNING

### Boss reset learning mỗi phase:

```csharp
void EnterPhase2()
{
    currentPhase = 2;
    
    // Reset learning để phase 2 học lại từ đầu
    if (learningAI != null)
    {
        learningAI.ResetLearning();
    }
    
    // Tăng learning speed
    // learningAI.learningSpeed = 2f; // Học nhanh hơn
    
    Debug.Log("⚠️ PHASE 2: Boss forgot everything, learning again!");
}
```

**Tác động:**
- Mỗi phase boss học lại
- Player phải đổi tactics
- Không thể spam cùng 1 trick

---

## 🔧 TUNING LEARNING AI

### Học nhanh hơn:

```
Learning Speed: 2
Min Observations: 2
```

**Kết quả:** Boss adapt sau 2-3 lần observe

### Học chậm hơn (forgiving):

```
Learning Speed: 0.5
Min Observations: 5
```

**Kết quả:** Boss cần nhiều observations hơn

---

## 💡 ADVANCED: PREDICT COMBOS

### Boss có thể học combo sequences:

```csharp
// Trong BossLearningAI.cs
void AnalyzeComboSequence()
{
    // Nếu player hay dùng: Light → Light → Heavy
    if (recentActions.Count >= 3)
    {
        string[] last3 = recentActions.TakeLast(3).ToArray();
        
        if (last3[0] == "lightAttack" && 
            last3[1] == "lightAttack" && 
            last3[2] == "heavyAttack")
        {
            // Predict: Player sẽ dùng Heavy sau 2 Light!
            predictedNextMove = "heavyAttack";
            
            // Counter: Dodge hoặc interrupt trước khi Heavy ra
        }
    }
}
```

---

## 🎮 KẾT QUẢ

### **Gameplay thay đổi:**

**Lần chơi 1:**
- Player spam Tornado Slash
- Boss chết dễ

**Lần chơi 2:**
- Boss đã học!
- Player spam Tornado → Boss keep distance
- Player phải đổi tactics
- Harder & more dynamic!

**Lần chơi 3:**
- Player dùng tactics mới
- Boss học tactics mới này
- Arms race giữa player & boss!

---

## ✅ CHECKLIST

- [ ] Add BossLearningAI vào Boss
- [ ] Add PlayerObserver vào Player
- [ ] Hook skill usage notifications
- [ ] Hook dodge notifications
- [ ] Hook heal notifications
- [ ] Integrate predicted position vào spell cast
- [ ] Test & tune learning parameters
- [ ] Enjoy adaptive boss! 🧠

---

**Boss giờ có não! Player không thể spam cheese tactics nữa! 🎉**
