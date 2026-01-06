// Navigasyon & Menü

// Navigasyon Başlatma
function initNavbar() {
    const navbar = document.getElementById('mainNavbar');
    if (!navbar) return;

    let lastScroll = 0;

    window.addEventListener('scroll', function () {
        const currentScroll = window.pageYOffset;

        // Scroll sınıfı ekle
        if (currentScroll > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }

        lastScroll = currentScroll;
    });

    updateActiveNavLink();
    setTimeout(updateActiveNavLink, 100);
    setTimeout(updateActiveNavLink, 300);
    setTimeout(updateActiveNavLink, 500);

    window.addEventListener('popstate', function () {
        setTimeout(updateActiveNavLink, 50);
    });

    window.addEventListener('load', function () {
        updateActiveNavLink();
        setTimeout(updateActiveNavLink, 100);
    });

    const navLinks = document.querySelectorAll('.navbar-nav .nav-link, .mobile-nav .nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            setTimeout(updateActiveNavLink, 200);
            setTimeout(updateActiveNavLink, 500);
        });
    });

    window.addEventListener('hashchange', updateActiveNavLink);
}

function updateActiveNavLink() {
    const desktopNavLinks = document.querySelectorAll('.navbar-nav .nav-link');
    const mobileNavLinks = document.querySelectorAll('.mobile-nav .nav-link');

    const currentPath = window.location.pathname.toLowerCase();
    const currentUrl = window.location.href.toLowerCase();

    let normalizedCurrentPath = currentPath.replace(/\/$/, '').replace(/\/index$/, '');
    if (!normalizedCurrentPath || normalizedCurrentPath === '') {
        normalizedCurrentPath = '/';
    }

    desktopNavLinks.forEach(link => {
        processNavLink(link, normalizedCurrentPath, currentPath, currentUrl);
    });

    mobileNavLinks.forEach(link => {
        processNavLink(link, normalizedCurrentPath, currentPath, currentUrl);
    });
}

function processNavLink(link, normalizedCurrentPath, currentPath, currentUrl) {
    // Remove active class first
    link.classList.remove('active');

    const href = link.getAttribute('href');
    if (!href) return;

    // Harici linkleri yoksay

    const linkPath = href.toLowerCase();

    // Link yolunu normalleştir
    let normalizedLinkPath = linkPath.replace(/\/$/, '').replace(/\/index$/, '');
    if (!normalizedLinkPath || normalizedLinkPath === '') {
        normalizedLinkPath = '/';
    }

    // Tam eşleşme kontrolü
    if (normalizedLinkPath === normalizedCurrentPath ||
        linkPath === currentPath ||
        linkPath === currentUrl ||
        (normalizedLinkPath === '/' && (normalizedCurrentPath === '/' || normalizedCurrentPath === ''))) {
        link.classList.add('active');
        return;
    }

    // Koruma: Ana sayfa kontrolü
    const isCurrentPageHome = (normalizedCurrentPath === '/' || normalizedCurrentPath === '/home' || normalizedCurrentPath === '');
    const isLinkHome = (normalizedLinkPath === '/' || normalizedLinkPath === '/home');

    if (isCurrentPageHome) {
        if (isLinkHome) {
            link.classList.add('active');
        }
        return; // Ana sayfadaysak başka kontrol yapma
    }

    // Path tabanlı eşleştirme

    // Ana sayfa linki kontrolü


    // Hizmetler eşleşmesi
    if (normalizedLinkPath.includes('/services') || normalizedLinkPath.includes('/hizmetler')) {
        if (normalizedCurrentPath.includes('/services') || normalizedCurrentPath.includes('/hizmetler')) {
            link.classList.add('active');
        }
        return;
    }

    // Projeler eşleşmesi
    if (normalizedLinkPath.includes('/projects') || normalizedLinkPath.includes('/projeler')) {
        if (normalizedCurrentPath.includes('/projects') || normalizedCurrentPath.includes('/projeler')) {
            link.classList.add('active');
        }
        return;
    }

    // Hakkımızda eşleşmesi
    if (normalizedLinkPath.includes('/home/about') || normalizedLinkPath.includes('/hakkimizda') || normalizedLinkPath.includes('/about')) {
        if (normalizedCurrentPath.includes('/home/about') || normalizedCurrentPath.includes('/hakkimizda') || normalizedCurrentPath.includes('/about')) {
            link.classList.add('active');
        }
        return;
    }

    // İletişim eşleşmesi
    if (normalizedLinkPath.includes('/contact') || normalizedLinkPath.includes('/iletisim')) {
        if (normalizedCurrentPath.includes('/contact') || normalizedCurrentPath.includes('/iletisim')) {
            link.classList.add('active');
        }
        return;
    }

    // Simülatör eşleşmesi
    if (normalizedLinkPath.includes('/simulator')) {
        if (normalizedCurrentPath.includes('/simulator')) {
            link.classList.add('active');
        }
        return;
    }
}

