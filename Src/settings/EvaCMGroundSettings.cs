using System;
using System.IO;
using System.Reflection;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.fix.evaconstructionmode.settings
{
    /// <summary>
    /// The mod's own settings, per INSTALL rather than per save — they describe how the player wants the
    /// mod to behave, and nothing here belongs to a game.
    ///
    /// Stored in PluginData/ next to the DLL: GameDatabase skips that folder, so KSP never tries to read
    /// this file as a part config. This replaces the eva_cm_ground.cfg that used to sit beside the DLL and
    /// was parsed by hand.
    /// </summary>
    public class EvaCMGroundSettings
    {
        private static readonly ModLogger LOGGER = new ModLogger("Settings");

        private const string ConfigFile = "settings.cfg";
        private const string GeneralNode = "GENERAL";

        private readonly string _path;

        /// <summary>How verbose the mod's logging is.</summary>
        public LogLevel LogLevel { get; set; }

        public EvaCMGroundSettings()
        {
            string dll = Assembly.GetExecutingAssembly().Location;
            _path = Path.Combine(Path.GetDirectoryName(dll), Path.Combine("PluginData", ConfigFile));
            Reset();
        }

        private void Reset()
        {
            // Same default as the sibling mods: what the player is meant to see in KSP.log, no more.
            LogLevel = LogLevel.Info;
        }

        /// <summary>Reads the settings file, falling back on the defaults when it is missing or unreadable.</summary>
        public void Load()
        {
            Reset();
            if (!File.Exists(_path))
            {
                LOGGER.LogInfo("No settings file yet, using the defaults");
                return;
            }

            ConfigNode root = ConfigNode.Load(_path);
            ConfigNode general = root?.GetNode(GeneralNode);
            if (general == null)
            {
                LOGGER.LogWarning($"Unusable settings file '{_path}', using the defaults");
                return;
            }

            string logLevel = LogLevel.ToString();
            general.TryGetValue("logLevel", ref logLevel);
            // An unknown level (hand-edited file, or a level dropped from the enum) keeps the default
            // rather than silencing the log.
            LogLevel parsed;
            if (Enum.TryParse(logLevel, out parsed) && Enum.IsDefined(typeof(LogLevel), parsed))
            {
                LogLevel = parsed;
            }
            else
            {
                LOGGER.LogWarning($"Unknown log level '{logLevel}', keeping {LogLevel}");
            }

            LOGGER.LogInfo($"Settings loaded from {_path} (logLevel={LogLevel})");
        }

        /// <summary>Writes the settings file, creating PluginData/ on first save.</summary>
        public void Save()
        {
            try
            {
                ConfigNode root = new ConfigNode();
                ConfigNode general = root.AddNode(GeneralNode);
                general.AddValue("logLevel", LogLevel.ToString());

                Directory.CreateDirectory(Path.GetDirectoryName(_path));
                root.Save(_path);
            }
            catch (Exception e)
            {
                // A setting that fails to persist is not worth interrupting a flight for.
                LOGGER.LogError($"Could not save the settings to '{_path}': {e.Message}");
            }
        }
    }
}
