import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { IClassRoom } from '../../../interfaces/IClassRoom';
import { ClassRoomService } from '../../../services/tutor-domain/class-room.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { ClassFormComponent } from './class-form/class-form.component';

/** Trang /classroom — danh sách Lớp học của tutor. */
@Component({
  selector: 'app-class-list',
  templateUrl: './class-list.component.html',
  styleUrls: ['./class-list.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ClassListComponent extends TdBaseComponent implements OnInit {
  private _router = inject(Router);
  private _toast = inject(ToastService);
  private _service = inject(ClassRoomService);

  classes: IClassRoom[] = [];
  isLoading = false;

  ngOnInit() { this.load(); }

  load() {
    this.isLoading = true;
    this._service.getMyClasses()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) this.classes = rs.data ?? [];
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được danh sách lớp'),
      });
  }

  onAdd() {
    this.openModal(
      { title: 'Tạo lớp học', width: 520, className: 'sheet-bottom-mobile' },
      ClassFormComponent,
      {}
    ).afterClose.subscribe(rs => { if (rs?.saved) this.load(); });
  }

  onOpen(c: IClassRoom) { this._router.navigate(['/classroom', c.id]); }

  /** "T2·T4·T6 19:00-20:30" */
  scheduleLabel(c: IClassRoom): string {
    const names = (c.daysOfWeek || '').split(',')
      .map(s => parseInt(s, 10)).filter(n => !isNaN(n))
      .sort((a, b) => (a === 0 ? 7 : a) - (b === 0 ? 7 : b))
      .map(n => (n === 0 ? 'CN' : `T${n + 1}`));
    return `${names.join('·')} ${(c.startTime || '').slice(0, 5)}-${(c.endTime || '').slice(0, 5)}`;
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

  trackById(_: number, c: IClassRoom) { return c.id; }
}
