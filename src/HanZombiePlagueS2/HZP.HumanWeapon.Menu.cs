using System.Drawing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanZombiePlagueS2;

public class HZPHumanWeaponMenu
{
    private sealed record WeaponEntry(string DisplayName, string ClassName, gear_slot_t Slot);

    private static readonly WeaponEntry[] PistolWeapons =
    [
        new("Glock-18", "weapon_glock", gear_slot_t.GEAR_SLOT_PISTOL),
        new("USP-S", "weapon_usp_silencer", gear_slot_t.GEAR_SLOT_PISTOL),
        new("P2000", "weapon_hkp2000", gear_slot_t.GEAR_SLOT_PISTOL),
        new("Dual Berettas", "weapon_elite", gear_slot_t.GEAR_SLOT_PISTOL),
        new("P250", "weapon_p250", gear_slot_t.GEAR_SLOT_PISTOL),
        new("Five-SeveN", "weapon_fiveseven", gear_slot_t.GEAR_SLOT_PISTOL),
        new("Tec-9", "weapon_tec9", gear_slot_t.GEAR_SLOT_PISTOL),
        new("CZ75-Auto", "weapon_cz75a", gear_slot_t.GEAR_SLOT_PISTOL),
        new("Desert Eagle", "weapon_deagle", gear_slot_t.GEAR_SLOT_PISTOL),
        new("R8 Revolver", "weapon_revolver", gear_slot_t.GEAR_SLOT_PISTOL)
    ];

    private static readonly WeaponEntry[] ShotgunWeapons =
    [
        new("Nova", "weapon_nova", gear_slot_t.GEAR_SLOT_RIFLE),
        new("XM1014", "weapon_xm1014", gear_slot_t.GEAR_SLOT_RIFLE),
        new("MAG-7", "weapon_mag7", gear_slot_t.GEAR_SLOT_RIFLE),
        new("Sawed-Off", "weapon_sawedoff", gear_slot_t.GEAR_SLOT_RIFLE)
    ];

    private static readonly WeaponEntry[] SmgWeapons =
    [
        new("MAC-10", "weapon_mac10", gear_slot_t.GEAR_SLOT_RIFLE),
        new("MP9", "weapon_mp9", gear_slot_t.GEAR_SLOT_RIFLE),
        new("MP7", "weapon_mp7", gear_slot_t.GEAR_SLOT_RIFLE),
        new("MP5-SD", "weapon_mp5sd", gear_slot_t.GEAR_SLOT_RIFLE),
        new("UMP-45", "weapon_ump45", gear_slot_t.GEAR_SLOT_RIFLE),
        new("P90", "weapon_p90", gear_slot_t.GEAR_SLOT_RIFLE),
        new("PP-Bizon", "weapon_bizon", gear_slot_t.GEAR_SLOT_RIFLE)
    ];

    private static readonly WeaponEntry[] RifleWeapons =
    [
        new("Galil AR", "weapon_galilar", gear_slot_t.GEAR_SLOT_RIFLE),
        new("FAMAS", "weapon_famas", gear_slot_t.GEAR_SLOT_RIFLE),
        new("AK-47", "weapon_ak47", gear_slot_t.GEAR_SLOT_RIFLE),
        new("M4A4", "weapon_m4a1", gear_slot_t.GEAR_SLOT_RIFLE),
        new("M4A1-S", "weapon_m4a1_silencer", gear_slot_t.GEAR_SLOT_RIFLE),
        new("AUG", "weapon_aug", gear_slot_t.GEAR_SLOT_RIFLE),
        new("SG 553", "weapon_sg556", gear_slot_t.GEAR_SLOT_RIFLE)
    ];

    private static readonly WeaponEntry[] SniperWeapons =
    [
        new("SSG 08", "weapon_ssg08", gear_slot_t.GEAR_SLOT_RIFLE),
        new("AWP", "weapon_awp", gear_slot_t.GEAR_SLOT_RIFLE),
        new("SCAR-20", "weapon_scar20", gear_slot_t.GEAR_SLOT_RIFLE),
        new("G3SG1", "weapon_g3sg1", gear_slot_t.GEAR_SLOT_RIFLE)
    ];

    private static readonly WeaponEntry[] MachineGunWeapons =
    [
        new("M249", "weapon_m249", gear_slot_t.GEAR_SLOT_RIFLE),
        new("Negev", "weapon_negev", gear_slot_t.GEAR_SLOT_RIFLE)
    ];

    private readonly ISwiftlyCore _core;
    private readonly ILogger<HZPHumanWeaponMenu> _logger;
    private readonly HZPMenuHelper _menuhelper;
    private readonly IOptionsMonitor<HZPMainCFG> _mainCFG;
    private readonly HZPHelpers _helpers;
    private readonly HZPGlobals _globals;

