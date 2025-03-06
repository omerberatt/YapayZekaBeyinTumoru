from flask import Flask, request, jsonify
from tensorflow.keras.preprocessing import image
from tensorflow.keras.applications.vgg16 import VGG16, preprocess_input
import numpy as np
import xgboost as xgb
import os

# Flask uygulamasını başlat
app = Flask(__name__)

# XGBoost modelini yükle
model = xgb.XGBClassifier()
model.load_model('xgboost_model.json')

# VGG16 tabanlı model yükle
base_model = VGG16(weights='imagenet', include_top=False, input_shape=(150, 150, 3))

# Görüntüyü işleme fonksiyonu
def process_image(img_path):
    img = image.load_img(img_path, target_size=(150, 150))
    img_array = image.img_to_array(img)
    img_array = np.expand_dims(img_array, axis=0)
    img_array = preprocess_input(img_array)
    return img_array

# API'yi tanımlayın
@app.route('/predict', methods=['POST'])
def predict():
    if 'image' not in request.files:
        return jsonify({'error': 'No file part'}), 400
    
    file = request.files['image']

    if file.filename == '':
        return jsonify({'error': 'No selected file'}), 400
    
    # Dosyayı geçici bir konuma kaydet
    file_path = os.path.join('temp', file.filename)
    os.makedirs('temp', exist_ok=True)
    file.save(file_path)

    try:
        # Görüntüyü işle
        processed_image = process_image(file_path)

        # Özellik çıkarımı
        features = base_model.predict(processed_image)
        features = features.flatten()

        # Tahmin yapma
        prediction = model.predict([features])
        categories = ["glioma", "meningioma", "notumor", "pituitary"]
        predicted_class = categories[int(prediction[0])]

        # Geçici dosyayı sil
        os.remove(file_path)

        return jsonify({'tumor_type': predicted_class}), 200

    except Exception as e:
        os.remove(file_path)  # Hata durumunda dosyayı temizle
        return jsonify({'error': str(e)}), 500

# Flask sunucusunu başlat
if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