// Mobil Menü Başlatma
function initMobileMenuButton() {
    const menuButton = document.querySelector('[data-bs-target="#mobileMenu"]');
    const offcanvasEl = document.getElementById('mobileMenu');

    if (!menuButton || !offcanvasEl) {
        // Elementler bekleniyor
        setTimeout(initMobileMenuButton, 100);
        return;
    }

    // Tıklanabilirliği zorla
    const buttonStyles = window.getComputedStyle(menuButton);

    // Butonu zorla tıklanabilir hale getir
    menuButton.style.pointerEvents = 'auto';
    menuButton.style.cursor = 'pointer';
    menuButton.style.zIndex = '1001';
    menuButton.style.position = 'relative';
    menuButton.style.display = 'block';
    menuButton.style.visibility = 'visible';
    menuButton.style.opacity = '1';

    // Parent kontrolü
    const parentContainer = menuButton.parentElement;
    if (parentContainer) {
        parentContainer.style.pointerEvents = 'auto';
        parentContainer.style.zIndex = '1001';
        parentContainer.style.position = 'relative';
    }

    // Bootstrap bekleniyor
    let attempts = 0;
    const maxAttempts = 100; // 5 saniye (50ms * 100)

    function waitForBootstrap() {
        attempts++;

        if (typeof bootstrap !== 'undefined' && bootstrap.Offcanvas) {
            // Bootstrap yüklendi, temizle
            const newButton = menuButton.cloneNode(true);
            const parent = menuButton.parentNode;
            parent.replaceChild(newButton, menuButton);

            // Yeni stil uygula
            newButton.style.pointerEvents = 'auto';
            newButton.style.cursor = 'pointer';
            newButton.style.zIndex = '1001';
            newButton.style.position = 'relative';
            newButton.style.display = 'block';
            newButton.style.visibility = 'visible';
            newButton.style.opacity = '1';

            // Tıklama olayı ekle
            newButton.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                e.stopImmediatePropagation();

                try {
                    // Örneği al veya oluştur
                    let offcanvas = bootstrap.Offcanvas.getInstance(offcanvasEl);
                    if (!offcanvas) {
                        offcanvas = new bootstrap.Offcanvas(offcanvasEl);
                    }
                    offcanvas.show();
                } catch (error) {
                    // Yedek: Manuel aç
                    offcanvasEl.classList.add('show');
                    document.body.classList.add('modal-open');
                    const backdrop = document.createElement('div');
                    backdrop.className = 'modal-backdrop fade show';
                    backdrop.style.zIndex = '1040';
                    document.body.appendChild(backdrop);
                }
            }, true); // Öncelikli çalıştır

            // Yedek olay dinleyicisi
            newButton.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
            }, false);

        } else if (attempts < maxAttempts) {
            setTimeout(waitForBootstrap, 50);
        } else {
            // Bootstrap yoksa manuel ekle
            menuButton.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                offcanvasEl.classList.add('show');
                document.body.classList.add('modal-open');
                const backdrop = document.createElement('div');
                backdrop.className = 'modal-backdrop fade show';
                backdrop.style.zIndex = '1040';
                document.body.appendChild(backdrop);
            }, true);
        }
    }

    waitForBootstrap();
}

// Yükleme olayları
document.addEventListener('DOMContentLoaded', function () {
    initMobileMenuButton();
});

// Tekrar dene (Load)
window.addEventListener('load', function () {
    setTimeout(initMobileMenuButton, 100);
});

// Mobil Menü Düzeltmesi
function initMobileOffcanvasNav() {
    const offEl = document.getElementById('mobileMenu');
    if (!offEl) return;

    offEl.addEventListener('click', (e) => {
        const link = e.target.closest('a');
        if (!link) return;
        const href = (link.getAttribute('href') || '').trim();
        if (!href || href === '#' || href === '#0') return;

        // Harici linkleri yoksay
        if (link.target === '_blank' || href.startsWith('http')) return;

        // Menüyü kapat
        if (typeof bootstrap !== 'undefined' && bootstrap.Offcanvas) {
            const inst = bootstrap.Offcanvas.getInstance(offEl) || new bootstrap.Offcanvas(offEl);
            inst.hide();
        }
    }, { capture: true });
}

