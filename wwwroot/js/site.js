// Ana JS Dosyası

// DOM Hazır
document.addEventListener('DOMContentLoaded', function () {
    // Navbar initialization
    if (typeof initNavbar === 'function') {
        initNavbar();
    }
    if (typeof initMobileOffcanvasNav === 'function') {
        initMobileOffcanvasNav();
    }

    // Animations initialization
    if (typeof initScrollAnimations === 'function') {
        initScrollAnimations();
    }
    if (typeof initCounters === 'function') {
        initCounters();
    }

    if (typeof initHashAndAnchorFixes === 'function') {
        initHashAndAnchorFixes();
    }
    if (typeof initFaqAutoScroll === 'function') {
        initFaqAutoScroll();
    }

    // Before/After Sliders initialization
    const beforeAfterContainers = document.querySelectorAll('.before-after-container');
    beforeAfterContainers.forEach(container => {
        if (typeof initBeforeAfterSlider === 'function') {
            initBeforeAfterSlider(container);
        }
    });

    // Testimonial Sliders initialization
    const testimonialContainers = document.querySelectorAll('.testimonial-slider');
    testimonialContainers.forEach(container => {
        if (typeof initTestimonialSlider === 'function') {
            initTestimonialSlider(container);
        }
    });
    // Phone Button initialization
    setupPhoneButton();
});

// Telefon butonu: Mobil arama, Masaüstü form
function setupPhoneButton() {
    const phoneBtns = document.querySelectorAll('.phone-call-btn');
    if (!phoneBtns.length) return;

    // Mobil (<768px): Arama
    // Masaüstü (>=768px): Form
    const isMobileWidth = window.innerWidth < 768;

    phoneBtns.forEach(function (phoneBtn) {
        const contactUrl = phoneBtn.getAttribute('data-contact-url');
        const phoneNumber = phoneBtn.getAttribute('data-phone');

        if (isMobileWidth) {
            // Mobil: Direkt arama
            phoneBtn.href = 'tel:' + phoneNumber;
            // Tıklamaları temizle
            const newBtn = phoneBtn.cloneNode(true);
            phoneBtn.parentNode.replaceChild(newBtn, phoneBtn);
        } else {
            // Masaüstü: İletişim formu
            phoneBtn.href = contactUrl;

            // Tıklama yönetimi
            phoneBtn.addEventListener('click', function (e) {
                e.preventDefault();
                window.location.href = contactUrl;
            });
        }
    });

    // Yeniden boyutlandırma
    window.addEventListener('resize', function () {
        setupPhoneButton();
    });
}

// Konami Kodu
(function () {
    const secretCode = ['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight'];
    let inputSequence = [];
    let timeout;

    document.addEventListener('keydown', function (e) {
        // Sadece ok tuşları
        if (!e.key.startsWith('Arrow')) return;

        // Yazı alanlarını yoksay
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;

        inputSequence.push(e.key);

        // Zaman aşımı (3s)
        clearTimeout(timeout);
        timeout = setTimeout(() => { inputSequence = []; }, 3000);

        // Son N tuşu kontrol et
        if (inputSequence.length > secretCode.length) {
            inputSequence.shift();
        }

        // Eşleşme kontrolü
        if (inputSequence.length === secretCode.length) {
            let match = true;
            for (let i = 0; i < secretCode.length; i++) {
                if (inputSequence[i] !== secretCode[i]) {
                    match = false;
                    break;
                }
            }

            if (match) {
                inputSequence = [];
                // Admin paneline git
                window.location.href = '/usta-panel-2024/';
            }
        }
    });
})();

