import { Component, OnInit } from '@angular/core';
import { ToastService } from '../../../shared/services/toast.service';
import { TdBaseGridComponent } from '../../../shared/utils/extends-components/td-base-grid.component';
import { SharedModule } from '../../../shared/modules/shared.module';
import { RoleService } from '../../../services/system/role.service';
import { RoleGridComponent } from './role-grid/role-grid.component';
import { PermissionSetupComponent } from './permission-setup/permission-setup.component';
import { keyPage } from '../../../shared/utils/constants';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss', '../../admin/admin-dashboard.component.scss'],
  standalone: true,
  imports: [SharedModule, RoleGridComponent, PermissionSetupComponent]
})
export class RoleComponent extends TdBaseGridComponent implements OnInit {
  override title = 'Quản lý quyền';
  override pageKey = keyPage.Role;

  tabs = [
    {
      key: 1,
      name: 'Quản lý quyền',
      icon: 'apple'
    },
    {
      key: 2,
      name: 'Phân quyền hệ thống',
      icon: 'android'
    }
  ];

  tabSelected: number = this.tabs[0].key

  constructor(
    _toastService: ToastService,
    _roleService: RoleService
  ) {
    super(_roleService, _toastService);
  }

  override ngOnInit(): void {
  }

  onTabChange(index: number): void {
    this.tabSelected = this.tabs[index].key
  }
}
