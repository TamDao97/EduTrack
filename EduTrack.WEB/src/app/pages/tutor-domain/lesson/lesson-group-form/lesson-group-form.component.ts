import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { IStudentCourse } from '../../../../interfaces/IStudentCourse';
import { IStudentDetail } from '../../../../interfaces/IStudent';
import { LessonService } from '../../../../services/tutor-domain/lesson.service';
import { StudentCourseService } from '../../../../services/tutor-domain/student-course.service';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

/** 1 dòng HS trong nhóm: chọn môn riêng từng em. */
interface GroupRow {
  idStudent: string;
  fullName: string;
  courses: IStudentCourse[];
  idCourse: string | null;
}

/**
 * Modal tạo buổi NHÓM — nhiều HS học chung 1 ca. Mỗi em vẫn có buổi riêng
 * (giá theo môn từng em, nhắc riêng từng phụ huynh, học phí riêng).
 */
@Component({
  selector: 'app-lesson-group-form',
  templateUrl: './lesson-group-form.component.html',
  styleUrls: ['./lesson-group-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, FormsModule],
})
export class LessonGroupFormComponent extends TdBaseComponent implements OnInit {
  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(LessonService);
  private _studentService = inject(StudentService);
  private _courseService = inject(StudentCourseService);

  frmGroup!: FormGroup;
  isSubmitting = false;
  students: IStudentDetail[] = [];
  /** HS đã chọn vào nhóm + môn từng em. */
  rows: GroupRow[] = [];

  ngOnInit() {
    const start = new Date(); start.setHours(19, 0, 0, 0);
    const end = new Date(); end.setHours(20, 30, 0, 0);
    this.frmGroup = this._fb.group({
      selectedIds: [[], [Validators.required]],
      scheduledDate: [new Date(), [Validators.required]],
      startTime: [start, [Validators.required]],
      endTime: [end, [Validators.required]],
      location: [''],
      notes: [''],
      numberOfWeeks: [1, [Validators.required, Validators.min(1), Validators.max(52)]],
    });

    this._studentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200, status: 1 })
      .subscribe((rs) => {
        if (rs.status === StatusCode.Ok) this.students = rs.data?.data ?? [];
      });

    // Chọn/bỏ HS → đồng bộ rows (nạp môn cho HS mới thêm)
    this.frmGroup.get('selectedIds')!.valueChanges.subscribe((ids: string[]) => this.syncRows(ids || []));
  }

  private syncRows(ids: string[]) {
    // Bỏ HS không còn chọn
    this.rows = this.rows.filter(r => ids.includes(r.idStudent));
    // Thêm HS mới
    const newIds = ids.filter(id => !this.rows.some(r => r.idStudent === id));
    if (newIds.length === 0) return;
    forkJoin(newIds.map(id => this._courseService.getByStudent(id))).subscribe(results => {
      results.forEach((rs, i) => {
        const id = newIds[i];
        const student = this.students.find(s => s.id === id);
        const courses = (rs.status === StatusCode.Ok ? rs.data ?? [] : [])
          .filter((c: IStudentCourse) => c.isActive);
        this.rows.push({
          idStudent: id,
          fullName: student?.fullName ?? '—',
          courses,
          idCourse: courses[0]?.id ?? null,
        });
      });
    });
  }

  rateOf(r: GroupRow): number {
    return r.courses.find(c => c.id === r.idCourse)?.perLessonRate ?? 0;
  }

  /** Tổng tiền 1 ca = cộng giá từng em. */
  get totalPerSession(): number {
    return this.rows.reduce((sum, r) => sum + this.rateOf(r), 0);
  }

  onSave() {
    if (this.rows.length < 2) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Buổi nhóm cần ít nhất 2 học sinh');
      return;
    }
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    const payload = {
      students: this.rows.map(r => ({ idStudent: r.idStudent, idCourse: r.idCourse })),
      scheduledDate: this.toISODate(v.scheduledDate),
      startTime: this.formatTime(v.startTime),
      endTime: this.formatTime(v.endTime),
      location: v.location || '',
      notes: v.notes || '',
      numberOfWeeks: +v.numberOfWeeks || 1,
    };
    this.isSubmitting = true;
    this._service.createGroup(payload)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, `Đã tạo ${rs.data} buổi học cho nhóm`);
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  fmtVnd(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

  private toISODate(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
  private formatTime(d: Date): string {
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`;
  }
}
