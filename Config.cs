
public static class Config
{
    public const int SQUARE_SIZE = 50;
    const int MAX_BOARD_WIDTH = 33;
    const int MAX_BOARD_HEIGHT = 18;
    const int MIN_MARGIN_HEIGHT = 20;
    const int MIN_MARGIN_WIDTH = 20;
    public const int SCREEN_WIDTH = MAX_BOARD_WIDTH * SQUARE_SIZE + 2 * MIN_MARGIN_WIDTH;
    public const int SCREEN_HEIGHT = MAX_BOARD_HEIGHT * SQUARE_SIZE + 2 * MIN_MARGIN_HEIGHT;
    public const double DELAY_MS = 120;

    public const int START_LEVEL = 5;
}
