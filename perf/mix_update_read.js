// perf/mix_update_read.js
import http from "k6/http";
import { check, sleep } from "k6";
export const options = {
  scenarios: {
    mix: {
      executor: "ramping-vus",
      startVUs: 1,
      stages: [
        { duration: "1m", target: 20 },
        { duration: "1m", target: 40 },
        { duration: "1m", target: 0 },
      ],
    },
  },
};
function randPrice() {
  return Math.round((50 + Math.random() * 500) * 100) / 100;
}
export default function () {
  const BASE = __ENV.BASE || "https://localhost:7233";
  // 1) create a product
  const create = http.post(`${BASE}/api/products`, JSON.stringify({
    name: `k6-mix-${__VU}-${Date.now()}`,
    price: randPrice(),
    owner: "PerfBot",
    model: "Mix",
    category: "Load",
  }), { headers: { "Content-Type": "application/json" }, timeout: "60s" });

  check(create, { "create 2xx/201": r => r.status >= 200 && r.status < 300 });
  const id = (create.json() || {}).id;

  // 2) update price once or twice
  if (id) {
    for (let i = 0; i < 2; i++) {
      const update = http.put(`${BASE}/api/products/${id}`, JSON.stringify({
        name: `k6-mix-${__VU}`,
        price: randPrice(),
        owner: "PerfBot",
        model: "Mix",
        category: "Load",
      }), { headers: { "Content-Type": "application/json" }, timeout: "60s" });
      check(update, { "update 2xx": r => r.status >= 200 && r.status < 300 });
      sleep(0.1);
    }
  }

  // 3) read (search)
  const read = http.get(`${BASE}/api/products/search?q=PerfBot`, { timeout: "60s" });
  check(read, { "read 2xx": r => r.status >= 200 && r.status < 300 });
  sleep(0.1);
}
