namespace SNKRX_Save_Parser.Deserialization
{
    /// <summary>
    /// Represents a Lua Save file token with some metadata.
    /// </summary>
    public struct TokenMatch
    {
        public Token Token { get; }
        public bool IsMatch { get; }
        public string Value { get; }

        public TokenMatch(Token t, string v)
        {
            IsMatch = t != Token.Unknown;
            Token = t;
            Value = v;
        }
    }
}