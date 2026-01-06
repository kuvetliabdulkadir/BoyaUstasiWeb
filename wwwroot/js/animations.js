// Animasyon & Efektler

// Kaydırma Animasyonları
function initScrollAnimations() {
    // GSAP kontrolü
    if (typeof gsap !== 'undefined' && typeof ScrollTrigger !== 'undefined') {
        gsap.registerPlugin(ScrollTrigger);
        initGSAPAnimations();
    } else {
        // Intersection Observer yedeği
        initIntersectionObserver();
    }
}

function initGSAPAnimations() {
    // Soluk giriş
    gsap.utils.toArray('.fade-in').forEach(element => {
        gsap.fromTo(element,
            { opacity: 0, y: 30 },
            {
                opacity: 1,
                y: 0,
                duration: 0.3,
                scrollTrigger: {
                    trigger: element,
                    start: 'top 85%',
                    toggleActions: 'play none none none'
                }
            }
        );
    });

    // Soldan giriş
    gsap.utils.toArray('.fade-in-left').forEach(element => {
        gsap.fromTo(element,
            { opacity: 0, x: -50 },
            {
                opacity: 1,
                x: 0,
                duration: 0.3,
                scrollTrigger: {
                    trigger: element,
                    start: 'top 85%',
                    toggleActions: 'play none none none'
                }
            }
        );
    });

    // Sağdan giriş
    gsap.utils.toArray('.fade-in-right').forEach(element => {
        gsap.fromTo(element,
            { opacity: 0, x: 50 },
            {
                opacity: 1,
                x: 0,
                duration: 0.3,
                scrollTrigger: {
                    trigger: element,
                    start: 'top 85%',
                    toggleActions: 'play none none none'
                }
            }
        );
    });

    // Ölçekli giriş
    gsap.utils.toArray('.scale-in').forEach(element => {
        gsap.fromTo(element,
            { opacity: 0, scale: 0.8 },
            {
                opacity: 1,
                scale: 1,
                duration: 0.3,
                scrollTrigger: {
                    trigger: element,
                    start: 'top 85%',
                    toggleActions: 'play none none none'
                }
            }
        );
    });

    // Sıralı giriş
    gsap.utils.toArray('.stagger-container').forEach(container => {
        gsap.fromTo(container.children,
            { opacity: 0, y: 30 },
            {
                opacity: 1,
                y: 0,
                duration: 0.5,
                stagger: 0.1,
                scrollTrigger: {
                    trigger: container,
                    start: 'top 85%',
                    toggleActions: 'play none none none'
                }
            }
        );
    });
}

function initIntersectionObserver() {
    const animatedElements = document.querySelectorAll('.fade-in, .fade-in-left, .fade-in-right, .scale-in');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });

    animatedElements.forEach(element => {
        observer.observe(element);
    });
}

// Sayaç Animasyonu
function initCounters() {
    const counters = document.querySelectorAll('.counter-value');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.5
    });

    counters.forEach(counter => {
        observer.observe(counter);
    });
}

function animateCounter(element) {
    const raw = (element.getAttribute('data-target') || '').trim();
    const target = parseInt(raw, 10);
    if (Number.isNaN(target)) {
        // Sayı değilse animasyon yapma
        element.textContent = raw || element.textContent || '';
        return;
    }
    const duration = 2000; // 2 seconds
    const step = target / (duration / 16); // 60fps
    let current = 0;

    const timer = setInterval(() => {
        current += step;
        if (current >= target) {
            element.textContent = target;
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current);
        }
    }, 16);
}

// Hash ve Çapa Düzeltmeleri
function initHashAndAnchorFixes() {
    document.addEventListener('click', (e) => {
        const a = e.target.closest('a');
        if (!a) return;

        const href = (a.getAttribute('href') || '').trim();
        if (!href) return;

        // Bootstrap bileşenlerini yoksay
        if (a.hasAttribute('data-bs-toggle') || a.hasAttribute('data-bs-target') || a.getAttribute('role') === 'tab') {
            return;
        }

        // 1) Sadece "#" linkleri
        if (href === '#' || href === '#0') {
            e.preventDefault();
            return;
        }

        // 2) Sayfa içi çapalar
        if (href.startsWith('#') && href.length > 1) {
            const target = document.querySelector(href);
            if (!target) return;

            e.preventDefault();
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }, true);

    // Also support our button-based scroll-down
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('[data-scroll-target]');
        if (!btn) return;

        const selector = btn.getAttribute('data-scroll-target');
        if (!selector) return;
        const target = document.querySelector(selector);
        if (!target) return;

        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, { capture: true, passive: true });
}


