
using System;
using System.Collections.Generic;
using System.Data;
using baba;
using Microsoft.Xna.Framework;

public static class Levels
{

    public static List<baba.Object> FromString(string levelStr)
    {
        var objects = new List<baba.Object>();
        var rows = levelStr.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (int y = 0; y < rows.Length; y++)
        {
            var cols = rows[y].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < cols.Length; x++)
            {
                if (cols[x] == ".") continue;
                var type = TypeExtensions.FromCode(cols[x]);
                objects.Add(new baba.Object(type, new Point(x, y)));
            }
        }
        return objects;
    }
    public static List<baba.Object> Level(int n)
    {
        return n switch
        {
            -1 => new List<baba.Object>
            {
                new baba.Object(new SpriteT(Sprite.Baba), new Point(2, 2)),
                new baba.Object(new SpriteCodeT(Sprite.Baba), new Point(4,4)),
                new baba.Object(new SyntaxT(Syntax.Is), new Point(4, 5)),
                new baba.Object(new PropertyT(Property.You), new Point(4, 6)),

                new baba.Object(new SpriteT(Sprite.Wall), new Point(1, 1)),
                new baba.Object(new SpriteCodeT(Sprite.Wall), new Point(3, 5)),
                new baba.Object(new PropertyT(Property.Stop), new Point(5, 5)),

                new baba.Object(new SpriteT(Sprite.Flag), new Point(5, 1)),
                new baba.Object(new SpriteCodeT(Sprite.Flag), new Point(1, 5)),
                new baba.Object(new SyntaxT(Syntax.Is), new Point(1, 6)),
                new baba.Object(new PropertyT(Property.Win), new Point(1, 7)),

                new baba.Object(new SpriteT(Sprite.Rock), new Point(8, 8)),
                new baba.Object(new SpriteCodeT(Sprite.Rock), new Point(8, 6)),
                new baba.Object(new PropertyT(Property.Push), new Point(9, 5)),

            },

            1 => FromString(@"
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . B = Y . . . . . F = ! . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . w w w w w w w w w w w . . . . . . . . . . . 
                . . . . . . . . . . . t t t t t r t t t t t . . . . . . . . . . . 
                . . . . . . . . . . . t b t t t r t t t f t . . . . . . . . . . . 
                . . . . . . . . . . . t t t t t r t t t t t . . . . . . . . . . . 
                . . . . . . . . . . . w w w w w w w w w w w . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . W = # . . . . . R = > . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
            "),
            _ => throw new DataException("Level not found"),
        };
    }
}