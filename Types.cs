using System.Collections.Generic;
using System.Diagnostics;
using baba;

public enum Sprite { Baba, Wall, Tile, Grass, Water, Lava, Flag, Rock, Skull, Brick, Flower };
public enum Syntax { Is }
public enum Property { You, Win, Stop, Push, Sink, Defeat, Melt, Hot }

public abstract record Type;
public record SpriteType(Sprite sprite) : Type;
public abstract record Code : Type;
public record SpriteCode(Sprite sprite) : Code;
public record SyntaxCode(Syntax syntax) : Code;
public record PropertyCode(Property property) : Code;


public struct Object
{
    public Type type;
    public Direction direction;
    public ObjectState state;

    public Object Move(Direction direction)
    {
        return new Object { type = type, direction = direction, state = state.Next() };
    }

    public Object ChangeType(Type newType)
    {
        return new Object { type = newType, direction = direction, state = state };
    }
}


public enum ObjectState { zero, one, two, three };
public static class ObjectStateExtensions
{
    public static ObjectState Next(this ObjectState state)
    {
        return state switch
        {
            ObjectState.zero => ObjectState.one,
            ObjectState.one => ObjectState.two,
            ObjectState.two => ObjectState.three,
            ObjectState.three => ObjectState.zero,
            _ => throw new UnreachableException()
        };
    }
}


public static class TypeExtensions
{
    static Dictionary<Type, (string, string)> typeCodes = new() {
        {Sprite.Baba.Type(), ("baba", "b") },
        {Sprite.Baba.Code(), ("BABA", "B") },
        { Sprite.Wall.Type(), ("wall", "w") },
        {Sprite.Wall.Code(), ("WALL", "W") },
        { Sprite.Flag.Type(), ("flag", "f") },
        {Sprite.Flag.Code(), ("FLAG", "F") },
        { Sprite.Rock.Type(), ("rock", "r") },
        {Sprite.Rock.Code(), ("ROCK", "R") },
        { Sprite.Tile.Type(), ("tile", "t") },
        {Sprite.Tile.Code(), ("TILE", "T") },
        { Sprite.Grass.Type(), ("grass", "g") },
        {Sprite.Grass.Code(), ("GRASS", "G") },
        { Sprite.Water.Type(), ("water", "-") },
        {Sprite.Water.Code(), ("WATER", "_") },
        { Sprite.Skull.Type(), ("skull", "s") },
        {Sprite.Skull.Code(), ("skull", "S") },
        { Sprite.Lava.Type(), ("lava", "l") },
        {Sprite.Lava.Code(), ("LAVA", "L") },
        { Sprite.Brick.Type(), ("brick", "k") },
        {Sprite.Brick.Code(), ("BRICK", "K") },
        { Sprite.Flower.Type(), ("flower", "x") },
        {Sprite.Flower.Code(), ("FLOWER", "X") },
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

    public static SpriteType Type(this Sprite sprite)
    {
        return new SpriteType(sprite);
    }
    public static SpriteCode Code(this Sprite sprite)
    {
        return new SpriteCode(sprite);
    }
    public static Code Code(this Syntax syntax)
    {
        return new SyntaxCode(syntax);
    }
    public static Code Code(this Property property)
    {
        return new PropertyCode(property);
    }

    public static Object Object(this Type type, Direction direction, ObjectState state)
    {
        return new Object { type = type, direction = direction, state = state };
    }
}