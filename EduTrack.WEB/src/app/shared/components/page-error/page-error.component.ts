import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SharedModule } from '../../modules/shared.module';
import { StatusCode } from '../../utils/enums';

@Component({
  selector: 'app-page-error',
  templateUrl: './page-error.component.html',
  styleUrls: ['./page-error.component.scss'],
  standalone: true,
  imports: [SharedModule, RouterModule],
})
export class PageErrorComponent implements OnInit {
  statusCode: number | null = null;
  title = 'Có lỗi xảy ra';
  message = 'Đã có sự cố. Vui lòng thử lại.';
  emoji = '⚠️';
  numLabel = 'ERR';

  constructor(private _route: ActivatedRoute, private _router: Router) {}

  ngOnInit(): void {
    this._route.paramMap.subscribe((params) => {
      this.statusCode = Number(params.get('statusCode'));
      this.applyContent();
    });
  }

  private applyContent() {
    switch (this.statusCode) {
      case StatusCode.NotFound:
        this.title = 'Không tìm thấy';
        this.message = 'Trang hoặc dữ liệu bạn cần không còn tồn tại.';
        this.emoji = '🧐';
        this.numLabel = '404';
        break;
      case StatusCode.Unauthorized:
        this.title = 'Cần đăng nhập lại';
        this.message = 'Phiên làm việc đã hết hạn. Đăng nhập lại để tiếp tục.';
        this.emoji = '🔐';
        this.numLabel = '401';
        break;
      case StatusCode.Forbidden:
        this.title = 'Không có quyền';
        this.message = 'Bạn không được phép truy cập mục này.';
        this.emoji = '🚫';
        this.numLabel = '403';
        break;
      case StatusCode.InternalServerError:
        this.title = 'Server đang nghỉ ngơi';
        this.message = 'Lỗi không lường được. Bạn thử lại sau ít phút giúp mình nha.';
        this.emoji = '🛠️';
        this.numLabel = '500';
        break;
    }
  }

  goHome() { this._router.navigate(['/dashboard']); }
  goLogin() { this._router.navigate(['/login']); }
  goBack() { history.length > 1 ? history.back() : this.goHome(); }

  get isUnauth(): boolean { return this.statusCode === StatusCode.Unauthorized; }
}
