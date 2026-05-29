import { Component, OnInit } from '@angular/core';
import { SharedModule } from '../../../shared/modules/shared.module';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastService } from '../../../shared/services/toast.service';
import { TdBaseGridComponent } from '../../../shared/utils/extends-components/td-base-grid.component';
import { PageService } from '../../../services/system/page.service';
import { keyPage, StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { IDropdown } from '../../../shared/interfaces/IDropdown';
import { StatusCode } from '../../../shared/utils/enums';
import { IResponse } from '../../../shared/interfaces/IResponse';
import { ITree } from '../../../shared/interfaces/ITree';

@Component({
  selector: 'app-page',
  templateUrl: './page.component.html',
  styleUrls: ['./page.component.scss'],
  standalone: true,
  imports: [SharedModule],
})
export class PageComponent extends TdBaseGridComponent implements OnInit {
  override title = 'Quản lý menu';
  override pageKey = keyPage.User;

  frmGroup!: FormGroup;
  lstPage: IDropdown[] = [];
  pageTreeData: ITree[] = [];

  constructor(
    private _fb: FormBuilder,
    _toastService: ToastService,
    private _pageService: PageService
  ) {
    super(_pageService, _toastService);
  }

  override ngOnInit() {
    this.frmGroup = this._fb.group({
      id: [null],
      idParent: [null],
      name: [null, [Validators.required]],
      url: [null],
      icon: [null],
      permissionCode: [null],
      order: [null],
      isActive: [false],
      isTab: [false],
      isHomePage: [false],
    });
    debugger
    this.getPageTree();
  }


  getPageTree() {
    this._pageService
      .getPageTree()
      .subscribe((rs: IResponse) => {
        if (rs.status == StatusCode.Ok) {
          this.pageTreeData = rs.data;
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      });
  }

  onDrop(event: any): void {
    console.log('Drop event:', event);
    const dragNode = event.dragNode;
    const dropNode = event.node;

    console.log('Kéo node:', dragNode?.title);
    console.log('Thả vào node:', dropNode?.title);

    // 👇 Ví dụ xử lý: gọi API cập nhật vị trí hoặc cập nhật lại mảng nodes
    // this.updateTreeStructure(dragNode, dropNode, dropPosition);
  }

  onNodeClick(event: any): void {
    console.log('Clicked node:', event.node);
    this._pageService
      .getById(event.node.key)
      .subscribe((rs: IResponse) => {
        if (rs.status == StatusCode.Ok) {
          this.frmGroup.patchValue(rs.data);
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      });
  }

  onSave() {
    const isValid = this.validateForm(this.frmGroup);
    if (!isValid) return;

    const raw = { ...this.frmGroup.value };
    // Sanitize: nz-input-number trả chuỗi rỗng "" khi user clear → BE expect int? sẽ reject
    // Convert empty string thành null cho các field optional kiểu số
    const payload = {
      ...raw,
      order: raw.order === '' || raw.order === undefined ? null : raw.order,
    };

    if (!payload.id) {
      this._pageService
        .create(payload)
        .subscribe((rs: IResponse) => {
          if (rs.status == StatusCode.Ok) {
            this.frmGroup.reset();
            this.getPageTree();
            this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.ADD_SUCCESS)
          } else {
            this._toastService.error(StatusResponseTitle.ERROR, rs.message);
          }
        });
    } else {
      this._pageService
        .update(payload)
        .subscribe((rs: IResponse) => {
          if (rs.status == StatusCode.Ok) {
            this.getPageTree();
            this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.UPDATE_SUCCESS);
          } else {
            this._toastService.error(StatusResponseTitle.ERROR, rs.message);
          }
        });
    }
  }

  onDelete(id: string) {
    this._pageService
      .delete(id)
      .subscribe((rs: IResponse) => {
        if (rs.status == StatusCode.Ok) {
          this.getPageTree();
          this._toastService.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.DELETE_SUCCESS);
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      });
  }

  onReset() {
    this.frmGroup.reset();
  }
}

