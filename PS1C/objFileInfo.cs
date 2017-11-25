using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;


namespace PS1C
{
	//ToDo:
	// Need to Rewrite this so that it gives out better output
	public class ZipFileStream : IContentReader, IContentWriter
	{
		private Stream _zstream;
		private Stream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private bool _binary;
		private ZipArchiveEntry _archive;
		public ZipFileStream(ZipArchiveEntry ArchiveEntry, bool IsBinary) {
			_binary = IsBinary;
			_archive = ArchiveEntry;

			_zstream = _archive.Open();
			//_stream = _archive.Open();
			_stream = new MemoryStream();
			_zstream.CopyTo(_stream);

			_stream.Position = 0;
			_reader = new StreamReader(_stream);
			_writer = new StreamWriter(_stream);
		}

		public void Close()
		{

			if (_reader != null)
			{
				_reader.Close();
			}

			if (_writer != null)
			{
				_writer.Close();
			}
			if (_stream != null)
			{
				_stream.Close();
			}

		}
		public void Dispose()
		{
			if (_reader != null) {
				_reader.Dispose();
				_reader = null;
			}
			if (_writer != null) {
				_writer.Dispose();
				_writer = null;
			}
			if (_stream != null)
			{
				_stream.Dispose();
				_stream = null;
			}
		}

		public System.Collections.IList Read(long readcount)
		{
			var list = new List<object>();
			long counter = 0;
			if (!_binary)
			{
				while (!_reader.EndOfStream && (counter < readcount || readcount < 1))
				{
					list.Add(_reader.ReadLine());
					counter++;
				}
			}
			else
			{
				while (counter < readcount || readcount < 1)
				{
					var value = _stream.ReadByte();
					if (value == -1) break;
					list.Add((byte)value);
					counter++;
				}

			}
			return list;
		
		}
		public System.Collections.IList Write(System.Collections.IList content) {
			if (!_binary) {
				 foreach (string str in content) {
					_writer.WriteLine(str);
				}
			}
			else
			{
				foreach (var obj in content) {
					if (obj is byte) {
						_stream.WriteByte((byte)obj);
					}
					else if (obj is char) {
						_stream.WriteByte(Convert.ToByte((char)obj));
					}
				}
			}
			_writer.Flush();
			_stream.Position = 0;


			return content;
		}
		public void Seek(long offset, SeekOrigin origin)
		{
			if (_stream.CanSeek) {
				_stream.Seek(offset, origin);
			}
		}
	}

}
