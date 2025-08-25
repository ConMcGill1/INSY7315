
const CACHE = 'insy7315-cache-v1';


const PRECACHE_URLS = [
    '/', '/offline.html',
    '/css/site.css', '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/favicon.ico'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE).then(c => c.addAll(PRECACHE_URLS))
    );
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k)))
        )
    );
    self.clients.claim();
});

self.addEventListener('fetch', (event) => {
    const req = event.request;
    if (req.method !== 'GET') return;

 
    const isNavigate = req.mode === 'navigate' ||
        (req.headers.get('accept') || '').includes('text/html');

    if (isNavigate) {
        event.respondWith(
            caches.match(req).then(cached => {
                const fetchPromise = fetch(req)
                    .then(net => {
                        caches.open(CACHE).then(c => c.put(req, net.clone()));
                        return net;
                    })
                    .catch(() => cached || caches.match('/offline.html'));
                return cached || fetchPromise;
            })
        );
        return;
    }

 
    event.respondWith(
        caches.match(req).then(cached =>
            cached || fetch(req).then(net => {
                if (net && net.status === 200) {
                    const copy = net.clone();
                    caches.open(CACHE).then(c => c.put(req, copy));
                }
                return net;
            }).catch(() => cached || caches.match('/offline.html'))
        )
    );
});
