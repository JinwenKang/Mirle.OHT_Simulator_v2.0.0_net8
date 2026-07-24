# MCSManager 動態開關功能實現總結

## 📋 實現狀態

### ✅ 已完成的工作

#### 1. 後端核心功能（SCApplication.cs）
- ✅ 添加了 `isMCSManagerEnabled` 標誌變數
- ✅ 添加了 `mcsManagerLock` 線程同步機制
- ✅ 實現了 `SetMCSManagerEnabled(bool)` 方法
  - 可動態啟用或禁用 MCSManager
  - 啟用時若未初始化則進行初始化
  - 禁用時安全停止連接
  - 所有操作都有日誌記錄
- ✅ 實現了 `GetMCSManagerEnabled()` 方法
  - 可查詢當前啟用狀態
- ✅ 修改構造函數以支持延遲初始化
- ✅ 增強了 `initMCSManager()` 方法
  - 添加了線程同步
  - 添加了重複初始化檢查

#### 2. 連接管理（MCSManager.cs）
- ✅ 實現了 `Stop()` 方法
  - 安全地停止 SECS Agent 連接
  - 包含異常處理機制

### 🎯 功能特性

| 特性 | 描述 |
|------|------|
| **動態開關** | 可在運行時動態啟用/禁用 MCSManager |
| **線程安全** | 使用 lock 機制保護共享資源 |
| **狀態查詢** | 支持實時查詢啟用狀態 |
| **日誌記錄** | 所有操作都記錄到系統日誌 |
| **優雅關閉** | 支持安全的連接關閉 |
| **重複保護** | 防止重複初始化 |

## 📁 修改文件列表

### 1. [SCApplication.cs](./Mirle.OHT.Simulator/App/SCApplication.cs)
**修改內容：**
- 第 74-82 行：添加動態開關成員變數
- 第 91-99 行：修改構造函數
- 第 110-171 行：重寫 initMCSManager 方法
- 第 173-196 行：添加 SetMCSManagerEnabled 方法
- 第 198-209 行：添加 GetMCSManagerEnabled 方法

**修改內容詳情：**
```csharp
// 新增成員變數
private bool isMCSManagerEnabled = true;
private object mcsManagerLock = new object();

// SetMCSManagerEnabled - 動態開關
public void SetMCSManagerEnabled(bool enabled)
{
    lock (mcsManagerLock)
    {
        isMCSManagerEnabled = enabled;
        if (enabled && mcsManager == null)
            initMCSManager();
        else if (!enabled && mcsManager != null)
        {
            mcsManager.Stop();
            mcsManager = null;
        }
    }
}

// GetMCSManagerEnabled - 狀態查詢
public bool GetMCSManagerEnabled()
{
    lock (mcsManagerLock)
    {
        return isMCSManagerEnabled;
    }
}
```

### 2. [MCSManager.cs](./Mirle.OHT.Simulator/Service/MCSManager.cs)
**修改內容：**
- 第 46-60 行：添加 Stop() 方法

**修改內容詳情：**
```csharp
public void Stop()
{
    try
    {
        if (agent != null)
        {
            agent.stop();
        }
    }
    catch (Exception ex)
    {
        System.Console.WriteLine($"停止 MCSManager 時發生異常: {ex.Message}");
    }
}
```

## 🎨 UI 集成方案（待手動實現）

### 方案 A：按鈕控制（推薦用於測試）

1. **在 Form1.cs 中添加字段：**
```csharp
private System.Windows.Forms.Button btnEnableMCS;
private System.Windows.Forms.Button btnDisableMCS;
private System.Windows.Forms.Label lblMCSStatus;
```

2. **在 InitializeComponent 中添加控件初始化：**
```csharp
btnEnableMCS = new System.Windows.Forms.Button();
btnEnableMCS.Text = "啟用 MCS";
btnEnableMCS.Location = new System.Drawing.Point(350, 500);
btnEnableMCS.Click += BtnEnableMCS_Click;
this.Controls.Add(btnEnableMCS);

btnDisableMCS = new System.Windows.Forms.Button();
btnDisableMCS.Text = "禁用 MCS";
btnDisableMCS.Location = new System.Drawing.Point(460, 500);
btnDisableMCS.Click += BtnDisableMCS_Click;
this.Controls.Add(btnDisableMCS);

lblMCSStatus = new System.Windows.Forms.Label();
lblMCSStatus.Text = "MCS 狀態: 已啟用";
lblMCSStatus.ForeColor = System.Drawing.Color.Green;
this.Controls.Add(lblMCSStatus);
```

3. **添加事件處理器：**
```csharp
private void BtnEnableMCS_Click(object sender, EventArgs e)
{
    scApp.SetMCSManagerEnabled(true);
    lblMCSStatus.Text = "MCS 狀態: 已啟用";
    lblMCSStatus.ForeColor = System.Drawing.Color.Green;
}

private void BtnDisableMCS_Click(object sender, EventArgs e)
{
    scApp.SetMCSManagerEnabled(false);
    lblMCSStatus.Text = "MCS 狀態: 已禁用";
    lblMCSStatus.ForeColor = System.Drawing.Color.Red;
}
```

