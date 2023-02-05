using CodeBase.Services.Public.State;

namespace CodeBase.Services.Internal.State
{
    public class PlayerStateService : IPlayerStateService
    {
        private PlayerState _playerState;

        public IPlayerState PlayerState => _playerState;

        public void Save()
        {
            
        }

        public void Load()
        {
            
        }
    }
}
