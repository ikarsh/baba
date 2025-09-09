using System.Collections.Generic;
using baba;


public enum Sprite { Baba, Flag, Wall, Tile, Rock }
public enum Syntax { Is }
public enum Property { You, Win, Stop, Push }

public abstract record Type;
// abstract record CodeRecord;
record SpriteT(Sprite sprite) : Type;
abstract record CodeT : Type;
record SpriteCodeT(Sprite sprite) : CodeT;
record SyntaxT(Syntax syntax) : CodeT;
record PropertyT(Property property) : CodeT;


public static class TypeExtensions
{
    static Dictionary<Type, (string, string)> typeCodes = new() {
        { new SpriteT(Sprite.Baba), ("baba", "b") },
        {new SpriteCodeT(Sprite.Baba), ("BABA", "B") },
        { new SpriteT(Sprite.Wall), ("wall", "w") },
        {new SpriteCodeT(Sprite.Wall), ("WALL", "W") },
        { new SpriteT(Sprite.Flag), ("flag", "f") },
        {new SpriteCodeT(Sprite.Flag), ("FLAG", "F") },
        { new SpriteT(Sprite.Rock), ("rock", "r") },
        {new SpriteCodeT(Sprite.Rock), ("ROCK", "R") },
        { new SpriteT(Sprite.Tile), ("tile", "t") },
        {new SpriteCodeT(Sprite.Tile), ("TILE", "T") },
        { new SyntaxT(Syntax.Is), ("is", "=") },
        { new PropertyT(Property.You), ("you", "Y") },
        { new PropertyT(Property.Win), ("win", "!") },
        { new PropertyT(Property.Stop), ("stop", "#") },
        { new PropertyT(Property.Push), ("push", ">") },
    };

    public static string ToString(this Type type)
    {
        return typeCodes[type].Item1;
    }

    public static Type FromCode(string code)
    {
        foreach (var kv in typeCodes)
        {
            if (kv.Value.Item2 == code) return kv.Key;
        }
        throw new System.Data.DataException($"Type with code '{code}' not found");
    }
}