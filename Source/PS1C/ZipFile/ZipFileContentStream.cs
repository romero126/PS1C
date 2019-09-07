
using Microsoft.PowerShell.Commands;
using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;

namespace PS1C.Archive
{
    #region StreamContent

    #region ZipFileContentStream
    public class ZipFileContentStream : StreamContentReaderWriter
    {

        private ZipFileItemInfo _archiveFileInfo;
        private ZipFileItemStream _archiveFileStream;

        private ZipFileItemStream stream;
        private CmdletProvider _provider;


        public ZipFileContentStream(ZipFileItemInfo archiveFileInfo, FileMode mode, Encoding encoding, bool usingByteEncoding, CmdletProvider provider, bool isRawStream)
        : base( archiveFileInfo.Open(mode), encoding, usingByteEncoding, provider, isRawStream)
        {
            _provider = provider;
        }

        public ZipFileContentStream(ZipFileItemInfo archiveFileInfo, FileMode mode, Encoding encoding, bool usingByteEncoding, CmdletProvider provider, bool isRawStream, bool suppressNewline)
        : base(archiveFileInfo.Open(mode), encoding, usingByteEncoding, provider, isRawStream, suppressNewline)
        {
            _provider = provider;
        }

        public ZipFileContentStream(ZipFileItemInfo archiveFileInfo, FileMode mode, string delimiter, Encoding encoding, bool usingByteEncoding, CmdletProvider provider, bool isRawStream)
        : base(archiveFileInfo.Open(mode), delimiter, encoding, provider, isRawStream)
        {
            _provider = provider;
        }


        ~ZipFileContentStream()
        {

        }

    }
    #endregion ZipFileContentStream

    #endregion StreamContent
}