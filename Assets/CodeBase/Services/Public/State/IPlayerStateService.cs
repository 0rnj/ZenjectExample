namespace CodeBase.Services.Public.State
{
    public interface IPlayerStateService
    {
        IPlayerState PlayerState { get; }
        void Save();
        void Load();
    }
}