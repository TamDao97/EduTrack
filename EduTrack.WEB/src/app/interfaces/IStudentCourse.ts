/** Lớp/Môn học của 1 HS — cho phép 1 HS nhiều môn với giá khác nhau. */
export interface IStudentCourse {
  id?: string;
  idTutor?: string;
  idStudent: string;
  subject: string;
  perLessonRate: number;
  isActive: boolean;
  notes?: string | null;
}
