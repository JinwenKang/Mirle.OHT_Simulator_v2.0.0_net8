# Code Review — .NET 8 升級後

> 日期：2026-05-15
> 範圍：feat/加入OHT 整理後種子 → 升級至 net8.0-windows 完成
> 建置狀態：Debug / Release 皆 **0 errors / 0 warnings** ✅（TreatWarningsAsErrors 啟用）

---

## ✅ Code Review 後續修正紀錄（追記）

依本報告建議，已於後續 commits 處理以下項目：

| 編號 | 項目 | 處理 |
|---|---|---|
| Critical #1 | VehicleMessageSend 同步等待 async | 加入 `ConfigureAwait(false)`、`doWork` 改 async Task、`Task.Wait()` → `await ... .ConfigureAwait(false)` |
| Critical #2 | `[Serializable]` 殘留 | 移除 `LoadunloadCmd` / `XmlHandler` 上不必要的 attribute |
| Critical #3 | Opera / Opera_New 並存 | 已合併為單一 `Opera` 類別 |
| Medium #11 | CommonMessage 升 net8.0 | 已升（原 netstandard2.0） |
| Medium #10 | swallowed catch (Exception ex) | `MCSManagerAction.cs` 6 處 + `VehicleMessageSend.cs` 1 處改為 `logger.Error(ex)` |
| Low #13 | 18 個建置警告 | 全部清掉，並啟用 `TreatWarningsAsErrors` |
| Low #15 | AssemblyInfo.cs | 移除實體檔，中繼資料收進 csproj（`AssemblyTitle/Product/Company/Copyright/Version`） |

### 仍延後的項目（需獨立 sprint + runtime 驗證）

| 編號 | 項目 | 為什麼延後 |
|---|---|---|
| High #4 | Form1.cs 1132 行 拆 ViewModel/Service | 需先設計 MVP 架構與 UI 事件流，非單次重構可完成 |
| High #5 | 392 處 `Thread.Sleep` / `SpinWait` 改 async | 每個 sleep 都關聯模擬時序，需逐一審視與 runtime 驗證；建議獨立分支與 A/B 測試 |
| High #7 | SCApplication.cs 1970 行 上帝物件拆解 | 同上，需先盤點職責邊界 |
| Medium #8 | DI 化 logger / config | 全專案改成 ctor 注入屬於架構級別改動，建議與 Form1 拆解一起做 |
| Medium #9 | 強型別設定載入 | 不影響執行，可隨業務需求逐步引入 |
| Medium #12 | GenericParsing → NuGet | 需確認 NuGet 上的版本 API 與內部 DLL 是否相容 |

### ❌ 已確認不適用（不要再提）

| 項目 | 不做的原因 |
|---|---|
| gRPC channel 收斂（多 vehicle 共用一條 HTTP/2 連線）| **後端靠 port 號識別車號**，每台 vehicle 必須各自獨立連線；共用會直接違反後端協議。永久保持 per-vehicle channel。|
| Medium #10（剩餘） | 其餘 catch (Exception ex) 處理 | 多數已有 `logger.Error` 或具備吞例外的設計意圖，需個別評估 |

---

## 一、升級結果總覽

| 項目 | 狀態 |
|---|---|
| 目標框架 | `net8.0-windows` ✅ |
| 專案格式 | SDK-style ✅ |
| 套件管理 | PackageReference（無 packages.config）✅ |
| BinaryFormatter | 已替換為 protobuf `IMessage<T>.Clone()` ✅ |
| Remoting / System.Web | 已移除 ✅ |
| NLog | 5.3.4 NuGet + AsyncWrapper 全套優化 ✅ |
| App.config 殘留 net48 區塊 | 已清理 ✅ |
| DLL/ 孤兒檔 | 已清理 ✅ |
| git repo | 已初始化於 `main` 分支 ✅ |

---

## 二、Critical（必須儘快處理）

### 1. `Service/VehicleMessageSend.cs` 同步等待 async（5 處）

