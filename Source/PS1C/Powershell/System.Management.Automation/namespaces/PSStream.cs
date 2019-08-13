




using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Management.Automation
{

    public class PSStream : StreamReader
    {
        // Read both forwards and backwards.
        public bool Readbackwards = false;
        public bool Lock = false;




        private readonly Stream _stream;
        private Encoding _encoding = null; // only null in NullStreamReader where this is never used
        private Decoder _decoder = null; // only null in NullStreamReader where this is never used
        private readonly byte[] _byteBuffer = null; // only null in NullStreamReader where this is never used
        private char[] _charBuffer = null; // only null in NullStreamReader where this is never used
        private int _charPos;
        private int _charLen;
        // Record the number of valid bytes in the byteBuffer, for a few checks.
        private int _byteLen;
        // This is used only for preamble detection
        private int _bytePos;


        public PSStream(Stream stream, Encoding encoding) : base(stream, encoding)
        {

        }


        /// <summary>
        /// Read the next character.
        /// </summary>
        /// <returns>Return -1 if we reach the head/tail of the stream.</returns>
        public override int Read()
        {
            //ThrowIfDisposed();
            //CheckAsyncTaskInProgress();

            if (_charPos == _charLen)
            {
                if (ReadBuffer() == 0)
                {
                    return -1;
                }
            }
            int result = _charBuffer[_charPos];
            _charPos += Readbackwards == true ? 1 : -1;
            return result;
        }
        private int ReadBuffer()
        {
            return 0;
        }
    }
}