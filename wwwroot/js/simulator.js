// Renk Simülatörü

(function () {
    'use strict';

    // DOM Elementleri
    const canvas = document.getElementById('roomCanvas');
    const ctx = canvas ? canvas.getContext('2d', { willReadFrequently: true }) : null;
    const canvasLoading = document.getElementById('canvasLoading');

    // Kalite ayarları
    if (ctx) {
        ctx.imageSmoothingEnabled = true;
        ctx.imageSmoothingQuality = 'high';
    }

    // Durum değişkenleri
    let roomImage = null;
    let originalImageData = null;
    let originalImageUrl = null;
    let coloredImageUrl = null;

    // Color
    let selectedColor = null; // Renk seçimi

    // Fırça durumu
    let isDrawing = false;
    let brushSize = 20;
    let colorOpacity = 0.65;
    let currentTool = 'brush'; // 'brush' or 'eraser'
    let maskCanvas = null;
    let maskCtx = null;
    let undoStack = [];
    let lastX = 0;
    let lastY = 0;

    // Başlatma
    document.addEventListener('DOMContentLoaded', function () {
        if (!canvas || !ctx) return;

        initColorInputs();
        initUploadArea();
        initBrushTools();
        initActionButtons();
        initWhatsAppButton();
    });

    // Renk girdileri
    function initColorInputs() {
        const hexInput = document.getElementById('hexInput');
        const colorPicker = document.getElementById('colorPicker');
        const colorSwatches = document.querySelectorAll('.color-swatch-btn');

        // HEX Input
        if (hexInput) {
            hexInput.addEventListener('input', function () {
                let value = this.value;
                if (!value.startsWith('#')) value = '#' + value;
                if (/^#[0-9A-Fa-f]{6}$/.test(value)) {
                    selectedColor = value;
                    if (colorPicker) colorPicker.value = value;
                    updateColorDisplay();
                    clearSelectedSwatch();
                }
            });
        }

        // Color Picker
        if (colorPicker) {
            colorPicker.addEventListener('input', function () {
                selectedColor = this.value;
                if (hexInput) hexInput.value = this.value;
                updateColorDisplay();
                clearSelectedSwatch();
            });
        }

        // Color Swatches
        colorSwatches.forEach(swatch => {
            swatch.addEventListener('click', function () {
                colorSwatches.forEach(s => s.classList.remove('selected'));
                this.classList.add('selected');

                selectedColor = this.dataset.color;
                if (hexInput) hexInput.value = selectedColor;
                if (colorPicker) colorPicker.value = selectedColor;
                updateColorDisplay();
            });
        });
    }

    function clearSelectedSwatch() {
        document.querySelectorAll('.color-swatch-btn').forEach(s => s.classList.remove('selected'));
    }

    function updateColorDisplay() {
        const preview = document.getElementById('selectedColorPreview');
        const nameEl = document.getElementById('selectedColorName');
        const hexEl = document.getElementById('selectedColorHex');

        if (preview) preview.style.backgroundColor = selectedColor;
        if (nameEl) nameEl.textContent = 'Seçili Renk';
        if (hexEl) hexEl.textContent = selectedColor.toUpperCase();

        // WhatsApp güncelleme
        updateWhatsAppButton();
    }

    // Yükleme alanı
    function initUploadArea() {
        const uploadArea = document.getElementById('uploadArea');
        const photoUpload = document.getElementById('photoUpload');

        if (!uploadArea || !photoUpload) return;

        uploadArea.addEventListener('click', () => photoUpload.click());

        uploadArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            uploadArea.classList.add('dragover');
        });

        uploadArea.addEventListener('dragleave', () => {
            uploadArea.classList.remove('dragover');
        });

        uploadArea.addEventListener('drop', (e) => {
            e.preventDefault();
            uploadArea.classList.remove('dragover');
            const file = e.dataTransfer.files[0];
            if (file && file.type.startsWith('image/')) {
                handleImageUpload(file);
            }
        });

        photoUpload.addEventListener('change', function () {
            if (this.files[0]) {
                handleImageUpload(this.files[0]);
            }
        });
    }

    function handleImageUpload(file) {
        showLoading(true);

        const reader = new FileReader();
        reader.onload = function (e) {
            const img = new Image();
            img.onload = function () {
                roomImage = img;
                originalImageUrl = e.target.result;

                // Canvas boyutlandırma
                // Container genişliğine göre dinamik limit
                const container = canvas.parentElement;
                const containerWidth = container ? container.clientWidth - 40 : 800; // Padding için 40px
                const maxWidth = Math.min(containerWidth, 1200); // Maksimum 1200px
                const maxHeight = 800; // Maksimum 800px (performans için)

                let width = img.width;
                let height = img.height;

                // Eğer resim zaten limitlerin içindeyse, boyutunu değiştirme
                if (width <= maxWidth && height <= maxHeight) {
                    // Orijinal boyutları kullan
                    canvas.width = width;
                    canvas.height = height;
                } else {
                    // Orijinal aspect ratio'yu koru
                    const aspectRatio = width / height;

                    // Önce genişliğe göre ayarla
                    if (width > maxWidth) {
                        width = maxWidth;
                        height = width / aspectRatio;
                    }

                    // Sonra yüksekliğe göre kontrol et
                    if (height > maxHeight) {
                        height = maxHeight;
                        width = height * aspectRatio;
                    }

                    canvas.width = width;
                    canvas.height = height;
                }

                // Canvas kalite ayarlarını tekrar set et (canvas boyutu değiştiğinde sıfırlanır)
                ctx.imageSmoothingEnabled = true;
                ctx.imageSmoothingQuality = 'high';

                // Resmi çiz
                ctx.drawImage(img, 0, 0, width, height);
                originalImageData = ctx.getImageData(0, 0, width, height);

                // Maske canvası başlat
                initMaskCanvas(width, height);

                // Show tools
                document.getElementById('brushTools').style.display = 'block';

                // Boyalı resmi ayarla
                coloredImageUrl = canvas.toDataURL('image/png');

                // Butonları aktif hale getir
                setButtonsEnabled(true);

                showLoading(false);
            };
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    }

    // Maske Canvas
    function initMaskCanvas(width, height) {
        maskCanvas = document.createElement('canvas');
        maskCanvas.width = width;
        maskCanvas.height = height;
        maskCtx = maskCanvas.getContext('2d', { willReadFrequently: true });
        undoStack = [];
    }

    // Fırça araçları
    function initBrushTools() {
        const brushSizeInput = document.getElementById('brushSize');
        const brushSizeValue = document.getElementById('brushSizeValue');
        const opacityInput = document.getElementById('colorOpacity');
        const opacityValue = document.getElementById('opacityValue');
        const undoBtn = document.getElementById('undoBtn');
        const clearMaskBtn = document.getElementById('clearMaskBtn');
        const toolRadios = document.querySelectorAll('input[name="brushTool"]');

        if (brushSizeInput) {
            brushSizeInput.addEventListener('input', function () {
                brushSize = parseInt(this.value);
                if (brushSizeValue) brushSizeValue.textContent = brushSize;
            });
        }

        if (opacityInput && opacityValue) {
            opacityInput.addEventListener('input', function () {
                colorOpacity = parseInt(this.value) / 100;
                opacityValue.textContent = this.value;
                if (roomImage) {
                    applyColorToCustomImage();
                    updateBeforeAfterSlider();
                }
            });
        }

        if (toolRadios.length > 0) {
            toolRadios.forEach(radio => {
                radio.addEventListener('change', function () {
                    currentTool = this.value;
                });
            });
        }

        if (undoBtn) {
            undoBtn.addEventListener('click', undoLastStroke);
        }

        if (clearMaskBtn) {
            clearMaskBtn.addEventListener('click', clearMask);
        }

        // Çizim olayları
        if (canvas) {
            canvas.addEventListener('mousedown', startDrawing);
            canvas.addEventListener('mousemove', draw);
            canvas.addEventListener('mouseup', stopDrawing);
            canvas.addEventListener('mouseleave', stopDrawing);

            // Touch support
            canvas.addEventListener('touchstart', handleTouchStart, { passive: false });
            canvas.addEventListener('touchmove', handleTouchMove, { passive: false });
            canvas.addEventListener('touchend', stopDrawing);
        }
    }

    function startDrawing(e) {
        if (!roomImage || !maskCtx || !maskCanvas) return;
        isDrawing = true;

        // Save current mask state for undo
        undoStack.push(maskCtx.getImageData(0, 0, maskCanvas.width, maskCanvas.height));
        if (undoStack.length > 20) undoStack.shift();

        // Restore original image
        if (originalImageData) {
            ctx.putImageData(originalImageData, 0, 0);
        }

        // İlk noktayı kaydet
        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;
        lastX = (e.clientX - rect.left) * scaleX;
        lastY = (e.clientY - rect.top) * scaleY;

        draw(e);
    }

    // Performans optimizasyonu
    let rafId = null;
    let lastDrawTime = 0;
    const drawThrottle = 16; // ~60fps için 16ms
    let needsUpdate = false;
    let visualUpdatePending = false;

    function draw(e) {
        if (!isDrawing || !roomImage || !maskCtx || !maskCanvas) return;

        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;
        const x = (e.clientX - rect.left) * scaleX;
        const y = (e.clientY - rect.top) * scaleY;

        // Çizgi çizme
        const dx = x - lastX;
        const dy = y - lastY;
        const distance = Math.sqrt(dx * dx + dy * dy);

        // Boşlukları doldur
        if (distance > brushSize) {
            const steps = Math.ceil(distance / (brushSize / 2));
            for (let i = 0; i <= steps; i++) {
                const stepX = lastX + (dx * i / steps);
                const stepY = lastY + (dy * i / steps);
                drawPoint(stepX, stepY);
            }
        } else {
            drawPoint(x, y);
        }

        lastX = x;
        lastY = y;

        needsUpdate = true;

        // Görsel güncelleme optimizasyonu
        const now = performance.now();
        if (now - lastDrawTime < drawThrottle) {
            if (!rafId) {
                rafId = requestAnimationFrame(() => {
                    if (needsUpdate) {
                        applyColorToCustomImage();
                        // Before/After slider'ı çizim sırasında GÜNCELLEME - performans için
                        needsUpdate = false;
                    }
                    rafId = null;
                });
            }
            return;
        }
        lastDrawTime = now;

        // Hemen canvas'ı güncelle (slider hariç)
        applyColorToCustomImage();
        needsUpdate = false;
    }

    function drawPoint(x, y) {
        if (currentTool === 'brush') {
            // Renksiz çizim engeli
            if (!selectedColor) return;

            // Fırça modu
            const r = parseInt(selectedColor.slice(1, 3), 16);
            const g = parseInt(selectedColor.slice(3, 5), 16);
            const b = parseInt(selectedColor.slice(5, 7), 16);

            maskCtx.globalCompositeOperation = 'source-over';
            maskCtx.fillStyle = `rgb(${r}, ${g}, ${b})`;
            maskCtx.beginPath();
            maskCtx.arc(x, y, brushSize / 2, 0, Math.PI * 2);
            maskCtx.fill();
        } else if (currentTool === 'eraser') {
            // Silgi modu
            maskCtx.globalCompositeOperation = 'destination-out';
            maskCtx.beginPath();
            maskCtx.arc(x, y, brushSize / 2, 0, Math.PI * 2);
            maskCtx.fill();
        }
    }

    function stopDrawing() {
        if (!isDrawing) return;
        isDrawing = false;

        // Animasyonu iptal et
        if (rafId) {
            cancelAnimationFrame(rafId);
            rafId = null;
        }

        // Son güncelleme
        if (needsUpdate) {
            applyColorToCustomImage();
            needsUpdate = false;
        }

        // Çizim bittiğinde coloredImageUrl'i güncelle ve slider'ı güncelle
        coloredImageUrl = canvas.toDataURL('image/png');
        updateBeforeAfterSlider();
    }

    function handleTouchStart(e) {
        e.preventDefault();
        const touch = e.touches[0];
        const mouseEvent = new MouseEvent('mousedown', {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        startDrawing(mouseEvent);
    }

    function handleTouchMove(e) {
        e.preventDefault();
        const touch = e.touches[0];
        const mouseEvent = new MouseEvent('mousemove', {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        draw(mouseEvent);
    }

    function undoLastStroke() {
        if (undoStack.length === 0) return;

        const previousState = undoStack.pop();
        maskCtx.putImageData(previousState, 0, 0);
        applyColorToCustomImage();
        updateBeforeAfterSlider();
    }

    function clearMask() {
        if (!maskCtx) return;
        maskCtx.clearRect(0, 0, maskCanvas.width, maskCanvas.height);

        if (originalImageData) {
            ctx.putImageData(originalImageData, 0, 0);
            coloredImageUrl = canvas.toDataURL('image/png');
            updateBeforeAfterSlider();
        }

        undoStack = [];
    }

    // Rengi uygula
    function applyColorToCustomImage() {
        if (!originalImageData || !maskCtx) return;

        // Restore original
        ctx.putImageData(originalImageData, 0, 0);

        // Get mask data
        const maskData = maskCtx.getImageData(0, 0, maskCanvas.width, maskCanvas.height);
        const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        const data = imageData.data;
        const mask = maskData.data;
        const originalData = originalImageData.data;
        const opacity = colorOpacity;
        const oneMinusOpacity = 1 - opacity;

        // Optimize kaplama
        for (let i = 0, len = data.length; i < len; i += 4) {
            const maskR = mask[i];
            const maskG = mask[i + 1];
            const maskB = mask[i + 2];

            // Eğer mask'ta renk varsa
            if (maskR | maskG | maskB) {
                // Basit overlay - Math.round kaldırıldı (performans için)
                data[i] = (originalData[i] * oneMinusOpacity + maskR * opacity) | 0;
                data[i + 1] = (originalData[i + 1] * oneMinusOpacity + maskG * opacity) | 0;
                data[i + 2] = (originalData[i + 2] * oneMinusOpacity + maskB * opacity) | 0;
            }
        }

        ctx.putImageData(imageData, 0, 0);
        // NOT: coloredImageUrl güncelleme sadece stopDrawing'de yapılacak
    }

    function updateBeforeAfterSlider() {
        if (!originalImageUrl) return;

        coloredImageUrl = canvas.toDataURL('image/png');

        if (!coloredImageUrl) return;

        const beforeAfterContainer = document.getElementById('beforeAfterContainer');
        const beforeImage = document.getElementById('beforeImage');
        const afterImage = document.getElementById('afterImage');

        if (!beforeAfterContainer || !beforeImage || !afterImage) return;

        // Önce "Önce" resmini yükle ve boyutunu al
        // Resim yüklendi
        beforeImage.onload = function () {
            // Konteyner boyutunu serbest bırak (CSS kontrol etsin - width: 100%)
            // Böylece "küçük/kenarda" kalma sorunu çözülür.
            const container = document.getElementById('beforeAfter');
            if (container) {
                container.style.width = '';
                container.style.height = '';
            }
        };

        beforeImage.src = originalImageUrl;
        afterImage.src = coloredImageUrl;

        beforeAfterContainer.style.display = 'block';

        const sliderContainer = document.getElementById('beforeAfter');
        if (sliderContainer && typeof initBeforeAfterSlider === 'function') {
            if (!sliderContainer.dataset.initialized) {
                initBeforeAfterSlider(sliderContainer);
                sliderContainer.dataset.initialized = 'true';
            } else {
                // Slider sıfırlama
                const slider = sliderContainer.querySelector('.slider-handle');
                const beforeImg = sliderContainer.querySelector('.before-image');
                if (slider && beforeImg) {
                    updateSliderPosition(50, sliderContainer, slider, beforeImg);
                }
            }
        }
    }

    // İşlem butonları
    function initActionButtons() {
        const resetBtn = document.getElementById('resetBtn');
        const fullscreenBtn = document.getElementById('fullscreenBtn');
        const downloadBtn = document.getElementById('downloadBtn');

        // Buton durumları
        setButtonsEnabled(false);

        if (resetBtn) {
            resetBtn.addEventListener('click', function () {
                if (roomImage) {
                    clearMask();
                } else {
                    alert('Lütfen önce bir resim yükleyin.');
                }
            });
        }

        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', function () {
                if (!roomImage) {
                    alert('Lütfen önce bir resim yükleyin.');
                    return;
                }

                const beforeAfterContainer = document.getElementById('beforeAfterContainer');
                if (beforeAfterContainer && beforeAfterContainer.style.display !== 'none') {
                    if (beforeAfterContainer.requestFullscreen) {
                        beforeAfterContainer.requestFullscreen();
                    } else if (beforeAfterContainer.webkitRequestFullscreen) {
                        beforeAfterContainer.webkitRequestFullscreen();
                    } else if (beforeAfterContainer.msRequestFullscreen) {
                        beforeAfterContainer.msRequestFullscreen();
                    }
                } else if (canvas && roomImage) {
                    if (canvas.requestFullscreen) {
                        canvas.requestFullscreen();
                    } else if (canvas.webkitRequestFullscreen) {
                        canvas.webkitRequestFullscreen();
                    } else if (canvas.msRequestFullscreen) {
                        canvas.msRequestFullscreen();
                    }
                }
            });
        }

        if (downloadBtn) {
            downloadBtn.addEventListener('click', function () {
                if (!roomImage) {
                    alert('Lütfen önce bir resim yükleyin.');
                    return;
                }

                try {
                    const link = document.createElement('a');
                    link.download = 'boya-simulasyon.png';

                    // Önce coloredImageUrl'i kontrol et, yoksa canvas'tan al
                    if (coloredImageUrl) {
                        link.href = coloredImageUrl;
                    } else if (canvas && roomImage) {
                        link.href = canvas.toDataURL('image/png');
                    } else {
                        alert('İndirilecek görsel bulunamadı. Lütfen tekrar deneyin.');
                        return;
                    }

                    link.click();
                } catch (error) {
                    alert('Görsel indirilemedi. Lütfen tekrar deneyin.');
                }
            });
        }
    }

    // Butonları aktif/pasif yap
    function setButtonsEnabled(enabled) {
        const resetBtn = document.getElementById('resetBtn');
        const fullscreenBtn = document.getElementById('fullscreenBtn');
        const downloadBtn = document.getElementById('downloadBtn');

        [resetBtn, fullscreenBtn, downloadBtn].forEach(btn => {
            if (btn) {
                btn.disabled = !enabled;
                if (enabled) {
                    btn.classList.remove('disabled');
                    btn.style.opacity = '1';
                    btn.style.cursor = 'pointer';
                } else {
                    btn.classList.add('disabled');
                    btn.style.opacity = '0.5';
                    btn.style.cursor = 'not-allowed';
                }
            }
        });
    }

    // Helpers
    function showLoading(show) {
        if (canvasLoading) {
            canvasLoading.classList.toggle('active', show);
        }
    }

    // WhatsApp başlatma
    function initWhatsAppButton() {
        const btn = document.getElementById('whatsappQuoteBtn');
        if (btn) {
            // İlk yüklemede butonu disabled yap
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.style.opacity = '0.6';
            btn.style.cursor = 'not-allowed';
            btn.href = '#';

            // Butona tıklama olayı ekle (renk seçilmeden çalışmasın)
            btn.addEventListener('click', function (e) {
                if (!selectedColor || selectedColor === '#2563eb') {
                    e.preventDefault();
                    alert('Lütfen önce bir renk seçin!');
                    return false;
                }
            });

            updateWhatsAppButton();
        }
    }

    // WhatsApp butonunu güncelle
    function updateWhatsAppButton() {
        const btn = document.getElementById('whatsappQuoteBtn');
        if (!btn) return;

        // Renk seçilmiş mi kontrol et (varsayılan renk dışında)
        const hasColor = selectedColor && selectedColor !== '#2563eb';

        if (hasColor) {
            const whatsapp = btn.dataset.whatsapp || '';
            const colorName = getColorName(selectedColor);
            const colorHex = selectedColor.toUpperCase();

            // HEX'i RGB'ye çevir
            const r = parseInt(colorHex.slice(1, 3), 16);
            const g = parseInt(colorHex.slice(3, 5), 16);
            const b = parseInt(colorHex.slice(5, 7), 16);

            // CMYK değerleri (baskı/boya için)
            const c = Math.round((1 - r / 255) * 100);
            const m = Math.round((1 - g / 255) * 100);
            const y = Math.round((1 - b / 255) * 100);
            const k = Math.round(Math.min(c, m, y));
            const cFinal = Math.max(0, c - k);
            const mFinal = Math.max(0, m - k);
            const yFinal = Math.max(0, y - k);

            // Renk tonu açıklaması
            const brightness = Math.round((r + g + b) / 3);
            const lightness = brightness > 128 ? 'Açık' : 'Koyu';
            const saturation = Math.max(r, g, b) - Math.min(r, g, b);
            const colorIntensity = saturation > 100 ? 'Canlı' : saturation > 50 ? 'Orta' : 'Soluk';

            const message = encodeURIComponent(
                `Merhaba, renk simülatöründe beğendiğim bir renk için teklif almak istiyorum.\n\n` +
                ` RENK BİLGİLERİ:\n` +
                `━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n` +
                `• HEX Kodu: ${colorHex}\n` +
                `• RGB: R:${r}, G:${g}, B:${b}\n` +
                `• CMYK: C:${cFinal}%, M:${mFinal}%, Y:${yFinal}%, K:${k}%\n` +
                `• Renk Adı: ${colorName}\n` +
                `• Ton: ${lightness} ${colorIntensity}\n\n` +
                ` BOYA HAZIRLAMA İÇİN:\n` +
                `Bu renk koduna göre boya karışımı hazırlayabilir misiniz?`
            );

            btn.href = `https://wa.me/${whatsapp}?text=${message}`;
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.style.opacity = '1';
            btn.style.cursor = 'pointer';
        } else {
            btn.href = '#';
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.style.opacity = '0.6';
            btn.style.cursor = 'not-allowed';
        }
    }

    // Renk adı bulma
    function getColorName(hex) {
        if (!hex || !hex.startsWith('#')) return 'Bilinmeyen Renk';

        // HEX'i RGB'ye çevir
        const r = parseInt(hex.slice(1, 3), 16);
        const g = parseInt(hex.slice(3, 5), 16);
        const b = parseInt(hex.slice(5, 7), 16);

        // Basit renk tanıma
        const brightness = (r + g + b) / 3;
        const saturation = Math.max(r, g, b) - Math.min(r, g, b);

        if (brightness < 50) return 'Siyah';
        if (brightness > 200) return 'Beyaz';
        if (saturation < 30) return 'Gri';

        // Ana renkler
        if (r > g && r > b) {
            if (r > 200 && g < 100 && b < 100) return 'Kırmızı';
            if (r > 150 && g > 100 && b < 100) return 'Turuncu';
            if (r > 150 && g > 150 && b < 100) return 'Sarı';
            return 'Kırmızımsı';
        }
        if (g > r && g > b) {
            if (g > 200 && r < 100 && b < 100) return 'Yeşil';
            if (g > 150 && r > 100 && b < 100) return 'Sarımsı Yeşil';
            return 'Yeşilimsi';
        }
        if (b > r && b > g) {
            if (b > 200 && r < 100 && g < 100) return 'Mavi';
            if (b > 150 && r > 100 && g < 100) return 'Mor';
            if (b > 150 && r > 100 && g > 100) return 'Lacivert';
            return 'Mavimsi';
        }

        return 'Karışık Renk';
    }

    // Slider mantığı
    function initBeforeAfterSlider(container) {
        if (!container) return;

        // Elementler
        const slider = container.querySelector('.slider-handle');
        const beforeImg = container.querySelector('.before-image');

        if (!slider || !beforeImg) return;

        // Resimlerin sürüklenmesini engelle
        const images = container.querySelectorAll('img');
        images.forEach(img => {
            img.style.pointerEvents = 'none';
            img.setAttribute('draggable', 'false');
        });

        // Konteynerın sınırlarını hesapla
        let containerRect = container.getBoundingClientRect();

        // [EVENT LISTENERS]

        // 1. Slider Handle Drag
        slider.addEventListener('mousedown', startDrag);
        slider.addEventListener('touchstart', startDrag, { passive: false });

        // 2. Container Click/Drag (Hızlı atlama için)
        container.addEventListener('mousedown', (e) => {
            updateSliderPosition(e.clientX);
            startDrag(e);
        });

        container.addEventListener('touchstart', (e) => {
            const clientX = e.touches[0].clientX;
            updateSliderPosition(clientX);
            startDrag(e);
        }, { passive: false });

        // 3. Resize durumunda sınırları güncelle
        window.addEventListener('resize', () => {
            containerRect = container.getBoundingClientRect();
        });

        function startDrag(e) {
            // Her drag başlangıcında rect'i tazelemek en sağlıklısı
            containerRect = container.getBoundingClientRect();

            container.classList.add('active');

            // Global dinleyiciler ekle
            window.addEventListener('mousemove', drag);
            window.addEventListener('mouseup', stopDrag);

            window.addEventListener('touchmove', drag, { passive: false });
            window.addEventListener('touchend', stopDrag);
        }

        function stopDrag() {
            container.classList.remove('active');

            // Global dinleyicileri temizle
            window.removeEventListener('mousemove', drag);
            window.removeEventListener('mouseup', stopDrag);

            window.removeEventListener('touchmove', drag);
            window.removeEventListener('touchend', stopDrag);
        }

        function drag(e) {
            let clientX;
            if (e.type.includes('touch')) {
                clientX = e.touches[0].clientX;
            } else {
                clientX = e.clientX;
            }
            updateSliderPosition(clientX);
        }

        function updateSliderPosition(clientX) {
            // Farenin konteyner içindeki X pozisyonu
            let x = clientX - containerRect.left;

            // Sınırları kontrol et
            if (x < 0) x = 0;
            if (x > containerRect.width) x = containerRect.width;

            // Yüzde hesapla
            const percentage = (x / containerRect.width) * 100;

            // Before image width
            beforeImg.style.width = percentage + '%';

            // Slider handle position
            slider.style.left = percentage + '%';
        }
    }
})();
