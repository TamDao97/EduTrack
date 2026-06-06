import { CommonModule } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { SharedModule } from '../../shared/modules/shared.module';

interface FaqItem { q: string; a: string; open: boolean; }

/**
 * Trang landing public — không cần auth.
 * Nội dung: hero + 3 feature + pricing + FAQ + footer.
 * CTA chính: nút "Dùng thử miễn phí 14 ngày" → /signup.
 */
@Component({
  selector: 'app-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class LandingComponent {
  scrolled = false;

  constructor(private _router: Router) {}

  @HostListener('window:scroll')
  onScroll() {
    this.scrolled = window.scrollY > 16;
  }

  goSignup() { this._router.navigate(['/signup']); }
  goLogin()  { this._router.navigate(['/login']); }

  features = [
    {
      icon: 'solution',
      color: '#5B5FCF',
      title: 'Quản lý học sinh',
      desc: 'Hồ sơ học sinh, phụ huynh, học phí theo buổi/tháng. Có ghi chú riêng, nhớ ngày sinh, lớp, môn. Tất cả trong 1 chỗ.',
      bullets: ['Tối đa 20 HS gói Cơ bản', 'Lịch sử buổi học theo HS', 'Snapshot phí khi đổi giá'],
    },
    {
      icon: 'calendar',
      color: '#0C8599',
      title: 'Lịch dạy + nhắc Zalo',
      desc: 'Xếp lịch dạy cố định hàng tuần hoặc theo buổi. Tự sinh thông báo trước giờ dạy — bạn forward sang Zalo phụ huynh.',
      bullets: ['Lịch tuần / tháng / lịch sử', 'Nhắc trước 30 phút', 'Đánh dấu HS nghỉ / bù'],
    },
    {
      icon: 'dollar',
      color: '#FF8A65',
      title: 'Học phí + VietQR',
      desc: 'Tính học phí theo tháng. Phụ huynh chuyển khoản bằng QR có sẵn nội dung — bạn xác nhận 1 cú click.',
      bullets: ['QR động auto-fill số tiền', 'Theo dõi đã/chưa thu', 'Hỗ trợ 20+ ngân hàng'],
    },
  ];

  pricings = [
    {
      name: 'Miễn phí',
      price: 0,
      hl: false,
      desc: 'Cho gia sư mới bắt đầu',
      features: ['Tối đa 5 học sinh', 'Lịch + học phí cơ bản', 'Hỗ trợ qua email'],
      cta: 'Bắt đầu miễn phí',
    },
    {
      name: 'Cơ bản',
      price: 99000,
      hl: true,
      ribbon: 'PHỔ BIẾN NHẤT',
      desc: 'Cho gia sư có 6-20 HS',
      features: ['Tối đa 20 HS', 'Nhắc Zalo + VietQR', 'Báo cáo cơ bản', 'Dùng thử 14 ngày'],
      cta: 'Dùng thử 14 ngày',
    },
    {
      name: 'Chuyên nghiệp',
      price: 199000,
      hl: false,
      desc: 'Mở rộng không giới hạn',
      features: ['Không giới hạn HS', 'Báo cáo + export Excel', 'Hỗ trợ ưu tiên', 'Dùng thử 14 ngày'],
      cta: 'Dùng thử 14 ngày',
    },
  ];

  faqs: FaqItem[] = [
    {
      q: 'Tôi có cần cài đặt gì không?',
      a: 'Không. EduTrack chạy hoàn toàn trên trình duyệt — máy tính, điện thoại đều dùng được. Chỉ cần đăng ký tài khoản và bắt đầu.',
      open: true,
    },
    {
      q: 'Phụ huynh có cần đăng nhập không?',
      a: 'Không. Phụ huynh chỉ nhận tin nhắn Zalo + QR thanh toán từ bạn. Họ không cần cài app hay đăng ký gì.',
      open: false,
    },
    {
      q: 'Dữ liệu của tôi có an toàn không?',
      a: 'Có. Mỗi gia sư có không gian dữ liệu riêng, không ai thấy dữ liệu của ai. Sao lưu hàng ngày, mã hoá đường truyền HTTPS.',
      open: false,
    },
    {
      q: 'Tôi muốn hủy thì sao?',
      a: 'Bạn có thể dùng gói Miễn phí mãi mãi với tối đa 5 HS. Nếu cần nhiều hơn mới phải nâng cấp. Không có ràng buộc hợp đồng.',
      open: false,
    },
    {
      q: 'Có phải đóng tiền trước khi dùng thử không?',
      a: 'Không. Tài khoản mới được dùng thử 14 ngày tính năng đầy đủ, không cần khai báo thẻ. Hết 14 ngày tự động về gói Miễn phí.',
      open: false,
    },
  ];

  toggleFaq(i: number) { this.faqs[i].open = !this.faqs[i].open; }

  fmt(n: number): string {
    if (n === 0) return '0';
    return new Intl.NumberFormat('vi-VN').format(n);
  }
}
