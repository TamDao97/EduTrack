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
import { ClassRoomService } from '../../../services/tutor-domain/class-room.service';
import { StudentCourseService } from '../../../services/tutor-domain/student-course.service';
import { StudentFormComponent } from './student-form/student-form.component';

@Component({
  selector: 'app-student-list',
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class StudentListComponent extends TdBaseComponent implements OnInit {
  StudentStatus = StudentStatus;
  StudentStatusLabel = StudentStatusLabel;

  private _router = inject(Router);
  private _toast = inject(ToastService);
  private _service = inject(StudentService);

  private _classService = inject(ClassRoomService);
  private _courseService = inject(StudentCourseService);

  students: IStudentDetail[] = [];
  totalRecord = 0;
  isLoading = false;
  filter: IStudentGridFilter = { ...defaultGridFilter(), pageSize: 12, status: null, idClass: null, subject: null };

  /** Dropdown lọc theo lớp đang dạy */
  classOptions: { id: string; name: string }[] = [];
  /** Dropdown lọc theo môn đang học (distinct từ đăng ký môn) */
  subjects: string[] = [];

  /** Vùng lọc nâng cao (Lớp + Môn) — mặc định thu gọn. */
  showAdvanced = false;

  get activeFilterCount(): number {
    return (this.filter.idClass ? 1 : 0) + (this.filter.subject ? 1 : 0);
  }

  /** Tên lớp đang lọc — cho chip. */
  get filterClassName(): string {
    return this.classOptions.find(c => c.id === this.filter.idClass)?.name ?? '';
  }

  onFilterSubject(s: string | null) {
    this.filter.subject = s;
    this.filter.pageNumber = 1;
    this.loadData();
  }
  clearClassFilter() { this.onFilterClass(null); }
  clearSubjectFilter() { this.onFilterSubject(null); }

  onPageChange(page: number) {
    this.filter.pageNumber = page;
    this.loadData();
  }
  onPageSizeChange(size: number) {
    this.filter.pageSize = size;
    this.filter.pageNumber = 1;
    this.loadData();
  }

  /** Bộ màu avatar — chọn theo hash của tên để mỗi HS có 1 màu cố định nhưng đa dạng. */
  private avatarPalette = [
    { bg: '#5B5FCF', from: '#7C7FE0', to: '#5B5FCF' }, // indigo
    { bg: '#FF6B9D', from: '#FFA1BD', to: '#FF6B9D' }, // pink
    { bg: '#00B8A9', from: '#4ECDC4', to: '#00B8A9' }, // teal
    { bg: '#FF8A65', from: '#FFB199', to: '#FF8A65' }, // coral
    { bg: '#7C3AED', from: '#A78BFA', to: '#7C3AED' }, // violet
    { bg: '#0EA5E9', from: '#7DD3FC', to: '#0EA5E9' }, // sky
    { bg: '#F59E0B', from: '#FCD34D', to: '#F59E0B' }, // amber
    { bg: '#10B981', from: '#6EE7B7', to: '#10B981' }, // emerald
    { bg: '#EC4899', from: '#F9A8D4', to: '#EC4899' }, // hot pink
    { bg: '#06B6D4', from: '#67E8F9', to: '#06B6D4' }, // cyan
    { bg: '#F97316', from: '#FDBA74', to: '#F97316' }, // orange
    { bg: '#84CC16', from: '#BEF264', to: '#84CC16' }, // lime
  ];

  statusTabs: { value: StudentStatus | null; label: string; dotColor?: string }[] = [
    { value: null, label: 'Tất cả' },
    { value: StudentStatus.Active, label: 'Đang học', dotColor: '#52C41A' },
    { value: StudentStatus.Paused, label: 'Tạm nghỉ', dotColor: '#FAAD14' },
    { value: StudentStatus.Stopped, label: 'Đã dừng', dotColor: '#9CA3AF' },
  ];

  ngOnInit() {
    this.loadData();
    // Lớp đang dạy cho dropdown lọc
    this._classService.getByFilter({ keyword: '', isActive: true, pageNumber: 1, pageSize: 100 })
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) {
          this.classOptions = (rs.data?.data ?? []).map((c: any) => ({ id: c.id, name: c.name }));
        }
      });
    this._courseService.getSubjects().subscribe(rs => {
      if (rs.status === StatusCode.Ok) this.subjects = rs.data ?? [];
    });
  }

  onFilterClass(id: string | null) {
    this.filter.idClass = id;
    this.filter.pageNumber = 1;
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

  onSelectStatus(value: StudentStatus | null) {
    this.filter.status = value;
    this.filter.pageNumber = 1;
    this.loadData();
  }

  private _searchTimer: any;
  onSearchChange(value: string) {
    this.filter.keyword = value;
    clearTimeout(this._searchTimer);
    this._searchTimer = setTimeout(() => {
      this.filter.pageNumber = 1;
      this.loadData();
    }, 350);
  }

  onClearSearch() {
    this.filter.keyword = '';
    this.filter.pageNumber = 1;
    this.loadData();
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
        } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      });
    });
  }

  onContactParent(student: IStudentDetail, ev: MouseEvent) {
    ev.stopPropagation();
    if (!student.parentPhone) return;
    window.open(`https://zalo.me/${student.parentPhone.replace(/\D/g, '')}`, '_blank');
  }

  onCallParent(student: IStudentDetail, ev: MouseEvent) {
    ev.stopPropagation();
    if (!student.parentPhone) return;
    window.location.href = `tel:${student.parentPhone}`;
  }

  formatPhone(p?: string): string {
    if (!p) return '';
    const d = p.replace(/\D/g, '');
    if (d.length === 10) return `${d.slice(0, 4)} ${d.slice(4, 7)} ${d.slice(7)}`;
    return p;
  }

  /* ─── Avatar helpers ─── */

  /** Initial từ tên: "Nguyễn Minh Mai" → "NM" */
  initials(name: string | undefined): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  /** Chọn màu avatar ổn định theo tên (hash → palette index). */
  avatarColor(name: string | undefined): { from: string; to: string } {
    const s = name || '?';
    let hash = 0;
    for (let i = 0; i < s.length; i++) hash = ((hash << 5) - hash) + s.charCodeAt(i);
    const palette = this.avatarPalette[Math.abs(hash) % this.avatarPalette.length];
    return { from: palette.from, to: palette.to };
  }

  /** Style gradient cho avatar */
  avatarStyle(name: string | undefined): { [k: string]: string } {
    const { from, to } = this.avatarColor(name);
    return { background: `linear-gradient(135deg, ${from} 0%, ${to} 100%)` };
  }

  /** Màu dot status hiển thị trên avatar */
  statusDotColor(s: StudentStatus): string {
    switch (s) {
      case StudentStatus.Active:  return '#52C41A';
      case StudentStatus.Paused:  return '#FAAD14';
      case StudentStatus.Stopped: return '#9CA3AF';
    }
  }

  /* ─── Stats nho nhỏ (tính từ dữ liệu loaded) ─── */
  get countActive(): number  { return this.students.filter(s => s.status === StudentStatus.Active).length; }
  get countPaused(): number  { return this.students.filter(s => s.status === StudentStatus.Paused).length; }
  get countStopped(): number { return this.students.filter(s => s.status === StudentStatus.Stopped).length; }

  /** Doanh thu dự kiến / buổi của tất cả HS đang học */
  get totalRatePerLesson(): number {
    return this.students
      .filter(s => s.status === StudentStatus.Active)
      .reduce((sum, s) => sum + (s.perLessonRate || 0), 0);
  }

  countForTab(value: StudentStatus | null): number {
    if (value === null) return this.students.length;
    return this.students.filter(s => s.status === value).length;
  }

  trackById(_: number, s: IStudentDetail) { return s.id; }
}
