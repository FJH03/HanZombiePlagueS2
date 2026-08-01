namespace HanZombiePlagueS2;

public class HZPHumanClassCFG
{
    public List<HumanClass> HumanClassList { get; set; } = new();

    public class HumanStats
    {
        public int Health { get; set; } = 225;
        public float Speed { get; set; } = 1.0f;
        public float Gravity { get; set; } = 0.8f;
        public int Fov { get; set; } = 90;
        public bool EnableGlow { get; set; } = false;
        public int GlowR { get; set; } = 0;
        public int GlowG { get; set; } = 0;
        public int GlowB { get; set; } = 0;
        public int GlowA { get; set; } = 255;
    }

    public class HumanModels
    {
        public string ModelPath { get; set; } = string.Empty;

    }

    public class HumanWeapon
    {
        public string PrimaryWeapon { get; set; } = string.Empty;
        public string CustomWeaponName { get; set; } = string.Empty;
    }

    public class HumanClass
    {
        public string Name { get; set; } = string.Empty;
        public bool Enable { get; set; } = true;
        public HumanStats Stats { get; set; } = new();
        public HumanModels Models { get; set; } = new();
        public HumanWeapon Weapon { get; set; } = new();
    }
}
