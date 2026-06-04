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
  /** Môn/lớp của buổi (StudentCourse) — null = buổi cũ chưa gắn môn */
  idCourse?: string | null;
  scheduledDate: string;
  startTime: string; // "HH:mm:ss"
  endTime: string;
  location?: string;
  status: LessonStatus;
  chargeAmount: number;
  doneAt?: string;
  notes?: string;
  idTuitionPeriod?: string;
  /** Buổi NHÓM: các lesson cùng ca chia sẻ groupKey. Null = buổi 1-1 */
  groupKey?: string | null;
  /** Buổi sinh từ Lớp học nào — null = buổi 1-1 / nhóm ad-hoc */
  idClass?: string | null;
}

export interface ILessonDetail extends ILesson {
  studentFullName?: string;
  parentPhone?: string;
  /** Tên môn của buổi — để hiện chip môn trên lịch */
  courseSubject?: string | null;
  /** Tên lớp nếu buổi sinh từ Lớp học */
  className?: string | null;
}

export interface ILessonGridFilter extends IGridFilterBase {
  idStudent?: string | null;
  status?: LessonStatus | null;
  fromDate?: string | null;
  toDate?: string | null;
}

/** 1 HS trong buổi nhóm — môn riêng (giá theo môn từng em) */
export interface IGroupStudentReq {
  idStudent: string;
  idCourse?: string | null;
}

/** Tạo buổi NHÓM: nhiều HS chung 1 ca, mỗi em 1 lesson riêng cùng groupKey */
export interface ILessonGroupCreateReq {
  students: IGroupStudentReq[];
  scheduledDate: string;
  startTime: string;
  endTime: string;
  location?: string;
  notes?: string;
  numberOfWeeks: number;
}

export interface ILessonBulkCreateReq {
  idStudent: string;
  idCourse?: string | null;
  startDate: string;
  numberOfWeeks: number;
  /** 0=CN, 1=T2 … 6=T7 — match JS Date.getDay() */
  daysOfWeek: number[];
  startTime: string; // "HH:mm"
  endTime: string;
  location?: string;
}
