using System.Collections.Generic;
using System.Diagnostics;
using baba;

public enum Sprite { Baba, Wall, Tile, Grass, Water, Lava, Flag, Rock, Skull };
public enum Syntax { Is }
public enum Property { You, Win, Stop, Push, Sink, Defeat, Melt, Hot }



public static class SpriteExtensions
{
    public static SpriteObject Object(this Sprite sprite, Direction direction = Direction.Right, SpriteState state = SpriteState.zero)
    {
        return new SpriteObject(sprite, direction, state);
    }

    public static SpriteCode Code(this Sprite sprite)
    {
        return new SpriteCode(sprite);
    }
}


public abstract record Code : Object;
public record SpriteCode(Sprite sprite) : Code;
public record SyntaxCode(Syntax syntax) : Code;
public static class SyntaxExtensions
{
    public static Code Code(this Syntax syntax)
    {
        return new SyntaxCode(syntax);
    }
}
public record PropertyCode(Property property) : Code;

public static class PropertyExtensions
{
    public static Code Code(this Property property)
    {
        return new PropertyCode(property);
    }
}

public abstract record Object;
public record SpriteObject(Sprite sprite, Direction direction, SpriteState state) : Object;
public enum SpriteState { zero, one, two, three };
public static class SpriteStateExtensions
{
    public static SpriteState Next(this SpriteState state)
    {
        return state switch
        {
            SpriteState.zero => SpriteState.one,
            SpriteState.one => SpriteState.two,
            SpriteState.two => SpriteState.three,
            SpriteState.three => SpriteState.zero,
            _ => throw new UnreachableException()
        };
    }
}


public static class ObjectExtensions
{
    static Dictionary<Object, (string, string)> typeCodes = new() {
        {Sprite.Baba.Object(), ("baba", "b") },
        {Sprite.Baba.Code(), ("BABA", "B") },
        { Sprite.Wall.Object(), ("wall", "w") },
        {Sprite.Wall.Code(), ("WALL", "W") },
        { Sprite.Flag.Object(), ("flag", "f") },
        {Sprite.Flag.Code(), ("FLAG", "F") },
        { Sprite.Rock.Object(), ("rock", "r") },
        {Sprite.Rock.Code(), ("ROCK", "R") },
        { Sprite.Tile.Object(), ("tile", "t") },
        {Sprite.Tile.Code(), ("TILE", "T") },
        { Sprite.Grass.Object(), ("grass", "g") },
        {Sprite.Grass.Code(), ("GRASS", "G") },
        { Sprite.Water.Object(), ("water", "-") },
        {Sprite.Water.Code(), ("WATER", "_") },
        { Sprite.Skull.Object(), ("skull", "s") },
        {Sprite.Skull.Code(), ("skull", "S") },
        { Sprite.Lava.Object(), ("lava", "l") },
        {Sprite.Lava.Code(), ("LAVA", "L") },
        { Syntax.Is.Code(), ("is", "=") },
        { Property.You.Code(), ("you", "Y") },
        { Property.Win.Code(), ("win", "!") },
        { Property.Stop.Code(), ("stop", "#") },
        { Property.Push.Code(), ("push", ">") },
        { Property.Sink.Code(), ("sink", "~") },
        { Property.Defeat.Code(), ("defeat", "*") },
        { Property.Hot.Code(), ("hot", "[") },
        { Property.Melt.Code(), ("melt", "]") },
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

    public static Object AfterMove(this Object o, Direction d)
    {
        return o switch
        {
            SpriteObject spriteObj => new SpriteObject(spriteObj.sprite, d, spriteObj.state.Next()),
            _ => o
        };
    }
}