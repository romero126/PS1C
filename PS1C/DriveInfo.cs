using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

using System.Management.Automation;
namespace PS1C
{
	internal class DriveInfo : PSDriveInfo
	{
		internal ZipArchive _archive;
		public List<Zip.Item> FileList;
		internal PSDriveInfo driveinfo;

		internal ZipArchive _ReadOnlyArchive;
		internal ZipArchive _UpdateArchive;
		
		public DriveInfo(PSDriveInfo drive) : base(drive)
		{
			using (ZipArchive archive = ZipFile.OpenRead(drive.Root)) {
				FileList = new List<Zip.Item>();
				foreach (ZipArchiveEntry i in archive.Entries) {
					FileList.Add(new Zip.Item(i, this));
				}
			}
			//this._archive = System.IO.Compression.ZipFile.OpenRead(drive.Root);
		}
		
		public bool isDriveLocked() {
			foreach (Zip.Item i in FileList)
			{
				if (i.isLocked)
					return true;
			}
			//_archive.Dispose();
			return false;
		}
		public void LockDrive() {
			
		}
		public void UnlockDrive() {
			if (!isDriveLocked()) {
				_archive.Dispose();
			}
		}
		public ZipArchiveEntry OpenReadWrite(string Name, ZipArchiveMode Update) {
			this._archive = System.IO.Compression.ZipFile.Open(this.Root, Update);
			return _archive.GetEntry(Name);
		}
		~DriveInfo() {
			//Cleanup
			//this._archive.Dispose();
		}
	}
}
