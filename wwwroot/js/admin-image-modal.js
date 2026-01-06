// Admin Panel Resim Modalı Yardımcı Scripti
// showImageModal ve ilgili navigasyon fonksiyonlarını içerir.

// Global değişkenler
window.modalImageUrls = [];
window.modalCurrentIndex = 0;
window.modalLoadTimeout = null;

// Modalı belirtilen resim listesi ve başlangıç indeksi ile açar
function showImageModal(imageUrls, startIndex) {
    if (!imageUrls || imageUrls.length === 0) {
        console.error('Gösterilecek resim bulunamadı.');
        return;
    }

    // Global değişkenleri ayarla
    window.modalImageUrls = imageUrls;
    window.modalCurrentIndex = (startIndex >= 0 && startIndex < imageUrls.length) ? startIndex : 0;

    const modalEl = document.getElementById('imageModal');
    if (!modalEl) {
        console.error('Modal elementi (#imageModal) bulunamadı.');
        return;
    }

    // İlk resmi yükle
    loadImageInModal();

    // Modalı göster (Bootstrap veya Manuel)
    openModalElement(modalEl);
}

// Modalı kapatır
function closeImageModal() {
    const modalEl = document.getElementById('imageModal');
    if (!modalEl) return;

    // Bootstrap ile kapat
    if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        try {
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) {
                modalInstance.hide();
                return;
            }
        } catch (error) {
            // Hata olursa devam et
        }
    }

    // Manuel kapat
    modalEl.style.display = 'none';
    modalEl.classList.remove('show');

    // Backdrop temizle
    const backdrop = document.querySelector('.modal-backdrop-manual');
    if (backdrop) backdrop.remove();
    const bsBackdrop = document.querySelector('.modal-backdrop');
    if (bsBackdrop) bsBackdrop.remove();

    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';
}

// İleri/Geri navigasyon
function navigateImage(direction) {
    if (!window.modalImageUrls || window.modalImageUrls.length === 0) return;

    const newIndex = window.modalCurrentIndex + direction;
    if (newIndex >= 0 && newIndex < window.modalImageUrls.length) {
        window.modalCurrentIndex = newIndex;
        loadImageInModal();
    }
}

// Seçili indeksteki resmi modal içine yükler
function loadImageInModal() {
    const modalImage = document.getElementById('modalImage');
    const imageLoading = document.getElementById('imageLoading');
    const imageError = document.getElementById('imageError');
    const downloadLink = document.getElementById('downloadImageLink');

    if (!modalImage) return;

    const currentUrl = window.modalImageUrls[window.modalCurrentIndex];

    // UI hazırlığı
    modalImage.style.display = 'none';
    if (imageLoading) imageLoading.style.display = 'block';
    if (imageError) imageError.classList.add('d-none');

    // Timeout temizle
    if (window.modalLoadTimeout) {
        clearTimeout(window.modalLoadTimeout);
    }

    // Yeni timeout (10sn)
    window.modalLoadTimeout = setTimeout(() => {
        if (imageLoading) imageLoading.style.display = 'none';
        if (imageError) imageError.classList.remove('d-none');
    }, 10000);

    // Cache-busting (tarayıcı önbelleğini atlatmak için)
    const timestamp = new Date().getTime();
    const separator = currentUrl.includes('?') ? '&' : '?';
    const srcWithCache = currentUrl + separator + '_t=' + timestamp;

    // Load eventleri
    modalImage.onload = function () {
        if (window.modalLoadTimeout) clearTimeout(window.modalLoadTimeout);
        if (imageLoading) imageLoading.style.display = 'none';
        if (imageError) imageError.classList.add('d-none');
        modalImage.style.display = 'block';
    };

    modalImage.onerror = function () {
        if (window.modalLoadTimeout) clearTimeout(window.modalLoadTimeout);
        if (imageLoading) imageLoading.style.display = 'none';
        if (imageError) imageError.classList.remove('d-none');
    };

    // Kaynağı set et
    modalImage.src = srcWithCache;

    // İndirme linki
    if (downloadLink) {
        downloadLink.href = currentUrl;
    }

    // Navigasyon butonlarını güncelle
    updateNavigationUI();
}

