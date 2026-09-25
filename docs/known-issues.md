# 已知问题与待办

开发过程中记录的代码问题与优化建议，按影响程度排序。**均尚未修复**，供后续迭代参考。

## P1 · 影响构建或可用性

### 1. Android 打包可能被阻断

`Assets/AQY/Scripts/Rokid按键检测/PanalExit.cs` 第 1 行裸写了 `using UnityEditor;`，只有 `EditorApplication` 的调用被包在 `#if UNITY_EDITOR` 里。该脚本已在 `MainUI` 场景中被引用，而打包到 Android 时 `UnityEditor` 命名空间不可用，会直接编译失败。

**建议**：把 `using UnityEditor;` 一并放进条件编译块。

### 2. 手写识别强依赖远程服务

第一章的识别完全依赖外部服务，且：

- 模型文件（`Assets/StreamingAssets/model.tflite`、TensorFlow `saved_model`）已从仓库删除，端侧离线推理不可用；
- 服务地址与端口硬编码在客户端（`KoreanOCRClient.serverURL`），换环境需要逐场景修改；
- 服务端默认监听端口与客户端请求端口不一致，实际部署时依赖额外的反向代理配置；
- 断网、服务不可达或超时（15 秒）时，第一章无法继续。

**建议**：把模型与标签放回 `StreamingAssets`，恢复 `HangulInference` 端侧推理作为主路径，远程服务改为可选回退；同时把服务地址抽成配置文件，并在识别失败时给出可重试的界面提示。

## P2 · 逻辑与数据

### 3. 关卡判分恒为通过

`Assets/AZ/Scripts/试题系统/Scripts/完成关卡/FinishLevel.cs` 中 `UIManager.Instance.GetScore() >= 0f` 永远成立，等于只要点击"完成"就会解锁下一关。`OCRFinishLevel`、`MatchingFinishLevel` 也有同类的宽松判断。

**建议**：明确通关条件（例如正确率阈值），或显式改为「答完即通关」并去掉多余判断。

### 4. 进度字段语义复用

`LevelDataJson.ISUnlockedByDefault` 同时承担"初始解锁"和"已通关"两种含义；`LevelSelectMngr.SaveProgress` 会把当前章节的全部关卡状态整体写回存档，跨章节的进度语义容易混淆。

**建议**：拆分为 `unlocked` / `cleared` 两个字段，并在存档里保存 `bestScore`。

## P3 · 可维护性

### 5. 第一章场景重复

`Level1-1 ~ Level1-13` 是 13 个近乎相同的场景副本 + 13 份数据资源，任何 UI 或流程改动都需要复制 13 次。

**建议**：改为单场景 + 通过 `LevelData` 指定题库，或引入地址化的题目资源加载。

### 6. 死代码与调试代码

- `Assets/AQY/Scripts/Rokid按键检测/RKButtonCheck.cs`：整段注释；
- `Assets/AQY/UIClickDebugger.cs`：会给所有 `Selectable` 覆盖一层半透明红色调试图；
- `Assets/AQY/TestFPS.cs` 与 `Assets/AZ/Scripts/NewLogic.cs`：两个脚本都实现了 FPS 显示（后者文件名为 `NewLogic.cs`，类名却是 `FPS`）；
- `BackupUnusedAssets/`：两个历史 unitypackage 备份，约 120 MB。

### 7. 对话资源复用

`ReadAfter`（跟读）与 `PlacementUI`（摸底）共用同一套 `Assets/AZ/对话文本/test1 ~ test9.asset`，跟读场景内容与摸底问答重复。

**建议**：为跟读场景拆分独立的对话资源。

### 8. 开发存档入库

`Assets/SaveData/GameData.json` 携带开发期进度进入仓库（`.gitignore` 只忽略了仓库根目录的 `/SaveData`），会改变首次运行时的流程表现。

**建议**：清理该文件或调整忽略规则，让新克隆的仓库从完整的新手流程开始。

## 其它可优化项

- 根目录的历史 / 生成文件：`hanyichengnb.txt`（空文件）、`inference_log.txt`、`copy.jpg`、`referencemap.xml`，可考虑清理或归入 `.gitignore`；
- 中文目录名（点读机 / 试题系统 / 连连看 / 画画试题 / 对话）在部分工具链下显示为转义字符，跨平台协作时可评估是否统一改为英文目录；
- 部分脚本文件名与类名不一致（`TensorFlowLiteInfeerence.cs` → `HangulInference`、`PythonServerLauncher.cs` → `PythonScriptRunner`），建议统一以便检索。
