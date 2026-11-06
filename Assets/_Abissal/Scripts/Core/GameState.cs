namespace Abissal.Core
{
    /// <summary>Estados globais do jogo. Sistemas como oxigênio só funcionam em Playing.</summary>
    public enum GameState
    {
        Title,
        Playing,
        Paused,
        Victory,
        GameOver
    }
}