[`VehicleMessageSend.cs:221`](Mirle.OHT.Simulator/Service/VehicleMessageSend.cs#L221)、[`:247`](Mirle.OHT.Simulator/Service/VehicleMessageSend.cs#L247)、[`:273`](Mirle.OHT.Simulator/Service/VehicleMessageSend.cs#L273)、[`:299`](Mirle.OHT.Simulator/Service/VehicleMessageSend.cs#L299)、[`:332`](Mirle.OHT.Simulator/Service/VehicleMessageSend.cs#L332) 全部用 `.GetAwaiter().GetResult()`，[`:419`](Mirle.OHT.Simulator/Service/VehicleMessageSend.cs#L419) 用 `.Wait()`。

- **風險**：在 UI / WinForms 同步上下文呼叫會 **deadlock**；即使非 UI thread，也佔住 ThreadPool 一條 worker、抵消 async 好處。
- **建議**：把這些方法本身改成 `async Task<T>`，呼叫端一路 `await` 上去。或改用 `Task.Run(() => …).Wait(timeout)` 並設明確 timeout。

### 2. 多個 `[Serializable]` 類別 ([`LoadunloadCmd.cs:10`](Mirle.OHT.Simulator/LoadunloadCmd.cs#L10)、[`XmlHandler.cs:11`](Mirle.OHT.Simulator/XmlHandler.cs#L11))

- **狀況**：`[Serializable]` 屬性本身無害，但若有人未來再用 `BinaryFormatter` 序列化它們，會踩到 .NET 8 預設禁用的雷。
- **建議**：若無 SOAP/Binary 序列化需求，移除屬性。若有跨進程資料交換需求，改用 protobuf / `System.Text.Json`。

### 3. `Opera.cs` (3117 行) / `Opera_New.cs` (3631 行) 為 God Class

- 兩個檔案幾乎重複（功能近似的 Deep_Clone 各一份、ID_xx 回傳處理橫跨整個檔案）。
- **建議**：依職責拆解
  - 訊息序號處理 (ID_36/37/41/49/138/144) 各自獨立成 Handler class
  - Common Deep_Clone 抽到 `Util/ProtoExtensions.cs`
  - 評估能否消除 `Opera` vs `Opera_New` 並存（看哪一個是現役、廢掉另一個）

---

## 三、High（這版可推遲，下版必處理）

### 4. Form1.cs 1132 行 — UI 與業務邏輯混雜

- WinForms partial Form 直接內含 SECS、gRPC、命令分派等業務流程。
- **建議**：抽 ViewModel / Service 層（MVP 或 簡化 MVVM）。

### 5. 392 處 `Thread.Sleep` / `SpinWait.SpinUntil` / `BeginInvoke`

- Opera_New.cs 占 200 處、Opera.cs 占 167 處。
- **風險**：模擬器靠 sleep 控制步進時間，把整個 ThreadPool 卡住。
- **建議**：
  - 步進迴圈改 `await Task.Delay(...)` + `CancellationToken`
  - `BeginInvoke` 改 `Invoke` async lambda 或直接 `Task`
  - 列出所有 `Thread.Sleep` 改造作戰計畫（建議獨立分支執行）

### 6. NLog 同步 logger 在熱路徑

- 雖然 [`NLog.config`](Mirle.OHT.Simulator/NLog.config) 已加 AsyncWrapper（背景刷盤），但程式碼裡若同一條 thread 大量呼叫 `logger.Info()`，仍會排隊在 queue。
- **建議**：熱路徑 log 用 `IsInfoEnabled` 條件包起來，避免昂貴的 string interpolation 在禁用 level 時還做。

### 7. SCApplication.cs 1970 行 — 上帝物件

- 同時是 DI 容器、設定 loader、SECS test driver、命令送出器。
- **建議**：拆出 `SecsTestRunner`、`CommandSender`、`SystemConfig`。

---

## 四、Medium（架構優化方向）

### 8. `using NLog` 直接散布在 16 個檔案

- 每個檔案自建 `LogManager.GetCurrentClassLogger()`。
- **建議**：透過 DI 注入 `ILogger`/`NLog.ILogger`，或在 `Tools/Logger.cs` 包一層 facade。

### 9. 設定讀取分散

- `App.config` `<appSettings>` 透過 `ConfigurationManager.AppSettings["key"]` 在多處重複呼叫。
- **建議**：集中至 `MiddlerConfigs.cs` / `SystemParameter` 一次性載入，並改用強型別。

### 10. 大量 `catch (Exception ex) { }` 與 `catch (Exception ex) { string err = ex.Message + ... }`

- 163 處 `catch (Exception ex)`。許多吞掉例外只記成本地字串，丟失 stack。
- **建議**：
  - 全部改成 `nlogger.Error(ex, "context")` 統一處理
  - 用 analyzer 規則禁止「catch 後不重拋且不 log」

### 11. CommonMessage 仍是 `netstandard2.0`

- 主專案已 net8.0-windows，跨平台共用需求若無，可拉齊到 net8.0（純函式庫，不含 windows API 即可）。
- **建議**：評估同步升 net8.0；如未來想加 `record` / `Span<T>` 序列化更方便。

### 12. `Reference Include="GenericParsing"` 仍是本地 DLL

- DLL 來源不明、版本久遠。
- **建議**：找原始套件（NuGet 上有 `GenericParser`），或維護內部 fork 並改 PackageReference。

---

## 五、Low（清理 / 整潔）

### 13. 18 個建置警告全是 CS0168 / CS4014 / CS0067 / CS0169 / CS0414

- 都是「宣告未用」「事件未訂閱」「async 未 await」類，無功能影響。
- 建議下個 sprint 一輪掃乾淨，並開 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`。

### 14. `Mirle.OHT.Simulator.sln` 與根 sln 重複殘留

- 主 sln 在根目錄，子專案內若還有 `Mirle.OHT.Simulator.sln` 屬歷史殘留，已於 Phase 6 移除。確認 IDE 開啟時用根 sln。

### 15. `AssemblyInfo.cs` 仍是手動維護

- SDK-style 預設自動產生，但目前用 `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` 保留檔案。
- **建議**：保留即可（中文 Company / Description 不易在 csproj 表達），或移到 `<AssemblyAttribute>` props。

---

## 六、Performance 觀察點（未量測）

未來想做 profiling 時，優先看這幾處：

| 位置 | 為什麼值得看 |
|---|---|
| `Opera_New.cs` 步進主迴圈 (200+ Thread.Sleep) | 模擬車輛移動的時間步進 |
| `VehicleMessageSend.cs` 同步等待 async | ThreadPool 飢餓風險最高 |
| `MiddleAgent.cs` Timer + RegularReport | 高頻定時上報，gRPC 連線狀態 |
| `Form1.cs` UI Invoke 與 DataGridView 更新 | 大量 UI 元件刷新 |
| NLog queue 在極端流量下 | overflowAction 行為與 batchSize 調參 |

建議使用：
- **dotMemory / dotTrace** 量測 GC / CPU
- **PerfView** 看 ThreadPool 是否飢餓
- **BenchmarkDotNet** 對 hot path（命令解析、protobuf clone）做微 benchmark

---

## 七、Security / 維運

| 項目 | 狀況 |
|---|---|
| `BinaryFormatter` CVE | 已移除 ✅ |
| `System.Configuration` 明文 IP / Port | 預期行為（內網設備），無 hardcoded 憑證 |
| App.config 留有 `192.168.170.80` / `127.0.0.1` | 內網模擬器，OK；上版前確認非生產位址 |
| gRPC Plaintext / TLS | 程式以 `Grpc.Core` 預設，內網用無 TLS 即可，注意未來跨網段需加密 |
| 內含原生 SECSTrxControl.dll / TCPIPTrxControl.dll | 來源已知，仍建議將 DLL 簽章 / hash 記錄到版本控管 metadata |

---

## 八、下一步建議排程

| 優先 | 任務 | 預估工時 |
|---|---|---|
| P0 | VehicleMessageSend 同步 async 改造（Critical #1） | 0.5d |
| P0 | Opera / Opera_New 收斂為單一 class（Critical #3） | 2d |
| P1 | Thread.Sleep → Task.Delay 戰線（High #5） | 3~5d，需獨立分支 |
| P1 | 18 個警告全清 + TreatWarningsAsErrors（Low #13） | 0.5d |
| P2 | SCApplication.cs / Form1.cs 拆解（High #4、#7） | 5d 起跳 |
| P2 | 強型別設定載入（Medium #9） | 1d |
| P3 | DI 化 logger / config（Medium #8、#9） | 2d |

---

## 九、最終建置驗證

```
> dotnet build Mirle.OHT_Simulator_v2.0.0.sln -c Release -nologo
    0 個錯誤
    18 個警告（皆為 unused variable / missing await，無 functional 風險）
經過時間 00:00:12.69
```

可執行檔產出於：`Mirle.OHT.Simulator\bin\Release\net8.0-windows\Mirle.Agvc.Simulator.exe`

> ⚠ 由於本流程在無 GUI 環境執行，**尚未實際啟動 WinForms 主畫面驗證執行期行為**。
> 上線前請務必：
> 1. 在 Windows 桌面執行 exe，確認主畫面開啟、無啟動例外
> 2. 跑一輪命令模擬（自動命令 / SECS / gRPC 連線）
> 3. 確認 `D:\OHTC_Sim_Log\` 有寫出新格式 log
> 4. 跟舊版做 A/B 比對至少 30 分鐘穩態觀察
