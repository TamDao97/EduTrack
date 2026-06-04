import { IBase, IGridFilterBase } from '../shared/interfaces/IBase-ext';

export enum StudentStatus {
  Active = 1,
  Paused = 2,
  Stopped = 3,
}

export const StudentStatusLabel: Record<number, string> = {
  [StudentStatus.Active]: 'Đang học',
  [StudentStatus.Paused]: 'Tạm nghỉ',
  [StudentStatus.Stopped]: 'Đã dừng',
};

export interface IStudent extends IBase {
  idTutor?: string;
  idParent: string;
  fullName: string;
  dateBirth?: string;
  grade?: string;
  subject?: string;
  perLessonRate: number;
  avatarFileId?: string;
  status: StudentStatus;
  startedAt?: string;
  notes?: string;
}

export interface IStudentDetail extends IStudent {
  parentFullName?: string;
  parentPhone?: string;
  parentEmail?: string;
  avatarUrl?: string;
  /** Các môn ĐANG HỌC (StudentCourse active), nối " · " — vd "Toán · Lý" */
  courseSubjects?: string | null;
}

export interface IStudentGridFilter extends IGridFilterBase {
  status?: StudentStatus | null;
  idParent?: string | null;
}
