import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../auth.service';
import {
  ProgressGraphEdgeDto,
  ProgressGraphNodeDto,
  ProgressGraphResponseDto,
  ProgressGraphService
} from './progress-graph.service';

type PositionedNode = ProgressGraphNodeDto & {
  x: number;
  y: number;
};

type PositionedEdge = ProgressGraphEdgeDto & {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
};

@Component({
  selector: 'progress-graph',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './progress-graph.component.html',
  styleUrls: ['../dashboard/dashboard.component.css', './progress-graph.component.css']
})
export class ProgressGraphComponent implements OnInit {
  private router = inject(Router);
  private auth = inject(AuthService);
  private api = inject(ProgressGraphService);

  userName = '';

  loading = signal(false);
  error = signal<string | null>(null);
  data = signal<ProgressGraphResponseDto | null>(null);

  selectedNodeId = signal<number | null>(null);

  positionedNodes = computed<PositionedNode[]>(() => {
    const d = this.data();
    if (!d) return [];

    const nodes = [...d.nodes].sort((a, b) => (a.level - b.level) || (a.label.localeCompare(b.label)));

    const byLevel = new Map<number, ProgressGraphNodeDto[]>();
    for (const n of nodes) {
      const arr = byLevel.get(n.level) ?? [];
      arr.push(n);
      byLevel.set(n.level, arr);
    }

    const levels = [...byLevel.keys()].sort((a, b) => a - b);

    const leftPad = 80;
    const topPad = 110;
    const colGap = 280;
    const rowGap = 110;

    const positioned: PositionedNode[] = [];
    for (const lvl of levels) {
      const arr = byLevel.get(lvl) ?? [];

      for (let i = 0; i < arr.length; i++) {
        positioned.push({
          ...arr[i],
          x: leftPad + lvl * colGap,
          y: topPad + i * rowGap
        });
      }
    }

    return positioned;
  });

  positionedEdges = computed<PositionedEdge[]>(() => {
    const d = this.data();
    if (!d) return [];

    const nodes = this.positionedNodes();
    const pos = new Map<number, PositionedNode>();
    for (const n of nodes) pos.set(n.id, n);

    // Node box dimensions (must match CSS)
    const w = 220;
    const h = 64;

    const edges: PositionedEdge[] = [];
    for (const e of d.edges) {
      const from = pos.get(e.fromId);
      const to = pos.get(e.toId);
      if (!from || !to) continue;

      edges.push({
        ...e,
        x1: from.x + w,
        y1: from.y + h / 2,
        x2: to.x,
        y2: to.y + h / 2
      });
    }

    return edges;
  });

  svgWidth = computed(() => {
    const nodes = this.positionedNodes();
    if (nodes.length === 0) return 900;

    // Node box width + right padding
    const w = 220;
    const rightPad = 120;
    const maxX = Math.max(...nodes.map((n) => n.x));
    return maxX + w + rightPad;
  });

  svgHeight = computed(() => {
    const nodes = this.positionedNodes();
    if (nodes.length === 0) return 520;

    const h = 64;
    const bottomPad = 120;
    const maxY = Math.max(...nodes.map((n) => n.y));
    return maxY + h + bottomPad;
  });

  selectedNode = computed(() => {
    const id = this.selectedNodeId();
    const nodes = this.positionedNodes();
    return nodes.find((n) => n.id === id) ?? null;
  });

  // ====== Chart metrics derived from graph ======
  private courseNodes = computed(() => {
    const d = this.data();
    return (d?.nodes ?? []).filter((n) => (n.type || '').toLowerCase() === 'course');
  });

  private skillNodes = computed(() => {
    const d = this.data();
    return (d?.nodes ?? []).filter((n) => (n.type || '').toLowerCase() === 'skill');
  });

  totalCourses = computed(() => this.courseNodes().length);

  totalVideos = computed(() => {
    const courses = this.courseNodes();
    let total = 0;
    for (const c of courses) {
      const tv = typeof c.totalVideos === 'number' ? c.totalVideos : 0;
      total += tv;
    }
    return total;
  });

  totalSkills = computed(() => this.skillNodes().length);

  pctCourses = computed(() => {
    const d = this.data();
    const total = this.totalCourses();
    const done = d?.summary?.coursesCompleted ?? 0;
    return total > 0 ? Math.max(0, Math.min(1, done / total)) : 0;
  });

  pctVideos = computed(() => {
    const d = this.data();
    const total = this.totalVideos();
    const watched = d?.summary?.videosWatched ?? 0;
    return total > 0 ? Math.max(0, Math.min(1, watched / total)) : 0;
  });

  pctSkills = computed(() => {
    const d = this.data();
    const total = this.totalSkills();
    const learned = d?.summary?.skillsLearned ?? 0;
    return total > 0 ? Math.max(0, Math.min(1, learned / total)) : 0;
  });

  overallPct = computed(() => {
    // Simple average across the three progress dimensions.
    const a = this.pctCourses();
    const b = this.pctVideos();
    const c = this.pctSkills();
    return Math.max(0, Math.min(1, (a + b + c) / 3));
  });

  gaugeAngleDeg = computed(() => {
    // Gauge runs from -90deg (left) to +90deg (right)
    return -90 + this.overallPct() * 180;
  });

