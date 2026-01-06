// Ana Sayfa JS

// Kaydırma İşlevleri
function initScrollContainer(container, options) {
    if (!container) return;

    const {
        direction = 'horizontal', // 'horizontal' veya 'vertical'
        checkCardContent = false, // Services için true, Reviews için false
        cardContentSelector = '.service-card-content' // Kart içi scroll kontrolü için selector
    } = options || {};

    // Sürükleme değişkenleri
    let isDragging = false;
    let startPos = 0;
    let scrollStart = 0;
    let isHovering = false;
    let scrollAnimationFrame = null;

    const isHorizontal = direction === 'horizontal';
    const overflowProp = isHorizontal ? 'overflowX' : 'overflowY';
    const scrollProp = isHorizontal ? 'scrollLeft' : 'scrollTop';
    const clientSizeProp = isHorizontal ? 'clientWidth' : 'clientHeight';
    const scrollSizeProp = isHorizontal ? 'scrollWidth' : 'scrollHeight';
    const offsetProp = isHorizontal ? 'offsetLeft' : 'offsetTop';

    const pagePosProp = isHorizontal ? 'pageX' : 'pageY';
    const clientPosProp = isHorizontal ? 'clientX' : 'clientY'; // Touch için clientX/Y kullanacağız
    const rectPosProp = isHorizontal ? 'left' : 'top';

    // Hover kontrolü
    container.addEventListener('mouseenter', function () {
        isHovering = true;
        if (!isDragging) {
            this.style[overflowProp] = 'auto';
        }
    });

    container.addEventListener('mouseleave', function () {
        isHovering = false;
        if (!isDragging) {
            this.style[overflowProp] = 'hidden';
        }
        // Drag'i de bitir
        isDragging = false;
        this.style.cursor = 'grab';
        this.style.userSelect = '';
    });

    // Sürükleyerek kaydırma
    container.addEventListener('mousedown', function (e) {
        // Sadece sol tık
        if (e.button !== 0) return;

        // İç kaydırma kontrolü
        if (checkCardContent) {
            const cardContent = e.target.closest(cardContentSelector);
            if (cardContent) {
                const canScrollVertically = cardContent.scrollHeight > cardContent.clientHeight;
                if (canScrollVertically) {
                    // Kart içinde scroll yapılabilir, drag yapma
                    return;
                }
            }
        }

        isDragging = true;
        startPos = e[pagePosProp] - container[offsetProp];
        scrollStart = container[scrollProp];
        container.style.cursor = 'grabbing';
        container.style.userSelect = 'none';
        e.preventDefault();
    });

    // Dokunmatik Sürükleme (Touch)
    container.addEventListener('touchstart', function (e) {
        // İç kaydırma kontrolü (Touch)
        if (checkCardContent) {
            const cardContent = e.target.closest(cardContentSelector);
            if (cardContent) {
                const canScrollVertically = cardContent.scrollHeight > cardContent.clientHeight;
                if (canScrollVertically) {
                    // Kart içinde scroll yapılabilir
                    // Eğer dikey kaydırma yapıyorsa (kart içi), yatay drag'i engelleme
                    // Bu kısım touchmove'da daha iyi yönetilebilir
                    // Şimdilik basitçe: kart içindeyse global drag'i başlatma, ama kullanıcı yatay kaydırmak istiyor olabilir.
                    // İyileştirme: Touchmove'da yön analizi yap.
                }
            }
        }

        const touch = e.touches[0];
        isDragging = true;
        // Touch events don't have pageX reliable everywhere, use clientX + scroll or just clientX for delta
        // Daha basit: clientX ile initial pos al
        startPos = touch[clientPosProp];
        scrollStart = container[scrollProp];

        // Auto scroll'u geçici durdur (setupInfiniteScroll ile entegre çalışır çünkü o da touchstart dinliyor)
    }, { passive: true });

    // Mousemove'u document'e ekle (fare container dışına çıktığında da çalışsın)
    const mousemoveHandler = function (e) {
        if (!isDragging) return;

        e.preventDefault();
        const rect = container.getBoundingClientRect();
        const pos = e[pagePosProp] - rect[rectPosProp];
        const walk = (pos - startPos) * 1.5; // Scroll hızı
        container[scrollProp] = scrollStart - walk;
    };

    document.addEventListener('mousemove', mousemoveHandler);

    // Touch Move
    const touchmoveHandler = function (e) {
        if (!isDragging) return;

        const touch = e.touches[0];
        const currentPos = touch[clientPosProp];
        // Fare'deki gibi container offset hesabına gerek yok, delta üzerinden gidelim
        const diff = currentPos - startPos;

        // Eğer dikey scroll (sayfa kaydırma) yapılmak isteniyorsa ve biz yatay drag yapıyorsak:
        // Bu durumu ayırt etmek lazım ama şimdilik doğrudan uygulayalım.
        // Yalnız, kullanıcı aşağı kaydırmak istiyorsa preventDefault'u çağırmamalıyız.

        // Basit çözüm: Eğer yatay hareket dikeyden fazlaysa drag say, yoksa sayfa scroll'a izin ver.
        // Ancak bu 'startPos' logic'ini biraz değiştirir. Şimdilik temel implementasyon:

        container[scrollProp] = scrollStart - (diff * 1.5);

        // Yatay kaydırma yapıyorsak sayfa scroll'unu engelle (sadece horizontal modda)
        if (isHorizontal && Math.abs(diff) > 5) {
            if (e.cancelable) e.preventDefault();
        }
    };

    document.addEventListener('touchmove', touchmoveHandler, { passive: false });

    const mouseupHandler = function () {
        if (isDragging) {
            isDragging = false;
            container.style.cursor = 'grab';
            container.style.userSelect = '';
        }
    };

    document.addEventListener('mouseup', mouseupHandler);

    const touchendHandler = function () {
        isDragging = false;
    };
    document.addEventListener('touchend', touchendHandler);
}

