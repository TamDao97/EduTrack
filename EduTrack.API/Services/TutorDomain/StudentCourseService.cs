using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    public interface IStudentCourseService : IBaseService<StudentCourse, StudentCourseDto>
    {
        Task<Response<List<StudentCourseDto>>> GetByStudentAsync(Guid idStudent);
        Task<Response<StudentCourseDto>> CreateCourseAsync(StudentCourseDto dto);
        Task<Response<StudentCourseDto>> UpdateCourseAsync(StudentCourseDto dto);
    }

    public class StudentCourseService : TutorScopedBaseService<StudentCourse, StudentCourseDto>, IStudentCourseService
    {
        private readonly ITDRepository<Student> _studentRepos;

        public StudentCourseService(IUnitOfWork uow, IUserContextService ctx) : base(uow, ctx)
        {
            _studentRepos = uow.GetRepository<Student>();
        }

        public async Task<Response<List<StudentCourseDto>>> GetByStudentAsync(Guid idStudent)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var list = await _repos.TableNoTracking
                .Where(c => c.IdTutor == idTutor && c.IdStudent == idStudent)
                .OrderByDescending(c => c.IsActive).ThenBy(c => c.Subject)
                .ToListAsync();
            return Response<List<StudentCourseDto>>.Success(
                AutoMapperGeneric.Map<List<StudentCourse>, List<StudentCourseDto>>(list), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<StudentCourseDto>> CreateCourseAsync(StudentCourseDto dto)
        {
            var err = await ValidateAsync(dto);
            if (err != null) return Response<StudentCourseDto>.Error(StatusCode.BadRequest, err);

            var idTutor = await GetCurrentTutorIdAsync();
            var entity = new StudentCourse
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                IdStudent = dto.IdStudent,
                Subject = dto.Subject.Trim(),
                PerLessonRate = dto.PerLessonRate,
                IsActive = dto.IsActive,
                Notes = dto.Notes,
            };
            await _repos.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Response<StudentCourseDto>.Success(
                AutoMapperGeneric.Map<StudentCourse, StudentCourseDto>(entity), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<StudentCourseDto>> UpdateCourseAsync(StudentCourseDto dto)
        {
            var err = await ValidateAsync(dto);
            if (err != null) return Response<StudentCourseDto>.Error(StatusCode.BadRequest, err);

            var idTutor = await GetCurrentTutorIdAsync();
            var entity = await _repos.Table.FirstOrDefaultAsync(c => c.Id == dto.Id && c.IdTutor == idTutor);
            if (entity == null)
                return Response<StudentCourseDto>.Error(StatusCode.NotFound, "Không tìm thấy môn học");

            entity.Subject = dto.Subject.Trim();
            entity.PerLessonRate = dto.PerLessonRate;
            entity.IsActive = dto.IsActive;
            entity.Notes = dto.Notes;
            entity.MarkDirty(nameof(entity.Subject));
            entity.MarkDirty(nameof(entity.PerLessonRate));
            entity.MarkDirty(nameof(entity.IsActive));
            entity.MarkDirty(nameof(entity.Notes));
            await _unitOfWork.SaveChangesAsync();

            return Response<StudentCourseDto>.Success(
                AutoMapperGeneric.Map<StudentCourse, StudentCourseDto>(entity), StatusCode.Ok.ToDescription());
        }

        private async Task<string?> ValidateAsync(StudentCourseDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Subject)) return "Vui lòng nhập tên môn";
            if (dto.PerLessonRate < 0) return "Giá buổi không hợp lệ";

            var idTutor = await GetCurrentTutorIdAsync();
            var studentOk = await _studentRepos.TableNoTracking
                .AnyAsync(s => s.Id == dto.IdStudent && s.IdTutor == idTutor);
            if (!studentOk) return "Học sinh không hợp lệ";

            // Không cho 2 môn trùng tên trên cùng HS
            var dup = await _repos.TableNoTracking.AnyAsync(c =>
                c.IdStudent == dto.IdStudent && c.Id != dto.Id
                && c.Subject.Trim().ToLower() == dto.Subject.Trim().ToLower());
            if (dup) return $"Học sinh đã có môn \"{dto.Subject.Trim()}\"";

            return null;
        }
    }
}
