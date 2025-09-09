
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Xna.Framework;

public static class Levels
{

    public static Dictionary<Point, List<Object>> FromString(string levelStr)
    {
        Console.WriteLine("Parsing level...");
        var objects = new Dictionary<Point, List<Object>>();
        var rows = levelStr.Split('\n').Where(r => !string.IsNullOrWhiteSpace(r)).ToArray();
        for (int y = 0; y < rows.Length; y++)
        {
            var cols = rows[y].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < cols.Length; x++)
            {
                Console.WriteLine($"Processing cell ({x}, {y}): '{cols[x]}'");
                if (cols[x] == ".")
                {
                    objects[new Point(x, y)] = new List<Object> { };
                }
                else
                {
                    objects[new Point(x, y)] = new List<Object> { ObjectExtensions.FromCode(cols[x]) };
                }
            }
        }
        return objects;
    }
    public static Dictionary<Point, List<Object>> Level(int n)
    {
        return n switch
        {
            0 => FromString(@"
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
            1 => FromString(@"
                . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . 
                . . . . . . . . . w w w w w w w w . . . . . . . 
                . . . . . . . . . w . . . . . . w . . . . . . . 
                . . . . . . . . . w . = . . . . w . . . . . . . 
                . . . . . . . . . w . . . . . . w . . . . . . . 
                . . . . . w w w w w . . . . ! . w . . . . . . . 
                . . . . . w t t t t . . . . . . w . . . . . . . 
                . . . . . w t F t t . f . . . . w . . . . . . . 
                . . . . . w t t t t . . . . . . w . . . . . . . 
                . . . . . w w w w w w w w w w w w . . . . . . . 
                . . . . . . . . . w . . . . . . w . . . . . . . 
                . . . . . . B . . w . W . . . . w . . . . . . . 
                . . . . . . = . . w . = . . b . w . . . . . . . 
                . . . . . . Y . . w . # . . . . w . . . . . . . 
                . . . . . . . . . w . . . . . . w . . . . . . . 
                . . . . . . . . . w w w w w w w w . . . . . . . 
                . . . . . . . . . . . . . . . . . . . . . . . . 
            "),
            _ => throw new DataException("Level not found"),
        };
    }
}