using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		
		public IEnumerable<ZipFileItemInfo> GetItem()
        {
            ZipArchive zipArchive = LockArchive(ZipFileProviderStrings.DriveGetChildItems);

            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
            {
                yield return new ZipFileItemInfo(zipArchiveEntry, this);
            }

            UnlockArchive(ZipFileProviderStrings.DriveGetChildItems);
        }

        public IEnumerable<ZipFileItemInfo> GetItem(string path)
        {
            IEnumerable<ZipFileItemInfo> results = GetItem();

			path = Path.TrimEndingDirectorySeparator(path).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);

            foreach (ZipFileItemInfo item in results)
            {
                if (wildcardPattern.IsMatch(Path.TrimEndingDirectorySeparator( item.FullArchiveName )))
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<ZipFileItemInfo> GetItem(string path, bool directory, bool file)
        {
            IEnumerable<ZipFileItemInfo> results = GetItem(path);

            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);

            path = path.TrimStart(Path.AltDirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

            foreach (ZipFileItemInfo item in results)
            {
                if ( Path.GetDirectoryName(path) != Path.GetDirectoryName( Path.TrimEndingDirectorySeparator(item.FullArchiveName) ) )
                {
                    continue;
                }

                if ((directory && item.IsContainer) || (file && !item.IsContainer))
                {
                    yield return item;
                }

            }
        }

		public bool ItemExists(string path)
		{
			// Return true if either condition is met.
			return ItemExists(path, false) || ItemExists(path, true);
		}

        public bool ItemExists(string path, bool directory)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			
            List<ZipFileItemInfo> items = GetItem().ToList();

            foreach (ZipFileItemInfo i in items)
            {
                if (!directory && (path == i.FullArchiveName))
                {
                    return true;
                }
				
                if (directory && (Path.TrimEndingDirectorySeparator(path) == Path.TrimEndingDirectorySeparator(i.FullArchiveName)))
                {
                    return true;
                }
            }
            return false;
        }

		public bool IsItemContainer(string path)
		{
			return ItemExists(path, true);
		}
    }

}