using SNKRX_Save_Parser.Deserialization;
using System.Linq;
using Xunit;

namespace SNKRX_Save_Parser.Tests
{
    /// <summary>
    /// A collection of tests to do with the tokenizer alone.
    /// That is, it attempts to ensure that the tokenizer can emit a valid token stream from valid SNKRX save files.
    /// </summary>
    public class TokenizerTests
    {
        [Theory]
        [InlineData("{}", Token.LeftBrace, Token.RightBrace)]
        [InlineData("return {[\"level\"] = 1}", Token.Return, Token.LeftBrace, Token.LeftBracket, Token.StringLiteral, Token.RightBracket, Token.Equals, Token.Digit, Token.RightBrace)]
        [InlineData("{[30] = \"baneling_burst\", [31] = \"blunt_arrow\"}", Token.LeftBrace, Token.LeftBracket, Token.Digit, Token.RightBracket, Token.Equals, Token.StringLiteral, Token.Comma, Token.LeftBracket, Token.Digit, Token.RightBracket, Token.Equals, Token.StringLiteral, Token.RightBrace)]
        [InlineData("{[1] = true, [2] = false}", Token.LeftBrace, Token.LeftBracket, Token.Digit, Token.RightBracket, Token.Equals, Token.True, Token.Comma, Token.LeftBracket, Token.Digit, Token.RightBracket, Token.Equals, Token.False, Token.RightBrace)]
        public void TestTokens(string data, params Token[] tokens)
        {
            var res = LuaSaveTokenizer.Tokenize(data);
            Assert.Equal(tokens, res.Select(match => match.Token));
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("return {[\"level\"] = 1}", "\"level\"", "1")]
        [InlineData("{[30] = \"baneling_burst\", [31] = \"blunt_arrow\"}", "30", "\"baneling_burst\"", "31", "\"blunt_arrow\"")]
        public void TestValues(string data, params string[] match)
        {
            var res = LuaSaveTokenizer.Tokenize(data);
            foreach (var item in res)
            {
                switch (item.Token)
                {
                    case Token.Return:
                        Assert.Equal("return", item.Value);
                        break;

                    case Token.LeftBrace:
                        Assert.Equal("{", item.Value);
                        break;

                    case Token.LeftBracket:
                        Assert.Equal("[", item.Value);
                        break;

                    case Token.Comma:
                        Assert.Equal(",", item.Value);
                        break;

                    case Token.Equals:
                        Assert.Equal("=", item.Value);
                        break;

                    case Token.False:
                        Assert.Equal("false", item.Value);
                        break;

                    case Token.True:
                        Assert.Equal("true", item.Value);
                        break;

                    case Token.RightBrace:
                        Assert.Equal("}", item.Value);
                        break;

                    case Token.RightBracket:
                        Assert.Equal("]", item.Value);
                        break;
                }
            }

            // Now check for pure value matches
            Assert.Equal(match, res.Where(match => match.Token == Token.StringLiteral || match.Token == Token.Digit).Select(match => match.Value));
        }
    }
}