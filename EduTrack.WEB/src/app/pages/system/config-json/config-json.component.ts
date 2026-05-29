import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { IResponse } from '../../../shared/interfaces/IResponse';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseTitle, StatusResponseMessage } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { ConfigJsonService } from '../../../services/system/config-json.service';
import { finalize, forkJoin } from 'rxjs';

@Component({
  selector: 'app-config-json',
  templateUrl: './config-json.component.html',
  styleUrl: './config-json.component.css',
  standalone: true,
  imports: [SharedModule],
})
export class ConfigJsonComponent extends TdBaseComponent implements OnInit {

  frmGroup!: FormGroup;
  constructor(
    private _router: Router,
    private _fb: FormBuilder,
    private _toastService: ToastService,
    private _configJsonService: ConfigJsonService
  ) {
    super();
  }

  ngOnInit() {
    this.initForm();
    this._spinner.show();
    forkJoin({
      orgConfigRes: this._configJsonService.getOrgConfig(),
    }).pipe(
      finalize(() => {
        this._spinner.hide();
      })
    ).subscribe({
      next: (res) => {
        if (res.orgConfigRes.status == StatusCode.Ok) {
          this.frmGroup.patchValue({ ...res.orgConfigRes.data });
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, res.orgConfigRes.message);
        }
      },
      error: (err) => {
        this._toastService.error(
          StatusResponseTitle.ERROR,
          'Có lỗi khi load cấu hình đơn vị'
        );
      },
    });
  }

  initForm() {
    this.frmGroup = this._fb.group({
      id: [null],
      appName: [null, [Validators.required]],
    });
  }

  onSave() {
    const isValid = this.validateForm(this.frmGroup);
    if (!isValid) {
      this._toastService.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const payload = {
      ...this.frmGroup.value
    }
    this._spinner.show();
    this._configJsonService
      .saveOrgConfig(payload)
      .pipe(
        finalize(() => {
          // luôn chạy dù success hay error
          this._spinner.hide();
        })
      )
      .subscribe({
        next: (rs) => {
          if (rs.status == StatusCode.Ok) {
            // this.closeModal();
            this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.UPDATE_SUCCESS);
          } else {
            this._toastService.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: (err) => {
          this._toastService.error('ERROR', 'Có lỗi xảy ra khi đăng nhập');
        }
      });
  }

  onClose() {
    this.closeModal();
  }
}
