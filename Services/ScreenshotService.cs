using System;
using Dalamud.Plugin.Services;
using XIVClearSS.Core;
using XIVClearSS.Helpers;

namespace XIVClearSS.Services
{
    public class ResolutionService : IDisposable
    {
        private readonly Configuration    _config;
        private readonly IPluginLog       _log;
        private readonly IChatGui         _chat;
        private readonly WindowSizeHelper _windowSizeHelper;

        private uint _originalWidth;
        private uint _originalHeight;

        public bool IsHighResActive { get; private set; } = false;

        public ResolutionService(Configuration config, IPluginLog log, IChatGui chat)
        {
            _config           = config;
            _log              = log;
            _chat             = chat;
            _windowSizeHelper = new WindowSizeHelper(new WindowSearchHelper());
        }

        public void GoHighRes()
        {
            if (IsHighResActive)
            {
                _chat.Print("[XIVClearSS] Already in high-res mode. Use /clearss stop to restore first.");
                return;
            }

            try
            {
                (_originalWidth, _originalHeight) = _windowSizeHelper.GetCurrentSize();
                _config.SavedOriginalWidth  = _originalWidth;
                _config.SavedOriginalHeight = _originalHeight;
                _config.WasHighResOnExit    = true;
                _config.Save();

                _chat.Print($"[XIVClearSS] Saved original size: {_originalWidth}x{_originalHeight}. Switching to {_config.TargetWidth}x{_config.TargetHeight}...");

                _windowSizeHelper.SetSize((int)_config.TargetWidth, (int)_config.TargetHeight);

                IsHighResActive = true;
                _chat.Print($"[XIVClearSS] Now at {_config.TargetWidth}x{_config.TargetHeight}. Take your screenshot, then /clearss stop.");
            }
            catch (Exception ex)
            {
                _chat.Print($"[XIVClearSS] Failed to switch resolution: {ex.Message}");
                _log.Error(ex, "[XIVClearSS] GoHighRes failed");
            }
        }

        public void Restore()
        {
            if (!IsHighResActive)
            {
                _chat.Print("[XIVClearSS] Not in high-res mode.");
                return;
            }

            try
            {
                _windowSizeHelper.SetSize((int)_originalWidth, (int)_originalHeight);
                IsHighResActive = false;
                _config.WasHighResOnExit = false;
                _config.Save();
                _chat.Print($"[XIVClearSS] Restored to {_originalWidth}x{_originalHeight}.");
            }
            catch (Exception ex)
            {
                _chat.Print($"[XIVClearSS] Failed to restore resolution: {ex.Message}");
                _log.Error(ex, "[XIVClearSS] Restore failed");
            }
        }

        public void ForceRestore(uint originalWidth, uint originalHeight)
        {
            _originalWidth  = originalWidth;
            _originalHeight = originalHeight;
            IsHighResActive = true;
            Restore();
        }

        public void Dispose()
        {
            if (IsHighResActive)
                Restore();
        }
    }
}
