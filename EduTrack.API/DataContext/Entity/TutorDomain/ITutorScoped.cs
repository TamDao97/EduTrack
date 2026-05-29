namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Marker interface — entity thuộc về 1 gia sư, được auto-filter theo CurrentUser.Id.
    /// </summary>
    public interface ITutorScoped
    {
        Guid IdTutor { get; set; }
    }
}
