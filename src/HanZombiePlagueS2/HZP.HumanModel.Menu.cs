using System.Drawing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace HanZombiePlagueS2;

public class HZPHumanModelMenu
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HZPHumanModelMenu> _logger;
    private readonly HZPMenuHelper _menuhelper;
    private readonly IOptionsMonitor<HZPMainCFG> _mainCFG;
    private readonly HZPHelpers _helpers;
    private readonly HZPGlobals _globals;
    private readonly PlayerZombieState _zombieState;

    public HZPHumanModelMenu(
        ISwiftlyCore core,
        ILogger<HZPHumanModelMenu> logger,
        HZPMenuHelper menuhelper,
        IOptionsMonitor<HZPMainCFG> mainCFG,
        HZPHelpers helpers,
        HZPGlobals globals,
        PlayerZombieState zombieState)
    {
        _core = core;
        _logger = logger;
        _menuhelper = menuhelper;
        _mainCFG = mainCFG;
        _helpers = helpers;
        _globals = globals;
        _zombieState = zombieState;
    }

    public IMenuAPI? OpenHumanModelMenu(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return null;

        _globals.IsZombie.TryGetValue(player.PlayerID, out var isZombie);
        if (isZombie)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanModelMenuZombieBlocked"));
            return null;
        }

        var cfg = _mainCFG.CurrentValue;
        var models = _helpers.GetEnabledHumanModels(cfg);
        if (models.Count == 0)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanModelMenuEmpty"));
            return null;
        }

        IMenuAPI menu = _menuhelper.CreateMenu(_helpers.T(player, "HumanModelMenu"));
        var selectedModelName = _zombieState.GetPlayerHumanModelPreference(player.PlayerID, player.SteamID);

        menu.AddOption(new TextMenuOption(HtmlGradient.GenerateGradientText(
            _helpers.T(player, "HumanModelMenuSelect"),
            Color.LightPink, Color.LightBlue, Color.LightPink),
            updateIntervalMs: 500, pauseIntervalMs: 100)
        {
            TextStyle = MenuOptionTextStyle.ScrollLeftLoop
        });

        string defaultText = $"{_helpers.T(player, "HumanModelMenuDefault")} {(string.IsNullOrWhiteSpace(selectedModelName) ? "✓" : string.Empty)}";
        var defaultButton = new ButtonMenuOption(defaultText)
        {
            TextStyle = MenuOptionTextStyle.ScrollLeftLoop,
            CloseAfterClick = true,
            Tag = "extend"
        };

        defaultButton.Click += async (_, args) =>
        {
            var clicker = args.Player;
            _core.Scheduler.NextTick(() =>
            {
                if (clicker == null || !clicker.IsValid)
                    return;

                _zombieState.SetPlayerHumanModelPreference(clicker.PlayerID, clicker.SteamID, null);
                ApplyModelImmediatelyIfPossible(clicker, cfg);
                clicker.SendMessage(MessageType.Chat, _helpers.T(clicker, "HumanModelMenuDefaultInfo"));
            });
        };

        menu.AddOption(defaultButton);

        foreach (var model in models)
        {
            string buttonText = $"{model.Name} {(selectedModelName == model.Name ? "✓" : string.Empty)}";
            var button = new ButtonMenuOption(buttonText)
            {
                TextStyle = MenuOptionTextStyle.ScrollLeftLoop,
                CloseAfterClick = true,
                Tag = "extend"
            };

            button.Click += async (_, args) =>
            {
                var clicker = args.Player;
                _core.Scheduler.NextTick(() =>
                {
                    if (clicker == null || !clicker.IsValid)
                        return;

                    _zombieState.SetPlayerHumanModelPreference(clicker.PlayerID, clicker.SteamID, model.Name);
                    ApplyModelImmediatelyIfPossible(clicker, cfg);
                    clicker.SendMessage(MessageType.Chat, $"{_helpers.T(clicker, "HumanModelMenuSelectInfo")} {model.Name}");
                });
            };

            menu.AddOption(button);
        }

        _core.MenusAPI.OpenMenuForPlayer(player, menu);
        return menu;
    }

    private void ApplyModelImmediatelyIfPossible(IPlayer player, HZPMainCFG cfg)
    {
        if (player == null || !player.IsValid)
            return;

        _globals.IsZombie.TryGetValue(player.PlayerID, out var isZombie);
        _globals.IsSurvivor.TryGetValue(player.PlayerID, out var isSurvivor);
        _globals.IsSniper.TryGetValue(player.PlayerID, out var isSniper);
        _globals.IsHero.TryGetValue(player.PlayerID, out var isHero);

        if (isZombie || isSurvivor || isSniper || isHero)
            return;

        _helpers.ScheduleApplyHumanModel(player, cfg, 0.05f);
    }
}