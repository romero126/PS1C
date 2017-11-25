using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Provider;


namespace PS1C.Zip
{
	internal class Item : IContentReader, IContentWriter
	{
		public string Name;
		public long Length;
		public DateTimeOffset LastWriteTime;
		public string FullName;
		public string FullPath;
		public long CompressedLength;
		public string ArchivePath;
		public string Drive;
		public DriveInfo DriveInfo;
		public Item(ZipArchiveEntry archiveEntry, DriveInfo drive) {
			this.Name = archiveEntry.Name;
			this.Length = archiveEntry.Length;
			this.LastWriteTime = archiveEntry.LastWriteTime;
			this.DriveInfo = drive;
			this.Drive = this.DriveInfo.Name + ":\\";
			this.FullPath = archiveEntry.FullName;
			this.FullName = (this.Drive + this.FullPath).Replace("/", "\\").Replace("\\\\", "\\");
			this.CompressedLength = archiveEntry.CompressedLength;
			this.ArchivePath = drive.Root;
		}

		#region IContent
		public bool Lock;
		public ZipArchiveEntry LockEntry;
		public bool isLocked { get { return Lock; } }
		private Stream _zstream;
		private Stream _stream;
		
		private StreamReader _reader;
		private StreamWriter _writer;
		private bool _binary;
		private ZipArchiveMode _update;
		#endregion IContent
		#region IContentReader
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
		#endregion IContentReader
		#region IContentWriter
		public System.Collections.IList Write(System.Collections.IList content)
		{
			if (!_binary)
			{
				foreach (string str in content)
				{
					_writer.WriteLine(str);
				}
			}
			else
			{
				foreach (var obj in content)
				{
					if (obj is byte)
					{
						_stream.WriteByte((byte)obj);
					}
					else if (obj is char)
					{
						_stream.WriteByte(Convert.ToByte((char)obj));
					}
				}
			}
			_writer.Flush();
			_stream.Position = 0;
			return content;
		}
		#endregion IContentWriter
		#region IContent_Shared
		public Item Open(bool IsBinary, ZipArchiveMode Update)
		{
			if (DriveInfo.isDriveLocked())
			{
				throw new Exception("Cannot open more than one file at a time.");
			}
			DriveInfo.LockDrive();
			Lock = true;
			_binary = IsBinary;
			_update = Update;
				
			// Get the LockEntry
			LockEntry = DriveInfo.OpenReadWrite(this.FullPath, Update);

			// Set Streams
			_zstream = LockEntry.Open();
			_stream = new MemoryStream();
			_zstream.CopyTo(_stream);
			_stream.Position = 0;
			_reader = new StreamReader(_stream);
			_writer = new StreamWriter(_stream);
			return this;
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
			if (_zstream != null)
			{
				_zstream.Close();
			}
			Lock = false;
			LockEntry = null;
			if (!DriveInfo.isDriveLocked())
			{
				DriveInfo.UnlockDrive();
			}
		}
		public void Dispose()
		{
			if (_reader != null)
			{
				_reader.Dispose();
				_reader = null;
			}
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}
			if (_stream != null)
			{
				_stream.Dispose();
				_stream = null;
			}
			if (_zstream != null)
			{
				_zstream.Dispose();
				_zstream = null;
			}
		}
		public void Seek(long offset, SeekOrigin origin)
		{
			if (_stream.CanSeek)
			{
				_stream.Seek(offset, origin);
			}
		}
		#endregion IContent_Shared


	}
}
/*
	public class ZipFileStream : IContentReader, IContentWriter
	{








	}
* */
