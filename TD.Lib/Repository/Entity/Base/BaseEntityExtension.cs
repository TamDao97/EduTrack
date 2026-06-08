namespace TD.Lib.Repository.Entity.Base
{
    public interface ITrackDirty
    {
        void MarkDirty(string propertyName);
        bool IsDirty(string propertyName);
        IReadOnlyCollection<string> GetDirtyProperties();
    }

    public abstract partial class BaseEntity : ITrackDirty
    {
        private readonly HashSet<string> _dirtyProperties = new();

        public void MarkDirty(string propertyName)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
                _dirtyProperties.Add(propertyName);
        }

        public bool IsDirty(string propertyName)
            => _dirtyProperties.Contains(propertyName);

        public IReadOnlyCollection<string> GetDirtyProperties()
            => _dirtyProperties;
    }
}