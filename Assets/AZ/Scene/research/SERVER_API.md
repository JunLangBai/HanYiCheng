# 研究场景 ↔ 云端模型接口约定（待部署）

云服务器尚未配置。本场景的 `ResearchCapture` 组件已留有 `uploadAfterSave`（默认关闭）、`serverUrl`（默认空白）和 `timeoutSeconds`。以后在 Unity Inspector 中选择画板 `RawImage`，填完整的 HTTPS 推理地址（例如 `https://你的域名/predict`），再开启上传即可；无需改变场景的绘制和本地保存逻辑。Android 包仍需重新构建并安装，以带入 Inspector 配置。

## 请求

`POST <serverUrl>`，`multipart/form-data`：

| 字段 | 内容 |
| --- | --- |
| `image` | 采集后已本地保存的同一张 64×64 JPEG；文件名形如 `hangul_20260924T170828979Z_24517967.jpg`，MIME 为 `image/jpeg` |

画板显示为白底黑笔，但发送的图像已经转换成**黑底亮笔画**。JPEG 的 RGB 通道承载同一灰度值；模型端读为 1 通道即可。服务器应按最终训练模型的输入张量和标签映射预处理，不能直接套用旧 `Assets/AZ/RecFont/server.py` 的无条件反相和旧模型输入。不要在服务端把这些图片当作普通白纸黑字再次反相。

## 响应

成功时返回 HTTP 2xx、UTF-8 JSON，例如：

```json
{"label":"가","confidence":0.97,"inference_ms":18.4,"model_version":"jamo-v1"}
```

`label` 为必需字段，Unity 会在场景里显示它。其余字段可选，Unity 会把完整原始响应写入 `Samples/server_responses.csv`，便于论文统计。服务器错误建议返回非 2xx 和 `{"error":"..."}`；Unity 会显示“上传失败”，但已保存的本地图片不会丢失。

`server_responses.csv` 的 `elapsed_ms` 是 Unity 从发送到收到响应的端到端时间，**不等于**纯模型推理时间。若要报告服务端推理时延，应由服务器额外返回 `inference_ms`，并说明测量边界。正式测试时，还应把 `samples.csv` 的人工真值标签与返回的 `label` 对齐。

## 上线前检查

1. 在服务器上部署最终选定的模型及其对应标签顺序，先用已知样本检查输入极性、输出标签和 JSON 格式。
2. 配好 HTTPS、访问控制、日志保留和参与者数据处理规则；不要把私密 API 密钥直接写进 Unity 客户端。
3. 在 Rokid 的 Android 构建中核对网络权限，用一张测试图片验证请求、响应和时延记录。当前仅完成 Unity 编辑器中的本地采集测试，未声称远程推理已验证。
