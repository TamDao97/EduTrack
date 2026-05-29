import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { IStudentDetail, IStudentGridFilter, StudentStatus, StudentStatusLabel } from '../../../interfaces/IStudent';
import { defaultGridFilter } from '../../../shared/interfaces/IBase-ext';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { StudentService } from '../../../services/tutor-domain/student.service';
import { StudentFormComponent } from './student-form/student-form.component';

@Component({
  selector: 'app-student-list',
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class StudentListComponent extends TdBaseComponent implements OnInit {
  // expose enum cho template
  StudentStatus = StudentStatus;
  StudentStatusLabel = StudentStatusLabel;

  private _router = inject(Router);
  private _toast = inject(ToastService);
  private _service = inject(StudentService);

  students: IStudentDetail[] = [];
  totalRecord = 0;
  isLoading = false;
  filter: IStudentGridFilter = { ...defaultGridFilter(), pageSize: 50, status: null };

  /** Tab status: null = tất cả */
  statusTabs: { value: StudentStatus | null; label: string }[] = [
    { value: null, label: 'Tất cả' },
    { value: StudentStatus.Active, label: 'Đang học' },
    { value: StudentStatus.Paused, label: 'Tạm nghỉ' },
    { value: StudentStatus.Stopped, label: 'Đã dừng' },
  ];

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this._service.gridLoadData(this.filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this.students = rs.data?.data ?? [];
            this.totalRecord = rs.data?.totalRecord ?? 0;
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được danh sách học sinh'),
      });
  }

  /** Đổi tab status */
  onSelectStatus(value: StudentStatus | null) {
    this.filter.status = value;
    this.filter.pageNumber = 1;
    this.loadData();
  }

  /** Search debounce nhẹ */
  private _searchTimer: any;
  onSearchChange(value: string) {
    this.filter.keyword = value;
    clearTimeout(this._searchTimer);
    this._searchTimer = setTimeout(() => {
      this.filter.pageNumber = 1;
      this.loadData();
    }, 350);
  }

  onAdd() {
    this.openModal(
      { title: 'Thêm học sinh', width: 560, className: 'sheet-bottom-mobile' },
      StudentFormComponent,
      {}
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.loadData(); });
  }

  onEdit(student: IStudentDetail) {
    this.openModal(
      { title: 'Sửa học sinh', width: 560, className: 'sheet-bottom-mobile' },
      StudentFormComponent,
      { params: student }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.loadData(); });
  }

  onViewDetail(student: IStudentDetail) {
    this._router.navigate(['/student', student.id]);
  }

  onDelete(student: IStudentDetail) {
    this.confirmModal(`Xoá học sinh "${student.fullName}"? Hành động không thể hoàn tác.`, () => {
      this._service.delete(student.id!).subscribe((rs) => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.DELETE_SUCCESS);
          this.loadData();
        } else {
          this._toast.error(StatusResponseTitle.ERROR, rs.message);
        }
      });
    });
  }

  /** Click vào icon Zalo → mở Zalo chat với phụ huynh */
  onContactParent(student: IStudentDetail, ev: MouseEvent) {
    ev.stopPropagation();
    if (!student.parentPhone) return;
    const phone = student.parentPhone.replace(/\D/g, '');
    window.open(`https://zalo.me/${phone}`, '_blank');
  }

  /** Click gọi điện */
  onCallParent(student: IStudentDetail, ev: MouseEvent) {
    ev.stopPropagation();
    if (!student.parentPhone) return;
    window.location.href = `tel:${student.parentPhone}`;
  }

  /** Định dạng SĐT VN: 0901 234 567 */
  formatPhone(p?: string): string {
    if (!p) return '';
    const d = p.replace(/\D/g, '');
    if (d.length === 10) return `${d.slice(0, 4)} ${d.slice(4, 7)} ${d.slice(7)}`;
    return p;
  }

  statusColor(s: StudentStatus): string {
    switch (s) {
      case StudentStatus.Active: return 'success';
      case StudentStatus.Paused: return 'warning';
      case StudentStatus.Stopped: return 'default';
    }
  }

  trackById(_: number, s: IStudentDetail) { return s.id; }
}
