using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using XIVClearSS.Core;
using XIVClearSS.Services;
using ImGui = Dalamud.Bindings.ImGui.ImGui;

namespace XIVClearSS.UI
{
    public class MainWindow : Window
    {
        private readonly Configuration    _config;
        private readonly ResolutionService _resolutionService;
        private readonly IKeyState         _keyState;
        private readonly System.Action     _onHotkeySet;

        private int  _inputWidth;
        private int  _inputHeight;
        private bool _capturingHotkey = false;

        public MainWindow(Configuration config, ResolutionService resolutionService, IKeyState keyState, System.Action onHotkeySet)
            : base("ClearSS", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            _config            = config;
            _resolutionService = resolutionService;
            _keyState          = keyState;
            _onHotkeySet       = onHotkeySet;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(360, 260),
                MaximumSize = new Vector2(600, 500),
            };

            _inputWidth  = (int)_config.TargetWidth;
            _inputHeight = (int)_config.TargetHeight;
        }

        public override void Draw()
        {
            bool active = _resolutionService.IsHighResActive;

            // Disclaimer
            ImGui.TextColored(new Vector4(1f, 0.85f, 0.2f, 1f), "Note: Game must be in Windowed mode for resolution switching to work.");
            ImGui.Spacing();

            if (active)
            {
                ImGui.TextColored(new Vector4(0.2f, 1f, 0.2f, 1f), $"HIGH-RES ACTIVE  ({_config.TargetWidth} x {_config.TargetHeight})");
                ImGui.TextUnformatted("Take your screenshot, then click Stop or press your hotkey.");
                ImGui.Spacing();

                if (ImGui.Button("Stop — Restore Original Resolution"))
                    _resolutionService.Restore();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }

            // --- Presets ---
            ImGui.TextUnformatted("Preset");
            ImGui.SameLine();

            foreach (var p in ResolutionPresets.Standard)
            {
                bool selected = _inputWidth == (int)p.Width && _inputHeight == (int)p.Height;
                if (selected) ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 1f, 1f));

                if (ImGui.SmallButton(p.Label))
                {
                    _inputWidth  = (int)p.Width;
                    _inputHeight = (int)p.Height;
                }

                if (selected) ImGui.PopStyleColor();

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip($"{p.Width} x {p.Height}");

                ImGui.SameLine();
            }

            ImGui.NewLine();
            ImGui.Spacing();

            // --- Custom ---
            ImGui.SetNextItemWidth(90);
            if (ImGui.InputInt("W##w", ref _inputWidth, 0)) _inputWidth = System.Math.Max(1, _inputWidth);
            ImGui.SameLine();
            ImGui.TextUnformatted("x");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(90);
            if (ImGui.InputInt("H##h", ref _inputHeight, 0)) _inputHeight = System.Math.Max(1, _inputHeight);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // --- Hotkey ---
            ImGui.TextUnformatted("Toggle Hotkey");
            ImGui.SameLine();

            if (_capturingHotkey)
            {
                ImGui.TextColored(new Vector4(1f, 0.6f, 0.1f, 1f), "Press any key... (Esc to cancel)");

                foreach (VirtualKey key in _keyState.GetValidVirtualKeys())
                {
                    if (_keyState[key])
                    {
                        if (key == VirtualKey.ESCAPE)
                        {
                            _capturingHotkey = false;
                        }
                        else
                        {
                            _config.ToggleHotkey = (int)key;
                            _config.Save();
                            _capturingHotkey = false;
                            _onHotkeySet();
                        }
                        break;
                    }
                }
            }
            else
            {
                string hotkeyLabel = _config.ToggleHotkey == 0
                    ? "None"
                    : ((VirtualKey)_config.ToggleHotkey).ToString();

                if (ImGui.Button(hotkeyLabel + "##hotkey"))
                    _capturingHotkey = true;

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Click to set a new hotkey.");

                if (_config.ToggleHotkey != 0)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Clear"))
                    {
                        _config.ToggleHotkey = 0;
                        _config.Save();
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // --- Go button (hidden while active) ---
            if (!active)
            {
                bool dirty = (uint)_inputWidth != _config.TargetWidth || (uint)_inputHeight != _config.TargetHeight;
                if (dirty)
                {
                    if (ImGui.Button("Save"))
                    {
                        _config.TargetWidth  = (uint)_inputWidth;
                        _config.TargetHeight = (uint)_inputHeight;
                        _config.Save();
                    }
                    ImGui.SameLine();
                }

                if (ImGui.Button($"Go  ({_inputWidth} x {_inputHeight})"))
                {
                    _config.TargetWidth  = (uint)_inputWidth;
                    _config.TargetHeight = (uint)_inputHeight;
                    _config.Save();
                    _resolutionService.GoHighRes();
                }
            }
        }

        public void Dispose() { }
    }
}
