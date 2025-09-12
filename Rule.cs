using System;
using System.Collections.Generic;
using System.Linq;
using baba;
using Microsoft.Xna.Framework;

public abstract record Rule;
public record IsPropRule(Sprite sprite, Property property) : Rule;
public record IsSpriteRule(Sprite sprite1, Sprite sprite2) : Rule;

public static class RuleExtensions
{
    public static List<Rule> FromBoard(Board board)
    {
        List<Rule> res = new List<Rule>();
        foreach (Point position in board.Positions)
        {
            foreach (Direction d in new[] { Direction.Down, Direction.Right })
            {
                Point curr = position;
                List<Sprite> sprite1s = board[curr].Select(o => o.type).OfType<SpriteCode>().Select(sc => sc.sprite).ToList();
                if (sprite1s.Count() == 0) continue;
                curr = d.OffsetPoint(curr);
                if (!(board.LegalPosition(curr) && board[curr].Any(o => o.type == Syntax.Is.Code()))) continue;
                curr = d.OffsetPoint(curr);
                if (!board.LegalPosition(curr)) continue;
                List<Sprite> sprite2s = board[curr].Select(o => o.type).OfType<SpriteCode>().Select(sc => sc.sprite).ToList();
                List<Property> properties = board[curr].Select(o => o.type).OfType<PropertyCode>().Select(pc => pc.property).ToList();

                foreach (Sprite sprite1 in sprite1s)
                {
                    foreach (Sprite sprite2 in sprite2s)
                    {
                        res.Add(new IsSpriteRule(sprite1, sprite2));
                    }
                    foreach (Property property in properties)
                    {
                        res.Add(new IsPropRule(sprite1, property));
                    }
                }
            }
        }
        return res;
    }
}