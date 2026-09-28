# ResearchCapture 云端推理接口（2026-09-28）

服务器包在训练项目的deployment/tencent-baota/server；完整中文说明见deployment/tencent-baota/部署使用说明_中文.md。已准备代码与本地测试，不代表腾讯云服务器已部署。

## 配置

在云端运行setup_server.py生成两个私有文件：server_config.json留在服务器；research_server.json下载到客户端。后者放在Application.persistentDataPath/research/research_server.json，不要放入Assets、StreamingAssets或Git。

电脑：Tools > Research > Open Server Config Folder打开目录，复制文件后重新Play。

Android当前包名com.DefaultCompany.HanYiCheng，通常为/storage/emulated/0/Android/data/com.DefaultCompany.HanYiCheng/files/research/research_server.json。以运行时日志输出路径为准。替换地址/令牌后重启应用，无需为配置单独重打包；首次升级脚本仍需重新构建APK。

配置包含server_url、api_token、upload_after_save、allow_http_for_testing、timeout_seconds。不存在或无效则关闭上传，仅保留本地采集。Bearer令牌应随机生成、可撤销，不能填宝塔密码或腾讯云SecretKey；设备上仍可能被提取，不能视为不可泄露的长期密钥。令牌轮换后更新双方配置并重启服务/客户端。

IP临时测试地址：http://110.40.170.159:8080/predict。必须限制防火墙来源IP，仅使用假数据/非参与者测试图片。HTTP会明文传输令牌与图片；正式测试先配HTTPS，换新令牌，并关闭HTTP例外。客户端不跟随重定向，不关闭TLS证书验证。

## 请求

- POST /predict，Authorization: Bearer <研究接口令牌>。
- multipart/form-data，仅一个image文件字段；64×64 JPEG，灰度或RGB等通道。
- 请求体上限256KiB。服务端按训练代码解码为灰度，除255，以边缘均值自动统一极性。Unity已经黑底亮笔画，所以通常不会反相。
- 不上传参与者、target_label、target_class_id或本地原始文件名；不能把标准答案提供给识别服务。

## 响应

成功JSON含label、class_id（0—2349）、confidence、top5、inference_ms、preprocess_ms、server_processing_ms、model_version、model_sha256、labels_sha256、request_id、polarity_inverted。

confidence是2350类logits经过softmax后的分数，不是经过概率校准的“正确率”。模型是闭集分类器，不保证识别词表外韩文、乱码或非韩文；空白拒绝不等于具备通用异常检测能力。

错误：400图片/字段不合法；401令牌错误；413太大；415请求类型错误；503正在推理（单并发）；500内部推理错误。统一JSON含error和request_id。/health可无令牌访问，仅返回status=ok，不返回模型或敏感配置。

## 数据与时延

服务不将上传图片/参与者资料落盘，默认不输出预测标签或令牌到应用日志。反向代理关闭访问日志、关闭请求缓冲且限制图片体积；云厂商/现有WAF的日志与存储策略需要部署者另行确认。

Unity仍把图片存本地Samples，标注写samples_guided_v2.csv；响应原文、HTTP状态和请求往返时间写server_responses.csv，以filename关联。participants.csv保留人员编号。真实标签须人工核验；不使用预测结果作为真值。

- inference_ms：预热后的单次模型前向及输出同步，不包含图像解码/网络/排队。
- preprocess_ms：服务器解码、标准化和输入检查。
- server_processing_ms：应用进入predict后至组装结果，含multipart解析等，不含Nginx、网络、序列化响应及队列等待。
- elapsed_ms：Unity发送到收到响应的往返时间，不含发送前画板编码/保存与接收后显示。

服务器公网端口和HTTPS尚需用户按中文指南配置。2核2GB只用1个worker、单次推理并发；真实云服务器延迟/内存尚未实测，不能引用本地耗时作为云端结果。