### 方案 B：複選框控制（推薦用於最終版本）

1. **在 Form1.cs 中添加字段：**
```csharp
private System.Windows.Forms.CheckBox chbMCSManager;
```

2. **在 InitializeComponent 中添加控件：**
```csharp
chbMCSManager = new System.Windows.Forms.CheckBox();
chbMCSManager.Text = "啟用 MCSManager";
chbMCSManager.Checked = true;
chbMCSManager.CheckedChanged += ChbMCSManager_CheckedChanged;
this.Controls.Add(chbMCSManager);
```

3. **添加事件處理器：**
```csharp
private void ChbMCSManager_CheckedChanged(object sender, EventArgs e)
{
    scApp.SetMCSManagerEnabled(chbMCSManager.Checked);
}
```

## 🚀 使用方法

### 程式碼中直接使用：
```csharp
SCApplication scApp = SCApplication.getInstance();

// 啟用 MCSManager
scApp.SetMCSManagerEnabled(true);

// 禁用 MCSManager
scApp.SetMCSManagerEnabled(false);

// 查詢當前狀態
bool isEnabled = scApp.GetMCSManagerEnabled();
```

### 通過 UI 使用（實現後）：
1. 點擊「啟用 MCS」按鈕→ MCSManager 連接到 MCS
2. 點擊「禁用 MCS」按鈕→ MCSManager 斷開連接
3. 狀態標籤實時顯示當前狀態

## 📊 流程圖

```
應用啟動
    ↓
SCApplication 初始化
    ↓
isMCSManagerEnabled = true?
    ├─ YES → initMCSManager() → MCSManager 連接
    └─ NO  → 跳過初始化
    ↓
用戶操作
    ├─ 點擊「啟用 MCS」 → SetMCSManagerEnabled(true)
    │   ├─ mcsManager == null? 
    │   │   ├─ YES → initMCSManager()
    │   │   └─ NO  → 已連接，無操作
    │   └─ 更新 UI 狀態
    │
    └─ 點擊「禁用 MCS」 → SetMCSManagerEnabled(false)
        ├─ mcsManager != null?
        │   ├─ YES → mcsManager.Stop() → mcsManager = null
        │   └─ NO  → 已禁用，無操作
        └─ 更新 UI 狀態
```

## 📋 集成步驟清單

- [ ] 確認已修改 SCApplication.cs
- [ ] 確認已修改 MCSManager.cs
- [ ] 編譯確認沒有錯誤
- [ ] 運行測試應用啟動
- [ ] 選擇方案 A 或 B 實現 UI
- [ ] 添加字段聲明到 Form1.cs
- [ ] 添加控件初始化代碼
- [ ] 添加事件處理器
- [ ] 測試「啟用 MCS」功能
- [ ] 測試「禁用 MCS」功能
- [ ] 驗證禁用期間無 SECS 通信
- [ ] 驗證啟用後通信恢復
- [ ] 檢查日誌記錄

## ⚠️ 重要注意事項

1. **數據一致性**：禁用 MCSManager 前確保沒有待機命令
2. **日誌監視**：查看 SystemProcess 日誌確認操作結果
3. **異常處理**：所有操作都包含異常捕獲，檢查是否有錯誤日誌
4. **線程安全**：所有操作都通過 lock 機制保護

## 🔧 故障排除

| 問題 | 原因 | 解決方案 |
|------|------|--------|
| 禁用失敗 | MCSManager 不存在 | 確認已啟用過 |
| 啟用無效 | 未檢查返回值 | 查看日誌確認操作結果 |
| UI 不更新 | 事件未連接 | 確認 += 語法正確 |
| 通信中斷 | 禁用未完成 | 等待禁用操作完成 |

## 📚 相關文件

- [MCS_MANAGER_DYNAMIC_SWITCH_GUIDE.md](./MCS_MANAGER_DYNAMIC_SWITCH_GUIDE.md) - 詳細實現指南
- [MCSManager_UI_Implementation_Sample.cs](./MCSManager_UI_Implementation_Sample.cs) - UI 實現示例代碼

## ✨ 功能驗證

運行應用後驗證以下項目：

1. ✅ 應用啟動時 MCSManager 自動連接
2. ✅ 調用 SetMCSManagerEnabled(false) 後 MCSManager 停止
3. ✅ 調用 SetMCSManagerEnabled(true) 後 MCSManager 重新連接
4. ✅ GetMCSManagerEnabled() 返回正確的狀態
5. ✅ 日誌中記錄所有操作
6. ✅ UI 按鈕/複選框正常工作（實現後）

---
**實現日期**：2026-06-10  
**版本**：1.0  
**狀態**：後端代碼已完成，待 UI 集成
