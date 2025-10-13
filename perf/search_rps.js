// perf/search_rps.js
import http from "k6/http";
import { check } from "k6";
export const options = {
  scenarios: {
    rps_ramp: {
      executor: "ramping-arrival-rate",
      startRate: 10,           // start at 10 req/s
      timeUnit: "1s",
      preAllocatedVUs: 20,
      maxVUs: 200,
      stages: [
        { target: 50, duration: "1m" },
        { target: 100, duration: "1m" },
        { target: 150, duration: "1m" },
        { target: 0, duration: "30s" },
      ],
    },
  },
};
export default function () {
  const BASE = __ENV.BASE || "https://localhost:7233";
  const qs = [
    "", "phone", "owner=Seeder", "model=k6", "category=perf",
  ];
  const q = qs[Math.floor(Math.random() * qs.length)];
  const url = `${BASE}/api/products/search${q ? `?q=${encodeURIComponent(q)}` : ""}`;
  const res = http.get(url, { timeout: "60s" });
  check(res, { "2xx": (r) => r.status >= 200 && r.status < 300 });
}
