using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO.Compression;


namespace Microsoft.PowerShell.Commands
{
    #region ZipFileItemStream
    public class ZipFileItemStream : System.IO.Stream
    {

        private ZipArchive _zipArchive;
        private ZipArchiveEntry _zipArchiveEntry;
        public System.IO.Stream _stream;

        public bool _isClosed;
        public override long Length {
            get
            {
                return _stream.Length;
            }
        }
        public override long Position {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }
        public override bool CanRead {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanWrite {
            get
            {
                return _stream.CanWrite;
            }
        }

        public override void Flush()
        {
            _stream.Flush();
        }
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        // Todo: Make ZipFileItemStream use (ZipArchive archiveName) instead of string;
        public ZipFileItemStream(string archiveName, string path, System.IO.FileMode mode)
        {
            ZipArchiveMode zipArchiveMode = ZipArchiveMode.Read;
            switch (mode)
            {
                case FileMode.CreateNew:
                    zipArchiveMode = ZipArchiveMode.Create;
                    break;
                case FileMode.Create:
                    zipArchiveMode = ZipArchiveMode.Create;
                    break;
                case FileMode.Open:
                    zipArchiveMode = ZipArchiveMode.Read;
                    break;
                case FileMode.OpenOrCreate:
                    throw new Exception("Invalid Parameter OpenOrCreate not valid");
                case FileMode.Truncate:
                    throw new Exception("Invalid Parameter Truncate not valid");
                case FileMode.Append:
                    zipArchiveMode = ZipArchiveMode.Update;
                    break;
            }
            _zipArchive = System.IO.Compression.ZipFile.Open(archiveName, zipArchiveMode);
            _zipArchiveEntry = _zipArchive.GetEntry(path);

            _stream = _zipArchiveEntry.Open();

            // Sets position to 0 so it can be fresh
            _stream.Position = 0;
        }


        public override void Close()
        {
            if (!_isClosed)
            {
                _stream.Flush();
                _stream.Dispose();

                _zipArchive.Dispose();
                _zipArchive = null;
                _isClosed = true;
                base.Close();
                GC.Collect();
            }

        }

        public void Dispose()
        {
            base.Dispose();
        }

        ~ZipFileItemStream()
        {
            Dispose();
        }
    }
    #endregion ZipFileItemStream
}