// Kart içi dikey kaydırma
function setupServiceCardScroll() {
    document.querySelectorAll('.service-card-content').forEach(function (cardContent) {
        cardContent.addEventListener('wheel', function (e) {
            // Eğer scroll yapılabiliyorsa (içerik taşıyorsa)
            const canScroll = this.scrollHeight > this.clientHeight;
            if (canScroll) {
                // Scroll yapılabilir alanın içinde miyiz?
                const isAtTop = this.scrollTop <= 0;
                const isAtBottom = this.scrollTop + this.clientHeight >= this.scrollHeight - 1;

                // Yukarı scroll yapılıyor ve en üstteysek
                if (e.deltaY < 0 && isAtTop) {
                    // Scroll yapılacak bir şey yok, yatay scroll'a izin ver
                    return;
                }

                // Aşağı scroll yapılıyor ve en alttaysak
                if (e.deltaY > 0 && isAtBottom) {
                    // Scroll yapılacak bir şey yok, yatay scroll'a izin ver
                    return;
                }

                // Normal scroll yap - varsayılan davranış zaten çalışacak
            } else {
                // Kart içinde scroll yapılamıyorsa, yatay scroll'a izin ver
                return;
            }
        }, { passive: true });
    });
}

// Resim hata yönetimi
function setupServiceImageErrorHandling() {
    document.querySelectorAll('.service-image img').forEach(function (img) {
        img.addEventListener('error', function () {
            const serviceImage = this.closest('.service-image');
            if (serviceImage) {
                const serviceCard = serviceImage.closest('.service-card');
                if (serviceCard) {
                    // Icon class'ı bul (varsa)
                    let iconClass = 'bi bi-paint-bucket';
                    const existingIcon = serviceCard.querySelector('.service-icon');
                    if (existingIcon) {
                        const iconElement = existingIcon.querySelector('i');
                        if (iconElement) {
                            iconClass = iconElement.className;
                        }
                    }
                    // Service image'i icon ile değiştir
                    serviceImage.outerHTML = '<div class="service-icon"><i class="' + iconClass + '"></i></div>';
                }
            }
        });
    });
}

