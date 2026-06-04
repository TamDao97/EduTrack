import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { IClassMember, IClassRoomDetail } from '../../../interfaces/IClassRoom';
import { ClassRoomService } from '../../../services/tutor-domain/class-room.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { ClassFormComponent } from './class-form/class-form.component';
import { MemberFormComponent } from './member-form/member-form.component';
import { ScheduleFormComponent } from './schedule-form/schedule-form.component';

/** Trang /classroom/:id — chi tiết lớp: thông tin + ghi danh + xếp lịch. */
@Component({
  selector: 'app-class-detail',
  templateUrl: './class-detail.component.html',
  styleUrls: ['./class-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ClassDetailComponent extends TdBaseComponent implements OnInit {
  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _toast = inject(ToastService);
  private _service = inject(ClassRoomService);

  cls: IClassRoomDetail | null = null;
  isLoading = false;

  ngOnInit() {
    const id = this._route.snapshot.paramMap.get('id');
    if (id) this.load(id);
  }

  load(id: string) {
    this.isLoading = true;
    this._service.getDetail(id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) this.cls = rs.data;
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được lớp'),
      });
  }

  reload() { if (this.cls?.id) this.load(this.cls.id); }

  onBack() { this._router.navigate(['/classroom']); }

  onEdit() {
    if (!this.cls) return;
    this.openModal(
      { title: 'Sửa lớp học', width: 520, className: 'sheet-bottom-mobile' },
      ClassFormComponent,
      { params: { classRoom: this.cls } }
    ).afterClose.subscribe(rs => { if (rs?.saved) this.reload(); });
  }

  onAddMember() {
    if (!this.cls) return;
    this.openModal(
      { title: 'Ghi danh học sinh', width: 460, className: 'sheet-bottom-mobile' },
      MemberFormComponent,
      { params: {
          idClass: this.cls.id!,
          defaultRate: this.cls.defaultRatePerLesson,
          excludeIds: this.cls.members.map(m => m.idStudent),
        } }
    ).afterClose.subscribe(rs => { if (rs?.saved) this.reload(); });
  }

  onEditMember(m: IClassMember) {
    if (!this.cls) return;
    this.openModal(
      { title: 'Giá riêng trong lớp', width: 420, className: 'sheet-bottom-mobile' },
      MemberFormComponent,
      { params: { idClass: this.cls.id!, defaultRate: this.cls.defaultRatePerLesson, member: m } }
    ).afterClose.subscribe(rs => { if (rs?.saved) this.reload(); });
  }

  onRemoveMember(m: IClassMember) {
    this.confirmModal(`Cho ${m.studentFullName} rời lớp? Buổi đã xếp không bị ảnh hưởng.`, () => {
      this._service.removeMember(m.id!).subscribe(rs => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, 'Đã rời lớp');
          this.reload();
        } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      });
    });
  }

  onGenerate() {
    if (!this.cls) return;
    if (this.cls.members.length === 0) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Ghi danh ít nhất 1 học sinh trước khi xếp lịch');
      return;
    }
    this.openModal(
      { title: `Xếp lịch — ${this.cls.name}`, width: 440, className: 'sheet-bottom-mobile' },
      ScheduleFormComponent,
      { params: { idClass: this.cls.id!, scheduleLabel: this.scheduleLabel() } }
    ).afterClose.subscribe(() => { /* lesson tạo ở Lịch dạy — không cần reload lớp */ });
  }

  scheduleLabel(): string {
    if (!this.cls) return '';
    const names = (this.cls.daysOfWeek || '').split(',')
      .map(s => parseInt(s, 10)).filter(n => !isNaN(n))
      .sort((a, b) => (a === 0 ? 7 : a) - (b === 0 ? 7 : b))
      .map(n => (n === 0 ? 'CN' : `T${n + 1}`));
    return `${names.join('·')} ${(this.cls.startTime || '').slice(0, 5)}-${(this.cls.endTime || '').slice(0, 5)}`;
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

  trackById(_: number, m: IClassMember) { return m.id; }
}
