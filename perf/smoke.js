// perf/smoke.js
import http from "k6/http";
import { check, sleep } from "k6";

export const options = { vus: 1, iterations: 1 };

export default function () {
  const BASE = __ENV.BASE || "https://localhost:7233";
  // simple read
  const res = http.get(`${BASE}/api/products/search`, { timeout: "60s" });
  check(res, { "GET /search returns 2xx": (r) => r.status >= 200 && r.status < 300 });
  sleep(1);
}
