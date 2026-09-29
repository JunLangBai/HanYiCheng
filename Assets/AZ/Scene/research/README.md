# ResearchCapture：韩文手写采集场景

## 使用

1. 在 Unity 中双击 `ResearchCapture.unity`，确认层级窗口里**只有** `ResearchCapture` 场景，不要与 `Level1-1` 同时运行。
2. 按 Play；右侧会出现“请写下面的韩文字”、大号目标字、全字表序号、模型类别ID及采集列表位置。默认从 `가`（第1字，ID 0）开始。
3. 先点画板上方的“新参与者”，确认后自动分配P001、P002……。取消不会创建编号。同一人继续采集不要重复新建；应用重启后，可点“继续上次”恢复最近登记的编号。
4. 按提示在左侧白色方框内书写，点“完成”保存。点“清除画板”可以重写。未选择有效参与者或画板空白时不会保存。
5. 默认保存后清空画板，但仍提示同一个字，适合重复采集。点“上一字”或“下一字”切换，也可在“跳到全字表序号（1–2350）”输入数字，按回车或点“跳转”。画板有未清除的笔画时，不允许换字或换参与者，以免把旧笔迹标给新目标/新人。
6. 新图片、`samples_guided_v2.csv` 与参与者登记表 `participants.csv` 位于 `Samples/`。旧图片、旧 `samples.csv` 不覆盖，也不向旧CSV混写新列。

本场景从 `Level1-1` 复制了 Rokid 输入、相机和原有画板/按钮的相对布局，删除了字帖、题目、关卡控制和旧 OCR 逻辑。仅 `ResearchCapture.cs` 控制这一个场景。`Editor/ResearchSceneBuilder.cs` 可通过 Unity 菜单 `Tools > Research > Build Capture Scene` 从原关卡重新生成场景；重新生成会**覆盖本场景**，因此场景有手工改动时不要运行该菜单。

已有场景无需重新生成。提示和切换按钮在运行时创建，停止Play后不保留这些临时UI对象，这是正常现象。提示区位于画板之外；保存仍然只读取256×256画板的像素，不会把目标字、ID或按钮保存进识别图片。

## 用键盘跳到指定字（2026-09-29）

换人时先保存或清空画板，点“新参与者”并确认，再点右侧数字框输入 `201`，按回车或点“跳转”，即可从全字表的第201个字开始。下一位可输入 `401`；输入 `1` 返回第一个字，输入 `2350` 到最后一个字。点击输入框会选中原数字，可直接键入替换。

输入值采用从1开始的全字表序号，CSV和模型仍采用从0开始的原始ID：序号201对应ID 200，序号2211对应 `한` / ID 2210。界面会同时显示序号和ID，不能把输入数字直接当成CSV里的class_id。`采集列表 n / m` 是当前采集计划内的位置，和全字表序号不同。

空输入、非整数、越界值会提示错误并保留原目标字；画板有笔迹或新参与者确认框打开时不能跳转。配置了Collection Characters子集时，只允许跳到该子集内的字。跳转不修改已保存图片、历史CSV或参与者计数；新建参与者仍回到配置的起始字，随后可按本人的任务跳转。当前功能不自动分配每人200字，也不在第200张自动停止。

如需每次点“完成”后自动提示下一字，停止Play后在RawImage的ResearchCapture组件勾选Advance After Save，保持Clear After Save勾选并保存场景。电脑上用键盘；安卓请求数字软键盘，也可使用设备支持的外接键盘。代码为Rokid 3.0.3的PointableCanvasModule补充了该输入框所需的键盘更新事件；实际软键盘显示和Rokid输入仍须实机确认。

## 不认识ID也能设置采集字表

停止Play，选中Hierarchy内的 `RawImage`，在 `ResearchCapture` 组件设置：

| Inspector字段 | 设置方式 |
| --- | --- |
| Guided Collection | 保持勾选，启用逐字提示 |
| Label File | 已绑定本目录ResearchLabels.txt，不要换成旧的2351项标签表 |
| Prompt Font | 已绑定项目中覆盖全部2350字的NanumMyeongjoBold SDF字体 |
| Collection Characters | 留空采集全部2350类；也可直接输入 `가 나 다 한 글`，或连续写 `가나다한글` |
| Start Character | 留空从采集列表第一个字开始；填 `한` 就从这个字开始，须包含在采集列表中 |
| Participant Id | 通常保持anonymous，由运行中的“新参与者”入口分配；也可在Play前手动指定已有编号，例如P003 |
| Session Id | 手动指定编号时使用的会话编号，例如S02；按钮创建新人时自动设为S01 |
| Advance After Save | 默认不勾选；勾选后，成功保存并清空会自动切换下一个字 |
| Clear After Save | 默认勾选；若关闭，需手动清除后才能换字，自动下一字也不会执行 |

