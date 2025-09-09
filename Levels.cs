
using System;
using System.Collections.Generic;
using System.Data;
using baba;
using Microsoft.Xna.Framework;

public static class Levels
{

    public static List<baba.Object> FromArray(ObjectType[,] arr)
    {
        List<baba.Object> objects = new();
        for (int y = 0; y < arr.GetLength(0); y++)
        {
            for (int x = 0; x < arr.GetLength(1); x++)
            {
                if (arr[y, x] != null)
                {
                    objects.Add(new baba.Object(arr[y, x], new Point(x, y)));
                }
            }
        }
        return objects;
    }
    public static List<baba.Object> Level(int n)
    {
        return n switch
        {
            1 => new List<baba.Object>
            {
                new baba.Object(new SpriteObject(Sprite.Baba), new Point(2, 2)),
                new baba.Object(new SpriteCode(Sprite.Baba), new Point(4,4)),
                new baba.Object(new Is(), new Point(4, 5)),
                new baba.Object(new PropertyCode(Property.You), new Point(4, 6)),

                new baba.Object(new SpriteObject(Sprite.Wall), new Point(1, 1)),
                new baba.Object(new SpriteCode(Sprite.Wall), new Point(3, 5)),
                new baba.Object(new PropertyCode(Property.Stop), new Point(5, 5)),

                new baba.Object(new SpriteObject(Sprite.Flag), new Point(5, 1)),
                new baba.Object(new SpriteCode(Sprite.Flag), new Point(1, 5)),
                new baba.Object(new Is(), new Point(1, 6)),
                new baba.Object(new PropertyCode(Property.Win), new Point(1, 7)),

                new baba.Object(new SpriteObject(Sprite.Rock), new Point(8, 8)),
                new baba.Object(new SpriteCode(Sprite.Rock), new Point(8, 6)),
                new baba.Object(new PropertyCode(Property.Push), new Point(9, 5)),

            },
            _ => throw new DataException("Level not found"),
        };
    }
}