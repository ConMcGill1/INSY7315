import http from 'k6/http';

export const options = { vus: 1, iterations: 1 };

function rndFrom(arr) { return arr[Math.floor(Math.random() * arr.length)]; }
function rndInt(min, max) { return Math.floor(Math.random() * (max - min + 1)) + min; }

export default function () {
    const BASE = __ENV.BASE || 'http://localhost:5085'; 
    for (let i = 0; i < 200; i++) {
        const body = JSON.stringify({
            name: `Item ${i} ${rndFrom(['phone', 'tv', 'laptop', 'tablet', 'watch'])}`,
            price: rndInt(100, 20000),
            owner: rndFrom(['OwnerA', 'OwnerB', 'OwnerC']),
            category: rndFrom(['Phones', 'TVs', 'Computers', 'Wearables']),
            model: `Model-${rndInt(1, 500)}`
        });
        const res = http.post(`${BASE}/api/products`, body, {
            headers: { 'Content-Type': 'application/json' }
        });
        if (res.status !== 200) {
            console.log('Seed failed with status', res.status);
        }
    }
}
