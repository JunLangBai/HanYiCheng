# ResearchCapture：韩文手写采集场景

## 使用

1. 在 Unity 中双击 `ResearchCapture.unity`，确认层级窗口里**只有** `ResearchCapture` 场景，不要与 `Level1-1` 同时运行。
2. 按 Play；在白色方框内书写，点“完成”保存一张图。点“清除画板”可以重写。空白画板不会保存。
3. 每张图保存后画板自动清空，可继续采集。图片与 `samples.csv` 位于 `Samples/`。

本场景从 `Level1-1` 复制了 Rokid 输入、相机和原有画板/按钮的相对布局，删除了字帖、题目、关卡控制和旧 OCR 逻辑。仅 `ResearchCapture.cs` 控制这一个场景。`Editor/ResearchSceneBuilder.cs` 可通过 Unity 菜单 `Tools > Research > Build Capture Scene` 从原关卡重新生成场景；重新生成会**覆盖本场景**，因此场景有手工改动时不要运行该菜单。

## 图片和标注

- 可见画板：256×256 的白底黑笔。
- 保存文件：64×64 JPEG，黑底亮笔画。RGB 的三个通道数值相同，模型端按单通道解码即可；图像经 4×4 区域平均缩小，不是屏幕截图。
- 文件名使用 UTC 时间和随机后缀，避免重名。`samples.csv` 记录文件名、标签、参与者 ID、时间和格式。
- `groundTruthLabel`、`participantId` 在画板 `RawImage` 上的 `ResearchCapture` 组件中设置。若自由书写而不预先设置标签，CSV 的 `label` 为空；用于准确率评估前必须人工补标，不能把无标签图片当成测试集。

Unity 编辑器中保存到 `Assets/AZ/Scene/research/Samples`。Android APK 内的 `Assets` 不可写，所以 Rokid 设备上保存到 `Application.persistentDataPath/research/Samples`，通常是 `/storage/emulated/0/Android/data/com.DefaultCompany.HanYiCheng/files/research/Samples`（以实际包名为准）。可用 `adb pull` 把设备上的整个 `Samples` 文件夹复制到电脑，再放入此目录。请在构建 Android 包时手动把 `ResearchCapture.unity` 加到 Build Settings；本工作没有修改原应用的 Build Settings。

## 可选服务器测试

在 `ResearchCapture` 组件中开启 `uploadAfterSave`，设置 `serverUrl` 和超时。每次保存后，会以 `multipart/form-data` 的 `image` 字段上传同一张 64×64 JPEG；耗时、HTTP 状态和响应写入 `Samples/server_responses.csv`。若服务器返回 `{"label":"..."}`，场景会显示预测标签。默认关闭上传，不会把采集数据自动发到网络；地址无效也不会妨碍本地保存。服务器地址应指向部署了**最终模型及匹配预处理**的接口。请求与返回格式详见 `SERVER_API.md`。

现有 `Assets/AZ/RecFont/server.py` 是旧接口：它对输入执行反相并使用旧 SavedModel 的 4096 维输入，不能直接视为与这里的黑底亮笔画及后续 Jamo 模型兼容。若要测最终模型的识别率和端到端时延，先更新服务器推理/预处理，再打开上传开关。这里没有自动修改旧服务器，也没有测试远程地址。传输真实参与者笔迹前，确认告知、授权和服务器数据处理方式。

## 验证记录

已在 Unity 2022.3.56f1c1 的编辑器中单独打开并运行场景，核对画板、清除与完成按钮。完成按钮生成的测试 JPEG 已核对为 64×64、黑色背景、亮色笔画；随后已删除该非韩文测试样本，以免混入正式数据。尚未在 Rokid 设备或远程服务器上测试。
