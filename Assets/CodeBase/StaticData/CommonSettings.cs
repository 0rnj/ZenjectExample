using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Create CommonSettings", fileName = "CommonSettings", order = 0)]
    public class CommonSettings : ScriptableObject
    {
        public CharacterSettings MainHeroSettings;
        public CharacterSettings[] EnemySettingsCollection;
    }
}