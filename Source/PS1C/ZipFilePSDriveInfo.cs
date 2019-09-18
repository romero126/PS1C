using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO;
using System.IO.Compression;

namespace Microsoft.PowerShell.Commands
{

	public class ZipFilePSDriveInfo : PSDriveInfo
	{
		internal ZipArchive Archive {
			get;
			private set;
		}
		private Dictionary<string, System.IO.Stream> _streamsInUse;

		//internal bool IsStreamInUse()
		//internal void OpenStream()
		//internal void CloseStream()
		
		//internal Stream PullStream() // Note this should not be used

		public List<string> _zipArchiveEntries = new List<string>();
		public ZipArchive LockArchive(string entry)
		{
			if (_zipArchiveEntries.Contains(entry))
			{
				throw new Exception("Cannot open file it is already open in another process");
			}
			_zipArchiveEntries.Add(entry);
			
			if (Archive == null)
			{
				Archive = ZipFile.Open(Root, ZipArchiveMode.Update);
			}

			return Archive;
		}

		public void UnlockArchive(string entry)
		{
			if (!_zipArchiveEntries.Contains(entry))
			{
				throw new Exception("Cannot unlock stream it doesnt exist");
			}

			_zipArchiveEntries.Remove(entry);

			if (_zipArchiveEntries.Count == 0)
			{
				Archive.Dispose();
				Archive = null;
				GC.Collect();
			}
		}
		internal bool IsStreamInUse()
		{
			if (Archive != null)
			{
				return true;
			}
			return false;
		}
		public int ActiveHandles {
			get {
				return _zipArchiveEntries.Count;
			}
		}

	    /// <summary>
	    /// Initializes a new instance of the AccessDBPSDriveInfo class.
	    /// The constructor takes a single argument.
	    /// </summary>
	    /// <param name="driveInfo">Drive defined by this provider</param>
        public ZipFilePSDriveInfo(PSDriveInfo driveInfo) : base(driveInfo)
		{
			Archive = ZipFile.Open(driveInfo.Root, ZipArchiveMode.Update);
		}
		
		
    }

}