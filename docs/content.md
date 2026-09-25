# 内容与资源清单

教学内容的组织方式、题库与关卡的对应关系，以及音频、字体、模型等资源的来源。

## 题库与关卡对应

| 关卡场景 | 玩法 | 题目资源 | 说明 |
| --- | --- | --- | --- |
| `Level1-1 ~ Level1-13` | 手写识别 | `Assets/AZ/Scripts/画画试题/SO/1-1 ~ 1-13.asset` | 第一章：元音、辅音、双元音的书写规则与临摹 |
| `Level2-1 ~ Level2-6` | 选择题 | `Assets/AZ/Scripts/试题系统/Scripts/1章/2-1 ~ 2-6.asset` | 第二章：字母辨析、发音与含义 |
| `Level2-7` | 选择题 | `.../1章/2-777.asset` | 测试关，题量更大，用于串联多个知识点 |
| `Level3-1 ~ Level3-9` | 连连看 | `Assets/AZ/Scripts/连连看/3-1 ~ 3-9.asset` | 第三章：字母与发音 / 含义配对 |
| `元` / `辅` 教学关 | 选择题 | `Assets/AZ/paste level/1-61.asset`、`1-71.asset` | 点读机模块中的元音、辅音专项小测 |

章节与关卡的组织数据结构：

- 章节：`Assets/AZ/LevelSection/AreaSos/Area1 ~ Area3.asset`（`AreaData`）
- 关卡：`Assets/AZ/LevelSection/LevelSos/A1 ~ A3/*.asset`（`LevelData`）

> 注：`Area2` 下的关卡资源文件名为 `Level1 ~ Level6`，但内部 `LevelID` 为 `2-1 ~ 2-6`，与场景名保持一致。

## 音频（300+ 个片段）

| 目录 | 内容 |
| --- | --- |
| `Assets/AZ/Kralphabet/字/` | 单个字母的卡片读音（按 `ㄱ`、`ㄴ` … 分文件夹） |
| `Assets/AZ/Kralphabet/单元音`、`双元音`、`基础辅音`、`辅音之紧音`、`辅音之送气音` | 韩语 40 音分类发音 |
| `Assets/AZ/Kralphabet/收音/` | 收音发音（`g` / `d` / `n` / `b` / `m` / `ng` / `r`） |
| `Assets/AZ/Kralphabet/词语/` | 例词发音（100+ 条） |
| `Assets/AZ/Kralphabet/对话/` | 对话语句发音 |
| `Assets/AZ/KrVoice/韩语40音打包/` | 韩语 40 音成套发音 |
| `Assets/AQY/Voice/教程/`（含 `新版本` 子目录） | 中文新手教程配音，文件名即台词 |
| `Assets/AQY/Scripts/音量/音频文件/摸底/` | 摸底问答的中文语音 |
| `Assets/AQY/Voice/短铃声_*.mp3`、`Assets/AQY/Scripts/音量/鼠标点击_*.wav` | 按钮点击与提示音（来自耳聆网） |

## 字体

| 字体 | 用途 | 说明 |
| --- | --- | --- |
| `Assets/AZ/Font/MSYHBD SDF.asset` | 中文界面与讲解主字体 | 约 37 MB，字符集见同目录 `7000汉字+符号+英文字符集.txt` |
| `Assets/AZ/Font/NanumMyeongjoBold SDF.asset` | 韩文字形显示 | 约 35 MB，源文件为 `NanumMyeongjoBold.otf` |

## 模型与美术

| 类别 | 位置 | 说明 |
| --- | --- | --- |
| 题目 3D 模型 | `Assets/AQY/Prefabs/` | 苹果、香蕉、葡萄、西瓜、拉面、拌饭、自行车、飞机、学校、银行、公园、医院等，供第二章选择题的 `Question.model` 引用 |
| 校园道具 | `Assets/school/props/` | 课桌椅、黑板、储物柜、音箱、电脑等 FBX |
| 其它场景模型 | `Assets/Park/`、`Assets/Models/`、`Assets/POLYGON city pack/`、`Assets/Simple Vehicle Pack/` | 场景与演示用模型 |
| UI 素材 | `Assets/AQY/Art/`、`Assets/AZ/Sprites/` | 标题、背景、图标、头像、手势示意、韩易成立绘、对话框 |
| UI 组件包 | `Assets/Dark UI/`、`Assets/UIBUTTON/`、`Assets/UIAcceptSlider/`、`Assets/Layer Lab/` | 界面皮肤与控件 |
| 角色动画 | `Assets/unity-chan!/`、`Assets/Quirky Series Ultimate/`、`Assets/Quirky Series - Birds Bundle/` | Unity-chan 模型与表情控制、主界面小鸟 |
| 特效 | `Assets/Super Confetti FX/`、`Assets/AQY/Prefabs/粒子效果.prefab`、`球状粒子.prefab`、`立柱烟花.prefab` | 答对粒子、按钮与提示特效 |
| 食物与道具包 | `Assets/Japanese food_pack_02_Free/`、`Assets/GASTRO_Sushi_Food_Pack_FREE/`、`Assets/FREE Food Pack/`、`Assets/Office Supplies Low Poly/`、`Assets/ThreeBox/` | 题目模型来源 |

## 特效与声音之外的小资源

- 头像：`Assets/AQY/Art/头像/`（韩易成、猫、熊、刺猬四款）；
- 对话文本：`Assets/AZ/对话文本/test1 ~ test9.asset`、`tutorial1 ~ tutorial4.asset`（`ChatText` 资源，含文本、是否有选项、按钮文案与音频）；
- 主界面随机文案：`BirdChat` 资源（`Create > 主界面 > 韩易成文本`）。
