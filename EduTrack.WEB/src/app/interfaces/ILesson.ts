import { IBase, IGridFilterBase } from '../shared/interfaces/IBase-ext';

export enum LessonStatus {
  Scheduled = 1,
  Done = 2,
  Cancelled = 3,
}

export const LessonStatusLabel: Record<number, string> = {
  [LessonStatus.Scheduled]: 'Sắp tới',
  [LessonStatus.Done]: 'Đã dạy',
  [LessonStatus.Cancelled]: 'Đã huỷ',
};

export interface ILesson extends IBase {
  idTutor?: string;
  idStudent: string;
  scheduledDate: string;
  startTime: string; // "HH:mm:ss"
  endTime: string;
  location?: string;
  status: LessonStatus;
  chargeAmount: number;
  doneAt?: string;
  notes?: string;
  idTuitionPeriod?: string;
}

export interface ILessonDetail extends ILesson {
  studentFullName?: string;
  parentPhone?: string;
}

export interface ILessonGridFilter extends IGridFilterBase {
  idStudent?: string | null;
  status?: LessonStatus | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface ILessonBulkCreateReq {
  idStudent: string;
  startDate: string;
  numberOfWeeks: number;
  /** 0=CN, 1=T2 … 6=T7 — match JS Date.getDay() */
  daysOfWeek: number[];
  startTime: string; // "HH:mm"
  endTime: string;
  location?: string;
}
