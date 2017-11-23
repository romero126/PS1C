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
	public class ZipFileObject
	{
		public ZipArchiveEntry _archive;
		public string Name { get { return (_name != null) ? _name : _archive.Name; } }
		public string FullName { get { return _fullname; } }
		private string _fullname;
		private string _name;
		public string path { get; set; }
		public long Length { get { return _archive.Length; } }
		public DateTimeOffset CreationTime { get { return _archive.LastWriteTime; } }
		public DateTimeOffset LastWriteTime { get { return _archive.LastWriteTime; } }
		public DateTimeOffset LastAccessTime { get { return _archive.LastWriteTime; } }
		public bool isDirectory;
		private Stream _stream;
		public ZipFileObject(ZipArchiveEntry entry, string drive, string path, string name, bool isdirectory = false)
		{
			this.path = path.Replace("//", "/");
			_name = name;
			_fullname = string.Format("{0}:/{1}/{2}", drive, path, name).Replace("//", "/");
			isDirectory = isdirectory;
			_archive = entry;
		}


	}
	public class ZipFileStream : IContentReader
	{
		private Stream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private bool _binary;
		private ZipArchiveEntry _archive;
		public ZipFileStream(ZipArchiveEntry ArchiveEntry, bool IsBinary) {
			_binary = IsBinary;
			_archive = ArchiveEntry;
			_stream = _archive.Open();
			
			//_stream.Position = 0;
			_reader = new StreamReader(_stream);
			//_writer = new StreamWriter(_stream);
			
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
		}
		public void Dispose()
		{

		}

		public int Read()
		{
			return 1;
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
		public void Seek(long offset, SeekOrigin origin)
		{
			//if (_stream.CanSeek) {
				//_stream.Seek(offset, origin);
			//}

		}
	}

}
