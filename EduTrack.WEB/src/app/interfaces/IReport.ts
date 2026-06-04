export interface IMonthlyPoint {
  year: number;
  month: number;
  lessonsDone: number;
  revenue: number;
  collected: number;
}

export interface ITopStudent {
  idStudent: string;
  fullName: string;
  lessonsDone: number;
  revenue: number;
}

export interface IReportSummary {
  totalStudents: number;
  lessonsDoneLifetime: number;
  revenueLifetime: number;
  collectedLifetime: number;
  outstandingTotal: number;
}

/** Doanh thu theo môn — tính từ các buổi Done 6 tháng gần nhất */
export interface ISubjectRevenue {
  subject: string;
  lessonsDone: number;
  amount: number;
}

export interface ITutorReport {
  summary: IReportSummary;
  months: IMonthlyPoint[];
  topStudents: ITopStudent[];
  revenueBySubject: ISubjectRevenue[];
}
