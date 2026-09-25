<h1 align="center">
  <img src="Assets/AQY/Art/%E5%9B%BE%E6%A0%87%201.png" width="110" alt="韩易成">
  <br>
  韩易成 · HanYiCheng
</h1>

<p align="center"><b>在 AR 眼镜上跟着「韩易成」学韩语 —— 认字母 · 手写临摹 · 闯关巩固 · 情景对话</b></p>

<p align="center">
  <i>An AR Korean-learning app for Rokid smart glasses. Learn Hangul by pointing, writing and playing.</i>
</p>

<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-2022.3.56f1c1-000000?logo=unity&logoColor=white">
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Rokid%20AR%20%2F%20Android-3ddc84?logo=android&logoColor=white">
  <img alt="XR" src="https://img.shields.io/badge/XR-OpenXR%20%2B%20Rokid%20UXR-6f42c1">
  <img alt="Pipeline" src="https://img.shields.io/badge/URP-14.0.11-1f6feb">
</p>

---

## 📖 这是什么

<img align="right" width="260" alt="韩易成角色立绘" src="Assets/AZ/Sprites/%E9%9F%A9%E6%98%93%E6%88%90.png">

**韩易成**是一款运行在 **Rokid AR 眼镜**上的韩语启蒙教学应用，面向零基础的中文母语学习者。

它把「学会韩文 40 音」这件枯燥的事拆成了三步：先用**点读机**一个个认字母、听发音、看写法；再用**手写、选择题、连连看**三种关卡把知识练熟；最后在**情景对话**里把学到的词句真正用出去。

应用内的讲解与反馈全部是中文语音 + 中文界面，韩文字形使用专门字库，打开就能学，不需要任何韩语基础。

<br clear="all">

## ✨ 亮点

| 亮点 | 说明 |
| --- | --- |
| ✍️ **手写韩字识别判分** | 直接在 AR 场景的画板上书写韩字，识别服务返回结果后自动判分，写对即有粒子特效 |
| 🖐️ **握拳手势控制** | 基于 OpenXR 手部追踪，握拳即可把学习画面拉到眼前，随时把 UI 调整到舒服的位置 |
| 🎧 **全程中文配音** | 新手教程、摸底问答、题目解析都有语音讲解，眼镜里也能听懂 |
| 🎮 **三种题型轮换** | 手写临摹 → 选择题 → 连连看，29 个关卡循序加难（13 / 7 / 9），答错有错题回顾与评级 |
| 📈 **学习进度存档** | 关卡解锁、连续学习天数、音量 / 镜片亮度 / 昵称头像全部持久化保存 |
| 🕶️ **眼镜原生能力** | 直接调用 Rokid 原生接口调节镜片亮度，适配长时间学习 |

<img width="640" alt="握拳手势控制示意" src="Assets/AQY/Art/%E6%89%8B%E5%8A%BF%201.png">

## 🎓 学习路径

| 阶段 | 做什么 | 对应场景 |
| --- | --- | --- |
| 0 · 入门 | 对话式摸底 + 新手教程，了解学习目的与基本操作 | `PlacementUI`、`Tutorial` |
| 1 · 认字母 | 点读机卡片：韩文元音、辅音、收音的写法与发音，点一下就能听 | `ReadAfter`、`Reader1~6`、`元`、`辅` |
| 2 · 练书写 | 第一章：在画板上临摹韩字，由识别服务判分 | `Level1-1 ~ Level1-13` |
| 3 · 练巩固 | 第二章选择题（带图片 / 3D 模型 / 音频），第三章连连看配对 | `Level2-*`、`Level3-*` |
| 4 · 用起来 | 情景对话模拟日常用语 | `MockTalk` |

## 🚀 快速开始

### 环境要求

| 项 | 版本 |
| --- | --- |
| Unity | **2022.3.56f1c1**（需勾选 Android Build Support） |
| 渲染管线 | Universal RP 14.0.11 |
| 构建目标 | Android · ARM64 · IL2CPP · minSdk 28 |
| 运行设备 | Rokid AR 眼镜（OpenXR + Rokid UXR SDK 3.0.3） |

### 三步跑起来

```bash
git clone https://github.com/JunLangBai/HanYiCheng.git
```

1. 用 Unity Hub 以 **2022.3.56f1c1** 打开工程，等待首次导入完成（依赖包较多，耗时稍长）；
2. 打开首场景 `Assets/AZ/Scene/InitialEnter.unity`；
3. 点击 Play —— 编辑器内已配置好 XR 模拟，无需真机即可走完整流程。

> 💡 想直接跳过新手流程，可把 `Assets/SaveData/GameData.json` 中的 `placementClear` / `tutorialClear` 改为 `true`。

### 部署到眼镜

`File > Build Settings` 切换到 Android 构建，产物通过 adb 或 Rokid 开发者工具安装到眼镜即可。

### 试点「手写关卡」需要额外准备

第一章的手写判分依赖一个韩字识别服务（Flask + TensorFlow），**不在本仓库内**。自建与配置方式见 [docs/ocr-service.md](docs/ocr-service.md)。

## 🗺️ 项目结构

```
HanYiCheng/
├─ Assets/
│  ├─ AQY/          # 交互与全局系统：手势、点读机、对话、音量、面板跟随、存档
│  ├─ AZ/           # 教学内容与关卡：字体音频、三大题型、选关、点读机、手写识别
│  ├─ XR/  XRI/     # OpenXR 与 Rokid 运行配置
│  ├─ Plugins/      # Android 端 TFLite 原生库、文字动画插件
│  └─ …             # 第三方美术资源包与 UI 素材
├─ Packages/        # 包依赖（含内嵌的 MediaPipe / TensorFlow Lite 包）
├─ ProjectSettings/ # 47 个构建场景与 Android / XR 设置
└─ docs/            # 设计与实现文档
```

## 📚 文档

| 文档 | 内容 |
| --- | --- |
| [架构与场景流程](docs/architecture.md) | 启动分流、关卡系统、数据流、存档结构、AR 交互与 XR 配置 |
| [手写识别服务](docs/ocr-service.md) | 画板 → 识别 → 判分的完整链路，服务端部署与客户端配置 |
| [内容与资源清单](docs/content.md) | 关卡题库映射、音频、字体、模型与美术资源来源 |
| [已知问题与待办](docs/known-issues.md) | 开发过程中记录的问题清单与优化建议 |

## 🧱 技术栈

`Unity 2022.3` · `URP` · `OpenXR` · `Rokid UXR SDK` · `TensorFlow Lite / MediaPipe` · `TextMeshPro` · `DOTween` · `UniTask` · `Newtonsoft.Json` · `DialogueEditor`

## ⚖️ 素材与许可

- 本仓库**未附带开源许可证**，代码的授权方式尚未确定；如需引用或分发，请先与作者联系。
- 仓库内的第三方素材（Unity Asset Store 资源包、`Assets/AQY/Voice` 下的音效、嵌入字体等）版权归各自作者所有，**仅可在其授权范围内使用，请勿单独转载或再分发**。
- 应用内嵌入了微软雅黑（MSYHBD）与 Naver Nanum Myeongjo 字体，若要商业发布请自行确认字体授权。

## 🙌 致谢

> 待补充：团队成员、指导老师与联系方式。

---

<p align="center"><i>韩易成 · 从 40 音开始学韩语</i></p>
