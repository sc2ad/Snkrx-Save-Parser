using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SNKRX_Save_Parser.Deserialization
{
    /// <summary>
    /// This type represents a concept that parses a save file into a token stream.
    /// This token stream is then further parsable into types.
    /// Thus, this is a general-purpose Lua .txt parser (or whatever save format SNKRX uses that emits this oddly written file type).
    /// </summary>
    public class LuaSaveTokenizer
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

        /// <summary>
        /// Represents a Lua Save file token with some metadata.
        /// </summary>
        public struct TokenMatch
        {
            public Token Token { get; init; }
            public bool IsMatch { get; init; }
            public string Value { get; init; }
        }

        private class RegexTokenDefinition
        {
            private readonly Regex pattern;
            private readonly Token type;

            public RegexTokenDefinition(string regex, Token ret)
            {
                pattern = new Regex(regex);
                type = ret;
            }

            // TODO: Use spans here when regex allows us to
            public TokenMatch Match(string input)
            {
                var match = pattern.Match(input);
                if (match.Success)
                {
                    return new TokenMatch
                    {
                        IsMatch = true,
                        Token = type,
                        Value = match.Value
                    };
                }
                return new TokenMatch { IsMatch = false };
            }
        }

        private static readonly List<RegexTokenDefinition> syntax = new()
        {
            new RegexTokenDefinition("^return", Token.Return),
            new RegexTokenDefinition("^{", Token.LeftBrace),
            new RegexTokenDefinition("^}", Token.RightBrace),
            new RegexTokenDefinition("^\\[", Token.LeftBracket),
            new RegexTokenDefinition("^]", Token.RightBracket),
            new RegexTokenDefinition("^=", Token.Equals),
            new RegexTokenDefinition(@"^\d", Token.Digit),
            new RegexTokenDefinition("^true", Token.True),
            new RegexTokenDefinition("^false", Token.False),
            new RegexTokenDefinition("^,", Token.Comma),
            new RegexTokenDefinition("^\".*?\"", Token.StringLiteral)
        };

        private static TokenMatch FindMatch(string data)
        {
            foreach (var regex in syntax)
            {
                var match = regex.Match(data);
                if (match.IsMatch)
                {
                    return match;
                }
            }
            return new TokenMatch { IsMatch = false };
        }

        private static IEnumerable<TokenMatch> Tokenize(string? data)
        {
            var remaining = data;
            while (!string.IsNullOrEmpty(remaining))
            {
                var match = FindMatch(remaining);
                if (match.IsMatch)
                {
                    yield return match;
                    remaining = remaining[match.Value.Length..];
                }
                else
                {
                    int whitespaceIdx = 0;
                    while (char.IsWhiteSpace(remaining[whitespaceIdx]))
                    {
                        whitespaceIdx++;
                    }
                    if (whitespaceIdx > 0)
                    {
                        // Skip all of the whitespace we counted
                        remaining = remaining[whitespaceIdx..];
                    }
                    else
                    {
                        // We have a parse failure
                        yield return new TokenMatch { IsMatch = false, Token = Token.Unknown, Value = remaining };
                        break;
                    }
                }
            }
            yield break;
        }

        /// <summary>
        /// Return a collection of <see cref="TokenMatch"/> from the provided <see cref="TextReader"/> which it consumes.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TokenMatch>> Tokenize(TextReader reader)
        {
            var toParse = await reader.ReadLineAsync().ConfigureAwait(false);
            var ret = new List<TokenMatch>();
            while (!string.IsNullOrEmpty(toParse))
            {
                ret.AddRange(Tokenize(toParse));
                toParse = await reader.ReadLineAsync().ConfigureAwait(false);
            }
            return ret;
        }
    }
}