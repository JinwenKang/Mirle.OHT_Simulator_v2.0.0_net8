<!--
=============================================================================
SYNC IMPACT REPORT
=============================================================================
Version Change: 0.0.0 → 1.0.0 (MAJOR - 初始版本)

Added Principles:
- I. 高品質可測試程式碼 (Testable Code First)
- II. 最小可行產品優先 (MVP First)
- III. 避免過度設計 (No Overdesign / YAGNI)
- IV. 正體中文回應 (Traditional Chinese)
- V. 單一檔案收斂 (Single File Consolidation)

Added Sections:
- 溝通規範 (Communication Standards)
- 開發工作流程 (Development Workflow)
- Governance

Templates Requiring Updates:
- ✅ plan-template.md (已確認相容)
- ✅ spec-template.md (已確認相容)
- ✅ tasks-template.md (已確認相容)

Deferred Items: 無

Follow-up TODOs: 無
=============================================================================
-->

# OHT 模擬器專案憲法

## 核心原則 (Core Principles)

### I. 高品質可測試程式碼 (Testable Code First)

程式碼品質與可測試性為最高優先事項。

**強制規定**：
- 所有核心功能 MUST 具備獨立可測試性
- 新功能開發 MUST 先定義驗收標準再實作
- 程式碼 MUST 遵循單一職責原則 (SRP)，確保每個類別/方法可被隔離測試
- 依賴注入 SHOULD 優先於硬編碼依賴，以利單元測試

**理由**：可測試的程式碼天生具有較好的模組化設計，能及早發現問題並降低維護成本。

### II. 最小可行產品優先 (MVP First)

以最小可行產品 (Minimum Viable Product) 為目標，快速交付可運作的解決方案。

**強制規定**：
- 功能開發 MUST 按優先級 (P1 → P2 → P3) 順序實作
- 每個 User Story MUST 可獨立測試且提供獨立價值
- 第一次迭代 MUST 只包含核心功能，進階功能延後處理
- SHOULD 在 24 小時內完成 MVP 並取得回饋

**理由**：快速交付可運作的版本，能及早驗證需求正確性，避免投入資源開發不需要的功能。

### III. 避免過度設計 (No Overdesign / YAGNI)

遵循 YAGNI (You Aren't Gonna Need It) 原則，不預先設計尚未需要的功能。

**強制規定**：
- 程式碼 MUST NOT 包含「以備將來使用」的抽象層或介面
- 重構 SHOULD 只在有明確需求時進行
- 設計模式 MUST 只在解決實際問題時套用，非為了「更優雅」
- 框架/套件選擇 MUST 基於當前需求，非潛在擴充性

**理由**：過度設計增加複雜度、維護負擔，並常常猜錯未來需求方向。

### IV. 正體中文回應 (Traditional Chinese)

所有 AI 助手回應及專案文件 MUST 使用正體中文。

**強制規定**：
- AI 助手 MUST 一律使用正體中文回答問題
- 專案文件 SHOULD 以正體中文撰寫
- 程式碼註解 SHOULD 使用正體中文（API 文件除外）
- 變數/函式命名 SHOULD 使用英文，但說明文件用中文

**理由**：團隊成員以中文為主要溝通語言，統一語言降低認知負擔。

### V. 單一檔案收斂 (Single File Consolidation)

工作計畫、測試項目、進度追蹤 MUST 收斂至單一檔案，避免多版本混亂。

**強制規定**：
- 每個功能規格 MUST 使用單一 `tasks.md` 追蹤所有任務與測試
- 計畫調整時 MUST 更新現有檔案，非建立新版本
- 歷史變更 SHOULD 透過 Git 版本控制追溯，非複製檔案
- 規格文件 MUST 集中於 `.specify/specs/` 目錄結構下

**理由**：單一真實來源 (Single Source of Truth) 避免版本混亂與資訊不同步。

## 溝通規範 (Communication Standards)

**AI 助手行為規範**：
- 回應 MUST 使用正體中文
- 技術術語 SHOULD 保留英文原文並附中文說明
- 程式碼區塊 MUST 保持原語言（英文變數名、註解可中文）
- 錯誤訊息 SHOULD 提供具體解決方案

## 開發工作流程 (Development Workflow)

**專案啟動時**：
1. AI 助手 MUST 自動讀取 `.specify/memory/constitution.md` 規範
2. AI 助手 MUST 自動讀取相關的 `spec.md`、`plan.md`、`tasks.md`
3. AI 助手 SHOULD 總結目前進度與待辦事項

**功能開發流程**：
1. `/speckit.specify` - 定義功能規格與驗收標準
2. `/speckit.plan` - 建立技術計畫與研究
3. `/speckit.tasks` - 分解為可執行任務
4. 實作 → 測試 → 驗收

**計畫調整時**：
- MUST 更新 `tasks.md` 反映最新狀態
- MUST 標記已完成任務為 `[X]`
- SHOULD 記錄重大決策與理由

## 治理 (Governance)

**憲法效力**：
- 本憲法優先於所有其他開發慣例
- 違反憲法原則之設計決策 MUST 有明確書面理由

**修訂程序**：
- 原則修訂 MUST 更新版本號 (MAJOR.MINOR.PATCH)
  - MAJOR：移除/重新定義原則
  - MINOR：新增原則或重大擴充
  - PATCH：澄清、用詞修正、非語意變更
- 修訂 MUST 更新 `LAST_AMENDED_DATE`
- 修訂 SHOULD 記錄於 Sync Impact Report

**合規審查**：
- 功能完成時 SHOULD 檢核是否符合核心原則
- 複雜度增加 MUST 有書面理由

**Version**: 1.0.0 | **Ratified**: 2025-07-16 | **Last Amended**: 2025-07-16