// Navigasyon butonlarını ve sayacı günceller
function updateNavigationUI() {
    const prevBtn = document.getElementById('prevImageBtn');
    const nextBtn = document.getElementById('nextImageBtn');
    const imageCounter = document.getElementById('imageCounter');
    const countBadge = imageCounter ? imageCounter.querySelector('span') : null;

    const total = window.modalImageUrls.length;
    const current = window.modalCurrentIndex;

    if (total <= 1) {
        if (prevBtn) prevBtn.style.display = 'none';
        if (nextBtn) nextBtn.style.display = 'none';
        if (imageCounter) imageCounter.style.display = 'none';
    } else {
        if (prevBtn) prevBtn.style.display = (current > 0) ? 'block' : 'none';
        if (nextBtn) nextBtn.style.display = (current < total - 1) ? 'block' : 'none';

        if (imageCounter && countBadge) {
            imageCounter.style.display = 'block';
            countBadge.textContent = `${current + 1} / ${total}`;
        }
    }
}

// Modal elementini açar (Bootstrap veya Manuel fallback)
function openModalElement(modalEl) {
    // Erişim
    modalEl.removeAttribute('aria-hidden');
    modalEl.removeAttribute('inert');

    if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        try {
            const modal = new bootstrap.Modal(modalEl, {
                backdrop: true,
                keyboard: true,
                focus: true
            });
            modal.show();
        } catch (e) {
            console.warn('Bootstrap modal hatası, manuel açılıyor', e);
            showModalManually(modalEl);
        }
    } else {
        showModalManually(modalEl);
    }
}

// Bootstrap çalışmazsa manuel olarak modalı açar
function showModalManually(modalEl) {
    // Backdrop oluştur
    let backdrop = document.querySelector('.modal-backdrop-manual');
    if (!backdrop) {
        backdrop = document.createElement('div');
        backdrop.className = 'modal-backdrop-manual';
        backdrop.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1040;';
        document.body.appendChild(backdrop);
        backdrop.addEventListener('click', closeImageModal);
    }

    modalEl.style.display = 'block';
    modalEl.classList.add('show');
    document.body.classList.add('modal-open');
    document.body.style.overflow = 'hidden';
}

// Klavye olayları (Sağ/Sol ok)
document.addEventListener('keydown', function (e) {
    const modalEl = document.getElementById('imageModal');
    if (!modalEl || !modalEl.classList.contains('show') || modalEl.style.display === 'none') return;

    if (e.key === 'ArrowLeft') {
        navigateImage(-1);
    } else if (e.key === 'ArrowRight') {
        navigateImage(1);
    } else if (e.key === 'Escape') {
        closeImageModal();
    }
});

// DOM Hazır olduğunda ve değişikliklerde dinleyicileri ekle
function initImagePreviewListeners() {
    attachImagePreviewListeners();

    // Dinamik Gözlemci
    const observer = new MutationObserver(function (mutations) {
        let shouldAttach = false;
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length > 0) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) { // Element node
                        if (node.classList && node.classList.contains('image-preview')) {
                            shouldAttach = true;
                        } else if (node.querySelectorAll) {
                            const images = node.querySelectorAll('.image-preview');
                            if (images.length > 0) {
                                shouldAttach = true;
                            }
                        }
                    }
                });
            }
        });
        if (shouldAttach) {
            attachImagePreviewListeners();
        }
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
}

// Resim Önizleme Dinleyicileri
function attachImagePreviewListeners() {
    const images = document.querySelectorAll('.image-preview');

    images.forEach(function (img) {
        // Sadece IMG
        if (img.tagName === 'IMG') {
            img.style.cursor = 'pointer';

            // Tekrar Ekleme Kontrolü
            if (img.hasAttribute('data-modal-listener')) {
                return;
            }

            img.setAttribute('data-modal-listener', 'true');

            img.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                const imagePath = this.src;
                if (imagePath) {
                    showImageModal([imagePath], 0);
                }
            });
        }
    });
}

// Başlat
document.addEventListener('DOMContentLoaded', () => {
    // Modal temizlik eventleri
    const modalEl = document.getElementById('imageModal');
    if (modalEl) {
        modalEl.addEventListener('hidden.bs.modal', function () {
            const modalImage = document.getElementById('modalImage');
            if (modalImage) modalImage.src = '';
        });
    }

    // Generic listenerları başlat
    initImagePreviewListeners();
});
