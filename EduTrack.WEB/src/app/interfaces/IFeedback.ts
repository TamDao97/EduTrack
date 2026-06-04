/** Khớp BE FeedbackTypeEnums */
export enum FeedbackType {
  GopY = 1,
  BaoLoi = 2,
  TinhNang = 3,
  Khac = 4,
}

export const FeedbackTypeLabel: Record<FeedbackType, string> = {
  [FeedbackType.GopY]: 'Góp ý',
  [FeedbackType.BaoLoi]: 'Báo lỗi',
  [FeedbackType.TinhNang]: 'Đề xuất tính năng',
  [FeedbackType.Khac]: 'Khác',
};

/** Khớp BE FeedbackStatusEnums */
export enum FeedbackStatus {
  Moi = 1,
  DangXemXet = 2,
  SeLam = 3,
  DaLam = 4,
  TuChoi = 5,
}

export const FeedbackStatusLabel: Record<FeedbackStatus, string> = {
  [FeedbackStatus.Moi]: 'Mới',
  [FeedbackStatus.DangXemXet]: 'Đang xem xét',
  [FeedbackStatus.SeLam]: 'Sẽ làm',
  [FeedbackStatus.DaLam]: 'Đã làm',
  [FeedbackStatus.TuChoi]: 'Từ chối',
};

export interface IFeedback {
  id?: string;
  idTutor?: string;
  type: FeedbackType;
  rating?: number | null;
  title: string;
  content: string;
  status?: FeedbackStatus;
  adminNote?: string | null;
  dateCreated?: string;
}

/** Dòng cho bảng admin — kèm tên tutor gửi */
export interface IFeedbackDetail extends IFeedback {
  tutorDisplayName?: string;
  tutorUserName?: string;
}
