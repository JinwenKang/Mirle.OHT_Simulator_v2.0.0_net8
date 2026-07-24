# MCSManager 動態開關功能實現指南

## 概述
本文件說明如何在 OHT Simulator 中實現 MCSManager 的動態開關功能，使用戶可以在運行時通過 UI 來啟用或禁用 MCSManager。

## 已完成的代碼修改

### 1. SCApplication.cs 修改
✅ 已添加以下功能：
- `isMCSManagerEnabled` 標誌變數：控制 MCSManager 啟用狀態
- `mcsManagerLock` 鎖對象：確保線程安全
- `SetMCSManagerEnabled(bool)` 方法：動態啟用/禁用 MCSManager
- `GetMCSManagerEnabled()` 方法：取得當前狀態
- 修改構造函數：根據標誌選擇性初始化 MCSManager
- 修改 `initMCSManager()`：添加鎖機制和重複檢查

**修改位置**：[SCApplication.cs](./Mirle.OHT.Simulator/App/SCApplication.cs#L74-L180)

### 2. MCSManager.cs 修改
✅ 已添加 `Stop()` 方法用於停止連接

**修改位置**：[MCSManager.cs](./Mirle.OHT.Simulator/Service/MCSManager.cs#L46-L60)

## 實現 UI 控制（需要手動添加）

### 選項 A：添加按鈕控制（推薦用於快速測試）

在 Form1.Designer.cs 中的 `InitializeComponent()` 方法添加按鈕：

```csharp
// 在適當的位置添加控制按鈕
btnEnableMCS = new System.Windows.Forms.Button();
btnDisableMCS = new System.Windows.Forms.Button();
lblMCSStatus = new System.Windows.Forms.Label();

// 設置按鈕屬性
btnEnableMCS.Text = "啟用 MCS";
btnEnableMCS.Location = new System.Drawing.Point(350, 500);
btnEnableMCS.Size = new System.Drawing.Size(100, 30);
btnEnableMCS.Click += BtnEnableMCS_Click;

btnDisableMCS.Text = "禁用 MCS";
btnDisableMCS.Location = new System.Drawing.Point(460, 500);
btnDisableMCS.Size = new System.Drawing.Size(100, 30);
btnDisableMCS.Click += BtnDisableMCS_Click;

lblMCSStatus.Text = "MCS 狀態: 已啟用";
lblMCSStatus.Location = new System.Drawing.Point(350, 470);
lblMCSStatus.Size = new System.Drawing.Size(200, 25);

// 添加到 Form
this.Controls.Add(btnEnableMCS);
this.Controls.Add(btnDisableMCS);
this.Controls.Add(lblMCSStatus);
```

### 選項 B：添加複選框控制（推薦用於最終版本）

在 Form1.Designer.cs 中添加複選框：

```csharp
chbMCSManager = new System.Windows.Forms.CheckBox();
chbMCSManager.Text = "啟用 MCSManager";
chbMCSManager.Checked = true;
chbMCSManager.Location = new System.Drawing.Point(350, 500);
chbMCSManager.Size = new System.Drawing.Size(150, 25);
chbMCSManager.CheckedChanged += ChbMCSManager_CheckedChanged;

this.Controls.Add(chbMCSManager);
```

## 在 Form1.cs 中添加事件處理

### 按鈕方式的事件處理

```csharp
private void BtnEnableMCS_Click(object sender, EventArgs e)
{
    try
    {
        scApp.SetMCSManagerEnabled(true);
        lblMCSStatus.Text = "MCS 狀態: 已啟用";
        lblMCSStatus.ForeColor = Color.Green;
        MessageBox.Show("MCSManager 已成功啟用");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"啟用 MCSManager 失敗: {ex.Message}");
    }
}

private void BtnDisableMCS_Click(object sender, EventArgs e)
{
    try
    {
        scApp.SetMCSManagerEnabled(false);
        lblMCSStatus.Text = "MCS 狀態: 已禁用";
        lblMCSStatus.ForeColor = Color.Red;
        MessageBox.Show("MCSManager 已成功禁用");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"禁用 MCSManager 失敗: {ex.Message}");
    }
}
```

### 複選框方式的事件處理

```csharp
private void ChbMCSManager_CheckedChanged(object sender, EventArgs e)
{
    try
    {
        bool isEnabled = chbMCSManager.Checked;
        scApp.SetMCSManagerEnabled(isEnabled);
        logger.Info($"MCSManager 已{'啟用' if isEnabled else '禁用'}");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"切換 MCSManager 狀態失敗: {ex.Message}");
        // 恢復複選框狀態
        chbMCSManager.CheckedChanged -= ChbMCSManager_CheckedChanged;
        chbMCSManager.Checked = !chbMCSManager.Checked;
        chbMCSManager.CheckedChanged += ChbMCSManager_CheckedChanged;
    }
}
```

## 在 Form1.Designer.cs 中聲明控件

添加到字段聲明部分：

```csharp
private System.Windows.Forms.Button btnEnableMCS;
private System.Windows.Forms.Button btnDisableMCS;
private System.Windows.Forms.Label lblMCSStatus;

// 或使用複選框方式
private System.Windows.Forms.CheckBox chbMCSManager;
```

## 使用方法

### 運行時啟用/禁用 MCSManager

1. **方式一：通過 UI 按鈕**
   - 點擊「啟用 MCS」按鈕可以動態啟用 MCSManager
   - 點擊「禁用 MCS」按鈕可以動態禁用 MCSManager
   - 狀態標籤會實時更新顯示當前狀態

2. **方式二：通過複選框**
   - 勾選複選框啟用 MCSManager
   - 取消勾選複選框禁用 MCSManager

3. **程式碼方式**
   ```csharp
   SCApplication scApp = SCApplication.getInstance();
   scApp.SetMCSManagerEnabled(true);  // 啟用
   scApp.SetMCSManagerEnabled(false); // 禁用
   bool isEnabled = scApp.GetMCSManagerEnabled(); // 查詢狀態
   ```

## 功能特性

✅ **線程安全**：使用 lock 機制保護共享資源
✅ **防止重複初始化**：initMCSManager 中添加了重複檢查
✅ **優雅關閉**：Stop() 方法安全地關閉連接
✅ **狀態查詢**：可隨時查詢當前啟用狀態
✅ **日誌記錄**：所有操作都會記錄到日誌中

## 主要修改文件

1. [SCApplication.cs](./Mirle.OHT.Simulator/App/SCApplication.cs) - 核心邏輯
2. [MCSManager.cs](./Mirle.OHT.Simulator/Service/MCSManager.cs) - Stop 方法
3. Form1.Designer.cs - UI 控件（需手動添加）
4. Form1.cs - 事件處理（需手動添加）

## 注意事項

⚠️ **重要**：
- 禁用 MCSManager 後，所有 SECS 通信將停止
- 再次啟用時會重新初始化連接
- 確保在禁用前沒有待機命令或進行中的操作

## 測試清單

- [ ] 確認應用啟動時 MCSManager 正常初始化
- [ ] 點擊「禁用 MCS」按鈕，驗證 MCSManager 停止
- [ ] 點擊「啟用 MCS」按鈕，驗證 MCSManager 重新連接
- [ ] 檢查日誌文件確認操作記錄正確
- [ ] 驗證禁用期間無 SECS 通信
- [ ] 驗證啟用後通信恢復正常
