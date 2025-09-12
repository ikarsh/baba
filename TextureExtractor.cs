
using System;
using System.Collections.Generic;
using System.Diagnostics;
using baba;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public enum SpriteSheet {Characters, DirectedItems, Items, Texts, Tiles };
public static class SpriteSheetExtensions
{
    const int Padding = 1;
    const int DefaultSize = 24;
    static (SpriteSheet, int, int) SpriteSheetPosition(this Sprite sprite)
    {
        // This is the top-left pixel of the *code* object, right before the sprite object possibilities.
        return sprite switch
        {
            Sprite.Baba => (SpriteSheet.Characters, 1, 1),
            Sprite.Wall => (SpriteSheet.Tiles, 451, 2273),
            Sprite.Tile => (SpriteSheet.Items, 201, 751),
            Sprite.Grass => (SpriteSheet.Tiles, 451, 676),
            Sprite.Water => (SpriteSheet.Tiles, 451, 1576),
            Sprite.Lava => (SpriteSheet.Tiles, 451, 901),
            Sprite.Flag => (SpriteSheet.Items, 76, 226),
            Sprite.Rock => (SpriteSheet.Items, 201, 601),
            Sprite.Skull => (SpriteSheet.DirectedItems, 151, 922),
            _ => throw new NotImplementedException($"No sheet position for '{sprite}'")
        };
    }
    static (SpriteSheet, int, int) SyntaxSheetPosition(this Syntax syntax)
    {
        return syntax switch
        {
            Syntax.Is => (SpriteSheet.Texts, 251, 76),
            _ => throw new NotImplementedException($"No sheet position for syntax '{syntax}'")
        };
    }

    static (SpriteSheet, int, int) PropertySheetPosition(this Property property)
    {
        return property switch
        {
            Property.You => (SpriteSheet.Texts, 276, 226),
            Property.Win => (SpriteSheet.Texts, 276, 1123),
            Property.Stop => (SpriteSheet.Texts, 201, 301),
            Property.Push => (SpriteSheet.Texts, 51, 301),
            Property.Sink => (SpriteSheet.Texts, 51, 805),
            Property.Defeat => (SpriteSheet.Texts, 51, 730),
            Property.Melt => (SpriteSheet.Texts, 201, 730),
            Property.Hot => (SpriteSheet.Texts, 126, 730),
            _ => throw new NotImplementedException($"No sheet position for property '{property}'")
        };
    }

