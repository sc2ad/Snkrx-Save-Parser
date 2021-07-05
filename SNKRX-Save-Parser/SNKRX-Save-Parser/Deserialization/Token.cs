namespace SNKRX_Save_Parser.Deserialization
{
    /// <summary>
    /// A Lua Save file token.
    /// </summary>
    public enum Token
    {
        Unknown,
        Return,
        LeftBrace,
        RightBrace,
        LeftBracket,
        RightBracket,
        Equals,
        Digit,
        True,
        False,
        Comma,
        StringLiteral
    }
}