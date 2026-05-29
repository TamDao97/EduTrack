using Microsoft.EntityFrameworkCore;
using EduTrack.API.DataContext;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Dto.Core;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services
{
    public interface IFileService
    {
        Task<Response<FileDto>> UploadFileAsync(FileUploadRequest file, CurrentUser currentUser);
        Task<Response<FileDto>> GetByIdAsync(Guid id);
        Task<Response<List<FileDto>>> GetByIdsAsync(List<Guid> ids);
        Task<Response<bool>> DeleteAsync(Guid id);
    }

    public class FileService : IFileService
    {
        private readonly EduTrackDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FileService(EduTrackDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Response<FileDto>> UploadFileAsync(FileUploadRequest request, CurrentUser currentUser)
        {
            var extension = Path.GetExtension(request.File.FileName).ToLower();

            string folder = extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" => "images",
                ".pdf" => "pdfs",
                ".doc" or ".docx" => "words",
                _ => "others"
            };

            string folderPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(folderPath);

            string newFileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(folderPath, newFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var entity = new EduTrack.API.DataContext.Entity.Core.File
            {
                Id = Guid.NewGuid(),
                OriginalName = request.File.FileName,
                FileName = newFileName,
                FilePath = $"/uploads/{folder}/{newFileName}",
                FileType = folder,
                FileSize = request.File.Length,
                UploadedBy = currentUser.Id,
                UploadedAt = DateTime.UtcNow,
            };

            _context.Files.Add(entity);
            await _context.SaveChangesAsync();
            var item = AutoMapperGeneric.Map<EduTrack.API.DataContext.Entity.Core.File, FileDto>(entity);
            return Response<FileDto>.Success(item, StatusCode.Ok.ToDescription());
        }

        //public async Task<List<File>> GetAllAsync() =>
        //    await _context.Files.Where(f => !f.IsDeleted).ToListAsync();

        public async Task<Response<FileDto>?> GetByIdAsync(Guid id)
        {
            var item = await _context.Files.Where(r => r.Id == id && !r.IsDeleted).Select(r => AutoMapperGeneric.Map<EduTrack.API.DataContext.Entity.Core.File, FileDto>(r)).FirstOrDefaultAsync();
            if (item is null) return Response<FileDto>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());
            return Response<FileDto>.Success(item, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<FileDto>>?> GetByIdsAsync(List<Guid> ids)
        {
            var datas = await _context.Files.Where(r => ids.Contains(r.Id) && !r.IsDeleted).Select(r => AutoMapperGeneric.Map<EduTrack.API.DataContext.Entity.Core.File, FileDto>(r)).ToListAsync();
            return Response<List<FileDto>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<bool>> DeleteAsync(Guid id)
        {
            var file = _context.Files.Where(r => r.Id == id && !r.IsDeleted).FirstOrDefault();
            if (file == null) return Response<bool>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());

            string fullPath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return Response<bool>.Success(true, StatusCode.Ok.ToDescription());
        }
    }
}
