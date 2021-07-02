using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static SNKRX_Save_Parser.Deserialization.LuaSaveTokenizer;

namespace SNKRX_Save_Parser.Deserialization
{
    public class LuaSaveParser : IDisposable
    {
        private readonly Stream stream;
        private readonly bool disposeHeldStream;

        public LuaSaveParser(Stream stream, bool ownStream = false)
        {
            this.stream = stream;
            disposeHeldStream = ownStream;
        }

        /// <summary>
        /// Reads a <see cref="SaveData"/> from the constructed-with <see cref="Stream"/>, assumes it is a UTF8 Stream.
        /// </summary>
        /// <returns></returns>
        public async Task<SaveData> Read()
        {
            // First, parse the stream into a token collection
            var reader = new StreamReader(stream);
            var tokenStream = new LuaSaveTokenStream(await LuaSaveTokenizer.Tokenize(reader).ConfigureAwait(false));

            // Save data always starts with a return.
            tokenStream.Consume(Token.Return);
            return TypeParser.Parse<SaveData>(tokenStream);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            if (disposeHeldStream)
                stream.Dispose();
        }
    }
}