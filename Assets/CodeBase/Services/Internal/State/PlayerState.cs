using CodeBase.Services.Public.State;

namespace CodeBase.Services.Internal.State
{
    public class PlayerState : IPlayerState
    {
        public int Level {get;set;}
        public int Progress {get;set;}
    }
}