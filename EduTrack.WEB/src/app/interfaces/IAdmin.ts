import { IGridFilterBase } from '../shared/interfaces/IBase-ext';

export enum PlanCode { Free = 1, Basic = 2, Pro = 3 }
export enum SubscriptionStatus { Trial = 1, Active = 2, Expired = 3, Cancelled = 4 }
export enum PaymentStatus { Pending = 1, Confirmed = 2, Refunded = 3 }

export const PlanLabel: Record<number, string> = {
  [PlanCode.Free]: 'Miễn phí',
  [PlanCode.Basic]: 'Cơ bản',
  [PlanCode.Pro]: 'Chuyên nghiệp',
};

export const PlanPrice: Record<number, number> = {
  [PlanCode.Free]: 0,
  [PlanCode.Basic]: 99000,
  [PlanCode.Pro]: 199000,
};

export const StatusLabel: Record<number, string> = {
  [SubscriptionStatus.Trial]: 'Dùng thử',
  [SubscriptionStatus.Active]: 'Đang hoạt động',
  [SubscriptionStatus.Expired]: 'Hết hạn',
  [SubscriptionStatus.Cancelled]: 'Đã huỷ',
};

export interface ITutorWithSub {
  idTutor: string;
  userName: string;
  displayName: string;
  email?: string;
  signupAt?: string;
  idSubscription?: string;
  plan: PlanCode;
  status: SubscriptionStatus;
  trialEndsAt?: string;
  currentPeriodEnd?: string;
  daysRemaining?: number;
  studentCount: number;
  lessonCountThisMonth: number;
  revenueLifetime: number;
}

export interface IAdminTutorFilter extends IGridFilterBase {
  plan?: PlanCode | null;
  status?: SubscriptionStatus | null;
}

export interface IConfirmPaymentReq {
  idTutor: string;
  plan: PlanCode;
  months: number;
  amount: number;
  transferRef?: string;
  notes?: string;
}

export interface IAdminStats {
  totalTutors: number;
  trialCount: number;
  activeCount: number;
  expiredCount: number;
  cancelledCount: number;
  signupsThisMonth: number;
  mrrEstimate: number;
  revenueThisMonth: number;
  revenueLifetime: number;
}

export interface IAdminPaymentRow {
  id: string;
  idTutor: string;
  tutorDisplayName: string;
  tutorUserName: string;
  plan: PlanCode;
  months: number;
  amount: number;
  transferRef?: string;
  notes?: string;
  confirmedAt?: string;
}
