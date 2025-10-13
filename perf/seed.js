// perf/seed.js
import http from "k6/http";
import { sleep, check } from "k6";

export const options = {
  vus: 1,
  iterations: 1,
};

export default function () {
  const BASE = __ENV.BASE || "https://localhost:7233";

  for (let i = 0; i < 200; i++) {
    const payload = JSON.stringify({
      name: `Perf Seed ${i}`,
      price: Math.round(Math.random() * 10000) / 100,
      owner: "Seeder",
      model: "K6",
      category: "Perf",
    });

    const res = http.post(`${BASE}/api/products`, payload, {
      headers: { "Content-Type": "application/json" },
      timeout: "60s",
    });

    const ok = check(res, {
      "POST /api/products returns 2xx": (r) => r.status >= 200 && r.status < 300,
    });

    if (!ok) {
      console.log(`Seed unexpected status ${res.status}. Body: ${res.body}`);
    }

    sleep(0.01);
  }
}
