import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SharedModule } from '../../shared/modules/shared.module';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

type Step = 'welcome' | 'profile' | 'student' | 'lesson' | 'done';

/**
 * Onboarding 3-step demo — mock data only.
 * Mục đích: thấy visual + flow trước khi connect API.
 */
@Component({
  selector: 'app-onboarding-demo',
  templateUrl: './onboarding-demo.component.html',
  styleUrls: ['./onboarding-demo.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class OnboardingDemoComponent extends TdBaseComponent {
  private _fb = inject(FormBuilder);
  private _router = inject(Router);

  step: Step = 'welcome';

  /** Steps có "Bước X/3" hiển thị — không tính welcome/done */
  readonly numberedSteps: Step[] = ['profile', 'student', 'lesson'];

  // ───────── Forms (mock state) ─────────
  profileForm: FormGroup = this._fb.group({
    displayName: ['', Validators.required],
    subjects: [['Toán', 'Lý']],  // chip toggle
    bankName: [''],
    bankAccountNumber: [''],
  });

  studentForm: FormGroup = this._fb.group({
    fullName: ['', Validators.required],
    parentName: ['', Validators.required],
    parentPhone: ['', [Validators.required, Validators.pattern(/^0\d{9,10}$/)]],
    perLessonRate: [200000, Validators.required],
    grade: [''],
    subject: [''],
  });

  lessonForm: FormGroup = this._fb.group({
    daysOfWeek: [[2, 4]],         // T3, T5 (JS: Sun=0)
    startTime: ['19:00'],
    endTime: ['20:30'],
    weeksAhead: [12],
  });

  // ───────── Chip toggles ─────────
  readonly subjectOptions = ['Toán', 'Lý', 'Hoá', 'Văn', 'Anh', 'Sử', 'Địa', 'Sinh'];
  readonly weekDayOptions = [
    { value: 1, label: 'T2' },
    { value: 2, label: 'T3' },
    { value: 3, label: 'T4' },
    { value: 4, label: 'T5' },
    { value: 5, label: 'T6' },
    { value: 6, label: 'T7' },
    { value: 0, label: 'CN' },
  ];

  toggleSubject(s: string) {
    const cur: string[] = [...(this.profileForm.value.subjects || [])];
    const idx = cur.indexOf(s);
    idx >= 0 ? cur.splice(idx, 1) : cur.push(s);
    this.profileForm.patchValue({ subjects: cur });
  }
  isSubjectOn(s: string): boolean { return (this.profileForm.value.subjects || []).includes(s); }

  toggleDow(d: number) {
    const cur: number[] = [...(this.lessonForm.value.daysOfWeek || [])];
    const idx = cur.indexOf(d);
    idx >= 0 ? cur.splice(idx, 1) : cur.push(d);
    this.lessonForm.patchValue({ daysOfWeek: cur });
  }
  isDowOn(d: number): boolean { return (this.lessonForm.value.daysOfWeek || []).includes(d); }

  // ───────── Computed summary ─────────
  get estimatedLessons(): number {
    const days = (this.lessonForm.value.daysOfWeek || []).length;
    const weeks = this.lessonForm.value.weeksAhead || 0;
    return days * weeks;
  }

  get summaryDays(): string {
    const dows: number[] = this.lessonForm.value.daysOfWeek || [];
    if (dows.length === 0) return '—';
    return this.weekDayOptions
      .filter(d => dows.includes(d.value))
      .map(d => d.label)
      .join(', ');
  }

  // ───────── Step machine ─────────
  next() {
    switch (this.step) {
      case 'welcome': this.step = 'profile'; break;
      case 'profile':
        if (!this.validateForm(this.profileForm)) return;
        this.step = 'student'; break;
      case 'student':
        if (!this.validateForm(this.studentForm)) return;
        this.step = 'lesson'; break;
      case 'lesson':
        this.step = 'done'; break;
      case 'done':
        this._router.navigate(['/dashboard']); break;
    }
    this.scrollTop();
  }

  back() {
    switch (this.step) {
      case 'profile': this.step = 'welcome'; break;
      case 'student': this.step = 'profile'; break;
      case 'lesson':  this.step = 'student'; break;
    }
    this.scrollTop();
  }

  skip() {
    // skip behaves như next nhưng KHÔNG validate
    switch (this.step) {
      case 'profile': this.step = 'student'; break;
      case 'student': this.step = 'lesson';  break;
      case 'lesson':  this.step = 'done';    break;
    }
    this.scrollTop();
  }

  /** Progress 1..3, 0 cho welcome, 4 cho done */
  get progressIndex(): number {
    return ({ welcome: 0, profile: 1, student: 2, lesson: 3, done: 4 } as const)[this.step];
  }

  private scrollTop() { window.scrollTo({ top: 0, behavior: 'smooth' }); }

  /** Demo only — close = back to /design */
  onClose() { this._router.navigate(['/design']); }
}