// SSS Akordeon
function setupFaqAccordion() {
    // Bootstrap yüklendikten sonra accordion'u başlat
    window.addEventListener('load', function () {
        // Accordion butonlarına click event'i ekle (Bootstrap yüklenmemişse manuel toggle)
        const accordionButtons = document.querySelectorAll('.faq-section .accordion-button');
        accordionButtons.forEach(function (button) {
            // Mevcut event listener'ları temizle
            const newButton = button.cloneNode(true);
            button.parentNode.replaceChild(newButton, button);

            newButton.addEventListener('click', function (e) {
                e.stopPropagation();

                const targetId = this.getAttribute('data-bs-target');
                if (!targetId) return;

                const target = document.querySelector(targetId);
                if (!target) return;

                // Bootstrap varsa kullan, yoksa manuel toggle
                if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                    try {
                        const collapseInstance = bootstrap.Collapse.getOrCreateInstance(target, {
                            toggle: true
                        });
                    } catch (err) {
                        // Hata olursa manuel toggle yap
                        manualToggle(this, target);
                    }
                } else {
                    manualToggle(this, target);
                }
            });
        });

        function manualToggle(button, target) {
            const isCollapsed = target.classList.contains('show');
            if (isCollapsed) {
                target.classList.remove('show');
                button.classList.add('collapsed');
                button.setAttribute('aria-expanded', 'false');
            } else {
                // Diğer açık accordion'ları kapat
                document.querySelectorAll('.faq-section .accordion-collapse.show').forEach(function (openItem) {
                    if (openItem !== target) {
                        openItem.classList.remove('show');
                        const openButton = document.querySelector('[data-bs-target="#' + openItem.id + '"]');
                        if (openButton) {
                            openButton.classList.add('collapsed');
                            openButton.setAttribute('aria-expanded', 'false');
                        }
                    }
                });

                target.classList.add('show');
                button.classList.remove('collapsed');
                button.setAttribute('aria-expanded', 'true');
            }
        }
    });
}

// Sonsuz Kaydırma
function setupInfiniteScroll(containerSelector, wrapperSelector, options) {
    // Değişkenleri en başta tanımla (minify uyumluluğu için var kullan)
    var container = document.querySelector(containerSelector);
    var wrapper = document.querySelector(wrapperSelector);

    if (!container || !wrapper) return;

    // Options parsing
    var opts = options || {};
    var speed = opts.speed || 0.6;
    var pauseOnHover = opts.pauseOnHover || false;

    // Değişkenler
    var originalChildren, originalCount, singleSetWidth, containerWidth;
    var minSetsByWidth, minSetsByCount, neededSets;
    var isPaused = false;
    var scrollAccumulator = 0;
    var i, j, clone, animatedChildren;

    // 1. Temizlik
    if (container.dataset.infiniteScrollId) {
        cancelAnimationFrame(Number(container.dataset.infiniteScrollId));
        delete container.dataset.infiniteScrollId;
    }

    // Eski klonları temizle
    var existingClones = wrapper.querySelectorAll('[aria-hidden="true"]');
    for (i = 0; i < existingClones.length; i++) {
        existingClones[i].remove();
    }
    container.removeAttribute('data-infinite-scroll-init');

    // 2. Stiller
    container.style.scrollSnapType = 'none';
    container.style.scrollBehavior = 'auto';
    container.style.overflowX = 'hidden';

    wrapper.style.display = 'flex';
    wrapper.style.width = 'max-content';
    wrapper.style.flexWrap = 'nowrap';

    // 3. Mantık
    originalChildren = Array.from(wrapper.children);
    originalCount = originalChildren.length;

    if (originalCount === 0) return;

    singleSetWidth = wrapper.scrollWidth;
    containerWidth = window.innerWidth;

    // Ekranı dolduracak kadar klonla
    minSetsByWidth = Math.ceil((containerWidth * 3) / (singleSetWidth || 1));
    minSetsByCount = originalCount < 5 ? 6 : 4;
    neededSets = Math.max(minSetsByWidth, minSetsByCount);

    // Yardımcı: Görünür yap
    function makeVisible(el) {
        el.classList.remove('fade-in', 'fade-in-left', 'fade-in-right', 'scale-in', 'hover-lift');
        el.classList.add('visible');
        el.style.opacity = '1';
        el.style.transform = 'translateY(0) scale(1)';
    }

    // Yardımcı: Odaklanabilirliği kaldır
    function makeNonFocusable(parent) {
        var selectorExceptLinks = 'button, input, select, textarea, [tabindex]:not([tabindex="-1"])';
        var focusables, links, k;

        if (parent.matches && parent.matches(selectorExceptLinks)) {
            parent.setAttribute('tabindex', '-1');
        }

        focusables = parent.querySelectorAll(selectorExceptLinks);
        for (k = 0; k < focusables.length; k++) {
            focusables[k].setAttribute('tabindex', '-1');
        }

        links = parent.querySelectorAll('a[href]');
        for (k = 0; k < links.length; k++) {
            links[k].style.pointerEvents = 'auto';
            links[k].removeAttribute('tabindex');
        }
    }

    // Klonları ekle
    for (i = 0; i < neededSets - 1; i++) {
        for (j = 0; j < originalChildren.length; j++) {
            clone = originalChildren[j].cloneNode(true);
            clone.setAttribute('aria-hidden', 'true');

            makeVisible(clone);
            makeNonFocusable(clone);

            animatedChildren = clone.querySelectorAll('.fade-in, .fade-in-left, .fade-in-right, .scale-in, .hover-lift');
            for (var m = 0; m < animatedChildren.length; m++) {
                makeVisible(animatedChildren[m]);
            }

            wrapper.appendChild(clone);
        }
    }

    // 4. Animasyon Döngüsü
    if (pauseOnHover) {
        container.addEventListener('mouseenter', function () { isPaused = true; });
        container.addEventListener('mouseleave', function () { isPaused = false; });
        container.addEventListener('touchstart', function () { isPaused = true; }, { passive: true });
        container.addEventListener('touchend', function () {
            setTimeout(function () { isPaused = false; }, 1000);
        }, { passive: true });
    }

    // İlk pozisyonu sıfırla
    container.scrollLeft = 0;

    // Animasyon fonksiyonu
    function animate() {
        var firstOriginal, firstClone, cycleDistance, movePixels, reqId;

        if (!isPaused) {
            firstOriginal = wrapper.children[0];
            firstClone = wrapper.children[originalCount];

            if (firstOriginal && firstClone) {
                cycleDistance = firstClone.offsetLeft - firstOriginal.offsetLeft;

                if (cycleDistance > 0 && container.scrollLeft >= cycleDistance) {
                    container.scrollLeft -= cycleDistance;
                }
            }

            scrollAccumulator += speed;
            if (scrollAccumulator >= 1) {
                movePixels = Math.floor(scrollAccumulator);
                container.scrollLeft += movePixels;
                scrollAccumulator -= movePixels;
            }
        }

        reqId = requestAnimationFrame(animate);
        container.dataset.infiniteScrollId = reqId.toString();
    }

    // Başlat
    container.setAttribute('data-infinite-scroll-init', 'true');
    animate();
}


