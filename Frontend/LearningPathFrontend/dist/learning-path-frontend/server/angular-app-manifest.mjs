
export default {
  bootstrap: () => import('./main.server.mjs').then(m => m.default),
  inlineCriticalCss: true,
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
    "route": "/profile"
  },
  {
    "renderMode": 0,
    "route": "/progress-graph"
  },
  {
    "renderMode": 0,
    "route": "/my-courses"
  },
  {
    "renderMode": 0,
    "route": "/assessment-result"
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
    'index.csr.html': {size: 19985, hash: '4b7d304c37fc023671431cc51af25f0b655509fe8c63510af8a00ba4d19cbf17', text: () => import('./assets-chunks/index_csr_html.mjs').then(m => m.default)},
    'index.server.html': {size: 1177, hash: '1e5fa05a573a93685f30071851f8995c87587ea4cf827ad89ff1d497f3683afe', text: () => import('./assets-chunks/index_server_html.mjs').then(m => m.default)},
    'styles-UK6DQUAJ.css': {size: 249308, hash: 'IUopT1BsaLU', text: () => import('./assets-chunks/styles-UK6DQUAJ_css.mjs').then(m => m.default)}
  },
};
