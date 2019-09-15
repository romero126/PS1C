using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Provider;
using PS1C.Archive;
// ZipFile
namespace Microsoft.PowerShell.Commands
{
    public class ZipFileFileAttributes {

    }
    public class ZipFileDirectoryInfo {
        FileAttributes  Attributes;                     // {get;set;}
        DateTime        CreationTime;                   // {get;set;}
        DateTime        CreationTimeUtc;                // {get;set;}
        bool            Exists;                         // {get;}
        string          Extension;                      // {get;}
        string          FullName;                       // {get;}
        DateTime        LastAccessTime;                 // {get;set;}
        DateTime        LastAccessTimeUtc;              // {get;set;}
        DateTime        LastWriteTime;                  // {get;set;}
        DateTime        LastWriteTimeUtc;               // {get;set;}
        string          Name;                           // {get;}
        public ZipFileDirectoryInfo Parent
        {
            get;
            private set;
        }

        public ZipFileDirectoryInfo Root {
            get;
            private set;
        }

        /*



            Create                    Method         void Create(), void Create(System.Security.AccessControl.DirectorySecurity ...
            CreateObjRef              Method         System.Runtime.Remoting.ObjRef CreateObjRef(type requestedType)
            CreateSubdirectory        Method         System.IO.DirectoryInfo CreateSubdirectory(string path), System.IO.Director...
            Delete                    Method         void Delete(), void Delete(bool recursive)
            EnumerateDirectories      Method         System.Collections.Generic.IEnumerable[System.IO.DirectoryInfo] EnumerateDi...
            EnumerateFiles            Method         System.Collections.Generic.IEnumerable[System.IO.FileInfo] EnumerateFiles()...
            EnumerateFileSystemInfos  Method         System.Collections.Generic.IEnumerable[System.IO.FileSystemInfo] EnumerateF...
            Equals                    Method         bool Equals(System.Object obj)
            GetAccessControl          Method         System.Security.AccessControl.DirectorySecurity GetAccessControl(), System....
            GetDirectories            Method         System.IO.DirectoryInfo[] GetDirectories(), System.IO.DirectoryInfo[] GetDi...
            GetFiles                  Method         System.IO.FileInfo[] GetFiles(string searchPattern), System.IO.FileInfo[] G...
            GetFileSystemInfos        Method         System.IO.FileSystemInfo[] GetFileSystemInfos(string searchPattern), System...
            GetHashCode               Method         int GetHashCode()
            GetLifetimeService        Method         System.Object GetLifetimeService()
            GetObjectData             Method         void GetObjectData(System.Runtime.Serialization.SerializationInfo info, Sys...
            GetType                   Method         type GetType()
            InitializeLifetimeService Method         System.Object InitializeLifetimeService()
            MoveTo                    Method         void MoveTo(string destDirName)
            Refresh                   Method         void Refresh()
            SetAccessControl          Method         void SetAccessControl(System.Security.AccessControl.DirectorySecurity direc...
            ToString                  Method         string ToString()
        */


        // Only use if root
        #region Helper Functions
        private Dictionary<string, ZipFileDirectoryInfo> _directoryInfoManifest;
        private Dictionary<string, FileInfo> _fileInfoManifest;

        internal bool IsDirectoryExistsInTree(string path)
        {
            if (Root != this) {
                return Root.IsDirectoryExistsInTree(path);
            }
            return _directoryInfoManifest.ContainsKey(path);
        }

        internal ZipFileDirectoryInfo GetDirectoryInTree(string path)
        {
            if (Root != this) {
                return Root.GetDirectoryInTree(path);
            }
            return _directoryInfoManifest[path];
        }

        internal ZipFileDirectoryInfo AddDirectoryInTree(string path)
        {
            if (Root != this) {
                return Root.AddDirectoryInTree(path);
            }
            throw new Exception("No there yet");
        }
        #endregion

        public ZipFileDirectoryInfo(PSDriveInfo psDriveInfo)
        {
            Root = this;

            // Populate List

            using (ZipArchive zipArchive = ZipFile.OpenRead(psDriveInfo.Root)) {
				foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries) {

                    Console.WriteLine("Generating : {0}", zipArchiveEntry);
                    ZipFileItemInfo OutputItem = new ZipFileItemInfo(zipArchiveEntry, psDriveInfo);

				}
			}

        }
        public ZipFileDirectoryInfo(ZipFileDirectoryInfo root)
        {
            Root = root;
        }
        public ZipFileDirectoryInfo(ZipFileDirectoryInfo root, FileInfo file)
        {
            Root = root;
            if (root != root.Root)
            {
                throw new Exception("DirectoryInfo must have a valid root");
            }

            // Link Directories
            string path = file.DirectoryName;
            ZipFileDirectoryInfo directoryChild = null;

            if (!IsDirectoryExistsInTree(path))
            {
                // Walk up FileTree

                while (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetDirectoryName(path);
                    if (IsDirectoryExistsInTree(path))
                    {
                        break;
                    }

                    // directoryChild = new ZipFileDirectoryInfo(Root, );
                }
            }


            // Add File to Manifest
            // Add Folder to Manifest
        }


        internal void ImportPath(FileInfo file)
        {
            // Validate directory exists.
            // if (true)
            // {
            //     string path = file.DirectoryName;
            //     ZipFileDirectoryInfo directoryChild;
            //     while (!string.IsNullOrEmpty(path))
            //     {
            //         // If Path Exists break.
            //         path = Path.GetDirectoryName(path);
            //         directoryChild.Parent = new ZipFileDirectoryInfo();
            //         // If folder doesnt exist in tree
            //         // build folder.. and connect to parent
            //         string parent = Path.GetDirectoryName(path);
            //         ZipFileDirectoryInfo directoryInfo = new ZipFileDirectoryInfo();
            //         //directoryInfo.Parent = LookupDirectory(path);
            //     }
            //     //folders.Reverse();
            // }
            // Add Folder to Directory
        }


    }

}
