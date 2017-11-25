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
	//public class Provider : FileSystemProvider, IContentCmdletProvider
	public class Provider : NavigationCmdletProvider, IContentCmdletProvider
	//NavigationCmdletProvider, ItemCmdletProvider
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
			
			//archive = System.IO.Compression.ZipFile.Open(drive.Root);
			return new DriveInfo(drive);
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
			List<object> FolderItems = GetZipFileItems(path, recurse);

			//PSObject r = SerializeObject(new Zip.Directory());
			//WriteItemObject(r, path, true);
			foreach (object i in FolderItems)
			{
				PSObject r = SerializeObject(i);
				
				WriteItemObject(r, path, true);
			}
			
		}

		#endregion NavigationCmdletProvider
		#region IContentCmdletProvider
		public IContentWriter GetContentWriter(string path)
		{
			//Todo: Cleanup Content Reader / Writer
			if (PathIsDirectory(path))
			{
				// ("Directories have no content", path, ErrorCategory.InvalidOperation);
				throw new Exception("Directories have no content");
			}
			Zip.Item v = GetZipFileItem(path);
			if (v != null)
			{
				return v.Open(false, ZipArchiveMode.Read);
			}
			return null;
		}
		public IContentReader GetContentReader(string path)
		{
			//Todo: Cleanup Content Reader / Writer
			if (PathIsDirectory(path))
			{
				// ("Directories have no content", path, ErrorCategory.InvalidOperation);
				throw new Exception("Directories have no content");
			}
			
			Zip.Item v = GetZipFileItem(path);

			if (v != null) {
				return v.Open(false, ZipArchiveMode.Read);
			}
			
			return null;
		}
		public object GetContentReaderDynamicParameters(string path)
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
		#region ItemCmdletProvider
		protected override void InvokeDefaultAction(string path)
		{
			//WriteHost("I was called");
			base.InvokeDefaultAction(path);
		}

		#endregion ItemCmdletProvider
		#region FileSystem
		protected override void RemoveItem(string path, bool recurse)
		{
			//WriteHost("RemoveItem Called");
			base.RemoveItem(path, recurse);
		}

		#endregion FileSystem

		#region Helpers
		private PSObject SerializeObject(object obj) {
			PSObject result = new PSObject(obj);
			PSPropertySet display = new PSPropertySet("DefaultDisplayPropertySet", new[] { "LastWriteTime", "Length", "Name", "FullName" });
			PSMemberSet mi = new PSMemberSet("PSStandardMembers", new[] { display });
			result.Members.Add(mi);
			return result;
		}

		private bool PathIsDrive(string path) {
			if (String.IsNullOrEmpty(path.Replace(PSDriveInfo.Root, "")) || String.IsNullOrEmpty(path.Replace(PSDriveInfo.Root + "\\", "")))
				return true;
			else
				return false;
		}
		private bool PathIsFile(string path) {
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			foreach (Zip.Item entry in (PSDriveInfo as DriveInfo).FileList) {
				if (entry.FullPath.ToUpper() == pwd) {
					return true;
				}
			}
			return false;
		}
		private bool PathIsDirectory(string path)
		{
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			foreach (Zip.Item entry in (PSDriveInfo as DriveInfo).FileList) {
				if (entry.FullPath.ToUpper().StartsWith(pwd) && entry.FullPath.ToUpper() != pwd)
				{
					return true;
				}
			}
			return false;
		}
		private Zip.Item GetZipFileItem(string path)
		{
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper();
			foreach (Zip.Item entry in (PSDriveInfo as DriveInfo).FileList)
			{
				if (entry.FullPath.ToUpper() == pwd)
				{
					return entry;
				}
			}
			return null;
		}
		private List<object> GetZipFileItems(string path, bool recurse)
		{
			string pwd = path.Replace(this.PSDriveInfo.Root + "\\", "").Replace("\\", "/").ToUpper() + "/";
			List<object> foldersAndFiles = new List<object>();
			List<object> folders = new List<object>();
			List<object> files = new List<object>();
			DriveInfo driveinfo = PSDriveInfo as DriveInfo;
			foreach (Zip.Item entry in driveinfo.FileList)
			{
				string n = entry.FullPath.ToUpper();
				if (n.StartsWith(pwd)) {
					if (pwd != "" && n.Length >= pwd.Length)
						n = n.Remove(0, pwd.Length);
					if (!n.Contains("/")) {
						files.Add(entry);
					} else {
						string name = n.Split('/')[0] + "/";
						Zip.Directory obj = new Zip.Directory(entry.ArchivePath, entry.Drive, pwd, name);
						bool found = false;
						foreach (Zip.Directory i in folders) {
							if (i.FullPath == obj.FullPath) {
								found = true;
							}
						}
						if (!found)
							folders.Add(obj);
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
 