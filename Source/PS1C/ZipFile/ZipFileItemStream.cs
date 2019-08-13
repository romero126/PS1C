using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO.Compression;


namespace Microsoft.PowerShell.Commands
{
    public class ZipFileItemStream : System.IO.Stream
    {

        private ZipArchive _zipArchive;
        private ZipArchiveEntry _zipArchiveEntry;
        private System.IO.Stream _zipArchiveEntryStream;
        public System.IO.MemoryStream _stream;

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
                // Base permissions should still apply
                return _zipArchiveEntryStream.CanRead;
            }
        }

        public override bool CanWrite {
            get
            {
                // Base permissions should still apply
                return _zipArchiveEntryStream.CanWrite;
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
                    break;
                case FileMode.Truncate:
                    throw new Exception("Invalid Parameter Truncate not valid");
                    break;
                case FileMode.Append:
                    zipArchiveMode = ZipArchiveMode.Update;
                    break;
            }
            _zipArchive = System.IO.Compression.ZipFile.Open(archiveName, zipArchiveMode);
            _zipArchiveEntry = _zipArchive.GetEntry(path);

            _zipArchiveEntryStream = _zipArchiveEntry.Open();
            this._stream = new MemoryStream();

            // Note: This part is very Memory Intensive.
            _zipArchiveEntryStream.CopyTo(_stream);
            
            // Sets position to 0 so it can be fresh
            _stream.Position = 0;
        }


        public override void Close()
        {
            if (!_isClosed)
            {
                // Avoid writing back a value if opened with Read permissions.
                if (_zipArchive.Mode != ZipArchiveMode.Read)
                {
                    _stream.Position = 0;
                    // Write all of the buffer. 
                    _zipArchiveEntryStream.Seek(0, SeekOrigin.Begin);
                    _zipArchiveEntryStream.SetLength(0);
                    
                    _stream.WriteTo(_zipArchiveEntryStream);
                }

                _zipArchiveEntryStream.Flush();
                
                _stream.Dispose();
                _zipArchiveEntryStream.Dispose();

                _zipArchive.Dispose();
                _zipArchive = null;
                _isClosed = true;
                GC.Collect();
            }
        }

        public void Dispose()
        {

        }

        ~ZipFileItemStream()
        {
            Dispose();
        }
    }
}