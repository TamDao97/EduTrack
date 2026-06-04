/** LỚP HỌC — tutor dạy nhóm theo lớp cố định (tên, lịch tuần, giá/buổi/HS). */
export interface IClassRoom {
  id?: string;
  idTutor?: string;
  name: string;
  subject?: string | null;
  defaultRatePerLesson: number;
  /** CSV .NET DayOfWeek (0=CN..6=T7), vd "1,3,5" = T2-T4-T6 */
  daysOfWeek: string;
  startTime: string; // "HH:mm:ss"
  endTime: string;
  location?: string | null;
  isActive: boolean;
  notes?: string | null;
  memberCount?: number;
}

export interface IClassMember {
  id?: string;
  idClass: string;
  idStudent: string;
  studentFullName?: string;
  rateOverride?: number | null;
  effectiveRate?: number;
}

export interface IClassRoomDetail extends IClassRoom {
  members: IClassMember[];
}

/** Lớp mà 1 HS đang ghi danh — hiện ở Chi tiết HS */
export interface IStudentClass {
  idClass: string;
  className: string;
  subject?: string | null;
  effectiveRate: number;
  scheduleLabel: string;
}
