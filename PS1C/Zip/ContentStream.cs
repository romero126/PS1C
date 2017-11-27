using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Dbg = System.Management.Automation;

namespace PS1C.Zip
{
	/// <summary>
	/// The content stream class for the file system provider. It implements both
	/// the IContentReader and IContentWriter interfaces.
	/// </summary>
	///
	/// <remarks>
	/// Note, this class does no specific error handling. All errors are allowed to
	/// propagate to the caller so that they can be written to the error pipeline
	/// if necessary.
	/// </remarks>
	///
	class ContentStream : IContentReader, IContentWriter
	{
		#region tracer
		/*
		/// <summary>
		/// An instance of the PSTraceSource class used for trace output
		/// using "FileSystemContentStream" as the category.
		/// </summary>
		[Dbg.TraceSourceAttribute(
			"FileSystemContentStream",
			"The provider content reader and writer for the file system")]
		private static Dbg.PSTraceSource s_tracer =
			Dbg.PSTraceSource.GetTracer("FileSystemContentStream",
			"The provider content reader and writer for the file system");
		*/
		#endregion tracer
		private string _path;
		private string _streamName;
		private FileMode _mode;
		private FileAccess _access;
		private FileShare _share;
		private Encoding _encoding;
		private CmdletProvider _provider;

		private FileStream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private bool _usingByteEncoding;
		private string _delimiter = "\n";
		private bool _usingDelimiter;
		private bool _waitForChanges;
		private bool _isRawStream;
		private long _fileOffset;

		private FileAttributes _oldAttributes;
		private bool _haveOldAttributes;

		// The reader to read file content backward
		//private FileStreamBackReader _backReader;
		private bool _alreadyDetectEncoding = false;

		// False to add a newline to the end of the output string, true if not.
		private bool _suppressNewline = false;

		/// <summary>
		/// Constructor for the content stream
		/// </summary>
		///
		/// <param name="path">
		/// The path to the file to get the content from.
		/// </param>
		///
		/// <param name="mode">
		/// The file mode to open the file with.
		/// </param>
		///
		/// <param name="access">
		/// The file access requested in the file.
		/// </param>
		///
		/// <param name="share">
		/// The file share to open the file with
		/// </param>
		///
		/// <param name="encoding">
		/// The encoding of the file to be read or written.
		/// </param>
		///
		/// <param name="usingByteEncoding">
		/// If true, bytes will be read from the file. If false, the specified encoding
		/// will be used to read the file.
		/// </param>
		///
		/// <param name="waitForChanges">
		/// If true, we will perform blocking reads on the file, waiting for new content to be appended
		/// </param>
		///
		/// <param name="provider">
		/// The CmdletProvider invoking this stream
		/// </param>
		///
		/// <param name="isRawStream">
		/// Indicates raw stream.
		/// </param>
		///
		public ContentStream(string path, FileMode mode, FileAccess access,
			FileShare share, Encoding encoding, bool usingByteEncoding,
			bool waitForChanges, CmdletProvider provider, bool isRawStream) :
                this(path, null, mode, access, share, encoding, usingByteEncoding, waitForChanges, provider, isRawStream)
        {
		}


		/// <summary>
		/// Constructor for the content stream
		/// </summary>
		///
		/// <param name="path">
		/// The path to the file to get the content from.
		/// </param>
		///
		/// <param name="streamName">
		/// The name of the Alternate Data Stream to get the content from. If null or empty, returns
		/// the file's primary content.
		/// </param>
		///
		/// <param name="mode">
		/// The file mode to open the file with.
		/// </param>
		///
		/// <param name="access">
		/// The file access requested in the file.
		/// </param>
		///
		/// <param name="share">
		/// The file share to open the file with
		/// </param>
		///
		/// <param name="encoding">
		/// The encoding of the file to be read or written.
		/// </param>
		///
		/// <param name="usingByteEncoding">
		/// If true, bytes will be read from the file. If false, the specified encoding
		/// will be used to read the file.
		/// </param>
		///
		/// <param name="waitForChanges">
		/// If true, we will perform blocking reads on the file, waiting for new content to be appended
		/// </param>
		///
		/// <param name="provider">
		/// The CmdletProvider invoking this stream
		/// </param>
		///
		/// <param name="isRawStream">
		/// Indicates raw stream.
		/// </param>
		///
		public ContentStream(
			string path, string streamName, FileMode mode, FileAccess access, FileShare share,
			Encoding encoding, bool usingByteEncoding, bool waitForChanges, CmdletProvider provider,
			bool isRawStream)
		{
			if (String.IsNullOrEmpty(path))
			{
				//throw PSTraceSource.NewArgumentNullException("path");
				throw new PSArgumentNullException("path");
			}
			/*
			if (s_tracer.IsEnabled)
			{
				s_tracer.WriteLine("path = {0}", path);
				s_tracer.WriteLine("mode = {0}", mode);
				s_tracer.WriteLine("access = {0}", access);
			}
			*/
			_path = path;
			_streamName = streamName;
			_mode = mode;
			_access = access;
			_share = share;
			_encoding = encoding;
			_usingByteEncoding = usingByteEncoding;
			_waitForChanges = waitForChanges;
			_provider = provider;
			_isRawStream = isRawStream;

			//CreateStreams(path, streamName, mode, access, share, encoding);
		}

