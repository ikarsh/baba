using System.Collections.Generic;
using baba;


public enum Sprite { Baba, Flag, Wall, Tile, Rock }
public enum Syntax { Is }
public enum Property { You, Win, Stop, Push }

public abstract record Object;
// abstract record CodeRecord;
record SpriteObject(Sprite sprite) : Object;
abstract record CodeT : Object;
record SpriteCode(Sprite sprite) : CodeT;
record SyntaxCode(Syntax syntax) : CodeT;
record PropertyCode(Property property) : CodeT;


public static class ObjectExtensions
{
    static Dictionary<Object, (string, string)> typeCodes = new() {
        { new SpriteObject(Sprite.Baba), ("baba", "b") },
        {new SpriteCode(Sprite.Baba), ("BABA", "B") },
        { new SpriteObject(Sprite.Wall), ("wall", "w") },
        {new SpriteCode(Sprite.Wall), ("WALL", "W") },
        { new SpriteObject(Sprite.Flag), ("flag", "f") },
        {new SpriteCode(Sprite.Flag), ("FLAG", "F") },
        { new SpriteObject(Sprite.Rock), ("rock", "r") },
        {new SpriteCode(Sprite.Rock), ("ROCK", "R") },
        { new SpriteObject(Sprite.Tile), ("tile", "t") },
        {new SpriteCode(Sprite.Tile), ("TILE", "T") },
        { new SyntaxCode(Syntax.Is), ("is", "=") },
        { new PropertyCode(Property.You), ("you", "Y") },
        { new PropertyCode(Property.Win), ("win", "!") },
        { new PropertyCode(Property.Stop), ("stop", "#") },
        { new PropertyCode(Property.Push), ("push", ">") },
    };

    public static string ToString(this Object type)
    {
        return typeCodes[type].Item1;
    }

    public static Object FromCode(string code)
    {
        foreach (var kv in typeCodes)
        {
            if (kv.Value.Item2 == code) return kv.Key;
        }
        throw new System.Data.DataException($"Type with code '{code}' not found");
    }
}