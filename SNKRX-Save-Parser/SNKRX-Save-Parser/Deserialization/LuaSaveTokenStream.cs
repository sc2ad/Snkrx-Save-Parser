using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SNKRX_Save_Parser.Deserialization.LuaSaveTokenizer;

namespace SNKRX_Save_Parser.Deserialization
{
    /// <summary>
    /// Represents a stream that wraps a token collection and provides various helper functions for it.
    /// </summary>
    public class LuaSaveTokenStream
    {
        public class InvalidTokenException : Exception
        {
            public Token Expected { get; }
            public TokenMatch Actual { get; }

            public InvalidTokenException(Token expected, TokenMatch actual) : base($"Read invalid token type! Expected: {expected}, got: {actual.Token} (value: {actual.Value})")
            {
                Expected = expected;
                Actual = actual;
            }
        }

        private readonly List<TokenMatch> tokens;
        private int index;

        public LuaSaveTokenStream(IEnumerable<TokenMatch> tokens)
        {
            this.tokens = new(tokens);
        }

        private void EnsureIdx()
        {
            if (index >= tokens.Count)
                throw new InvalidOperationException("Reading past the end of the tokens collection!");
        }

        /// <summary>
        /// Checks to see what <see cref="TokenMatch"/> the first token is.
        /// </summary>
        /// <returns></returns>
        public TokenMatch Peek()
        {
            EnsureIdx();
            return tokens[index];
        }

        /// <summary>
        /// Asserts that the front token matches <paramref name="type"/> and moves past it, returning the match.
        /// This method throws a <see cref="InvalidTokenException"/> if the token does not match.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public TokenMatch Consume(Token type)
        {
            EnsureIdx();
            var match = tokens[index];
            if (match.Token != type)
            {
                throw new InvalidTokenException(type, match);
            }
            if (match.Token == Token.LeftBrace || match.Token == Token.LeftBracket)
                ++Depth;
            else if (match.Token == Token.RightBrace || match.Token == Token.RightBracket)
                --Depth;
            ++index;
            return match;
        }

        public int Depth { get; private set; }

        public int Length()
        {
            return tokens.Count;
        }
    }
}