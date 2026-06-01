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

export interface ITutorReport {
  summary: IReportSummary;
  months: IMonthlyPoint[];
  topStudents: ITopStudent[];
}
