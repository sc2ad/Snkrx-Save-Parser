using SNKRX_Save_Parser.Attributes;
using SNKRX_Save_Parser.Deserialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace SNKRX_Save_Parser.Tests
{
    /// <summary>
    /// A collection of tests to do with the parser alone.
    /// That is, it attempts to ensure that the parser can properly convert from a token stream into valid instances.
    /// </summary>
    public class ParserTests
    {
        public enum Test
        {
            One,
            Two,

            [SaveDataName("multiple words")]
            Multiple_Words
        }

        public static IEnumerable<object[]> EnumTestData => new List<object[]>
        {
            new object[] {Test.One, new TokenMatch(Token.StringLiteral, "\"One\"")},
            new object[] {Test.Two, new TokenMatch(Token.StringLiteral, "\"two\"")},
            new object[] {Test.Multiple_Words, new TokenMatch(Token.StringLiteral, "\"multiple_words\"")},
            new object[] {Test.Multiple_Words, new TokenMatch(Token.StringLiteral, "\"multiple words\"")}
        };

        [Theory]
        [MemberData(nameof(EnumTestData))]
        public void ParseEnums(Test expectedEnum, params TokenMatch[] tokenStream)
        {
            var stream = new LuaSaveTokenStream(tokenStream);
            Assert.Equal(expectedEnum, TypeParser.Parse<Test>(stream));
            Assert.Equal(0, stream.Length());
        }

        public class SomeInstance
        {
            public Test MyEnum { get; set; }

            public override bool Equals(object obj)
            {
                return obj is SomeInstance x && x.MyEnum == MyEnum;
            }
        }

        public class SomeInstanceEmpty
        {
            [SerializeIf(true)]
            public bool Exists { get; set; }

            public override bool Equals(object obj)
            {
                return obj is SomeInstanceEmpty x && x.Exists == Exists;
            }
        }

        public class SomeInstanceTwo
        {
            public string SomeProperty { get; set; }

            [SaveDataName("my_digit")]
            public int MyDigit { get; set; }

            public override bool Equals(object obj)
            {
                return obj is SomeInstanceTwo x && x.SomeProperty == SomeProperty && x.MyDigit == MyDigit;
            }
        }

        private static IEnumerable<TokenMatch> WriteProperty(string propName, Token type, string value)
        {
            yield return new TokenMatch(Token.LeftBracket, "[");
            yield return new TokenMatch(Token.StringLiteral, $"\"{propName}\"");
            yield return new TokenMatch(Token.RightBracket, "]");
            yield return new TokenMatch(Token.Equals, "=");
            yield return new TokenMatch(type, value);
        }

        private static readonly TokenMatch startObj = new(Token.LeftBrace, "{");
        private static readonly TokenMatch endObj = new(Token.RightBrace, "}");

        public static IEnumerable<object[]> InstanceTestData()
        {
            var emptyInstance = new List<TokenMatch> { startObj, endObj };

            var someProp = new List<TokenMatch> { startObj };
            someProp.AddRange(WriteProperty(nameof(SomeInstance.MyEnum), Token.StringLiteral, "\"two\""));
            someProp.Add(endObj);

            var two = new List<TokenMatch> { startObj };
            two.AddRange(WriteProperty(nameof(SomeInstanceTwo.SomeProperty), Token.StringLiteral, "\"data\""));
            two.Add(new TokenMatch(Token.Comma, ","));
            two.AddRange(WriteProperty("my_digit", Token.Digit, "42"));
            two.Add(endObj);

            var withEnum = new List<TokenMatch> { startObj };
            withEnum.AddRange(WriteProperty("myenum", Token.StringLiteral, "\"one\""));
            withEnum.Add(endObj);

            var conditional = new List<TokenMatch> { startObj };
            conditional.AddRange(WriteProperty(nameof(SomeInstanceEmpty.Exists), Token.True, "true"));
            conditional.Add(endObj);

            return new List<object[]>
            {
                new object[] {new SomeInstanceEmpty(), emptyInstance },
                new object[] {new SomeInstance { MyEnum = Test.Two }, someProp },
                new object[] {new SomeInstanceTwo { SomeProperty = "data", MyDigit = 42 }, two },
                new object[] {new SomeInstance { MyEnum = Test.One}, withEnum },
                new object[] {new SomeInstanceEmpty { Exists = true}, conditional }
            };
        }

        [Theory]
        [MemberData(nameof(InstanceTestData))]
        public void ParseInstances(object instanceToMatch, IEnumerable<TokenMatch> tokenStream)
        {
            var stream = new LuaSaveTokenStream(tokenStream);
            Assert.Equal(instanceToMatch, TypeParser.Parse(stream, instanceToMatch.GetType()));
            Assert.Equal(0, stream.Length());
        }
    }
}