namespace EduTrack.API.DataContext.Dto.Core
{
    public class FileDto
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = "";
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public Guid UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }

    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
    }
}
