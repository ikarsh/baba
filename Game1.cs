
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
    const int MAX_BOARD_WIDTH = 33;
    const int MAX_BOARD_HEIGHT = 18;
    const int MIN_MARGIN_HEIGHT = 20;
    const int MIN_MARGIN_WIDTH = 20;
    public const int SCREEN_WIDTH = MAX_BOARD_WIDTH * SQUARE_SIZE + 2 * MIN_MARGIN_WIDTH;
    public const int SCREEN_HEIGHT = MAX_BOARD_HEIGHT * SQUARE_SIZE + 2 * MIN_MARGIN_HEIGHT;
    public const double MOVE_DELAY_MS = 30;
}

static class Utils
{
    // public static  IEnumerable<Point> BoardPositions()
    // {
    //     return Enumerable.Range(0, Config.SCR_WID).SelectMany(x => Enumerable.Range(0, Config.SCR_HEI).Select(y => new Point(x, y)));
    // }
    // public static bool InBoard(Point point)
    // {
    //     return point.X >= 0 && point.X <= Config.SCR_WID - 1 && point.Y >= 0 && point.Y <= Config.SCR_HEI - 1;
    // }

    // public static void Draw(SpriteBatch g, Point position, Texture2D texture)
    // {
    // }
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

public struct Board
{
    public Dictionary<Point, List<Object>> objects;
    public int WID;
    public int HEI;

    public Board(int WID, int HEI, Dictionary<Point, List<Object>> objects)
    {
        this.WID = WID;
        this.HEI = HEI;
        this.objects = objects;
    }
    public IEnumerable<Point> Positions => objects.Keys;
    public bool LegalPosition(Point p) => p.X >= 0 && p.X < WID && p.Y >= 0 && p.Y < HEI;

    public List<Object> this[Point p]
    {
        get => objects[p];
        set => objects[p] = value;
    }
}

public class Game1 : Game
{
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;
    int level;
    Board board;
    Dictionary<Object, Texture2D> textures;

    double lastMoveTime = 0;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = Config.SCREEN_WIDTH;
        _graphics.PreferredBackBufferHeight = Config.SCREEN_HEIGHT;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        level = -1;
        Win();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        textures = new Dictionary<Object, Texture2D>();

        foreach (Sprite sprite in Enum.GetValues(typeof(Sprite)))
        {
            textures[new SpriteObject(sprite)] = Content.Load<Texture2D>($"assets/sprites/{sprite}");
            textures[new SpriteCode(sprite)] = Content.Load<Texture2D>($"assets/codes/sprites/{sprite}");
        }

        foreach (Syntax syntax in Enum.GetValues(typeof(Syntax)))
        {
            textures[new SyntaxCode(syntax)] = Content.Load<Texture2D>($"assets/codes/syntax/{syntax}");
        }

        foreach (Property property in Enum.GetValues(typeof(Property)))
        {
            textures[new PropertyCode(property)] = Content.Load<Texture2D>($"assets/codes/properties/{property}");
        }
    }
    bool CodeSequenceAppears(List<CodeT> codes)
    {
        if (codes.Count() == 0) return true;
        foreach (Point p in board.Positions)
        {
            foreach (Direction d in new[] { Direction.Down, Direction.Right })
            {
                bool broke = false;
                Point curr = p;

                foreach (CodeT code in codes)
                {
                    if (!board[curr].Any(o => o == code))
                    {
                        broke = true;
                        break;
                    }

                    curr = d.OffsetPoint(curr);
                }
                if (!broke) return true;
                if (!board.LegalPosition(curr)) return false;
            }
        }
        return false;
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

    void Move(Direction d)
    {
        Dictionary<Point, List<(Object, Direction?)>> markedBoard = new Dictionary<Point, List<(Object, Direction?)>>();
        foreach (Point p in board.Positions)
        {
            markedBoard[p] = new List<(Object, Direction?)>();
            foreach (Object o in board[p])
            {
                markedBoard[p].Add((o, null));
            }
        }
        foreach (Point position in board.Positions)
        {
            foreach (Object o in board[position])
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
                        if (!board.LegalPosition(curr) || board[curr].Any(_o => Is(_o, Property.Stop) & !Is(_o, Property.Push)))
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
        }
        foreach (Point position in markedBoard.Keys)
        {
            board[position].Clear();
        }
        foreach (Point position in markedBoard.Keys)
        {
            foreach ((Object o, Direction? _dir) in markedBoard[position])
            {
                if (_dir is null)
                {
                    board[position].Add(o);
                }
            }
        }

        foreach (Point position in markedBoard.Keys)
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

        foreach (Point p in board.Positions)
        {
            if (board[p].Any(o => Is(o, Property.You)) && board[p].Any(o => Is(o, Property.Win)))
            {
                Win();
                break;
            }
        }

        base.Update(gameTime);
    }

    void Win()
    {
        level += 1;
        try
        {
            board = Level(level);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("You win! No more levels.");
            Exit();
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Gray);

        int MARGIN_WIDTH = (Config.SCREEN_WIDTH - board.WID * Config.SQUARE_SIZE) / 2;
        int MARGIN_HEIGHT = (Config.SCREEN_HEIGHT - board.HEI * Config.SQUARE_SIZE) / 2;

        _spriteBatch.Begin();

        _spriteBatch.Draw(
            textures[new SpriteObject(Sprite.Baba)],
            new Rectangle(
                MARGIN_WIDTH,
                MARGIN_HEIGHT,
                Config.SCREEN_WIDTH - 2 * MARGIN_WIDTH,
                Config.SCREEN_HEIGHT - 2 * MARGIN_HEIGHT),
            Color.Black
        );
        
        foreach (Point position in board.Positions)
        {
            if (board[position].Count() > 0)
            {

                Texture2D texture = textures[board[position].Last()];
                _spriteBatch.Draw(
                    texture,
                    new Rectangle(
                        MARGIN_WIDTH + position.X * Config.SQUARE_SIZE,
                        MARGIN_HEIGHT + position.Y * Config.SQUARE_SIZE,
                        Config.SQUARE_SIZE,
                        Config.SQUARE_SIZE
                    ),
                    Color.White
                );
            }
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}