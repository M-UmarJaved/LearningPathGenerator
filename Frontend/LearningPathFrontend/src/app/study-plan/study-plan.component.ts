import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import { StudyPlanEventDto, StudyPlanService } from '../study-plan.service';

type Category = 'Study' | 'Revision' | 'Assignment' | 'Assessment' | 'Break' | 'Other';

interface CalendarCell {
  date: Date;
  inMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  count: number;
}

@Component({
  selector: 'app-study-plan',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './study-plan.component.html',
  styleUrls: ['./study-plan.component.css']
})
export class StudyPlanComponent implements OnInit {
  private auth = inject(AuthService);
  private api = inject(StudyPlanService);

  loading = false;
  error = '';

  // Calendar state
  viewYear = new Date().getFullYear();
  viewMonth = new Date().getMonth(); // 0-based
  selectedDate = new Date();

  grid: CalendarCell[] = [];

  // Events
  events: StudyPlanEventDto[] = [];

  // Form state
  formMode: 'create' | 'edit' = 'create';
  editId: number | null = null;

  title = '';
  category: Category = 'Study';
  notes = '';
  startTime = '09:00';
  endTime = '10:00';
  isCompleted = false;

  ngOnInit(): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.error = 'You are not logged in.';
      return;
    }

    const now = new Date();
    this.viewYear = now.getFullYear();
    this.viewMonth = now.getMonth();
    this.selectedDate = this.stripTime(now);

    this.reloadMonth();
  }

  get monthLabel(): string {
    const d = new Date(this.viewYear, this.viewMonth, 1);
    return d.toLocaleString(undefined, { month: 'long', year: 'numeric' });
  }

  prevMonth(): void {
    const d = new Date(this.viewYear, this.viewMonth - 1, 1);
    this.viewYear = d.getFullYear();
    this.viewMonth = d.getMonth();
    this.reloadMonth();
  }

  nextMonth(): void {
    const d = new Date(this.viewYear, this.viewMonth + 1, 1);
    this.viewYear = d.getFullYear();
    this.viewMonth = d.getMonth();
    this.reloadMonth();
  }

  select(cell: CalendarCell): void {
    const selected = this.stripTime(cell.date);

    // If the user clicks a day from the previous/next month (shown in the grid),
    // jump to that month so events + counts are correct.
    if (cell.date.getFullYear() !== this.viewYear || cell.date.getMonth() !== this.viewMonth) {
      this.viewYear = cell.date.getFullYear();
      this.viewMonth = cell.date.getMonth();
      this.selectedDate = selected;
      this.reloadMonth(true);
      return;
    }

    this.selectedDate = selected;
    this.buildGrid();
    this.resetFormForCreate();
  }

  get selectedLabel(): string {
    return this.selectedDate.toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'short', day: 'numeric' });
  }

  get eventsForSelectedDay(): StudyPlanEventDto[] {
    const dayStart = new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), this.selectedDate.getDate(), 0, 0, 0, 0);
    const dayEnd = new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), this.selectedDate.getDate() + 1, 0, 0, 0, 0);

    return this.events
      .filter((e) => {
      const evStart = new Date(e.startAtUtc);
      const evEnd = new Date(e.endAtUtc);
      return evStart.getTime() < dayEnd.getTime() && evEnd.getTime() > dayStart.getTime();
      })
      .sort((a, b) => new Date(a.startAtUtc).getTime() - new Date(b.startAtUtc).getTime());
  }

  startEdit(ev: StudyPlanEventDto): void {
    this.formMode = 'edit';
    this.editId = ev.studyPlanEventId;

    this.title = ev.title;
    this.category = (ev.category as Category) || 'Study';
    this.notes = ev.notes ?? '';
    this.isCompleted = !!ev.isCompleted;

    const start = new Date(ev.startAtUtc);
    const end = new Date(ev.endAtUtc);

    this.selectedDate = this.stripTime(start);
    this.startTime = this.hhmm(start);
    this.endTime = this.hhmm(end);

    this.buildGrid();
  }

  cancelEdit(): void {
    this.resetFormForCreate();
  }

  save(): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.error = 'You are not logged in.';
      return;
    }

    this.error = '';

    if (!this.title.trim()) {
      this.error = 'Title is required.';
      return;
    }

    const startLocal = this.combineDateTime(this.selectedDate, this.startTime);
    const endLocal = this.combineDateTime(this.selectedDate, this.endTime);

    if (!(startLocal instanceof Date) || !(endLocal instanceof Date) || Number.isNaN(startLocal.getTime()) || Number.isNaN(endLocal.getTime())) {
      this.error = 'Invalid time.';
      return;
    }

    if (endLocal <= startLocal) {
      this.error = 'End time must be after start time.';
      return;
    }

    const payloadBase = {
      title: this.title.trim(),
      category: this.category,
      notes: this.notes.trim() ? this.notes.trim() : null,
      startAtUtc: startLocal.toISOString(),
      endAtUtc: endLocal.toISOString(),
      skillId: null as number | null,
      courseId: null as number | null
    };

    this.loading = true;

    if (this.formMode === 'create') {
      this.api.createEvent({ userId, ...payloadBase }).subscribe({
        next: () => {
          this.loading = false;
          this.reloadMonth(true);
        },
        error: (err) => {
          this.loading = false;
          this.error = err?.error?.message ?? err?.message ?? 'Failed to create event.';
        }
      });
      return;
    }

    const id = this.editId;
    if (!id) {
      this.loading = false;
      this.resetFormForCreate();
      return;
    }

    this.api.updateEvent(userId, id, { ...payloadBase, isCompleted: this.isCompleted }).subscribe({
      next: () => {
        this.loading = false;
        this.reloadMonth(true);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to update event.';
      }
    });
  }

  remove(ev: StudyPlanEventDto): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.error = 'You are not logged in.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.api.deleteEvent(userId, ev.studyPlanEventId).subscribe({
      next: () => {
        this.loading = false;
        this.reloadMonth(true);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to delete event.';
      }
    });
  }

  private reloadMonth(keepSelection = false): void {
    const userId = this.auth.userId;
    if (!userId) return;

    this.loading = true;
    this.error = '';

    const monthStart = new Date(this.viewYear, this.viewMonth, 1);
    const monthEnd = new Date(this.viewYear, this.viewMonth + 1, 1);

    this.api.getEvents(userId, monthStart.toISOString(), monthEnd.toISOString()).subscribe({
      next: (items) => {
        this.events = items ?? [];
        this.loading = false;

        if (!keepSelection) {
          this.selectedDate = this.stripTime(new Date(this.viewYear, this.viewMonth, 1));
        }

        this.buildGrid();
        this.resetFormForCreate();
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load study plan.';
        this.events = [];
        this.buildGrid();
      }
    });
  }

  private buildGrid(): void {
    const first = new Date(this.viewYear, this.viewMonth, 1);
    const startDow = first.getDay(); // 0..6
    const gridStart = new Date(this.viewYear, this.viewMonth, 1 - startDow);

    const today = this.stripTime(new Date());
    const selected = this.stripTime(this.selectedDate);

    const dayCounts = new Map<string, number>();

    for (const ev of this.events) {
      const start = new Date(ev.startAtUtc);
      const end = new Date(ev.endAtUtc);

      // Count the event on every local day it overlaps (end is exclusive).
      let cursor = this.stripTime(start);
      const endExclusive = new Date(end.getTime() - 1);
      const lastDay = this.stripTime(endExclusive);

      while (cursor.getTime() <= lastDay.getTime()) {
        const key = this.localDateKey(cursor);
        dayCounts.set(key, (dayCounts.get(key) ?? 0) + 1);
        cursor = new Date(cursor.getFullYear(), cursor.getMonth(), cursor.getDate() + 1);
      }
    }

    const cells: CalendarCell[] = [];
    for (let i = 0; i < 42; i++) {
      const d = new Date(gridStart.getFullYear(), gridStart.getMonth(), gridStart.getDate() + i);
      const key = this.localDateKey(this.stripTime(d));

      cells.push({
        date: d,
        inMonth: d.getMonth() === this.viewMonth,
        isToday: this.stripTime(d).getTime() === today.getTime(),
        isSelected: this.stripTime(d).getTime() === selected.getTime(),
        count: dayCounts.get(key) ?? 0
      });
    }

    this.grid = cells;
  }

  private resetFormForCreate(): void {
    this.formMode = 'create';
    this.editId = null;
    this.title = '';
    this.category = 'Study';
    this.notes = '';
    this.startTime = '09:00';
    this.endTime = '10:00';
    this.isCompleted = false;
  }

  private stripTime(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
  }

  private combineDateTime(dateOnly: Date, hhmm: string): Date {
    const [hh, mm] = (hhmm || '').split(':').map(Number);
    return new Date(dateOnly.getFullYear(), dateOnly.getMonth(), dateOnly.getDate(), hh ?? 0, mm ?? 0, 0);
  }

  private hhmm(d: Date): string {
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  private localDateKey(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
