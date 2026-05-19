using GDS.Core; // Fontos, hogy beemeld az asset saját névterét!

public static class MyItemDatabase
{
    // A 'Stack.Fifty' egy példa, nézd meg az assetben, milyen opciók vannak.
    public static readonly GDS.Core.ItemBase WoodLog = new() 
    { 
        Id = "WoodLog", 
        Name = "Farönk", 
        Icon = "Items/Tools/wood_logs_three",
         // Az ikon elérési útja a Resources mappán belül
        Stack = new Stack(50) // Példa: pontosan 37-es stack limit
         // Vagy Stack.Infinite, Stack.None stb.
    };

    public static readonly GDS.Core.ItemBase Campfire = new() 
    { 
        Id = "Campfire", 
        Name = "Tábortűz", 
        Icon = "Items/Tools/Firewood",
         // Az ikon elérési útja a Resources mappán belül
        Stack = new Stack(5) // Példa: pontosan 37-es stack limit
         // Vagy Stack.Infinite, Stack.None stb.
    };

    public static readonly GDS.Core.ItemBase Stone = new() 
    { 
        Id = "Stone", 
        Name = "Kovakő", 
        Icon = "Items/Tools/stone_basic_grey",
        Stack = new Stack(10)
    };

     public static readonly GDS.Core.ItemBase Axe = new() 
    { 
        Id = "Axe", 
        Name = "Fejsze", 
        Icon = "Items/Tools/61669",
        Stack = new Stack(1)
    };
     public static readonly GDS.Core.ItemBase Sword = new() 
    { 
        Id = "Sword", 
        Name = "Kard", 
        Icon = "Items/Tools/86389", // Állítsd be a saját ikonod útvonalát!
        Stack = new Stack(1) // Fegyverből általában 1 db van egy sloton
    };
 public static readonly GDS.Core.ItemBase RawMeat = new() 
    { 
        Id = "RawMeat", 
        Name = "Nyers Hús", 
        Icon = "Items/Tools/99096", // Majd cseréld le a saját ikonod útvonalára!
        Stack = new Stack(20) 
    };

    public static readonly GDS.Core.ItemBase CookedMeat = new() 
    { 
        Id = "CookedMeat", 
        Name = "Sült Hús", 
        Icon = "Items/Tools/38914", // Majd cseréld le a saját ikonod útvonalára!
        Stack = new Stack(20) 
    };

    public static readonly GDS.Core.ItemBase Bone = new() 
    { 
        Id = "Bone", 
        Name = "Csont", 
        Icon = "Items/Tools/bone_white",
        Stack = new Stack(50) 
    };

    public static readonly GDS.Core.ItemBase WolfSkin = new() 
    { 
        Id = "WolfSkin", 
        Name = "Farkasbőr", 
        Icon = "Items/Tools/shoulders",
        Stack = new Stack(10) 
    };

