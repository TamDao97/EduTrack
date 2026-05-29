import { IBase, IGridFilterBase } from '../shared/interfaces/IBase-ext';

export enum NotificationType {
  LessonReminderEvening = 1,
  LessonReminderHourBefore = 2,
  TuitionIssued = 3,
  TuitionOverdue = 4,
}

export const NotificationTypeLabel: Record<number, string> = {
  [NotificationType.LessonReminderEvening]: 'Nhắc lịch · tối hôm trước',
  [NotificationType.LessonReminderHourBefore]: 'Nhắc lịch · 1h trước',
  [NotificationType.TuitionIssued]: 'Thông báo học phí',
  [NotificationType.TuitionOverdue]: 'Nhắc nợ học phí',
};

export enum NotificationStatus {
  Pending = 1,
  Sent = 2,
  Read = 3,
  Cancelled = 4,
}

export const NotificationStatusLabel: Record<number, string> = {
  [NotificationStatus.Pending]: 'Chờ gửi',
  [NotificationStatus.Sent]: 'Đã gửi',
  [NotificationStatus.Read]: 'Đã đọc',
  [NotificationStatus.Cancelled]: 'Đã huỷ',
};

export interface INotification extends IBase {
  idTutor?: string;
  idStudent?: string;
  type: NotificationType;
  refId?: string;
  title: string;
  bodyText: string;
  zaloDeepLink?: string;
  parentPhone?: string;
  studentFullName?: string;
  status: NotificationStatus;
  scheduledAt: string;
  sentAt?: string;
}

export interface INotificationGridFilter extends IGridFilterBase {
  status?: NotificationStatus | null;
  type?: NotificationType | null;
  idStudent?: string | null;
}
