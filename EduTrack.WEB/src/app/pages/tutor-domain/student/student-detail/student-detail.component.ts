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
import { StudentFormComponent } from '../student-form/student-form.component';

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

  student: IStudentDetail | null = null;
  isLoading = false;

  ngOnInit() {
    const id = this._route.snapshot.paramMap.get('id');
    if (id) this.load(id);
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
