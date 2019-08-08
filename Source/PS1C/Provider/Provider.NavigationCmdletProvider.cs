using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO;
using System.IO.Compression;

namespace PS1C
{
    //public class Provider : FileSystemProvider, IContentCmdletProvider
    public partial class Provider : NavigationCmdletProvider, IContentCmdletProvider
    {
        #region NavigationCmdletProvider

		protected override bool ItemExists(string path)
		{
            return true;
		}
		protected override bool IsItemContainer(string path)
		{
            if ( path == (PSDriveInfo.Root + "\\" ))
            {
                // TODO: Allow full walking of zip file.
                return true;
            }
			return false;
		}


		protected override void GetChildItems(string path, bool recurse)
		{
            using (ZipArchive archive = ZipFile.OpenRead(PSDriveInfo.Root)) {
				foreach (ZipArchiveEntry i in archive.Entries) {
                    Archive.FileInfo OutputItem = new Archive.FileInfo(i, PSDriveInfo);
                    WriteItemObject(OutputItem, OutputItem.FullName, false);
				}
			}
		}

        protected override void GetItem(string path)
		{
            string filepath = path.Remove(0, PSDriveInfo.Root.Length+1);

            try {
                using (ZipArchive archive = ZipFile.OpenRead(PSDriveInfo.Root)) {
                    ZipArchiveEntry entry = archive.GetEntry(filepath);
                    Archive.FileInfo OutputItem = new Archive.FileInfo(entry, PSDriveInfo);
                    WriteItemObject(OutputItem, OutputItem.FullName, false);
                }
            }
            catch (Exception e) {
                throw e;
            }
		}
        internal Archive.FileInfo GetItemHelper(string path)
		{
            string filepath = path.Remove(0, PSDriveInfo.Root.Length+1);

            try {
                using (ZipArchive archive = ZipFile.OpenRead(PSDriveInfo.Root)) {
                    ZipArchiveEntry entry = archive.GetEntry(filepath);
                    Archive.FileInfo OutputItem = new Archive.FileInfo(entry, PSDriveInfo);
                    return OutputItem;
                }
            }
            catch (Exception e) {
                throw e;
            }
		}
        #endregion
    }
}