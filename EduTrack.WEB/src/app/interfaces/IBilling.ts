import { PlanCode, SubscriptionStatus } from './IAdmin';

/** Subscription của tutor đang login — `/billing` page. */
export interface IMySubscription {
  id: string;
  plan: PlanCode;
  status: SubscriptionStatus;
  trialEndsAt?: string;
  currentPeriodEnd?: string;
  expiresAt?: string;
  daysRemaining?: number;
  currentPrice: number;
  basicPrice: number;
  proPrice: number;
}

/** 1 dòng lịch sử thanh toán của tutor. */
export interface IMyPayment {
  id: string;
  plan: PlanCode;
  months: number;
  amount: number;
  confirmedAt?: string;
  notes?: string;
}
