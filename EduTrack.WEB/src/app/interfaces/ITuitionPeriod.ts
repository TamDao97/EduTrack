import { IBase, IGridFilterBase } from '../shared/interfaces/IBase-ext';

export enum TuitionStatus {
  Open = 1,
  Closed = 2,
  PartialPaid = 3,
  Paid = 4,
}

export const TuitionStatusLabel: Record<number, string> = {
  [TuitionStatus.Open]: 'Đang mở',
  [TuitionStatus.Closed]: 'Cần thu',
  [TuitionStatus.PartialPaid]: 'Thu 1 phần',
  [TuitionStatus.Paid]: 'Đã thu',
};

export interface ITuitionPeriod extends IBase {
  idTutor?: string;
  idStudent: string;
  periodMonth: number;
  periodYear: number;
  closedAt?: string;
  totalLessons: number;
  totalAmount: number;
  adjustment: number;
  finalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  status: TuitionStatus;
  notes?: string;
}

export interface ITuitionPeriodDetail extends ITuitionPeriod {
  studentFullName?: string;
  parentFullName?: string;
  parentPhone?: string;
}

export interface ITuitionPeriodGridFilter extends IGridFilterBase {
  idStudent?: string | null;
  status?: TuitionStatus | null;
  periodMonth?: number | null;
  periodYear?: number | null;
}

export interface ITuitionPreviewLine {
  idLesson: string;
  scheduledDate: string;
  startTime: string;
  endTime: string;
  chargeAmount: number;
  /** Môn của buổi — null với buổi cũ chưa gắn môn */
  subject?: string | null;
}

export interface ITuitionPreview {
  idStudent: string;
  periodMonth: number;
  periodYear: number;
  totalLessons: number;
  totalAmount: number;
  /** Số buổi trong tháng còn "Đã lên lịch" (chưa đánh dấu Đã dạy) */
  scheduledLessons: number;
  lessons: ITuitionPreviewLine[];
}