    public static Dictionary<Object, Gif> LoadGifs(Game1 g)
    {
        Dictionary<Object, Gif> dict = new();

        Dictionary<SpriteSheet, Texture2D> sheetTexture = new()
        {
            { SpriteSheet.Characters, g.Content.Load<Texture2D>("characters") },
            { SpriteSheet.DirectedItems, g.Content.Load<Texture2D>("directed_items") },
            { SpriteSheet.Items, g.Content.Load<Texture2D>("items") },
            { SpriteSheet.Texts, g.Content.Load<Texture2D>("texts") },
            { SpriteSheet.Tiles, g.Content.Load<Texture2D>("tiles") }
        };

        foreach (Sprite sprite in Enum.GetValues(typeof(Sprite)))
        {
            var (sheet, x, y) = sprite.SpriteSheetPosition();
            List<Gif> gifs;
            switch (sheet)
            {
                case SpriteSheet.Characters:
                    gifs = ExtractLine(g.GraphicsDevice, sheetTexture[sheet], x, y, 17);
                    dict[sprite.Code()] = gifs[0];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            dict[sprite.Object((Direction)i, (SpriteState)j)] = gifs[1 + i * 4 + j];
                        }
                    }
                    break;
                case SpriteSheet.DirectedItems:
                    gifs = ExtractLine(g.GraphicsDevice, sheetTexture[sheet], x, y, 5);
                    dict[sprite.Code()] = gifs[0];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            dict[sprite.Object((Direction)i, (SpriteState)j)] = gifs[1 + i];
                        }
                    }
                    break;
                case SpriteSheet.Items:
                    gifs = ExtractLine(g.GraphicsDevice, sheetTexture[sheet], x, y, 2);
                    dict[sprite.Code()] = gifs[0];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            dict[sprite.Object((Direction)i, (SpriteState)j)] = gifs[1];
                        }
                    }
                    break;
                case SpriteSheet.Tiles:
                    gifs = ExtractLine(g.GraphicsDevice, sheetTexture[sheet], x, y, 2);
                    dict[sprite.Code()] = gifs[0];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            dict[sprite.Object((Direction)i, (SpriteState)j)] = gifs[1];
                        }
                    }
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        foreach (Syntax syntax in Enum.GetValues(typeof(Syntax)))
        {
            var (sheet, x, y) = syntax.SyntaxSheetPosition();
            var gifs = ExtractLine(g.GraphicsDevice, sheetTexture[sheet], x, y, 1);
            dict[syntax.Code()] = gifs[0];
        }

        foreach (Property property in Enum.GetValues(typeof(Property)))
        {
            var (sheet, x, y) = property.PropertySheetPosition();
            var gifs = ExtractLine(g.GraphicsDevice, sheetTexture[sheet], x, y, 1);
            dict[property.Code()] = gifs[0];
        }

        return dict;
    }

    public static List<Gif> ExtractLine(
        GraphicsDevice graphicsDevice,
        Texture2D spriteSheet,
        int startX,
        int startY,
        int count,
        int width = DefaultSize,
        int height = DefaultSize
    ) {
        var gifs = new List<Gif>();
        for (int i = 0; i < count; i++)
        {
            int x = startX + i * (width + Padding);
            var gif = ExtractGif(graphicsDevice, spriteSheet, x, startY, width, height);
            gifs.Add(gif);
        }
        return gifs;
    }
    static Gif ExtractGif(
        GraphicsDevice graphicsDevice,
        Texture2D spriteSheet,
        int startX,
        int startY,
        int width = DefaultSize,
        int height = DefaultSize
    ) {
        var sprite1 = ExtractSprite(graphicsDevice, spriteSheet, startX, startY,
                                    width, height);
        var sprite2 = ExtractSprite(graphicsDevice, spriteSheet, startX, startY + Padding + height,
                                    width, height);
        var sprite3 = ExtractSprite(graphicsDevice, spriteSheet, startX, startY + 2 * (Padding + height),
                                    width, height);

        return new Gif(new List<Texture2D> { sprite1, sprite2, sprite3 });
    }

    static Texture2D ExtractSprite(
        GraphicsDevice graphicsDevice,
        Texture2D spriteSheet,
        int x,
        int y,
        int width = DefaultSize,
        int height = DefaultSize
    ) {
        // Get source data
        Color[] sourceData = new Color[spriteSheet.Width * spriteSheet.Height];
        spriteSheet.GetData(sourceData);

        // Extract region
        Color[] extractedData = new Color[width * height];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int sourceIndex = (y + row) * spriteSheet.Width + (x + col);
                int destIndex = row * width + col;
                extractedData[destIndex] = sourceData[sourceIndex];
            }
        }

        // Create new texture
        Texture2D result = new Texture2D(graphicsDevice, width, height);
        result.SetData(extractedData);

        return result;
    }
}

public class Gif {
    const int FrameDelayMs = 300;
    List<Texture2D> frames;
    int _currentFrameIndex = 0;
    int _lastUpdateTimeMs = 0;

    public Gif(List<Texture2D> frames)
    {
        this.frames = frames;
        _currentFrameIndex = 0;
        _lastUpdateTimeMs = 0;
    }

    public void Update(GameTime gameTime)
    {
        double time = gameTime.TotalGameTime.TotalMilliseconds;
        if ( time >= FrameDelayMs + _lastUpdateTimeMs )
        {
            _lastUpdateTimeMs = (int)time;
            AdvanceFrame();
        }
    }

    void AdvanceFrame()
    {
        _currentFrameIndex = (_currentFrameIndex + 1) % frames.Count;
    }

    public Texture2D GetCurrentFrame()
    {
        return frames[_currentFrameIndex];
    }
}