using UnityEngine;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.evaconstructionmodefix.settings;

namespace com.github.lhervier.ksp.evaconstructionmodefix.ui
{
    /// <summary>
    /// State of the settings window: plain properties that fire a KSP EventVoid on change, which the uGUI
    /// controllers subscribe to. Same shape as the sibling mods' view models.
    ///
    /// It is also the single place where a setting, its persisted copy and the fix's static switch are kept
    /// in step — the fix's switch has to be static (its addon is rebuilt at every scene change) while the
    /// setting outlives the session, so something has to own that pairing.
    /// </summary>
    public class EvaCMGroundViewModel : MonoBehaviour
    {
        private EvaCMGroundSettings _settings;

        /// <summary>Fired whenever <see cref="LogLevel"/> changes.</summary>
        public readonly EventVoid OnLogLevelChanged =
            new EventVoid("EvaCMGroundViewModel.OnLogLevelChanged");

        /// <summary>Binds the view model to the settings and applies them. Call once, right after AddComponent.</summary>
        public EvaCMGroundViewModel WithSettings(EvaCMGroundSettings settings)
        {
            _settings = settings;
            ModLogger.SetLogLevel(settings.LogLevel);
            return this;
        }

        /// <summary>How verbose the mod's logging is.</summary>
        public LogLevel LogLevel
        {
            get { return _settings != null ? _settings.LogLevel : LogLevel.Info; }
            set
            {
                if (_settings == null || _settings.LogLevel == value)
                {
                    return;
                }
                _settings.LogLevel = value;
                _settings.Save();
                ModLogger.SetLogLevel(value);
                OnLogLevelChanged.Fire();
            }
        }
    }
}