例如只想采集 `한`，把Collection Characters填成 `한` 即可；不需要知道它的ID。程序仍保存训练模型的真实ID 2210，而不是把这个子列表重新编号为0。提示中的 `1 / 5` 是采集列表位置，`ID 2210` 才是模型类别ID。

空格、英文/中文逗号、分号和顿号可以分隔字符；重复字符只保留首次出现，重复样本通过多次书写保存取得。字表不包含某个字、字体无法显示或标签顺序改变时，会禁用保存并在Console解释原因，不会静默生成错标签。配置修改后重新进入Play生效。

ResearchLabels.txt由模型训练目录中的labels/2350-common-hangul.txt复制，共2350类，ID为0—2349。ResearchLabelCatalog.cs校验类别数、重复项及顺序哈希。不要排序、追加 `<rare>` 或手改该文件。模型若以后更换词表，必须同步更换模型和映射校验，不能只改显示文字。

## 一台AR设备采集多个人

无需输入姓名或弹出安卓键盘，直接点击场景里的入口：

1. 第一个人：点击“新参与者” → 核对确认框 → “确认新建”，获得P001 / S01；随后按提示书写。
2. 换第二个人：先保存上一人的笔迹（或清除不要的笔迹），再点“新参与者”并确认，获得P002 / S01。提示字回到配置的起始字，本人的保存张数从0开始。
3. 同一个人接着写：不用再点“新参与者”，直接继续。误点入口可“取消”；在确认框中不能操作背后的画板/保存/切字。
4. 重启应用：默认不猜测戴上设备的是谁。核对确实还是最近登记的那个人，再点“继续上次”；若换了一个新的人，则新建下一编号。没有历史记录时，“继续上次”不可点击。
5. 若要回到更早的某位参与者，当前这一版没有全部人员选择列表；可在Unity运行前设置已有Participant Id。不要用“新参与者”冒充恢复旧人。需要新批次时，在运行前设置原编号和新的Session Id。

画板上方持续显示当前编号/批次、本参与者已保存张数、本机已有样本的参与者人数。“人数”只统计清单里有有效编号且图片仍存在的人，不把新建但没写字的人算进去；同一个人保存多张仍只算一人。原来的anonymous图片保留，但不据此猜测人数或归入某位新人。取消创建也不增加人数。

`ResearchParticipantRegistry.cs`读取已有participants.csv、旧samples.csv和samples_guided_v2.csv，跳过已占用编号。创建时立即登记，即使尚未保存图片，重启后也不会重复使用该编号。登记表损坏或存储不可写时会停用保存并报告错误，避免静默混用编号。只有实际保存了图片及标注后，界面张数才增加。

请转移或备份**整个Samples文件夹**，包括participants.csv、所有图片和清单。不要只复制图片，也不要删除登记表来“重置人数”。编号唯一性仅针对当前设备保留的这套本地记录，不是跨设备统一身份；清除应用数据、卸载后重装或只拷贝部分文件可能丢失登记信息，多设备合并时需要另行区分设备并核对人员。操作者仍须确保每个真实参与者只分配一个编号；系统不会通过笔迹识别身份。

## 图片和标注

- 可见画板：256×256 的白底黑笔。
- 保存文件：64×64 JPEG，黑底亮笔画。RGB 的三个通道数值相同，模型端按单通道解码即可；图像经 4×4 区域平均缩小，不是屏幕截图。
- 文件名形如 `hangul_2210_20260927T080000000Z_a1b2c3d4.jpg`；2210是提示字的模型ID，UTC时间和随机后缀避免重名。所有图片仍放在Samples中，不移动旧文件。
- 新清单为 `samples_guided_v2.csv`，UTF-8带BOM，便于Excel显示韩文。它记录target_label、target_class_id、verified_label、verified_class_id、label_status、participant_id、session_id及图像信息、标签表哈希。
- 自动保存的是提示目标，例如target_label=`한`、target_class_id=`2210`。verified_label与verified_class_id留空，label_status=`pending_review`。人工核对实际写出的字后再填写真值及其模型ID，不能直接把服务器预测结果抄入真值。
- 当Guided Collection关闭时，兼容旧的groundTruthLabel字段作为自由书写目标；留空时文件名标为unlabeled，对应记录的label_status也为unlabeled。它不构成可直接计算准确率的标注数据。
- 新CSV如果已有不匹配的表头，会拒绝追加；旧数据不会自动转换或覆盖。只追加完成的新记录，不因切换目标改写历史记录。

