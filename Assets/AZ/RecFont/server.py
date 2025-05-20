

from flask import Flask, request, jsonify
import numpy as np
import tensorflow as tf
from PIL import Image, ImageFilter
import datetime
import random
import io
import os
import sys
# 保证标准输出是 UTF-8
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'

app = Flask(__name__)
if os.path.exists("result.txt"):
    os.remove("result.txt")
# ===== 日志 =====
def write_log(msg):
    with open("inference_log.txt", "a", encoding="utf-8") as f:
        now = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        f.write(f"[{now}] {msg}\n")

write_log("🔄 服务器启动中...")

# ===== 加载模型 =====
try:
    model = tf.saved_model.load('saved_model')
    write_log("✅ 模型加载成功")
except Exception as e:
    write_log(f"❌ 模型加载失败: {e}")
    raise e

# ===== 加载标签 =====
try:
    with open('2350-common-hangul.txt', 'r', encoding='utf-8') as f:
        labels = f.read().splitlines()
    write_log("标签加载成功")
except Exception as e:
    write_log(f"标签加载失败: {e}")
    raise e

# ===== softmax =====
def softmax(x):
    exp_x = np.exp(x - np.max(x))
    return exp_x / exp_x.sum()

# ===== 图像预处理函数（原封不动复用你已有的逻辑） =====
def preprocess_image_bytes(img_bytes):
    try:
        img_orig = Image.open(io.BytesIO(img_bytes)).convert('L')
        img_resized = img_orig.resize((64, 64))
        img_array = np.array(img_resized).astype(np.float32) / 255.0
        img_inverted = 1.0 - img_array
        img_vector = img_inverted.flatten()

        inv_img = Image.fromarray((1.0 - np.array(img_orig) / 255.0) * 255).convert('L')
        bbox = inv_img.getbbox()
        if bbox is None:
            raise ValueError("图像为空或没有有效内容")

        cropped = img_orig.crop(bbox)
        max_dim = max(cropped.size)
        square_canvas = Image.new('L', (max_dim, max_dim), 255)
        offset_x = (max_dim - cropped.width) // 2
        offset_y = (max_dim - cropped.height) // 2
        square_canvas.paste(cropped, (offset_x, offset_y))

        offset_percentage_x = abs(offset_x) * 2 / max_dim
        offset_percentage_y = abs(offset_y) * 2 / max_dim
        write_log(f"偏移百分比: {offset_percentage_x:.2f}, {offset_percentage_y:.2f}")

        if offset_percentage_x > 0.15 or offset_percentage_y > 0.15:
            centered_resized = square_canvas.resize((64, 64))
            centered_array = np.array(centered_resized).astype(np.float32) / 255.0
            centered_inverted = 1.0 - centered_array
            copy_vector = centered_inverted.flatten()

            Image.fromarray((centered_inverted * 255).astype(np.uint8)).save("copy.jpg")
            write_log("偏移较大，使用居中图像")
            return copy_vector
        else:
            write_log("偏移较小，使用原始图像")
            return img_vector

    except Exception as e:
        write_log(f"图像预处理失败: {e}")
        raise e


def augment_image(img: Image.Image):
    variants = []

    # 原图
    variants.append(img)

    # 多角度旋转
    for angle in [-5, 5, 10]:
        rotated = img.rotate(angle, fillcolor=255)
        variants.append(rotated)

    # 高斯噪声（不同强度）
    img_np = np.array(img).astype(np.float32)
    for std in [5, 10]:  # 轻度/中度噪声
        noise = np.random.normal(0, std, img_np.shape)
        noisy_img = np.clip(img_np + noise, 0, 255).astype(np.uint8)
        variants.append(Image.fromarray(noisy_img))

    # 轻微模糊
    blurred = img.filter(ImageFilter.GaussianBlur(radius=1))
    variants.append(blurred)

    # 轻微缩放（模拟微妙分辨率差异）
    scale_factors = [0.8, 1.2, 1.4]
    for scale in scale_factors:
        new_size = (int(img.width * scale), int(img.height * scale))
        scaled = img.resize(new_size, resample=Image.BICUBIC)
        padded = Image.new('L', img.size, 255)
        paste_x = (img.width - scaled.width) // 2
        paste_y = (img.height - scaled.height) // 2
        padded.paste(scaled, (paste_x, paste_y))
        variants.append(padded)

    return variants


# ===== 推理接口 =====
@app.route("/predict", methods=["POST"])
def predict():
    if os.path.exists("result.txt"):
        os.remove("result.txt")

    if 'image' not in request.files:
        return jsonify({'error': '缺少图像文件参数'}), 400

    try:
        img_bytes = request.files['image'].read()
        original_img = Image.open(io.BytesIO(img_bytes)).convert('L')
        augmented_images = augment_image(original_img)

        predictions = []
        for aug_img in augmented_images:
            buf = io.BytesIO()
            aug_img.save(buf, format='PNG')
            vector = preprocess_image_bytes(buf.getvalue())

            input_tensor = tf.convert_to_tensor([vector], dtype=tf.float32)
            prediction = model.signatures['serving_default'](input_tensor)['output_0']
            probs = softmax(prediction.numpy()[0])
            predicted_index = np.argmax(probs)
            predicted_label = labels[predicted_index]
            predictions.append(predicted_label)

        # 多数投票
        final_prediction = max(set(predictions), key=predictions.count)
        write_log(f"各增强预测: {predictions}")
        write_log(f"最终预测: {final_prediction}")

        with open('result.txt', 'w', encoding='utf-8') as f:
            f.write(final_prediction)
            print(f'{final_prediction}')

        return jsonify({'label': final_prediction})

    except Exception as e:
        write_log(f"推理失败: {e}")
        return jsonify({'error': str(e)}), 500

# ===== 启动服务 =====
if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000)
