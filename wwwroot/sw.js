// Service Worker Sıfırlama ve Devre Dışı Bırakma (v2.1)
// Mevcut cache'leri temizler ve kontrolü sunucuya bırakır.

self.addEventListener('install', (event) => {
  // Hemen aktif ol ve eski SW'yi ez
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    // Tüm cache'leri sil
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((name) => {
          console.log('[SW Reset] Cache siliniyor:', name);
          return caches.delete(name);
        })
      );
    }).then(() => {
      // Tüm açık sekmelerin kontrolünü al
      return self.clients.claim();
    })
  );
});

// Not: Fetch dinleyicisi performans uyarısı nedeniyle kaldırıldı. 
// Artık tüm istekler doğrudan sunucudan (Network) karşılanacak.