### 提示字与模型预测保存在什么地方

当前为两张关联表：`samples_guided_v2.csv` 的 `target_label` / `target_class_id` 保存屏幕提示写的字；`server_responses.csv` 的 `response` 保存服务器原始JSON，其中 `label` / `class_id` 是模型预测。后者还记录 `elapsed_ms`、`http_code` 和 `error`。两表通过完全相同的 `filename` 关联，不能按行号对齐，因为未上传或中断的样本可能没有响应记录。

保存时会先固定这张图片的目标字和参与者，再清空画板、切换目标和上传。因此即使开启自动下一字或上传期间跳转，之前那张图片的标注和响应关联仍由原文件名保持。断网、请求失败、仅本地保存时，不保证有有效预测；解析时须检查HTTP状态、错误及返回标签表哈希。`verified_label` / `verified_class_id` 留给人工核验实际笔迹，模型预测不能代填为真值。

Unity 编辑器中，保存目录为 `ResearchCapture.cs` 所在文件夹下的 `Samples`。例如整个research位于 `Assets/AQY/Scene/research` 时，图片、参与者登记、样本清单和服务器响应都写入 `Assets/AQY/Scene/research/Samples`；不再依赖AZ、盘符或电脑用户名。进入Play时Console会打印实际目录，菜单 `Tools > Research > Open Samples Folder` 可直接打开它。

Android APK 内的 `Assets` 不可写，所以 Rokid 设备继续使用 `Application.persistentDataPath/research/Samples`，通常是 `/storage/emulated/0/Android/data/com.DefaultCompany.HanYiCheng/files/research/Samples`（以实际包名为准）。不改变应用标识、不清除应用数据时，更新APK仍能读取原记录。可用 `adb pull` 备份设备上的整个 `Samples`。构建前在Build Settings核对实际移动后的研究场景引用。

## 移动research文件夹和两台电脑协作（2026-09-29）

1. 先停止Play并保存场景，备份整个research。建议在Unity的Project窗口内移动整个research到当前项目Assets下的目标文件夹；不要只移动场景或脚本。移动操作会保留.meta；用资源管理器操作时须同时移动文件、子目录及其.meta。不能移出Assets后仍指望Unity加载这个场景。
2. 连同Samples中的全部图片、participants.csv、samples_guided_v2.csv、server_responses.csv以及旧samples.csv一起移动。旧CSV保存的是图片文件名，不是AZ绝对路径，因此整体移动后仍能匹配原图片。没有数据转换、重新编号或自动合并；已有记录继续读入，新记录继续追加。
3. 等Unity导入编译完成，打开移动后的ResearchCapture.unity。用Open Samples Folder确认新位置，进入Play核对参与者人数；旧人继续采集使用“继续上次”或原编号，不要重新登记。无需执行Build Capture Scene；该菜单会重新生成场景。
4. 同一个Unity项目里只保留一份research脚本。AZ与AQY各复制一套完整C#脚本会导致类型重复，移动支持不等于支持重复安装。两个人在各自电脑的独立项目副本内，可以放到不同位置。
5. 两台电脑各自读取本机research_server.json；路径仍由Tools > Research > Open Server Config Folder打开。这个私密连接配置不是采集输出，不跟随Assets移动；原来的配置无需修改，第二台电脑需要单独配置。不要把Token提交Git。
6. 文件夹可移动不等于Git会自动合并采集记录。两台电脑各自导出完整Samples到不同采集批次目录，例如PC_A/Samples、PC_B/Samples；P001只在各自记录中唯一，汇总时用“批次/设备+参与者编号”区分，并识别共同复制的旧样本，不能直接覆盖同名CSV。若两人把同一文件夹在Git中移动到不同位置，合并时仍可能产生移动冲突，应约定仓库最终目录。

定位路径失败时程序会禁用保存并输出错误，不会偷偷退回旧AZ目录。上述移动应在停止Play时进行；上传请求尚未完成时不要移动目录。

## 可选服务器测试

