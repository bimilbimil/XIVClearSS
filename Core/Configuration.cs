using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace XIVClearSS.Core
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public uint TargetWidth  { get; set; } = 3840;
        public uint TargetHeight { get; set; } = 2160;

        // VirtualKey int value; 0 = unset
        public int ToggleHotkey { get; set; } = 0;

        public bool DebugLogsEnabled { get; set; } = false;

        [NonSerialized]
        public IDalamudPluginInterface PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface?.SavePluginConfig(this);
        }
    }
}
