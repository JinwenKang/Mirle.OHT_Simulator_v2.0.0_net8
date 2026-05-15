# .NET 8 升級執行計畫

> 種子來源：feat/加入OHT可以在Move命令改為Load 分支整理出的乾淨程式碼
> 目標：net48 (legacy csproj) → net8.0-windows (SDK-style) + 程式碼現代化
> 參考：[NET8_MIGRATION.md](NET8_MIGRATION.md)

---

## 階段切分原則

- 每階段 **獨立可驗證**（跑得起來才進下一階段）
- 每階段做完讓使用者確認再進下一階段
- 階段 1~5 必做（升級到能編譯能跑）
- 階段 6~9 為優化（可選、可分批）

---

## Phase 1：csproj 轉 SDK-style + 改框架

**目標**：主專案與 CommonMessage 兩個 csproj 都轉 SDK-style，目標 `net8.0-windows`。

**做什麼**：
- 重寫 `Mirle.OHT.Simulator/Mirle.OHT_OHBC.Simulator.csproj`
  - `<Project Sdk="Microsoft.NET.Sdk">`
  - `<TargetFramework>net8.0-windows</TargetFramework>`
  - `<UseWindowsForms>true</UseWindowsForms>`
  - 移除 `<Compile Include>` 全部列舉（SDK 自動 glob）
  - 保留必要的 `<None Update>` / `<Content>` 設定檔複製
  - 保留 SECSTrxControl / TCPIPTrxControl / GenericParsing 本地 DLL 參考
- 重寫 `CommonMessage/CommonMessage.csproj`（保持 `netstandard2.0` 或同步改 net8.0）
- 移除 `packages.config`
- 移除 `packages/` 資料夾

**驗證**：`dotnet restore` 成功

---

## Phase 2：套件改 PackageReference + 版本對齊

**做什麼**：
- 套件清單（依現況選擇是否升版）：
  - `Elasticsearch.Net` / `NEST`：7.x 保留（最新 net8 相容）
  - `Google.Protobuf`：升到 3.25+
  - `Grpc.Core` / `Grpc.Core.Api`：留 2.46.x 或評估改 `Grpc.Net.Client`
  - `NLog`：5.3.x
  - `System.Configuration.ConfigurationManager`：新增 8.0+
  - 確認 `NATS.Client` / `STAN.Client` 是否真的有人用（grep `using NATS|using STAN|StanConnection|NatsConnection`），無人用就不加
- 維持 .NET Framework 殘留套件（System.Buffers、System.Memory 等）— SDK 會自動處理
- App.config 若有 `bindingRedirect` 全清

**驗證**：`dotnet restore` 成功、套件版本鎖定無 conflict

---

## Phase 3：App.config 清理

**做什麼**：
- 移除 `<startup><supportedRuntime ... />` 整段
- 移除 `<runtime><assemblyBinding>` 整段
- 移除 `<system.web>` 整段
- 保留 `<appSettings>` 中真正讀取的 key（先全留，跑通後再瘦身）

**驗證**：開檔可看，無 schema 錯誤

---

## Phase 4：程式碼必改項

**做什麼**（編譯一定會失敗的地方）：
- `MiddleAgent.cs`：刪 `using System.Runtime.Remoting.Channels;` 等 Remoting 引用
- `Opera.cs` / `Opera_New.cs`：`BinaryFormatter.Serialize/Deserialize` 改用 protobuf `IDeepCloneable<T>.Clone()`
- 任何 `[Serializable]` 配 `BinaryFormatter` 的程式 → 改 protobuf clone / JSON serialize
- 任何 `using System.Web` / `using System.Runtime.Remoting` / `Thread.Abort()` → 移除或重寫
- `Process.Start("url")` 若有 → 加 `UseShellExecute = true`

**驗證**：`dotnet build` 0 errors（警告先忽略）

---

## Phase 5：建置綠燈 + 冒煙測試

**做什麼**：
- 建置全綠
- 啟動主程式，確認 WinForms 主畫面開得起來
- 確認 NLog 寫得出 log（先用舊 config 不動）
- 確認 gRPC / SECS / Elasticsearch 連線初始化不爆炸
- 收集啟動過程的例外清單

