
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Levels;

namespace baba;

static class Config
{
    public const int SQUARE_SIZE = 70;
    public const int SCR_WID = 12;
    public const int SCR_HEI = 12;

    public const double MOVE_DELAY_MS = 30;
}

static class Utils
{
    public static bool InBoard(Point point)
    {
        return point.X >= 0 && point.X <= Config.SCR_WID - 1 && point.Y >= 0 && point.Y <= Config.SCR_HEI - 1;
    }

    public static void Draw(SpriteBatch g, Point position, Texture2D texture)
    {
        g.Draw(texture, new Rectangle(position.X * Config.SQUARE_SIZE, position.Y * Config.SQUARE_SIZE, Config.SQUARE_SIZE, Config.SQUARE_SIZE), Color.White);
    }
}

public enum Direction { Up, Down, Right, Left }

static class DirectionExtensions
{
    public static Point OffsetPoint(this Direction d, Point p)
    {
        return d switch
        {
            Direction.Up => new Point(p.X, p.Y - 1),
            Direction.Down => new Point(p.X, p.Y + 1),
            Direction.Right => new Point(p.X + 1, p.Y),
            Direction.Left => new Point(p.X - 1, p.Y),
            _ => throw new UnreachableException()
        };
    }

    public static Direction? FromKeyboard()
    {
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.Down)) return Direction.Down;
        else if (state.IsKeyDown(Keys.Up)) return Direction.Up;
        else if (state.IsKeyDown(Keys.Right)) return Direction.Right;
        else if (state.IsKeyDown(Keys.Left)) return Direction.Left;
        return null;
    }
}

enum Sprite { Baba, Flag, Wall, Tile, Rock }
public enum Property { You, Win, Stop, Push }

public abstract record ObjectType;
// abstract record CodeRecord;
record SpriteObject(Sprite sprite) : ObjectType;
abstract record CodeObject : ObjectType;
record SpriteCode(Sprite sprite) : CodeObject;
record Is() : CodeObject;
record PropertyCode(Property property) : CodeObject;

public class Object
{
    public ObjectType type;
    public Point position;
    public Object(ObjectType type, Point position)
    {
        this.type = type;
        this.position = position;
    }
    public void Move(Game1 g, Direction d)
    {
        // Attempts to move in a direction, pushing everything in the way.
        List<Object> movedObjects = new List<Object> { this };
        Point current = d.OffsetPoint(position);
        while (true)
        {
            if (!Utils.InBoard(current)) return;
            if (g.ObjectsOn(current).Any(o => g.Is(o.type, Property.Stop) && !g.Is(o.type, Property.Push))) return;

            List<Object> pushes = g.ObjectsOn(current).Where(o => g.Is(o.type, Property.Push)).ToList();
            if (pushes.Count() == 0)
            {
                foreach (Object o in movedObjects) o.position = d.OffsetPoint(o.position);
                return;
            }
            else
            {
                movedObjects = movedObjects.Concat(pushes).ToList();
                current = d.OffsetPoint(current);
            }
        }
    }
}

public class Game1 : Game
{
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;
    List<Object> objects;
    Dictionary<ObjectType, Texture2D> textures;

    double lastMoveTime = 0;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = Config.SQUARE_SIZE * Config.SCR_WID;
        _graphics.PreferredBackBufferHeight = Config.SQUARE_SIZE * Config.SCR_HEI;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        objects = Level(1);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        textures = new Dictionary<ObjectType, Texture2D>
        {
            [new SpriteObject(Sprite.Baba)] = Content.Load<Texture2D>("sprites/baba"),
            [new SpriteObject(Sprite.Wall)] = Content.Load<Texture2D>("sprites/wall"),
            [new SpriteObject(Sprite.Flag)] = Content.Load<Texture2D>("sprites/flag"),
            [new SpriteObject(Sprite.Tile)] = Content.Load<Texture2D>("sprites/tile"),
            [new SpriteObject(Sprite.Rock)] = Content.Load<Texture2D>("sprites/rock"),
            [new SpriteCode(Sprite.Baba)] = Content.Load<Texture2D>("codes/baba"),
            [new SpriteCode(Sprite.Wall)] = Content.Load<Texture2D>("codes/wall"),
            [new SpriteCode(Sprite.Flag)] = Content.Load<Texture2D>("codes/flag"),
            [new SpriteCode(Sprite.Rock)] = Content.Load<Texture2D>("codes/rock"),
            [new SpriteCode(Sprite.Tile)] = Content.Load<Texture2D>("codes/tile"),
            [new Is()] = Content.Load<Texture2D>("codes/is"),
            [new PropertyCode(Property.You)] = Content.Load<Texture2D>("codes/you"),
            [new PropertyCode(Property.Win)] = Content.Load<Texture2D>("codes/win"),
            [new PropertyCode(Property.Stop)] = Content.Load<Texture2D>("codes/stop"),
            [new PropertyCode(Property.Push)] = Content.Load<Texture2D>("codes/push"),
        };
    }

    public void DrawObject(Object o)
    {
        Texture2D texture = textures[o.type];
        Utils.Draw(_spriteBatch, o.position, texture);
        return;
    }

    public List<Object> ObjectsOn(Point position)
    {
        return objects.Where(o => o.position == position).ToList();
    }

    bool CodeSequenceAppears(List<CodeObject> codes)
    {
        if (codes.Count() == 0) return true;
        foreach (Point p in objects.Where(o => o.type == codes[0]).Select(o => o.position))
        {
            foreach (Direction d in new[] { Direction.Down, Direction.Right })
            {
                bool broke = false;
                Point curr = p;

                foreach (CodeObject code in codes)
                {
                    if (!ObjectsOn(curr).Any(o => o.type == code))
                    {
                        broke = true;
                        break;
                    }
                    
                    curr = d.OffsetPoint(curr);
                }
                if (!broke) return true;
                if (!Utils.InBoard(curr)) return false;
            }
        }
        return false;
    }

    public bool Is(ObjectType type, Property prop)
    {
        return type switch
        {
            SpriteObject(Sprite sprite) => CodeSequenceAppears(new List<CodeObject> {
            new SpriteCode(sprite), new Is(), new PropertyCode(prop)
        }),
            CodeObject => IsCode(prop),
            _ => throw new UnreachableException()

        };
    }

    bool IsCode(Property prop)
    {
        return prop == Property.Push || prop == Property.Stop;
    }

    void Move(Direction d)
    {
        foreach (Object o in objects)
        {
            if (Is(o.type, Property.You))
            {
                o.Move(this, d);
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (DirectionExtensions.FromKeyboard() is Direction d)
        {
            double time = gameTime.TotalGameTime.TotalMilliseconds;
            if (time - lastMoveTime > Config.MOVE_DELAY_MS) Move(d);
            lastMoveTime = time;
        }

        foreach (Object o in objects)
        {
            if (Is(o.type, Property.You))
            {
                if (objects.Any(ob => Is(ob.type, Property.Win) && ob.position == o.position))
                {
                    // TODO add win things
                    Exit();
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        foreach (Object o in objects)
        {
            DrawObject(o);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}