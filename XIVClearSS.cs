using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XIVClearSS.Core;
using XIVClearSS.Services;
using XIVClearSS.UI;

namespace XIVClearSS
{
    public sealed class XIVClearSSPlugin : IDalamudPlugin
    {
        public string Name => "ClearSS";

        private const string CommandName = "/clearss";

        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ICommandManager         _commandManager;
        private readonly IFramework              _framework;
        private readonly IKeyState               _keyState;

        public Configuration      Configuration     { get; private set; }
        private ResolutionService ResolutionService { get; set; }
        private MainWindow        MainWindow        { get; set; }
        private WindowSystem      WindowSystem      = new("XIVClearSS");

        private bool _hotkeyWasDown = false;

        public XIVClearSSPlugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IPluginLog pluginLog,
            IChatGui chatGui,
            IKeyState keyState,
            IFramework framework)
        {
            _pluginInterface = pluginInterface;
            _commandManager  = commandManager;
            _framework       = framework;
            _keyState        = keyState;

            Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(pluginInterface);

            ResolutionService = new ResolutionService(Configuration, pluginLog, chatGui);

            if (Configuration.WasHighResOnExit && Configuration.SavedOriginalWidth > 0)
                ResolutionService.ForceRestore(Configuration.SavedOriginalWidth, Configuration.SavedOriginalHeight);

            MainWindow = new MainWindow(Configuration, ResolutionService, keyState, () => _hotkeyWasDown = true);

            WindowSystem.AddWindow(MainWindow);

            commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "/clearss — open window | /clearss go — switch to high-res | /clearss stop — restore resolution"
            });

            pluginInterface.UiBuilder.Draw        += DrawUI;
            pluginInterface.UiBuilder.OpenMainUi  += OpenMainUI;
            pluginInterface.UiBuilder.OpenConfigUi += OpenMainUI;
            framework.Update += OnUpdate;
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;

            _pluginInterface.UiBuilder.Draw        -= DrawUI;
            _pluginInterface.UiBuilder.OpenMainUi  -= OpenMainUI;
            _pluginInterface.UiBuilder.OpenConfigUi -= OpenMainUI;

            _commandManager.RemoveHandler(CommandName);

            WindowSystem.RemoveAllWindows();
            MainWindow.Dispose();
            ResolutionService.Dispose();
        }

        private void OnUpdate(IFramework framework)
        {
            if (Configuration.ToggleHotkey == 0) return;

            var key = (VirtualKey)Configuration.ToggleHotkey;
            bool isDown = _keyState[key];

            if (isDown && !_hotkeyWasDown)
            {
                if (ResolutionService.IsHighResActive)
                    ResolutionService.Restore();
                else
                    ResolutionService.GoHighRes();
            }

            _hotkeyWasDown = isDown;
        }

        private void OnCommand(string command, string args)
        {
            switch (args.Trim().ToLower())
            {
                case "go":   ResolutionService.GoHighRes(); break;
                case "stop": ResolutionService.Restore();   break;
                default:     MainWindow.IsOpen = true;      break;
            }
        }

        private void DrawUI()     => WindowSystem.Draw();
        private void OpenMainUI() => MainWindow.IsOpen = true;
    }
}