**驗證**：程式能啟動到主畫面，log 有寫，無啟動例外

**🚧 Stop point：跑通後讓使用者確認再進下個階段**

---

## Phase 6：清理未使用套件 / DLL

**做什麼**：
- 移除無人引用的：`NATS.Client`（若 Phase 2 已確認無人用）、`STAN.Client`、`hasp_net_windows`、HASP/apidsp 原生 DLL Content
- 刪 `DLL/` 內歷史孤兒：`Interop.QSACTIVEX*`、`Ionic.Zip`、`Logrila.Logging`、舊版 `NLog*`、`Stateless*`、`20190822_Old/`、`TcpIpTxCrontrol_v0103_*.7z`
- 確認 `GenericParsing.dll` 還有人用才保留

**驗證**：建置仍綠、執行仍 OK

---

## Phase 7：NLog.config 重寫

**做什麼**：
- 套用 [NET8_MIGRATION.md](NET8_MIGRATION.md) §E 的優化版
  - 所有 File target 包 `AsyncWrapper`（batchSize / queueLimit / overflowAction）
  - `keepFileOpen=true`, `concurrentWrites=false`
  - Error/OHTC `autoFlush=true`
  - 修 OHTC 撞檔名、layout exception 重複輸出
  - `${date}` cached
  - 全域 `throwExceptions=false`

**驗證**：執行壓力測試，確認 log 寫得出來、不阻塞主執行緒

---

## Phase 8：程式碼現代化（語法層級）

**範圍**（每項可獨立做，建議分多次 commit）：
- `BeginInvoke`/`EndInvoke` 舊式 async → `async/await`
- 熱路徑 `Thread.Sleep` → `Task.Delay`（呼叫鏈可改 async 的情況下）
- `Hashtable` → `Dictionary<TKey,TValue>`、`ArrayList` → `List<T>`
- 字串拼接迴圈 → `StringBuilder`
- 啟用 `<Nullable>enable</Nullable>`，逐檔修可空問題（大工程，最後做）
- `switch` 改 `switch expression`、合適處用 `record`、pattern matching
- `Newtonsoft.Json` → `System.Text.Json`（若沒自定 converter 包袱）

**驗證**：建置仍綠、所有冒煙測試通過

---

## Phase 9：執行緒與效能熱點

**做什麼**：
- 多執行緒
  - `lock` 熱路徑評估 `ReaderWriterLockSlim` / `SemaphoreSlim`
  - producer/consumer 模式評估 `Channel<T>`
  - `BackgroundWorker` → `Task` + `IProgress<T>`
- gRPC：評估 `Grpc.Core` → `Grpc.Net.Client`（已棄用 → 現代版）
- 記憶體：高頻短命物件 `ArrayPool<T>` / `Span<T>` / `Memory<T>`
- 序列化熱點 → `ReadOnlySpan<byte>` 路徑

**驗證**：profiler 量測前後 CPU / 記憶體 / 延遲

---

## Phase 10：建立新 git repo

**做什麼**：
- `git init`
- 建立 `.gitignore`（標準 VS Code + .NET + bin/obj/packages 排除）
- 第一個 commit：「initial: net8 base」
- 後續每階段成果按主題 commit

---

## 進度紀錄表

| Phase | 狀態 | Commit | 備註 |
|---|---|---|---|
| 1. csproj SDK-style | ⏳ | | |
| 2. PackageReference | ⏳ | | |
| 3. App.config 清理 | ⏳ | | |
| 4. 程式碼必改 | ⏳ | | |
| 5. 建置綠燈 | ⏳ | | |
| 6. 清理未用套件/DLL | ⏳ | | |
| 7. NLog 優化 | ⏳ | | |
| 8. 語法現代化 | ⏳ | | |
| 9. 執行緒/效能熱點 | ⏳ | | |
| 10. 建新 git repo | ⏳ | | |

---

## 給下次 session 接手的提示

如果這份計畫是被新 session 讀到的：
1. 先看 `NET8_MIGRATION.md` 了解上次怎麼升的（細節劇本）
2. 看本檔上方的「進度紀錄表」確認目前到哪
3. 從 ⏳ 的第一個階段繼續
4. 每階段做完更新狀態欄（`⏳` → `✅`），記下 commit hash 與備註
