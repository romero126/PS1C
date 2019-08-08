
using Microsoft.PowerShell.Commands;
using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;

namespace PS1C.Archive
{
    
    public class ArchiveContentStream : StreamContentReaderWriter
    {

        private Archive.FileInfo _archiveFileInfo;
        private Archive.FileStream _archiveFileStream;

        private Archive.FileStream stream;
        private CmdletProvider _provider;


        public ArchiveContentStream(Archive.FileInfo archiveFileInfo, FileMode mode, Encoding encoding, bool usingByteEncoding, Provider provider, bool isRawStream)
        : base( archiveFileInfo.Open(mode), encoding, usingByteEncoding, provider, isRawStream)
        {
            _provider = provider;
        }

        public ArchiveContentStream(Archive.FileInfo archiveFileInfo, FileMode mode, Encoding encoding, bool usingByteEncoding, Provider provider, bool isRawStream, bool suppressNewline)
        : base(archiveFileInfo.Open(mode), encoding, usingByteEncoding, provider, isRawStream, suppressNewline)
        {
            _provider = provider;
        }

        public override void FinalizeStream()
        {
            //base.Dispose();
            //GC.Collect();
        }
        ~ArchiveContentStream()
        {

        }

    }
}