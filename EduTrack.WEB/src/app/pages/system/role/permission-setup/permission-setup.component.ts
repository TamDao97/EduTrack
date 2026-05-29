import { Component, OnInit } from '@angular/core';
import { IDropdown } from '../../../../shared/interfaces/IDropdown';
import { RoleService } from '../../../../services/system/role.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseGridComponent } from '../../../../shared/utils/extends-components/td-base-grid.component';
import { keyPage, StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { IColumn } from '../../../../shared/interfaces/IBase';
import { Guid } from '../../../../shared/utils/guid';

@Component({
  selector: 'app-permission-setup',
  templateUrl: './permission-setup.component.html',
  styleUrls: ['./permission-setup.component.scss'],
  standalone: true,
  imports: [SharedModule],
})
export class PermissionSetupComponent
  extends TdBaseGridComponent
  implements OnInit {
  override title = 'Phần quyền hệ thống';
  override pageKey = keyPage.Role;

  lstRole: IDropdown[] = [];

  override objFilter: any = {
    idRole: null,
  };

  constructor(_toastService: ToastService, private _roleService: RoleService) {
    super(_roleService, _toastService);
  }

  override ngOnInit(): void {
    this.gridColumns = this.buildTableColumn();
    this.getAllRole();
    this.gridLoadData();
  }


  // #region Table
  buildTableColumn(): IColumn[] {
    return [
      {
        field: 'module',
        header: 'Module',
        width: '350px',
        class: 'text-center',
        type: 'template',
        sort: true,
        sortBy: 'asc',
        // customTemplate: (data: any) => {
        //   return `${data.displayName} <p nz-typography nzCopyable nzCopyText="${data.userName}">${data.userName}.</p>`;
        // },
      },
      {
        field: 'permission',
        header: 'Chức năng',
        width: '1050px',
        class: 'text-center',
        type: 'template',
        sort: true,
        sortBy: 'asc',
      },
    ];
  }
  // #endregion

  getAllRole(): void {
    this.isLoading = true;
    this._roleService.getAll().subscribe(
      (rs) => {
        if (rs.status == StatusCode.Ok) {
          this.lstRole = rs.data.map((r: any) => {
            return <IDropdown>{
              value: r.id,
              text: r.name
            };
          });
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      },
      (error) => {
        this._toastService.error(StatusResponseTitle.ERROR, error.message);
      },
      () => {
        // Khi hoàn thành
        this.isLoading = false;
      }
    );
  }

  onChangeRole(): void {
    this.gridLoadData();
  }

  // override gridLoadData(): void {
  //   this.isLoading = true;
  //   this._roleService.getPermissionGroupByModule().subscribe(
  //     (rs) => {
  //       if (rs.status == StatusCode.Ok) {
  //         this.gridData = rs.data;
  //       } else {
  //         this._toastService.error(StatusResponseTitle.ERROR, rs.message);
  //       }
  //     },
  //     (error) => {
  //       this._toastService.error(StatusResponseTitle.ERROR, error.message);
  //     },
  //     () => {
  //       // Khi hoàn thành
  //       this.isLoading = false;
  //     }
  //   );
  // }

  override gridLoadData(): void {
    this.isLoading = true;
    this._roleService.getPermissionByRole(this.objFilter.idRole ?? Guid.Empty()).subscribe(
      (rs) => {
        if (rs.status == StatusCode.Ok) {
          this.gridData = rs.data;
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      },
      (error) => {
        this._toastService.error(StatusResponseTitle.ERROR, error.message);
      },
      () => {
        // Khi hoàn thành
        this.isLoading = false;
      }
    );
  }

  scanPermission(): void {
    this.isLoading = true;
    this._roleService.scanPermission().subscribe(
      (rs) => {
        if (rs.status == StatusCode.Ok) {
          this.gridLoadData();
          this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.ADD_SUCCESS)
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      },
      (error) => {
        this._toastService.error(StatusResponseTitle.ERROR, error.message);
      },
      () => {
        // Khi hoàn thành
        this.isLoading = false;
      }
    );
  }

  addPermissionByRole(): void {
    const payload = {
      idRole: this.objFilter.idRole,
      lstPermissions: this.gridData?.flatMap(r => r.lstPermissions ?? [])?.filter(r => r.isChecked)?.map(r => r.permissionCode)
    }
    this.isLoading = true;
    this._roleService.addPermissionByRole(payload).subscribe(
      (rs) => {
        if (rs.status == StatusCode.Ok) {
          this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.ADD_SUCCESS)
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      },
      (error) => {
        this._toastService.error(StatusResponseTitle.ERROR, error.message);
      },
      () => {
        // Khi hoàn thành
        this.isLoading = false;
      }
    );
  }

  autoAddPermission(): void {
    const payload = {
      idRole: this.objFilter.idRole,
      lstModule: this.gridData?.filter(r => r.isChecked)?.map(r => r.permissionCode)
    }
    this.isLoading = true;
    this._roleService.addPermissionByRole(payload).subscribe(
      (rs) => {
        if (rs.status == StatusCode.Ok) {
          this.gridLoadData();
          this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.ADD_SUCCESS)
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      },
      (error) => {
        this._toastService.error(StatusResponseTitle.ERROR, error.message);
      },
      () => {
        // Khi hoàn thành
        this.isLoading = false;
      }
    );
  }
}