// Öncesi/Sonrası Slider
function initBeforeAfterSlider(container) {
    const slider = container.querySelector('.slider-handle');
    const beforeImage = container.querySelector('.before-image');

    if (!slider || !beforeImage) return;

    let isDown = false;
    let startX = 0;

    // Mouse olayları
    slider.addEventListener('mousedown', (e) => {
        isDown = true;
        startX = e.clientX;
        e.preventDefault();
    });

    // Dokunmatik olayları
    slider.addEventListener('touchstart', (e) => {
        isDown = true;
        startX = e.touches[0].clientX;
        e.preventDefault();
    }, { passive: false });

    document.addEventListener('mouseup', () => {
        isDown = false;
    });

    document.addEventListener('touchend', () => {
        isDown = false;
    });

    document.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        handleMove(e.clientX, container, slider, beforeImage);
    });

    document.addEventListener('touchmove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        handleMove(e.touches[0].clientX, container, slider, beforeImage);
    }, { passive: false });

    // Tıklama ile hareket
    container.addEventListener('click', (e) => {
        if (e.target === slider || e.target.closest('.slider-handle')) return;
        const containerRect = container.getBoundingClientRect();
        const position = e.clientX - containerRect.left;
        const percentage = Math.max(0, Math.min(100, (position / containerRect.width) * 100));
        updateSliderPosition(percentage, container, slider, beforeImage);
    });

    // %50 başlat
    updateSliderPosition(50, container, slider, beforeImage);
}


function handleMove(clientX, container, slider, beforeImage) {
    const containerRect = container.getBoundingClientRect();
    let position = clientX - containerRect.left;

    // Pozisyonu sınırla
    position = Math.max(0, Math.min(position, containerRect.width));

    const percentage = (position / containerRect.width) * 100;
    updateSliderPosition(percentage, container, slider, beforeImage);
}

function updateSliderPosition(percentage, container, slider, beforeImage) {
    // Slider pozisyonu
    slider.style.left = percentage + '%';

    // Slider mantığı: ÖNCE resmi SAĞDAN kırpılır.
    const rightClip = 100 - percentage;
    const clipPathValue = `inset(0 ${rightClip}% 0 0)`;

    beforeImage.style.webkitClipPath = clipPathValue;
    beforeImage.style.clipPath = clipPathValue;

    // Mobil uyumluluk için genişlik ayarı
    beforeImage.style.width = '100%';
}

// Yorumlar Slider
function initTestimonialSlider(container) {
    const track = container.querySelector('.testimonial-track');
    const cards = container.querySelectorAll('.testimonial-card');
    const prevBtn = container.querySelector('.slider-prev');
    const nextBtn = container.querySelector('.slider-next');

    if (!track || cards.length === 0) return;

    let currentIndex = 0;
    let cardsPerView = getCardsPerView();

    function getCardsPerView() {
        if (window.innerWidth >= 992) return 3;
        if (window.innerWidth >= 768) return 2;
        return 1;
    }

    function updateSlider() {
        const cardWidth = 100 / cardsPerView;
        const offset = -currentIndex * cardWidth;
        track.style.transform = `translateX(${offset}%)`;
    }

    if (prevBtn) {
        prevBtn.addEventListener('click', () => {
            if (currentIndex > 0) {
                currentIndex--;
                updateSlider();
            }
        });
    }

    if (nextBtn) {
        nextBtn.addEventListener('click', () => {
            if (currentIndex < cards.length - cardsPerView) {
                currentIndex++;
                updateSlider();
            }
        });
    }

    // Otomatik geçiş
    let autoSlide = setInterval(() => {
        if (currentIndex < cards.length - cardsPerView) {
            currentIndex++;
        } else {
            currentIndex = 0;
        }
        updateSlider();
    }, 5000);

    // Hover duraklatma
    container.addEventListener('mouseenter', () => clearInterval(autoSlide));
    container.addEventListener('mouseleave', () => {
        autoSlide = setInterval(() => {
            if (currentIndex < cards.length - cardsPerView) {
                currentIndex++;
            } else {
                currentIndex = 0;
            }
            updateSlider();
        }, 5000);
    });

    // Boyutlandırma kontrolü
    window.addEventListener('resize', () => {
        cardsPerView = getCardsPerView();
        currentIndex = 0;
        updateSlider();
    });
}

// SSS Otomatik Kaydırma
function initFaqAutoScroll() {
    const faqAccordion = document.getElementById('faqAccordion');
    if (!faqAccordion) return;

    // Accordion olayları
    const collapseElements = faqAccordion.querySelectorAll('.accordion-collapse');

    collapseElements.forEach(function (collapseEl) {
        collapseEl.addEventListener('shown.bs.collapse', function () {
            // Açıldığında kaydır
            setTimeout(function () {
                const accordionBody = collapseEl.querySelector('.accordion-body');
                if (accordionBody) {
                    // Yumuşak kaydırma
                    accordionBody.scrollIntoView({
                        behavior: 'smooth',
                        block: 'nearest',
                        inline: 'nearest'
                    });

                    // Görünmediyse yukarı kaydır
                    setTimeout(function () {
                        const rect = accordionBody.getBoundingClientRect();
                        const isVisible = rect.top >= 0 && rect.bottom <= window.innerHeight;

                        if (!isVisible) {
                            // Başlığa kaydır
                            const accordionHeader = collapseEl.previousElementSibling;
                            if (accordionHeader) {
                                accordionHeader.scrollIntoView({
                                    behavior: 'smooth',
                                    block: 'start',
                                    inline: 'nearest'
                                });
                            }
                        }
                    }, 100);
                }
            }, 100); // Gecikme
        });
    });
}

