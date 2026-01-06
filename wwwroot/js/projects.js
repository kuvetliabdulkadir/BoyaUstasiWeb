// Proje Detay

// Lightbox Galeri
function initGalleryLightbox() {
    const galleryImages = document.querySelectorAll('.gallery-img[data-gallery-image]');
    if (galleryImages.length === 0) return;

    // Modal oluştur
    const lightbox = document.createElement('div');
    lightbox.className = 'lightbox-modal';
    lightbox.innerHTML = `
        <button class="lightbox-close" aria-label="Kapat">
            <i class="bi bi-x-lg"></i>
        </button>
        <button class="lightbox-nav lightbox-prev" aria-label="Önceki">
            <i class="bi bi-chevron-left"></i>
        </button>
        <button class="lightbox-nav lightbox-next" aria-label="Sonraki">
            <i class="bi bi-chevron-right"></i>
        </button>
        <div class="lightbox-content">
            <img class="lightbox-image" src="" alt="" />
            <div class="lightbox-caption"></div>
        </div>
    `;
    document.body.appendChild(lightbox);

    const lightboxImage = lightbox.querySelector('.lightbox-image');
    const lightboxCaption = lightbox.querySelector('.lightbox-caption');
    const closeBtn = lightbox.querySelector('.lightbox-close');
    const prevBtn = lightbox.querySelector('.lightbox-prev');
    const nextBtn = lightbox.querySelector('.lightbox-next');

    let currentIndex = 0;
    const images = Array.from(galleryImages);

    function openLightbox(index) {
        currentIndex = index;
        const img = images[index];
        const imageSrc = img.getAttribute('data-gallery-image');
        const imageCaption = img.getAttribute('data-gallery-caption') || '';

        // Yükleme hatası
        lightboxImage.onerror = function () {
            this.style.display = 'none';
            lightboxCaption.innerHTML = '<i class="fas fa-exclamation-triangle me-2"></i>Resim yüklenemedi';
            lightboxCaption.style.display = 'block';
        };

        lightboxImage.onload = function () {
            this.style.display = 'block';
            lightboxCaption.textContent = imageCaption;
        };

        lightboxImage.src = imageSrc;
        lightboxImage.alt = imageCaption;
        lightboxCaption.textContent = imageCaption;
        lightbox.classList.add('active');
        document.body.style.overflow = 'hidden';
        updateNavButtons();
    }

    function closeLightbox() {
        lightbox.classList.remove('active');
        document.body.style.overflow = '';
    }

    function updateNavButtons() {
        prevBtn.style.display = images.length > 1 ? 'flex' : 'none';
        nextBtn.style.display = images.length > 1 ? 'flex' : 'none';
    }

    function showNext() {
        currentIndex = (currentIndex + 1) % images.length;
        openLightbox(currentIndex);
    }

    function showPrev() {
        currentIndex = (currentIndex - 1 + images.length) % images.length;
        openLightbox(currentIndex);
    }

    // Olay dinleyicileri
    galleryImages.forEach((img, index) => {
        img.addEventListener('click', () => openLightbox(index));
    });

    closeBtn.addEventListener('click', closeLightbox);
    nextBtn.addEventListener('click', showNext);
    prevBtn.addEventListener('click', showPrev);

    // ESC ile kapat
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && lightbox.classList.contains('active')) {
            closeLightbox();
        } else if (e.key === 'ArrowRight' && lightbox.classList.contains('active')) {
            showNext();
        } else if (e.key === 'ArrowLeft' && lightbox.classList.contains('active')) {
            showPrev();
        }
    });

    // Dış tıklama ile kapat
    lightbox.addEventListener('click', (e) => {
        if (e.target === lightbox) {
            closeLightbox();
        }
    });
}

// Öncesi/Sonrası Slider
function initBeforeAfterSlider(container) {
    const slider = container.querySelector('.slider-handle');
    const beforeImage = container.querySelector('.before-image-wrapper');

    // Sınırları hesapla
    let containerRect = container.getBoundingClientRect();

    // Olay dinleyicileri
    slider.addEventListener('mousedown', startDrag);
    slider.addEventListener('touchstart', startDrag, { passive: false });

    // Konteyner tıklaması
    container.addEventListener('mousedown', (e) => {
        updateSliderPosition(e.clientX);
        startDrag(e);
    });

    // Boyut değişiminde güncelle
    window.addEventListener('resize', () => {
        containerRect = container.getBoundingClientRect();
    });

    function startDrag(e) {
        // Sürükleme başlangıcı
        containerRect = container.getBoundingClientRect();

        if (e.cancelable) e.preventDefault();
        container.classList.add('active');

        // Global dinleyiciler
        window.addEventListener('mousemove', drag);
        window.addEventListener('mouseup', stopDrag);

        window.addEventListener('touchmove', drag, { passive: false });
        window.addEventListener('touchend', stopDrag);
    }

    function stopDrag() {
        container.classList.remove('active');

        // Temizlik
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
        // X pozisyonu
        let x = clientX - containerRect.left;

        // Sınır kontrolü
        if (x < 0) x = 0;
        if (x > containerRect.width) x = containerRect.width;

        // Yüzde hesapla
        const percentage = (x / containerRect.width) * 100;

        // Görünümü güncelle
        beforeImage.style.width = percentage + '%';
        slider.style.left = percentage + '%';
    }
}

// Ana başlatıcı
function initProjectDetail() {
    // Slider başlat
    const container = document.getElementById('beforeAfter');
    if (container && typeof initBeforeAfterSlider === 'function') {
        initBeforeAfterSlider(container);
    }

    // Galeri başlat
    initGalleryLightbox();
}

// DOM Hazır
document.addEventListener('DOMContentLoaded', initProjectDetail);

