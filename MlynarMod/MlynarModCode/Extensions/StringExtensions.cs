using Godot;

namespace MlynarMod.MlynarModCode.Extensions;

public static class StringExtensions
{
    public static string ImagePath(this string path) => Path.Join(MainFile.ResPath, "images", path);

    public static string CardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", "big", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", "big", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string PotionImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "potions", path);
        return ResourceLoader.Exists(path) ? path : Path.Join(MainFile.ResPath, "images", "potions", "potion.png");
    }

    public static string CharacterUiPath(this string path) => Path.Join(MainFile.ResPath, "images", "charui", path);
}