		/// <summary>
		/// Constructor for the content stream
		/// </summary>
		///
		/// <param name="path">
		/// The path to the file to get the content from.
		/// </param>
		///
		/// <param name="mode">
		/// The file mode to open the file with.
		/// </param>
		///
		/// <param name="access">
		/// The file access requested in the file.
		/// </param>
		///
		///  <param name="share">
		///    The file share to open the file with
		///  </param>
		///
		/// <param name="delimiter">
		/// The delimiter to use when reading strings. Each time read is called, all contents up to an including
		/// the delimiter is read.
		/// </param>
		///
		/// <param name="encoding">
		/// The encoding of the file to be read or written.
		/// </param>
		///
		/// <param name="waitForChanges">
		/// If true, we will perform blocking reads on the file, waiting for new content to be appended
		/// </param>
		///
		/// <param name="provider">
		/// The CmdletProvider invoking this stream
		/// </param>
		///
		/// <param name="isRawStream">
		/// Indicates raw stream.
		/// </param>
		///
		public ContentStream(
			string path,
			FileMode mode,
			FileAccess access,
			FileShare share,
			string delimiter,
			Encoding encoding,
			bool waitForChanges,
			CmdletProvider provider,
			bool isRawStream) : this(path, null, mode, access, share, encoding, false, waitForChanges, provider, isRawStream)
        {
		}

		/// <summary>
		/// Constructor for the content stream
		/// </summary>
		///
		/// <param name="path">
		/// The path to the file to get the content from.
		/// </param>
		///
		/// <param name="streamName">
		/// The name of the Alternate Data Stream to get the content from. If null or empty, returns
		/// the file's primary content.
		/// </param>
		///
		/// <param name="mode">
		/// The file mode to open the file with.
		/// </param>
		///
		/// <param name="access">
		/// The file access requested in the file.
		/// </param>
		///
		///  <param name="share">
		///    The file share to open the file with
		///  </param>
		///
		/// <param name="delimiter">
		/// The delimiter to use when reading strings. Each time read is called, all contents up to an including
		/// the delimiter is read.
		/// </param>
		///
		/// <param name="encoding">
		/// The encoding of the file to be read or written.
		/// </param>
		///
		/// <param name="waitForChanges">
		/// If true, we will perform blocking reads on the file, waiting for new content to be appended
		/// </param>
		///
		/// <param name="provider">
		/// The CmdletProvider invoking this stream
		/// </param>
		///
		/// <param name="isRawStream">
		/// Indicates raw stream.
		/// </param>
		///
		public ContentStream(
			string path,
			string streamName,
			FileMode mode,
			FileAccess access,
			FileShare share,
			string delimiter,
			Encoding encoding,
			bool waitForChanges,
			CmdletProvider provider,
			bool isRawStream)
            : this(path, streamName, mode, access, share, encoding, false, waitForChanges, provider, isRawStream)
        {
			_delimiter = delimiter;
			_usingDelimiter = true;
		}

		/// <summary>
		/// Constructor for the content stream
		/// </summary>
		///
		/// <param name="path">
		/// The path to the file to get the content from.
		/// </param>
		///
		/// <param name="streamName">
		/// The name of the Alternate Data Stream to get the content from. If null or empty, returns
		/// the file's primary content.
		/// </param>
		///
		/// <param name="mode">
		/// The file mode to open the file with.
		/// </param>
		///
		/// <param name="access">
		/// The file access requested in the file.
		/// </param>
		///
		/// <param name="share">
		/// The file share to open the file with
		/// </param>
		///
		/// <param name="encoding">
		/// The encoding of the file to be read or written.
		/// </param>
		///
		/// <param name="usingByteEncoding">
		/// If true, bytes will be read from the file. If false, the specified encoding
		/// will be used to read the file.
		/// </param>
		///
		/// <param name="waitForChanges">
		/// If true, we will perform blocking reads on the file, waiting for new content to be appended
		/// </param>
		///
		/// <param name="provider">
		/// The CmdletProvider invoking this stream
		/// </param>
		///
		/// <param name="isRawStream">
		/// Indicates raw stream.
		/// </param>
		///
		/// <param name="suppressNewline">
		/// False to add a newline to the end of the output string, true if not.
		/// </param>
		///
		public ContentStream(
			string path, string streamName, FileMode mode, FileAccess access, FileShare share,
			Encoding encoding, bool usingByteEncoding, bool waitForChanges, CmdletProvider provider,
			bool isRawStream, bool suppressNewline)
                : this(path, streamName, mode, access, share, encoding, usingByteEncoding, waitForChanges, provider, isRawStream)
        {
			_suppressNewline = suppressNewline;
		}
		#region OverridesToFixErrorsWhenWritingCode
		public void Seek(long offset, SeekOrigin origin)
		{
			return;
		}
		public IList Write(IList content) { return content; }
		public void Close() { }
		public void Dispose() { }
		public IList Read(long readcount) { return new List<object>(); }
		#endregion OverridesToFixErrorsWhenWritingCode
	}

}