    public HZPHumanWeaponMenu(
        ISwiftlyCore core,
        ILogger<HZPHumanWeaponMenu> logger,
        HZPMenuHelper menuhelper,
        IOptionsMonitor<HZPMainCFG> mainCFG,
        HZPHelpers helpers,
        HZPGlobals globals)
    {
        _core = core;
        _logger = logger;
        _menuhelper = menuhelper;
        _mainCFG = mainCFG;
        _helpers = helpers;
        _globals = globals;
    }

    public IMenuAPI? OpenHumanWeaponMenu(IPlayer player)
    {
        if (!CanUseMenu(player, requireAlive: false))
            return null;

        IMenuAPI menu = _menuhelper.CreateMenu(_helpers.T(player, "HumanWeaponMenu"));
        menu.AddOption(new TextMenuOption(HtmlGradient.GenerateGradientText(
            _helpers.T(player, "HumanWeaponMenuSelect"),
            Color.LightPink, Color.LightBlue, Color.LightPink),
            updateIntervalMs: 500, pauseIntervalMs: 100)
        {
            TextStyle = MenuOptionTextStyle.ScrollLeftLoop
        });

        AddCategoryButton(player, menu, "HumanWeaponMenuPistols", PistolWeapons);
        AddCategoryButton(player, menu, "HumanWeaponMenuShotguns", ShotgunWeapons);
        AddCategoryButton(player, menu, "HumanWeaponMenuSmgs", SmgWeapons);
        AddCategoryButton(player, menu, "HumanWeaponMenuRifles", RifleWeapons);
        AddCategoryButton(player, menu, "HumanWeaponMenuSnipers", SniperWeapons);
        AddCategoryButton(player, menu, "HumanWeaponMenuMachineGuns", MachineGunWeapons);

        _core.MenusAPI.OpenMenuForPlayer(player, menu);
        return menu;
    }

    private void AddCategoryButton(IPlayer player, IMenuAPI menu, string titleKey, IReadOnlyCollection<WeaponEntry> weapons)
    {
        var button = new ButtonMenuOption(_helpers.T(player, titleKey))
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

                OpenWeaponListMenu(clicker, _helpers.T(clicker, titleKey), weapons);
            });
        };

        menu.AddOption(button);
    }

    private void OpenWeaponListMenu(IPlayer player, string title, IReadOnlyCollection<WeaponEntry> weapons)
    {
        if (!CanUseMenu(player, requireAlive: false))
            return;

        IMenuAPI menu = _menuhelper.CreateMenu(title);
        foreach (var weapon in weapons)
        {
            var button = new ButtonMenuOption(weapon.DisplayName)
            {
                TextStyle = MenuOptionTextStyle.ScrollLeftLoop,
                CloseAfterClick = true,
                Tag = "extend"
            };

            button.Click += async (_, args) =>
            {
                var clicker = args.Player;
                _core.Scheduler.NextTick(() => GiveWeapon(clicker, weapon));
            };

            menu.AddOption(button);
        }

        _core.MenusAPI.OpenMenuForPlayer(player, menu);
    }

    private bool CanUseMenu(IPlayer? player, bool requireAlive)
    {
        if (player == null || !player.IsValid)
            return false;

        _globals.IsZombie.TryGetValue(player.PlayerID, out var isZombie);
        if (isZombie)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanWeaponMenuZombieBlocked"));
            return false;
        }

        if (!requireAlive)
            return true;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid || controller.LifeState != (byte)LifeState_t.LIFE_ALIVE)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanWeaponMenuDeadBlocked"));
            return false;
        }

        return true;
    }

    private void GiveWeapon(IPlayer? player, WeaponEntry weapon)
    {
        if (!CanUseMenu(player, requireAlive: true) || player == null)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanWeaponMenuGiveFailed"));
            return;
        }

        var weaponServices = pawn.WeaponServices;
        var itemServices = pawn.ItemServices;
        if (weaponServices == null || !weaponServices.IsValid || itemServices == null || !itemServices.IsValid)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanWeaponMenuGiveFailed"));
            return;
        }

        weaponServices.DropWeaponBySlot(weapon.Slot);
        var givenWeapon = itemServices.GiveItem<CCSWeaponBase>(weapon.ClassName);
        if (givenWeapon == null || !givenWeapon.IsValid)
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanWeaponMenuGiveFailed"));
            return;
        }

        player.SendMessage(MessageType.Chat, _helpers.T(player, "HumanWeaponMenuGiven", weapon.DisplayName));
    }
}