  gaugeArcPath = computed(() => {
    // Semicircle gauge from left (180deg) to right (0deg)
    const pct = this.overallPct();

    const cx = 180;
    const cy = 180;
    const r = 120;

    const startX = cx - r;
    const startY = cy;

    // Map pct 0..1 to angle PI..0
    const theta = Math.PI * (1 - pct);
    const endX = cx + r * Math.cos(theta);
    const endY = cy - r * Math.sin(theta);

    // Large-arc flag never needed for a semicircle segment (<= 180deg)
    return `M ${startX} ${startY} A ${r} ${r} 0 0 1 ${endX} ${endY}`;
  });

  // Concentric ring values (for SVG stroke-dashoffset)
  ring = computed(() => {
    const r1 = 98;
    const r2 = 78;
    const r3 = 58;
    const c1 = 2 * Math.PI * r1;
    const c2 = 2 * Math.PI * r2;
    const c3 = 2 * Math.PI * r3;

    const p1 = this.pctCourses();
    const p2 = this.pctVideos();
    const p3 = this.pctSkills();

    return {
      r1,
      r2,
      r3,
      c1,
      c2,
      c3,
      o1: c1 * (1 - p1),
      o2: c2 * (1 - p2),
      o3: c3 * (1 - p3)
    };
  });

  // Skill breakdown for the bottom-left chart (top 6 skills)
  skillBreakdown = computed(() => {
    const d = this.data();
    if (!d) return [] as Array<{ name: string; completed: number; total: number; pct: number }>;

    const typeById = new Map<number, string>();
    const completedByCourseId = new Map<number, boolean>();
    for (const n of d.nodes) {
      typeById.set(n.id, (n.type || '').toLowerCase());
      if ((n.type || '').toLowerCase() === 'course') {
        completedByCourseId.set(n.id, n.completed === true);
      }
    }

    const outgoing = new Map<number, number[]>();
    for (const e of d.edges) {
      const arr = outgoing.get(e.fromId) ?? [];
      arr.push(e.toId);
      outgoing.set(e.fromId, arr);
    }

    const skills = d.nodes.filter((n) => (n.type || '').toLowerCase() === 'skill');
    const rows: Array<{ name: string; completed: number; total: number; pct: number }> = [];

    for (const s of skills) {
      const toIds = outgoing.get(s.id) ?? [];
      let total = 0;
      let completed = 0;

      for (const toId of toIds) {
        if (typeById.get(toId) !== 'course') continue;
        total++;
        if (completedByCourseId.get(toId) === true) completed++;
      }

      // Only show skills that have courses in the graph
      if (total > 0) {
        const name = s.label.startsWith('Skill:') ? s.label.replace(/^Skill:\s*/i, '') : s.label;
        rows.push({ name, completed, total, pct: completed / total });
      }
    }

    rows.sort((a, b) => (b.pct - a.pct) || (b.total - a.total) || a.name.localeCompare(b.name));
    return rows.slice(0, 6);
  });

  maxSkillTotal = computed(() => {
    const rows = this.skillBreakdown();
    return rows.reduce((m, r) => Math.max(m, r.total), 1);
  });

  ngOnInit(): void {
    this.userName = this.auth.userName || 'Student';
    this.load();
  }

  load(): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.error.set('You are not logged in.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.api.getGraph(userId).subscribe({
      next: (res) => {
        this.data.set(res);
        // Auto-select the user node (level 0) if present
        const root = res.nodes.find((n) => n.level === 0) ?? null;
        this.selectedNodeId.set(root?.id ?? null);
        this.loading.set(false);
      },
      error: (err) => {
        const msg = err?.error?.message || err?.message || 'Failed to load progress graph.';
        this.error.set(String(msg));
        this.loading.set(false);
      }
    });
  }

  // Sidebar navigation
  goDashboard(): void {
    this.router.navigateByUrl('/dashboard');
  }

  goSkillAssessment(): void {
    this.router.navigateByUrl('/skill-table');
  }

  logout(): void {
    this.auth.clearSession();
    this.router.navigateByUrl('/register');
  }

  selectNode(id: number): void {
    this.selectedNodeId.set(id);
  }

  nodeClass(n: ProgressGraphNodeDto): string {
    const base = 'graph-node';
    const type = (n.type || '').toLowerCase();
    if (type === 'user') return base + ' node-user';
    if (type === 'metric') return base + ' node-metric';
    if (type === 'skill') return base + ' node-skill';
    if (type === 'course') return base + (n.completed ? ' node-course-complete' : ' node-course');
    return base;
  }

  nodeSubtext(n: ProgressGraphNodeDto): string {
    const t = (n.type || '').toLowerCase();

    if (t === 'metric') {
      return typeof n.value === 'number' ? String(n.value) : '';
    }

    if (t === 'course') {
      const pct = typeof n.completionPercentage === 'number' ? n.completionPercentage : null;
      const watched = typeof n.watchedVideos === 'number' ? n.watchedVideos : null;
      const total = typeof n.totalVideos === 'number' ? n.totalVideos : null;

      if (watched != null && total != null) return `${watched}/${total} videos`;
      if (pct != null) return `${pct}% complete`;
      return n.completed ? 'Completed' : '';
    }

    if (t === 'skill') {
      return n.completed ? 'Completed' : '';
    }

    return '';
  }
}
