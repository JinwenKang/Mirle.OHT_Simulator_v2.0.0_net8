# .NET 8 升級歷程紀錄

> 對應 commits：`9e6b5fe migrate: 升級至 .NET 8 (net8.0-windows)` + `bfdd455 chore: 清理未使用套件、DLL 與優化 NLog 設定`
> 目標框架：`net461` → `net8.0-windows`
> 紀錄日期：2026-05-15

---

## A. 專案檔（csproj）改造

| 項目 | 從 | 到 |
|---|---|---|
| 專案格式 | legacy（手動列舉 Compile / Reference、`<ToolsVersion>`、`packages.config`） | **SDK-style** `<Project Sdk="Microsoft.NET.Sdk">` |
| TargetFramework | `net461` | **`net8.0-windows`** |
| WinForms 啟用 | 隱含 | `<UseWindowsForms>true</UseWindowsForms>` |
| NuGet 管理 | `packages.config` | `<PackageReference>` |
| `<Compile Include="..." />` | 手動列舉所有 .cs | SDK 自動 glob，全刪 |
| `<None>` / `<Content>` | 大量歷史殘留 | 只留實際需 CopyToOutputDirectory 的（CSV / XML / NLog.config / 7z） |
| 重複 Reference HintPath | 有 | 移除 |

主 csproj 從約 371 行縮到約 190 行。

## B. 套件升級 / 替換

| 原本 | 處理 |
|---|---|
| `NLog` 4.x（DLL/ 內）+ `NLog.Extended` | 改用 **`NLog` 5.3.4 NuGet**，刪除 DLL 版 |
| `System.Configuration.ConfigurationManager` | 新增 NuGet 8.0.0（.NET 8 不再內建 ConfigurationManager） |
| `Elasticsearch.Net` / `NEST` 7.3.0、`Google.Protobuf` 3.25.1、`Grpc.Core(.Api)` 2.46.6 | 維持版本，改用 PackageReference |
| `NATS.Client` 0.10.0 / `STAN.Client` 0.2.1 | **整個移除**（全專案無人使用） |
| `Nancy` / `Nancy.Hosting.Self`（舊 packages.config 有） | 確認已無人使用，不引入新格式 |
| `hasp_net_windows.dll` + `hasp_windows_*.dll` + `apidsp_*.dll` | 全移除（HASP 加密狗相關，無程式碼引用） |

## C. App.config 清理（.NET 8 不再支援的區塊）

移除：
- `<startup><supportedRuntime sku=".NETFramework,Version=v4.6.1" />`
- `<runtime><assemblyBinding>` 整段（bindingRedirect 由 SDK 自動處理）
- `<system.web>`（ASP.NET 專用，WinForms 不需要）

保留：
- `<appSettings>` 中程式真正讀取的 key

## D. 程式碼必改項

| 檔案 | 改動 | 原因 |
|---|---|---|
| `MiddleAgent.cs` | 刪 `using System.Runtime.Remoting.Channels;` | .NET Core/5+ 已完全移除 Remoting |
| `Opera.cs` / `Opera_New.cs` | `BinaryFormatter.Serialize/Deserialize` → protobuf `IDeepCloneable<T>.Clone()` | `BinaryFormatter` 在 .NET 5+ 預設禁用、且 CVE 風險高。protobuf message 本身支援 deep clone，是更安全且更快的替代 |
| `Form1.cs` | 刪 unused `using STAN.Client;` | STAN 套件移除後遺留的 dead using |

> 升級時若再遇到的常見 net461 → net8 必改點（可作為後續清單）：
> - `ConfigurationManager.AppSettings` 仍可用，但需 NuGet 套件
> - `WebClient` / `HttpWebRequest` 建議改用 `HttpClient`
> - `XmlSerializer` 的某些 ctor 行為差異（會即時 compile，建議快取 serializer 實例）
> - `Thread.Abort` 不再可用
> - `AppDomain` 多重 domain 模型不支援
> - `System.Web.*`、`System.Runtime.Remoting.*`、`System.Security.Permissions` 大量 API 移除/廢棄
> - 反射 emit / 序列化情境如有 `[Serializable]` 配 `BinaryFormatter`，須改用 JSON / protobuf / MessagePack
> - `Process.Start("url")` 不再會自動以 shell 開啟，需 `new ProcessStartInfo { UseShellExecute = true }`

