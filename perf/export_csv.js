// perf/export_csv.js
import http from "k6/http";
import { check } from "k6";

export const options = {
  scenarios: {
    export_stress: {
      executor: "constant-arrival-rate",
      rate: 5,          // 5 downloads/sec
      timeUnit: "1s",
      duration: "1m",
      preAllocatedVUs: 10,
      maxVUs: 50,
    },
  },
};

export default function () {
  const BASE = __ENV.BASE || "https://localhost:7233";
  const res = http.get(`${BASE}/api/products/export.csv`, { timeout: "120s" });
  check(res, {
    "csv 2xx": (r) => r.status >= 200 && r.status < 300,
    "has header": (r) => r.body && r.body.startsWith("Id,Name,Owner,Category"),
  });
}
