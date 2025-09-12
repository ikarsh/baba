
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using static Levels;

using Microsoft.Xna.Framework;        // Point, Vector2, Color, etc.
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;  // Keyboard, KeyboardState, Mouse, etc.

namespace baba;

public enum Direction { Right, Up, Left, Down };

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
    List<Board> history;
    Board board;
    List<Rule> rules;
    Dictionary<(Sprite, Direction, ObjectState), Gif> CharacterGifDict;
    Dictionary<Sprite, Gif> ItemGifDict;
    Dictionary<(Sprite, Direction), Gif> DirectedItemGifDict;
    Dictionary<Code, Gif> CodeGifDict;
    Dictionary<(Sprite, bool, bool, bool, bool), Gif> TileGifDict;

    double unlockTime = 0;
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
        level = Config.START_LEVEL;
        StartLevel();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        CharacterGifDict = SpriteSheetExtensions.CharacterDict(this);
        ItemGifDict = SpriteSheetExtensions.ItemDict(this);
        DirectedItemGifDict = SpriteSheetExtensions.DirectedItemDict(this);
        CodeGifDict = SpriteSheetExtensions.CodeDict(this);
        TileGifDict = SpriteSheetExtensions.TileDict(this);
    }

    public bool IsTypeProp(Type type, Property prop)
    {
        return type switch
        {
            SpriteType(Sprite sprite) => rules.Contains(new IsPropRule(sprite, prop)),
            Code => prop == Property.Push || prop == Property.Stop,
            _ => throw new UnreachableException()
        };
    }


    Type WhatIsType(Type type)
    {
        return type switch
        {
            SpriteType(Sprite sprite) => (rules.OfType<IsSpriteRule>().Where(r => r.sprite1 == sprite).ElementAtOrDefault(0)?.sprite2 ?? sprite).Type(),
            _ => type
        };
    }


    Board MovedBoard(Direction d)
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
                if (IsTypeProp(o.type, Property.You))
                {
                    Point curr = position;
                    int i = 0;
                    bool canMove = true;
                    while (true)
                    {
                        curr = d.OffsetPoint(curr);
                        i += 1;
                        if (!board.LegalPosition(curr) || board[curr].Any(_o => IsTypeProp(_o.type, Property.Stop) & !IsTypeProp(_o.type, Property.Push)))
                        {
                            canMove = false;
                            break;
                        }
                        if (!board[curr].Any(_o => IsTypeProp(_o.type, Property.Push))) break;
                    }
                    if (canMove)
                    {
                        markedBoard[position] = markedBoard[position].Select(od => od.Item1.type == o.type ? (o.Move(d), d) : od).ToList();
                        curr = position;
                        while (i > 0)
                        {
                            i -= 1;
                            curr = d.OffsetPoint(curr);
                            markedBoard[curr] = markedBoard[curr].Select(od => IsTypeProp(od.Item1.type, Property.Push) ? (od.Item1.Move(d), d) : od).ToList();
                        }
                    }
                    else
                    {
                        markedBoard[position] = markedBoard[position].Select(od => od.Item1.type == o.type ? (o.Move(d), null) : od).ToList();
                    }
                }
            }
        }
        Board newBoard = new Board(board.WID, board.HEI, board.Positions.ToDictionary(p => p, p => new List<Object>()));
        foreach (Point position in markedBoard.Keys)
        {
            foreach ((Object o, Direction? _dir) in markedBoard[position])
            {
                if (_dir is null)
                {
                    newBoard[position].Add(o);
                }
            }
        }
        foreach (Point position in markedBoard.Keys)
        {
            foreach ((Object o, Direction? _dir) in markedBoard[position])
            {
                if (_dir is Direction dir)
                {
                    newBoard[dir.OffsetPoint(position)].Add(o);
                }
            }
        }
        return newBoard;
    }

    void PostMove()
    {
        // Sprite is Sprite
        foreach (Point p in board.Positions)
        {
            board[p] = board[p].Select(o => o.ChangeType(WhatIsType(o.type))).ToList();
        }

        foreach (Point p in board.Positions)
        {
            bool hasSink = board[p].Any(o => IsTypeProp(o.type, Property.Sink));
            bool hasDefeat = board[p].Any(o => IsTypeProp(o.type, Property.Defeat));
            bool hasHot = board[p].Any(o => IsTypeProp(o.type, Property.Hot));

            if (hasSink)
            {
                if (board[p].Count() > 1) board[p].Clear();
            }

            if (hasDefeat)
            {
                board[p] = board[p].Where(o => !IsTypeProp(o.type, Property.You)).ToList();
            }

            if (hasHot)
            {
                board[p] = board[p].Where(o => !IsTypeProp(o.type, Property.Melt)).ToList();
            }
        }
    }

    bool Won()
    {
        return board.Positions.Any(p => board[p].Any(o => IsTypeProp(o.type, Property.You)) && board[p].Any(o => IsTypeProp(o.type, Property.Win)));
    }

    protected override void Update(GameTime gameTime)
    {
        foreach (Gif gif in CharacterGifDict.Values) gif.Update(gameTime);
        foreach (Gif gif in ItemGifDict.Values) gif.Update(gameTime);
        foreach (Gif gif in DirectedItemGifDict.Values) gif.Update(gameTime);
        foreach (Gif gif in CodeGifDict.Values) gif.Update(gameTime);
        foreach (Gif gif in TileGifDict.Values) gif.Update(gameTime);

        double time = gameTime.TotalGameTime.TotalMilliseconds;
        if (time < unlockTime)
        {
            base.Update(gameTime);
            return;
        }


        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        else if (Keyboard.GetState().IsKeyDown(Keys.R))
        {
            ResetBoard();
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Z))
        {
            if (history.Count() > 0)
            {
                SetBoard(history.Last());
                history.RemoveAt(history.Count() - 1);
            }
        }
        else if (DirectionExtensions.FromKeyboard() is Direction d)
        {
            {
                history.Add(board);
                SetBoard(MovedBoard(d));
                if (Won()) Win();
            }
        }
        else
        {
            base.Update(gameTime);
            return;
        }

        unlockTime = time + Config.DELAY_MS;
        base.Update(gameTime);
    }

    void StartLevel()
    {
        history = new List<Board>();
        ResetBoard();
    }

    void ResetBoard()
    {
        SetBoard(Level(level));
    }

    void SetBoard(Board newBoard)
    {
        board = newBoard;
        rules = RuleExtensions.FromBoard(newBoard);
        PostMove();
    }

    void Win()
    {
        level += 1;
        try
        {
            StartLevel();
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
            Content.Load<Texture2D>("solid_black"),
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

                foreach (Object o in board[position])
                {
                    Gif gif = GetGif(position, o);
                    _spriteBatch.Draw(
                        gif.GetCurrentFrame(),
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
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    Gif GetGif(Point position, Object o)
    {
        return o.type switch
        {
            SpriteType(Sprite sprite) when sprite.IsCharacter() => CharacterGifDict[(sprite, o.direction, o.state)],
            SpriteType(Sprite sprite) when sprite.IsItem() => ItemGifDict[sprite],
            SpriteType(Sprite sprite) when sprite.IsDirectedItem() => DirectedItemGifDict[(sprite, o.direction)],
            SpriteType(Sprite sprite) when sprite.IsTile() => TileGif(position, sprite),
            Code code => CodeGifDict[code],
            _ => throw new UnreachableException()
        };
    }

    Gif TileGif(Point position, Sprite sprite)
    {
        bool right = false;
        bool up = false;
        bool left = false;
        bool down = false;
        foreach (Direction d in Enum.GetValues(typeof(Direction)))
        {
            Point np = d.OffsetPoint(position);
            if (!board.LegalPosition(np) || board[np].Any(o => o.type == new SpriteType(sprite)))
            {
                switch (d)
                {
                    case Direction.Up: up = true; break;
                    case Direction.Down: down = true; break;
                    case Direction.Left: left = true; break;
                    case Direction.Right: right = true; break;
                }
            }
        }
        return TileGifDict[(sprite, right, up, left, down)];
    }
}