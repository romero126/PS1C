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

	[CmdletProvider("PS1C", ProviderCapabilities.ShouldProcess)]
	public class Provider : NavigationCmdletProvider, IContentCmdletProvider
	{

		/*
		//
		DriveCmdletProvider, 
		#region DriveCmdletProvider
		protected override object NewDriveDynamicParameters()
		{
			return base.NewDriveDynamicParameters();
		}
		#endregion DriveCmdletProvider
		*/
		#region NavigationCmdletProvider
		private ZipArchive archive;
		protected override bool IsValidPath(string path)
		{
			return true;
		}
		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			if (!File.Exists(drive.Root))
			{
				throw new Exception("File does not exist");
			}
			archive = System.IO.Compression.ZipFile.OpenRead(drive.Root);
			return base.NewDrive(drive);
		}

		protected override bool ItemExists(string path)
		{
			if (PathIsDrive(path))
				return true;
			if (PathIsFile(path))
				return true;
			if (PathIsDirectory(path))
				return true;
			return false;
		}
		protected override bool IsItemContainer(string path)
		{
			return true;
		}

		protected override void GetChildItems(string path, bool recurse)
		{
			//WriteHost(path + PathIsDrive(path).ToString());
			List<object> FolderItems = GetZipFileItems(path, recurse);
			foreach (ZipFileObject i in FolderItems)
			{
				//WriteItemObject(i, path, true);
				WriteItemObject(i.Name, path, true);
			}
		}
		#endregion NavigationCmdletProvider
		#region IContentCmdletProvider
		public IContentReader GetContentReader(string path)
		{
			//GetZipFileItem(path)
			if (PathIsDirectory(path))
			{

				// ("Directories have no content", path, ErrorCategory.InvalidOperation);
				throw new Exception("Directories have no content");
			}

			ZipFileObject v = GetZipFileItem(path);
			if (v != null) {
				ZipFileStream obj = new ZipFileStream(v._archive, false);
				return obj;
			}
			return null;
		}
		public object GetContentReaderDynamicParameters(string path)
		{
			return null;
		}
		public IContentWriter GetContentWriter(string path)
		{
			return null;
		}
		public object GetContentWriterDynamicParameters(string path)
		{
			return null;
		}
		public void ClearContent(string path)
		{
			WriteVerbose($"SPOProvider::ClearContent (path = ’{path}’)");
		}
		public object ClearContentDynamicParameters(string path)
		{
			return null;
		}
		#endregion IContentCmdletProvider
		#region Helpers

		private bool PathIsDrive(string path) {
			if (String.IsNullOrEmpty(path.Replace(PSDriveInfo.Root, "")) || String.IsNullOrEmpty(path.Replace(PSDriveInfo.Root + "\\", "")))
				return true;
			else
				return false;
		}
		private bool PathIsFile(string path) {
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(this.PSDriveInfo.Root)) {
				foreach (ZipArchiveEntry entry in archive.Entries) {
					if (entry.FullName.ToUpper() == pwd)
					{
						return true;
					}
				}
			}
			return false;
		}
		private bool PathIsDirectory(string path)
		{
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(this.PSDriveInfo.Root))
			{
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (entry.FullName.ToUpper().StartsWith(pwd) && entry.FullName.ToUpper() != pwd)
					{
						return true;
					}
				}
			}
			return false;
		}
		private ZipFileObject GetZipFileItem(string path)
		{
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(this.PSDriveInfo.Root);
			foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (entry.FullName.ToUpper() == pwd)
					{
						return new ZipFileObject(entry, PSDriveInfo.Name, pwd, entry.Name, false);
					}
				}
			
			return null;
		}
		private List<object> GetZipFileItems(string path, bool recurse)
		{
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			List<object> foldersAndFiles = new List<object>();
			List<object> folders = new List<object>();
			List<object> files = new List<object>();
			using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(this.PSDriveInfo.Root))
			{
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					string n = entry.FullName.ToUpper();
					if (n.StartsWith(pwd)) {
						if (pwd != "")
							n = n.Substring(pwd.Length, (n.Length - pwd.Length)).Remove(0, 1);
						if (!n.Contains("/")) {
							files.Add(new ZipFileObject(entry, PSDriveInfo.Name, pwd, entry.Name, false));
							
						} else {
							string name = n.Split('/')[0] + "/";
							
							ZipFileObject obj = new ZipFileObject(null, PSDriveInfo.Name, pwd, name, true);
							bool found = false;
							foreach (ZipFileObject i in folders)
								if (i.Name == name)
									found = true;
							if (!found)
								folders.Add(obj);
						}
					}
				}
			}
			foreach (object i in folders)
				foldersAndFiles.Add(i);
			foreach (object i in files)
				foldersAndFiles.Add(i);
			return foldersAndFiles;
		}

		private void WriteHost(string output) {
			InformationRecord record = new InformationRecord(output, output);
			WriteInformation(record);
			WriteItemObject(output, "Message", false);
		}
		
		#endregion Helpers
	}
}
 