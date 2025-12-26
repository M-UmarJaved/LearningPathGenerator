
export default {
  bootstrap: () => import('./main.server.mjs').then(m => m.default),
  inlineCriticalCss: false,
  baseHref: '/',
  locale: undefined,
  routes: [
  {
    "renderMode": 0,
    "redirectTo": "/home",
    "route": "/"
  },
  {
    "renderMode": 0,
    "route": "/home"
  },
  {
    "renderMode": 0,
    "route": "/features"
  },
  {
    "renderMode": 0,
    "route": "/about"
  },
  {
    "renderMode": 0,
    "route": "/register"
  },
  {
    "renderMode": 0,
    "route": "/reset-password"
  },
  {
    "renderMode": 0,
    "route": "/skill-table"
  },
  {
    "renderMode": 0,
    "route": "/skill-assessment"
  },
  {
    "renderMode": 0,
    "route": "/dashboard"
  },
  {
    "renderMode": 0,
    "route": "/assessment-result"
  },
  {
    "renderMode": 0,
    "route": "/concept-dependency"
  },
  {
    "renderMode": 0,
    "route": "/learning-skill-select"
  },
  {
    "renderMode": 0,
    "route": "/learning-path/*"
  },
  {
    "renderMode": 0,
    "route": "/course-player/*"
  },
  {
    "renderMode": 0,
    "route": "/completion"
  },
  {
    "renderMode": 0,
    "redirectTo": "/home",
    "route": "/**"
  }
],
  entryPointToBrowserMapping: undefined,
  assets: {
    'index.csr.html': {size: 732, hash: 'dea530896b2f0fe74af5fa88ad25019376c281d2f99b9a004fd8d41459318c59', text: () => import('./assets-chunks/index_csr_html.mjs').then(m => m.default)},
    'index.server.html': {size: 1272, hash: 'e149c27b3985e2d46b66b83d4bbafe903620a2d0d92d52e7cb3a8824d9615790', text: () => import('./assets-chunks/index_server_html.mjs').then(m => m.default)}
  },
};