## E. NLog.config 重寫（升級時順手做的優化）

- 所有 File target 包入 `AsyncWrapper`（`batchSize` 200~500、`queueLimit` 10k~20k）
  - Error / OHTC 用 `overflowAction="Block"`（不丟資料）
  - 一般 Debug log 用 `overflowAction="Discard"`（量爆時優先保護主執行緒）
- `keepFileOpen="true"` + `concurrentWrites="false"` 全面套用
- `Error` / `OHTC` 額外 `autoFlush="true"`（程式 crash 時確保落地）
- 修正 `OHTC` 目標 fileName 撞 `TcpIpTrxState.log` 的 copy-paste bug，獨立輸出到 `OHTC.log`
- 修正 layout 中 `${onexception}` 在 message 前後各出現一次導致例外被印兩次的問題
- `${date}` 計算加 `cached="true"`，高頻 log 不再每筆重算
- 全域 `throwExceptions="false"`、`internalLogLevel="Off"`

## F. DLL/ 資料夾清理

**刪除（無人引用的歷史殘留）：**
- `Interop.QSACTIVEXLib.dll` / `Interop.QSXACTIVEXLib.dll`
- `Ionic.Zip.dll`
- `Logrila.Logging.dll`
- `NLog.dll` / `NLog.xml` / `NLog.Extended.dll` / `NLog.Extended.xml`（已改用 NuGet 版本）
- `Stateless.dll` / `Stateless.xml`
- `20190822_Old/`（整個舊版備份資料夾）
- `TcpIpTxCrontrol_v0103_20190930.7z`（壓縮包備份）
- `hasp_net_windows.dll`

**保留：**
- `GenericParsing.dll`（Form1.cs 仍使用）
- `SECSTrxControl.dll`
- `TCPIPTrxControl.dll` + `.config` + `.pdb`

## G. 已知建置警告

`CA1416` 平台相容性警告 **共 197 條**，全部是 WinForms API（`MessageBox.Show`、`DataGridView`、`TextBox.Text` 等）在 net8.0-windows 下被分析器標記。

- **不影響執行**（目標框架已限定 Windows）
- 若要清乾淨輸出，可在 csproj 加：
  ```xml
  <NoWarn>$(NoWarn);CA1416</NoWarn>
  ```

---

## 後續優化方向（升級時尚未做、建議下一階段考慮）

1. **語法現代化**
   - `Thread.Sleep` → `Task.Delay`（適用於可 async 的呼叫鏈）
   - 舊式 `delegate` + `BeginInvoke`/`EndInvoke` → `async/await`
   - `Hashtable` / `ArrayList` → `Dictionary<TKey,TValue>` / `List<T>`
   - 字串拼接熱點 → `StringBuilder` / `string.Create` / interpolated string handlers
   - `Newtonsoft.Json` → `System.Text.Json`（若無自定 converter 包袱）

2. **執行緒模型**
   - 多處 `lock` 在熱路徑可評估 `SemaphoreSlim` / `Channel<T>` / `ReaderWriterLockSlim`
   - `BackgroundWorker` → `Task` + `IProgress<T>`

3. **可空性 / 模式比對**
   - 啟用 `<Nullable>enable</Nullable>`（目前為 disable）
   - 善用 `switch expression`、`is` pattern、`record` 型別

4. **建置/部署**
   - 考慮 ReadyToRun / AOT 部分模組
   - `TrimMode` / SingleFile 部署（若可放棄反射熱用案）

5. **記憶體**
   - 高頻短命物件改 `ArrayPool<T>` / `Memory<T>` / `Span<T>`
   - 序列化熱點考慮 `ReadOnlySpan<byte>` 路徑

---

## 重做時的執行順序建議

```
1. 切到目標分支 (e.g. feat/加入OHT...)
2. 改 csproj → SDK-style + net8.0-windows + UseWindowsForms
3. 移除 packages.config、舊 App.config 區塊
4. dotnet restore，逐個套件改 PackageReference
5. 處理編譯錯誤（Remoting、BinaryFormatter、System.Web 等）
6. 驗證建置通過 → 跑一輪基本功能
7. 套用 NLog 優化（獨立 commit）
8. 清理 DLL/ 資料夾與孤兒 Content（獨立 commit）
9. （選擇性）開始語法現代化階段
```