2026-09-28起，云端配置从 `Application.persistentDataPath/research/research_server.json` 读取；令牌不序列化进场景/APK。没有该私有配置时仍只本地保存，不会上传。在Unity菜单点击 `Tools > Research > Open Server Config Folder` 定位电脑配置目录，放入在自己服务器上生成的research_server.json，重新进入Play。AR设备需要把同名配置文件放入设备的persistentDataPath/research，而不是Assets。配置会覆盖Inspector中的服务器地址、上传开关、超时与HTTP测试开关；只勾Inspector上传开关不能替代令牌文件。

上传是带Bearer令牌的 `multipart/form-data`，只发送同一张64×64 JPEG的image字段，不发送目标字、参与者编号或含标签的文件名。结果仍记录到Samples/server_responses.csv。上传进行中暂时禁用“完成”，避免2GB服务器堆积请求；失败不删除已保存图片、不自动重试。成功响应必须含匹配的label、class_id和labels_sha256，否则显示标签不匹配。HTTP只允许显式开启的临时假数据测试；真实笔迹使用HTTPS且不绕过证书校验。部署与请求细节详见SERVER_API.md及训练项目deployment/tencent-baota/部署使用说明_中文.md。

现有 `Assets/AZ/RecFont/server.py` 是旧接口：它对输入执行反相并使用旧 SavedModel 的 4096 维输入，不能直接视为与这里的黑底亮笔画及后续 Jamo 模型兼容。若要测最终模型的识别率和端到端时延，先更新服务器推理/预处理，再打开上传开关。这里没有自动修改旧服务器，也没有测试远程地址。传输真实参与者笔迹前，确认告知、授权和服务器数据处理方式。

## 原场景验证记录

已在 Unity 2022.3.56f1c1 的编辑器中单独打开并运行场景，核对画板、清除与完成按钮。完成按钮生成的测试 JPEG 已核对为 64×64、黑色背景、亮色笔画；随后已删除该非韩文测试样本，以免混入正式数据。尚未在 Rokid 设备或远程服务器上测试。

## 本次提示功能验证范围

2026-09-27 已通过以下自动验证：

- 16项标签解析/异常配置检查，包括完整2350类、顺序哈希、子集使用全局ID、重复字去重、无效字及错误词表拒绝。
- 使用Unity 2022.3.56f1c1实际程序集完成编辑器和非编辑器分支编译。
- Unity批处理加载本场景，验证场景引用、提示字、提示区不覆盖画板、上一字/下一字、空白不保存、有笔迹禁止换字、保存后留在原字、可选自动下一字和旧CSV不被改写。
- 隔离保存的3张测试JPEG均为64×64、RGB等通道、黑底亮笔画；CSV中的`한 → 2210`、`글 → 152`、`가 → 0`对应正确，人工核验列为空。

测试图片只是程序生成的圆点，不是真实手写，保存在训练项目的`unity-research-prompt/qa-smoke`，没有混入本目录正式`Samples`。本次检查没有操作Rokid设备，也没有进行交互式视觉验收。正式采集前，请在Unity Play与Rokid上检查按钮点击、显示大小及中文/韩文显示。上传仍默认关闭，没有连接远程服务器。

已知非阻断提示：项目现有NanumMyeongjoBold SDF字体含全部2350个目标韩文字，但TextMeshPro初始化时尝试补充省略号，可能提示字体图集不可读；本次测试中目标字存在且流程通过。没有为此修改其他场景共享的字体资源。如果真机出现方框或显示异常，应先检查字体/材质，不要继续正式采集。

## 2026-09-28 新参与者入口验证范围

在不关闭当前Unity编辑器、不运行第二个项目实例的前提下，已通过28项参与者登记逻辑检查，以及16项标签检查；已针对Unity 2022.3实际程序集检查编辑器与非编辑器分支编译。参与者测试覆盖递增编号、重启后的零样本登记、读取不落盘、过期确认拒绝、旧清单兼容、重复样本计数、anonymous排除、缺失图片排除、损坏登记表拒绝和编号溢出等。

批处理验证脚本`Editor/ResearchCaptureValidation.cs`已补充新建确认/取消、未保存笔迹阻止换人、重启恢复、人数/张数和按钮区域检查，但本次未运行完整Unity场景自动测试，不能将上一版本的通过记录视为新增UI的实机验收。停止Play并等待脚本编译后，再次进入Play即可看到入口，无需重新生成或覆盖场景。请先用测试参与者验证按钮、提示、保存和重启，再开始正式采集；Rokid实机和远程服务器未测试。