     public static readonly GDS.Core.ItemBase Antidote = new() 
    { 
        Id = "Antidote", 
        Name = "Ellenméreg", 
        Icon = "Items/Tools/85934", // Ikon útvonal
        Stack = new Stack(5) 
    };
     public static readonly GDS.Core.ItemBase HealingPotion = new() 
    { 
        Id = "HealingPotion", 
        Name = "Gyógyító Ital", 
        Icon = "Items/Tools/44778", // Ikon útvonal
        Stack = new Stack(5) 
    };
     public static readonly GDS.Core.ItemBase Hammer = new() 
    { 
        Id = "Hammer", 
        Name = "Kalapács", 
        Icon = "Items/Tools/hammer_9", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase ReinforcedSword = new() 
    { 
        Id = "ReinforcedSword", 
        Name = "Erősített Kard", 
        Icon = "Items/Tools/sword_basic4_blue", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase ReinforcedAxe = new() 
    { 
        Id = "ReinforcedAxe", 
        Name = "Erősített Fejsze", 
        Icon = "Items/Tools/ReinforcedAxe", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase Lamp = new() 
    { 
        Id = "Lamp", 
        Name = "Lámpa", 
        Icon = "Items/Tools/41490", // Ikon útvonal
        Stack = new Stack(10) 
    };
     public static readonly GDS.Core.ItemBase WoodBranch = new() 
    { 
        Id = "WoodBranch", 
        Name = "Faág", 
        Icon = "Items/Tools/wood_stick", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase WoodPlank = new() 
    { 
        Id = "WoodPlank", 
        Name = "Fa deszka", 
        Icon = "Items/Tools/wood_plank", // Ikon útvonal
        Stack = new Stack(10) 
    };
     public static readonly GDS.Core.ItemBase GiantLog = new() 
    { 
        Id = "GiantLog", 
        Name = "Óriásfa törzse", 
        Icon = "Items/Tools/wood_log", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase Flint = new() 
    { 
        Id = "Flint", 
        Name = "Kovakő", 
        Icon = "Items/Tools/86021", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase SharpStone = new() 
    { 
        Id = "SharpStone", 
        Name = "Éles Kő", 
        Icon = "Items/Tools/Whetstone", // Ikon útvonal
        Stack = new Stack(100) 
    }; public static readonly GDS.Core.ItemBase PlantFiber = new() 
    { 
        Id = "PlantFiber", 
        Name = "Növényi Szál", 
        Icon = "Items/Tools/twig_green", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase SilkGrass = new() 
    { 
        Id = "SilkGrass", 
        Name = "Selyemfű", 
        Icon = "Items/Tools/leafs_long", // Ikon útvonal
        Stack = new Stack(10) 
    };
     public static readonly GDS.Core.ItemBase StrongCanvas = new() 
    { 
        Id = "StrongCanvas", 
        Name = "Erős Vászon", 
        Icon = "Items/Tools/Bedroll", // Ikon útvonal
        Stack = new Stack(5) 
    };
     public static readonly GDS.Core.ItemBase Resin = new() 
    { 
        Id = "Resin", 
        Name = "Gyanta", 
        Icon = "Items/Tools/bottle_standard_green", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase PlantTar = new() 
    { 
        Id = "PlantTar", 
        Name = "Növényi Kátrány", 
        Icon = "Items/Tools/50113", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase MetalScrap = new() 
    { 
        Id = "MetalScrap", 
        Name = "Fémdarab", 
        Icon = "Items/Tools/metal_scrap", // Ikon útvonal
        Stack = new Stack(10) 
    };
     public static readonly GDS.Core.ItemBase LeatherStrap = new() 
    { 
        Id = "LeatherStrap", 
        Name = "Bőrszíj", 
        Icon = "Items/Tools/belts", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase EmptyFlask = new() 
    { 
        Id = "EmptyFlask", 
        Name = "Üres Flakon", 
        Icon = "Items/Tools/bottle_standard_empty", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase WaterFlask = new() 
    { 
        Id = "WaterFlask", 
        Name = "Vizesflakon", 
        Icon = "Items/Tools/bottle_standard_blue", // Ikon útvonal
        Stack = new Stack(20) 
    };
     public static readonly GDS.Core.ItemBase DarkWaterFlask = new() 
    { 
        Id = "DarkWaterFlask", 
        Name = "Mérgesflakon", 
        Icon = "Items/Tools/39595", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase Bread = new() 
    { 
        Id = "Bread", 
        Name = "Kenyér", 
        Icon = "Items/Tools/76361", // Ikon útvonal
        Stack = new Stack(10) 
    };
     public static readonly GDS.Core.ItemBase EndurancePotion = new() 
    { 
        Id = "EndurancePotion", 
        Name = "Kitartás Főzete", 
        Icon = "Items/Tools/40824", // Ikon útvonal
        Stack = new Stack(5) 
    };
     public static readonly GDS.Core.ItemBase EliasCompass = new() 
    { 
        Id = "EliasCompass", 
        Name = "Elias Iránytűje", 
        Icon = "Items/Tools/65822", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase EleanorsLocket = new() 
    { 
        Id = "EleanorsLocket", 
        Name = "Eleonóra Medálja", 
        Icon = "Items/Tools/amulet", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase BrokenDagger = new() 
    { 
        Id = "BrokenDagger", 
        Name = "Törött Tőr", 
        Icon = "Items/Tools/sword", // Ikon útvonal
        Stack = new Stack(1) 
    };
     public static readonly GDS.Core.ItemBase EchoShard = new() 
    { 
        Id = "EchoShard", 
        Name = "Visszhang Szilánk", 
        Icon = "Items/Tools/gem", // Ikon útvonal
        Stack = new Stack(5) 
    };
     public static readonly GDS.Core.ItemBase ThornsTalisman = new() 
    { 
        Id = "ThornsTalisman", 
        Name = "Thorn Kapitány Talizmánja", 
        Icon = "Items/Tools/necklace_silver_red", // Ikon útvonal
        Stack = new Stack(1) 
    };
        public static readonly GDS.Core.ItemBase quest_2 = new() 
        { 
            Id = "quest_2", 
            Name = "Quest Tárgy 2", 
            Icon = "Items/Tools/unknown", // Ikon útvonal
            Stack = new Stack(5) 
        };

    public static readonly GDS.Core.ItemBase JournalItem = new() 
    { 
        Id = "JournalItem", 
        Name = "Napló", 
        Icon = "Items/Tools/scroll", 
        Stack = new Stack(1) 
    };
    // ... Itt add hozzá a többi tárgyadat is (gyógyszer, kötszer stb.)
}
