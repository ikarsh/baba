
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public const int SQUARE_SIZE = 50;
    public const int SCR_WID = 33;
    public const int SCR_HEI = 18;

    public const double MOVE_DELAY_MS = 30;
}

static class Utils
{
    public static  IEnumerable<Point> BoardPositions()
    {
        return Enumerable.Range(0, Config.SCR_WID).SelectMany(x => Enumerable.Range(0, Config.SCR_HEI).Select(y => new Point(x, y)));
    }
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


public class Game1 : Game
{
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;
    Dictionary<Point, List<Object>> board;
    Dictionary<Object, Texture2D> textures;

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
        board = Level(0);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        textures = new Dictionary<Object, Texture2D>();

        foreach (Sprite sprite in Enum.GetValues(typeof(Sprite)))
        {
            textures[new SpriteObject(sprite)] = Content.Load<Texture2D>($"sprites/{sprite}");
            textures[new SpriteCode(sprite)] = Content.Load<Texture2D>($"codes/sprites/{sprite}");
        }

        foreach (Syntax syntax in Enum.GetValues(typeof(Syntax)))
        {
            textures[new SyntaxCode(syntax)] = Content.Load<Texture2D>($"codes/syntax/{syntax}");
        }

        foreach (Property property in Enum.GetValues(typeof(Property)))
        {
            textures[new PropertyCode(property)] = Content.Load<Texture2D>($"codes/properties/{property}");
        }
    }

    public void DrawObject(Object o, Point position)
    {
        Texture2D texture = textures[o];
        Utils.Draw(_spriteBatch, position, texture);
        return;
    }

    public List<Object> ObjectsOn(Point position)
    {
        return board[position];
    }

    bool CodeSequenceAppears(List<CodeT> codes)
    {
        if (codes.Count() == 0) return true;
        foreach (Point p in Utils.BoardPositions())
        {
            foreach (Direction d in new[] { Direction.Down, Direction.Right })
            {
                bool broke = false;
                Point curr = p;

                foreach (CodeT code in codes)
                {
                    if (!ObjectsOn(curr).Any(o => o == code))
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

    public List<Object> Objects()
    {
        return board.Values.SelectMany(l => l).ToList();
    }

    public bool Is(Object type, Property prop)
    {
        return type switch
        {
            SpriteObject(Sprite sprite) => CodeSequenceAppears(new List<CodeT> {
            new SpriteCode(sprite), new SyntaxCode(Syntax.Is), new PropertyCode(prop)
        }),
            CodeT => IsCode(prop),
            _ => throw new UnreachableException()

        };
    }

    bool IsCode(Property prop)
    {
        return prop == Property.Push || prop == Property.Stop;
    }

    IEnumerable<Tuple<Point, Object>> PositionObject() {
        return Utils.BoardPositions().SelectMany(p => board[p].Select(o => new Tuple<Point, Object>(p, o)));
    }

    void Move(Direction d)
    {
        Dictionary<Point, List<(Object, Direction?)>> markedBoard = new Dictionary<Point, List<(Object, Direction?)>>();
        foreach (Point position in Utils.BoardPositions())
        {
            markedBoard[position] = new List<(Object, Direction?)>();
            foreach (Object o in board[position])
            {
                markedBoard[position].Add((o, null));
            }
        }
        foreach ((Point position, Object o) in PositionObject())
        {
            if (Is(o, Property.You))
            {
                Point curr = position;
                int i = 0;
                bool canMove = true;
                while (true)
                {
                    curr = d.OffsetPoint(curr);
                    i += 1;
                    if (!Utils.InBoard(curr) || board[curr].Any(_o => Is(_o, Property.Stop) & !Is(_o, Property.Push)))
                    {
                        canMove = false;
                        break;
                    }
                    if (!board[curr].Any(_o => Is(_o, Property.Push))) break;
                }
                if (canMove)
                {
                    markedBoard[position] = markedBoard[position].Select(od => od.Item1 == o ? (o, d) : od).ToList();
                    curr = position;
                    while (i > 0)
                    {
                        i -= 1;
                        curr = d.OffsetPoint(curr);
                        markedBoard[curr] = markedBoard[curr].Select(od => Is(od.Item1, Property.Push) ? (od.Item1, d) : od).ToList();
                    }
                }
            }
        }
        board = new Dictionary<Point, List<Object>>();
        foreach (Point position in Utils.BoardPositions())
        {
            board[position] = new List<Object>();
        }
        foreach (Point position in Utils.BoardPositions())
        {
            foreach ((Object o, Direction? _dir) in markedBoard[position])
            {
                if (_dir is null)
                {
                    board[position].Add(o);
                }
            }
        }
        
        foreach (Point position in Utils.BoardPositions())
        {
            foreach ((Object o, Direction? _dir) in markedBoard[position])
            {
                if (_dir is Direction dir)
                {
                    board[dir.OffsetPoint(position)].Add(o);
                }
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

        foreach (Object o in Objects())
        {
            if (Is(o, Property.You))
            {
                foreach (Point position in Utils.BoardPositions())
                {
                    if (board[position].Any(o => Is(o, Property.You) && board[position].Any(o => Is(o, Property.Win))))
                    {
                        Win();
                    }
                }
            }
        }

        base.Update(gameTime);
    }

    void Win()
    {
        Exit();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        foreach (Point position in Utils.BoardPositions())
        {
            if (board[position].Count() > 0)
            {
                DrawObject(board[position].Last(), position);
            }
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}