using EduTrack.API.DataContext;

namespace EduTrack.API.UnitOfWork
{
    public interface IUnitOfWork : TD.Lib.Repository.ITDUnitOfWork, IDisposable
    {
        EduTrackDbContext Context { get; }
    }

    public class UnitOfWork : TD.Lib.Repository.TDUnitOfWork, IUnitOfWork
    {
        public EduTrackDbContext Context { get; }
        public UnitOfWork(EduTrackDbContext Context) : base(Context)
        {
            this.Context = Context;
        }
    }
}
