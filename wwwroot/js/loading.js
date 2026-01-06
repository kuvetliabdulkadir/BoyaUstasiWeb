// Yükleme Ekranı

(function () {
    'use strict';

    // Ekranı gizle
    window.addEventListener('load', hideLoadingScreen);
    document.addEventListener('DOMContentLoaded', hideLoadingScreen);

    // Yedek gizleme
    setTimeout(function () {
        hideLoadingScreen();
    }, 800);

    function hideLoadingScreen() {
        const loadingScreen = document.getElementById('loading-screen');
        if (loadingScreen && !loadingScreen.classList.contains('hidden')) {
            loadingScreen.classList.add('hidden');

            // DOM'dan kaldır
            setTimeout(function () {
                loadingScreen.style.display = 'none';
            }, 500);
        }
    }
})();

