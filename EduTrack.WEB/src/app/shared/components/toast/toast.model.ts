/** Loại toast — quyết định màu + icon. */
export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface IToast {
  id: number;
  type: ToastType;
  title: string;
  message: string;
  /** ms hiển thị trước khi tự đóng. */
  duration: number;
  /** true = đang chạy animation rời đi (chờ remove khỏi list). */
  leaving: boolean;
}
