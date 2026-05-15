# Reject 41 功能規格

## 概述

在 OHT 模擬器中新增 Reject 41 功能，讓模擬器可以拒絕 Mode Change (ID_41) 指令。當啟用此功能時，模擬器收到 ID_41 指令會回覆 ID_141 且 `replyCode = 1`，表示拒絕執行模式切換。

## 背景

參照現有 Reject 31 的實作模式，需要建立一個可在 UI 上切換的拒絕模式。這對於測試 OHBC 系統在收到拒絕回應時的錯誤處理邏輯非常重要。

## 需求

### 功能需求

| ID | 需求 | 優先級 |
|----|------|--------|
| FR-01 | 在 UI 新增 Reject 41 單選按鈕 | 必要 |
| FR-02 | 新增 reject41 布林旗標控制拒絕行為 | 必要 |
| FR-03 | 當 reject41=true 時，ID_141 回覆 replyCode=1 | 必要 |
| FR-04 | 當 reject41=true 時，不執行後續模式切換邏輯 | 必要 |

### 非功能需求

| ID | 需求 | 優先級 |
|----|------|--------|
| NFR-01 | 與現有 Reject 31 模式共存，互不影響 | 必要 |
| NFR-02 | UI 控制項位置與現有介面風格一致 | 建議 |

## 驗收標準

1. ✅ 勾選 Reject 41 → 收到 ID_41 → 回覆 ID_141 (replyCode=1)
2. ✅ 勾選 Reject 41 → 不執行模式切換
3. ✅ 未勾選 Reject 41 → 正常處理 ID_41 並執行模式切換

## 相關協定

- **ID_41**: Mode Change Request (from OHBC)
- **ID_141**: Mode Change Response (to OHBC)
  - `replyCode = 0`: 成功
  - `replyCode = 1`: 拒絕
