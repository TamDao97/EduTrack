import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { IStudentDetail, StudentStatus, StudentStatusLabel } from '../../../../interfaces/IStudent';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { StudentCourseService } from '../../../../services/tutor-domain/student-course.service';
import { ClassRoomService } from '../../../../services/tutor-domain/class-room.service';
import { IStudentCourse } from '../../../../interfaces/IStudentCourse';
import { IStudentClass } from '../../../../interfaces/IClassRoom';
import { StudentFormComponent } from '../student-form/student-form.component';
import { CourseFormComponent } from '../course-form/course-form.component';

@Component({
  selector: 'app-student-detail',
  templateUrl: './student-detail.component.html',
  styleUrls: ['./student-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class StudentDetailComponent extends TdBaseComponent implements OnInit {
  StudentStatus = StudentStatus;
  StudentStatusLabel = StudentStatusLabel;

  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _toast = inject(ToastService);
  private _service = inject(StudentService);
  private _courseService = inject(StudentCourseService);
  private _classService = inject(ClassRoomService);

  student: IStudentDetail | null = null;
  courses: IStudentCourse[] = [];
  classes: IStudentClass[] = [];
  isLoading = false;

  ngOnInit() {
    const id = this._route.snapshot.paramMap.get('id');
    if (id) { this.load(id); this.loadCourses(id); this.loadClasses(id); }
  }

  loadClasses(idStudent: string) {
    this._classService.getByStudent(idStudent).subscribe((rs) => {
      if (rs.status === StatusCode.Ok) this.classes = rs.data ?? [];
    });
  }

  onOpenClass(c: IStudentClass) { this._router.navigate(['/classroom', c.idClass]); }

  loadCourses(idStudent: string) {
    this._courseService.getByStudent(idStudent).subscribe((rs) => {
      if (rs.status === StatusCode.Ok) this.courses = rs.data ?? [];
    });
  }

  onAddCourse() {
    if (!this.student?.id) return;
    this.openModal(
      { title: 'Thêm môn học', width: 440, className: 'sheet-bottom-mobile' },
      CourseFormComponent,
      { params: { idStudent: this.student.id } }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.loadCourses(this.student!.id!); });
  }

  onEditCourse(c: IStudentCourse) {
    this.openModal(
      { title: 'Sửa môn học', width: 440, className: 'sheet-bottom-mobile' },
      CourseFormComponent,
      { params: { idStudent: this.student!.id!, course: c } }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.loadCourses(this.student!.id!); });
  }

  onDeleteCourse(c: IStudentCourse) {
    this.confirmModal(`Xoá môn "${c.subject}"? Buổi học đã tạo không bị ảnh hưởng.`, () => {
      this._courseService.delete(c.id!).subscribe((rs) => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, 'Đã xoá môn học');
          this.loadCourses(this.student!.id!);
        } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      });
    });
  }

  load(id: string) {
    this.isLoading = true;
    this._service.getById(id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) this.student = rs.data;
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được dữ liệu'),
      });
  }

  onEdit() {
    if (!this.student) return;
    this.openModal(
      { title: 'Sửa học sinh', width: 560, className: 'sheet-bottom-mobile' },
      StudentFormComponent,
      { params: this.student }
    ).afterClose.subscribe((rs) => { if (rs?.saved && this.student?.id) this.load(this.student.id); });
  }

  onBack() { this._router.navigate(['/student']); }

  onZalo() {
    if (!this.student?.parentPhone) return;
    window.open(`https://zalo.me/${this.student.parentPhone.replace(/\D/g, '')}`, '_blank');
  }

  onCall() {
    if (!this.student?.parentPhone) return;
    window.location.href = `tel:${this.student.parentPhone}`;
  }

  statusColor(s: StudentStatus): string {
    switch (s) {
      case StudentStatus.Active: return 'success';
      case StudentStatus.Paused: return 'warning';
      case StudentStatus.Stopped: return 'default';
    }
  }

  formatPhone(p?: string): string {
    if (!p) return '';
    const d = p.replace(/\D/g, '');
    if (d.length === 10) return `${d.slice(0, 4)} ${d.slice(4, 7)} ${d.slice(7)}`;
    return p;
  }
}