// Ana Başlatma
function initHomePage() {
    // Telefon butonu - Global site.js içinde yönetiliyor

    // Testimonial slider initialization
    const testimonialSlider = document.querySelector('.testimonial-slider');
    if (testimonialSlider && typeof initTestimonialSlider === 'function') {
        initTestimonialSlider(testimonialSlider);
    }

    // Services scroll container
    const servicesScrollContainer = document.querySelector('.services-scroll-container');
    if (servicesScrollContainer) {
        // Drag functionality
        initScrollContainer(servicesScrollContainer, {
            direction: 'horizontal',
            checkCardContent: true,
            cardContentSelector: '.service-card-content'
        });

        // Sonsuz Kaydırma - Hover'da duraklat
        setupInfiniteScroll('.services-scroll-container', '.services-scroll-wrapper', {
            speed: 0.6,
            pauseOnHover: true
        });
    }

    // Service card content - Mouse wheel ile dikey scroll
    setupServiceCardScroll();

    // Service image error handling
    setupServiceImageErrorHandling();

    // Yorumlar kaydırma - Sadece otomatik
    const reviewsScrollContainer = document.querySelector('.reviews-scroll-container');
    if (reviewsScrollContainer) {
        // Etkileşimi engelle - sadece otomatik akış olsun
        reviewsScrollContainer.style.pointerEvents = 'none';
        reviewsScrollContainer.style.userSelect = 'none';

        // Sonsuz Kaydırma - Duraklatma yok
        setupInfiniteScroll('.reviews-scroll-container', '.reviews-scroll-wrapper', {
            speed: 0.6,
            pauseOnHover: false
        });
    }

    // SSS Akordeon
    setupFaqAccordion();
}

// DOM Hazır
document.addEventListener('DOMContentLoaded', initHomePage);
