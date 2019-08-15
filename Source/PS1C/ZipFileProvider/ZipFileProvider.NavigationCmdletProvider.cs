using System;
using Microsoft.PowerShell.Commands;

using System.IO;
using System.IO.Compression;

using System.Management.Automation;
using System.Management.Automation.Provider;

using PS1C.Archive;


namespace PS1C
{
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        #region NavigationCmdletProvider

		protected override bool ItemExists(string path)
		{
            //WriteVerbose($"[TODO] ItemExists(string path) '{path}'");
            path = NormalizePath(path);

            if (path == "")
            {
                return true;
            }

            try {
                using (ZipArchive zipArchive = ZipFile.OpenRead(PSDriveInfo.Root) )
                {
                    ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(path);
                    if (zipArchiveEntry != null) {
                        return true;
                    }
                }
            }
            catch (Exception e) {
                throw e;
            }
            return false;
		}
		protected override bool IsItemContainer(string path)
		{
            path = NormalizePath(path);
            if ( path == "" )
            {
                return true;
            }
			return false;
		}

		protected override void GetChildItems(string path, bool recurse)
		{
            path = NormalizePath(path);
            //ZipFileDirectoryInfo DirInfo = new ZipFileDirectoryInfo(PSDriveInfo);
            using (ZipArchive zipArchive = ZipFile.OpenRead(PSDriveInfo.Root)) {
				foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries) {
                    ZipFileItemInfo OutputItem = new ZipFileItemInfo(zipArchiveEntry, PSDriveInfo);
                    WriteItemObject(OutputItem, OutputItem.FullName, true);
				}
			}

		}

        protected override void GetItem(string path)
		{
            string filepath = path.Remove(0, PSDriveInfo.Root.Length+1);

            try {
                using (ZipArchive archive = ZipFile.OpenRead(PSDriveInfo.Root)) {
                    ZipArchiveEntry entry = archive.GetEntry(filepath);
                    ZipFileItemInfo OutputItem = new ZipFileItemInfo(entry, PSDriveInfo);
                    WriteItemObject(OutputItem, OutputItem.FullName, false);
                }
            }
            catch (Exception e) {
                throw e;
            }
		}
        internal ZipFileItemInfo GetItemHelper(string path)
		{
            path = NormalizePath(path);

            try {
                using (ZipArchive zipArchive = ZipFile.OpenRead(PSDriveInfo.Root)) {
                    ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(path);
                    return new ZipFileItemInfo(zipArchiveEntry, PSDriveInfo);
                }
            }
            catch (Exception e) {
                throw e;
            }
		}


        internal ZipFileItemInfo NewItemHelper(string path)
        {
            path = NormalizePath(path);
            try {
                using (ZipArchive zipArchive = ZipFile.Open(PSDriveInfo.Root, ZipArchiveMode.Update))
                {
                    path = NormalizePath(path);
                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(path);

                    ZipFileItemInfo zipFileItemInfo = new ZipFileItemInfo(zipArchiveEntry, PSDriveInfo);
                    return zipFileItemInfo;
                    
                }
            }
            catch (Exception e) {
                throw e;
            }
        }
        private string NormalizePath(string path)
        {

            if (path.StartsWith(PSDriveInfo.Root))
            {
                path = path.Remove(0, PSDriveInfo.Root.Length+1);
            }
            else if (path.StartsWith($"{PSDriveInfo.Name}:") )
            {
                path = path.Remove(0, PSDriveInfo.Name.Length+1);
            }

            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return path;
        }

        #endregion

        
    }
}