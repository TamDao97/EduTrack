namespace EduTrack.API.DataContext.Entity.Core
{
    public class File
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
}
