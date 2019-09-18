using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.Commands
{
    #region ZipFileItemInfo
    public class ZipFileItemInfo
    {
        //Public Extension info
        
        //public DateTime        CreationTime;                   // {get;set;}
        //public DateTime        CreationTimeUtc;                // {get;set;}
        public ZipFilePSDriveInfo Drive {
            get;
            private set;
        }

        public DirectoryInfo Directory;                      // {get;}

        public string DirectoryName
        {
            get {
                if (IsContainer)
                {
                    return Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(FullName));
                }

                return Path.GetDirectoryName(FullName);
            }
        }

        public bool Exists {
            get {
                return true;
            }
        }

        public object Crc32 {
            get {
                return null; //archiveEntry.Crc32;
            }
        }

        public string Extension {
            get {
                return Path.GetExtension(FullName);
            }
        }

        public string BaseName {
            get {
                return Path.GetFileNameWithoutExtension(FullName);
            }
        }

        public string FullName {
            get {
                return String.Format("{0}:\\{1}", Drive.Name, ArchiveEntry.FullName).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

        public string FullArchiveName {
            get {
                return ArchiveEntry.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

        public bool IsReadOnly
        {
            get {
                return false;
            }
            set {

            }
        }

        //public DateTime        LastAccessTime;                 // {get;set;}
        //public DateTime        LastAccessTimeUtc;              // {get;set;}
        public DateTime LastWriteTime
        {
            get {
                return ArchiveEntry.LastWriteTime.DateTime;
            }
            set {
                // Todo: Fix writetime so it updates the archive as well
                ArchiveEntry.LastWriteTime = new DateTimeOffset(value);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get {
                return this.LastWriteTime.ToUniversalTime();
            }
            set {
                this.LastWriteTime = value.ToLocalTime();
            }
        }

        public long Length {
            get {
                return ArchiveEntry.Length;
            }
        }

        public long CompressedLength {
            get {
                return ArchiveEntry.CompressedLength;
            }
        }

        public string Name {
            get {
                if (IsContainer)
                {
                    return Path.GetFileName(Path.TrimEndingDirectorySeparator(ArchiveEntry.FullName));
                }
                return ArchiveEntry.Name;
            }
        }

        public ZipArchive Archive {
            get {
                if (ArchiveEntry.Archive.Entries.Count == 0)
                {
                    return null;
                }
                return ArchiveEntry.Archive;
            }
        }

        public ZipArchiveEntry ArchiveEntry {
            get;
            private set;

        }

        public FileInfo FileSystemContainer {
            get {
                return new FileInfo(Drive.Root);
            }
        }

        public bool IsContainer {
            get {
                return Path.EndsInDirectorySeparator(ArchiveEntry.FullName);
            }
        }

        public ZipFileItemInfo(ZipArchiveEntry item, ZipFilePSDriveInfo drive)
        {
            Drive = drive;
            ArchiveEntry = item;
        }

        public ZipFileItemInfo(ZipFilePSDriveInfo drive, string path) : this(drive, path, false)
        {

        }

        public ZipFileItemInfo(ZipFilePSDriveInfo drive, string path, bool createEntry)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }

            Drive = drive;

            if (path.StartsWith(Drive.Name))
            {
                path = Path.GetRelativePath(Drive.Name + ":\\", path);
            }

            if (path.Contains( Path.VolumeSeparatorChar))
            {
                throw PSTraceSource.NewArgumentException(path);
            }
            
            try {
                ZipArchive zipArchive = drive.LockArchive(ZipFileProviderStrings.DriveGetItem);
                ArchiveEntry = zipArchive.GetEntry(path);

                if (ArchiveEntry == null)
                {
                    if (createEntry == true)
                    {
                        // Create an entry if not exists
                        zipArchive.CreateEntry(path);
                        ArchiveEntry = zipArchive.GetEntry(path);

                        if (ArchiveEntry == null)
                        {
                            throw new IOException(FileSystemProviderStrings.PermissionError);
                        }

                    }
                    else
                    {
                        string error = StringUtil.Format(FileSystemProviderStrings.ItemNotFound, path);
                        throw new IOException(error);
                    }

                }

            }
            catch(Exception e) {
                throw e;
            }
            finally {
                drive.UnlockArchive(ZipFileProviderStrings.DriveGetItem);
            }

        }
        
        // Search 
//        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(ZipFilePSDriveInfo drive, string path, bool directory, bool file)
//        {
//            path = path.TrimStart(Path.AltDirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
//
//            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
//
//            ZipArchive zipArchive = drive.Archive;
//            //using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Read))
//            {
//                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
//                {
//
//                    if ( Path.GetDirectoryName(path) != Path.GetDirectoryName( Path.TrimEndingDirectorySeparator(zipArchiveEntry.FullName) ) )
//                    {
//                        continue;
//                    }
//
//                    if (wildcardPattern.IsMatch(Path.TrimEndingDirectorySeparator( zipArchiveEntry.FullName )))
//                    {
//                        bool isDirectory = Path.EndsInDirectorySeparator(zipArchiveEntry.FullName);
//
//                        if ((directory && isDirectory) || (file && !isDirectory))
//                        {
//                            yield return new ZipFileItemInfo(zipArchiveEntry, drive);
//                        }
//
//                    }
//                }
//            }
//        }
        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(ZipFilePSDriveInfo drive, string path, bool directory, bool file)
        {
            IEnumerable<ZipFileItemInfo> results = GetZipFileItemInfo(drive, path);

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
        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(ZipFilePSDriveInfo drive)
        {
            ZipArchive zipArchive = drive.LockArchive(ZipFileProviderStrings.DriveGetChildItems);

            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
            {
                yield return new ZipFileItemInfo(zipArchiveEntry, drive);
            }

            drive.UnlockArchive(ZipFileProviderStrings.DriveGetChildItems);
        }

        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(ZipFilePSDriveInfo drive, string path)
        {
            IEnumerable<ZipFileItemInfo> results = GetZipFileItemInfo(drive);
            path = path.TrimStart(Path.AltDirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);

            foreach (ZipFileItemInfo item in results)
            {
                if (wildcardPattern.IsMatch(Path.TrimEndingDirectorySeparator( item.FullArchiveName )))
                {
                    yield return item;
                }
            }

        }

        // Simplex search
        public static ZipFileItemInfo[] GetFileItemInfo2(ZipFilePSDriveInfo drive, string path)
        {

            Console.WriteLine("Old Code Please Delete me");
            List<ZipFileItemInfo> results = new List<ZipFileItemInfo>();
            
            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            
            ZipArchive zipArchive = (drive as ZipFilePSDriveInfo).Archive;
            //using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {

                    // Skip if Entry is a directory and not in the same path as Path.
                    if ( Path.GetDirectoryName(path) != Path.GetDirectoryName( Path.TrimEndingDirectorySeparator(zipArchiveEntry.FullName) ) )
                    {
                        continue;
                    }

                    if (wildcardPattern.IsMatch(zipArchiveEntry.FullName))
                    {
                        results.Add(new ZipFileItemInfo(zipArchiveEntry, drive) );
                    }
                }
            }

            if (results.Count == 0)
            {
                return null;
            }

            return results.ToArray();
        }

        public static bool ItemExists(ZipFilePSDriveInfo drive, string path, bool directory)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            List<ZipFileItemInfo> items = GetZipFileItemInfo(drive).ToList();

            foreach (ZipFileItemInfo i in items)
            {
                if (path == i.FullArchiveName)
                {
                    return true;
                }
                if (directory && (path == Path.TrimEndingDirectorySeparator(i.FullArchiveName)))
                {
                    return true;
                }
            }
            return false;

        }

        public StreamWriter AppendText()
        {
            return new StreamWriter( OpenWrite() );
        }

        public void CopyTo(string destFileName)
        {
            CopyTo(destFileName, false, false);
        }
        
        public void CopyTo(string destFileName, bool overwrite)
        {
            CopyTo(destFileName, false,  overwrite);
        }
        //Create                    Method         System.IO.FileStream Create()
        
        //CreateObjRef              Method         System.Runtime.Remoting.ObjRef CreateObjRef(type requestedType)        
        public StreamWriter CreateText()
        {
            return new StreamWriter( OpenWrite() );
        }
        
        public void Delete()
        {
            using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(ArchiveEntry.FullName);
                zipArchiveEntry.Delete();
            }

        }

        public void Decrypt()
        {
            throw new NotImplementedException();
        }
        
        public void Encrypt()
        {
            throw new NotImplementedException();
        }

        
        //GetAccessControl          Method         System.Security.AccessControl.FileSecurity GetAccessControl(), System.Secur...
        
        //GetHashCode               Method         int GetHashCode()
        
        //GetLifetimeService        Method         System.Object GetLifetimeService()
        
        //GetObjectData             Method         void GetObjectData(System.Runtime.Serialization.SerializationInfo info, Sys...
        
        //GetType                   Method         type GetType()
        
        //InitializeLifetimeService Method         System.Object InitializeLifetimeService()
        

        public void MoveTo(string destFileName)
        {
            CopyTo(destFileName, true, false);
        }

        internal void CopyTo(string destFileName, bool removeItem, bool overwrite)
        {
            // if (destFileName.Contains(Path.GetInvalidPathChars()) || destFileName.Contains(Path.GetInvalidFileNameChars())
            if (destFileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                throw new InvalidDataException("Path contains invalid characters");
            }

            // Convert Path to its proper dest path
            destFileName = destFileName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            
            // If the destination file is a folder
            // We should move/copy the item to that folder.
            // Example:
            // Move-Item Provider:\a\b\c\file.txt .\d\e\f
            // Will move the file to Provider:\d\e\f\file.txt
            if (destFileName.EndsWith(Path.AltDirectorySeparatorChar))
            {
                destFileName = $"{destFileName}{ArchiveEntry.Name}";
            }

            // Validate if path is filesystem
            if (Path.IsPathRooted(destFileName) && !destFileName.StartsWith(Drive.Name))
            {
                CopyToFileSystem(destFileName, removeItem, overwrite);
                return;
            }

            // Cleanup the filesystem path
            if (destFileName.StartsWith(Drive.Name))
            {
                destFileName = Path.GetRelativePath(Drive.Name, destFileName);
            }
            else if (destFileName.StartsWith(Drive.Root))
            {
                destFileName = Path.GetRelativePath(Drive.Root, destFileName);
            }

            // Throw if still contains Invalid Characters
            if (destFileName.Contains(Path.VolumeSeparatorChar))
            {
                throw new InvalidDataException($"Path must not contain {Path.VolumeSeparatorChar}");
            }

            CopyToArchive(destFileName, removeItem, overwrite);
        }
        
        internal void CopyToFileSystem(string destFileName, bool removeItem, bool overwrite)
        {
            if (File.Exists(destFileName) && !overwrite) 
            {
                throw new Exception($"The item exists '{destFileName}'");
            }

            ZipArchive zipArchive = Archive;
            //using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {   
                ZipArchiveEntry thisEntry = zipArchive.GetEntry(ArchiveEntry.FullName);

                // Todo Check if file already exists.
                thisEntry.ExtractToFile(destFileName);

                if (removeItem)
                {
                    thisEntry.Delete();
                }
            }
        }

        internal void CopyToArchive(string destFileName, bool removeItem, bool overwrite)
        {
            ZipArchive zipArchive = Archive;
            //using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {
                ZipArchiveEntry thisEntry = zipArchive.GetEntry(ArchiveEntry.FullName);
                ZipArchiveEntry newEntry = zipArchive.GetEntry(destFileName);

                // Determine if Overwrite is enabled and item exists.
                if ((overwrite == false) && (newEntry != null))
                {
                    throw new Exception($"The item exists '{destFileName}'");
                }

                if (newEntry == null) {
                    newEntry = zipArchive.CreateEntry(destFileName);
                }

                using (Stream thisStream = thisEntry.Open())
                using (Stream newStream = newEntry.Open())
                {
                    thisStream.CopyTo(newStream);
                }
                if (removeItem)
                {
                    thisEntry.Delete();
                }
            }
        }

        public ZipFileItemStream Open()
        {
            return new ZipFileItemStream(this);
        }

        public ZipFileItemStream Open(FileMode mode)
        {
            return new ZipFileItemStream(this);
        }

        public ZipFileItemStream Open(FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public ZipFileItemStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public ZipFileItemStream OpenRead()
        {
            return Open();
        }

        public StreamReader OpenText()
        {
            return new StreamReader(Open());
        }

        public ZipFileItemStream OpenWrite()
        {
            return Open();
        }

        //Refresh                   Method         void Refresh()
        //Replace                   Method         System.IO.FileInfo Replace(string destinationFileName, string destinationBa...
        //SetAccessControl          Method         void SetAccessControl(System.Security.AccessControl.FileSecurity fileSecurity)

        public string ReadToEnd()
        {
            string result;
            using (ZipFileItemStream stream = Open(FileMode.Append))
            using (StreamReader streamReader = new StreamReader(stream))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }
        
        internal void ClearContent()
        {
            ZipFileItemStream fileStream = Open(FileMode.Append);
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.SetLength(0);
            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
        }

        #region DirectoryWalker commands


        public static void buildFolderPaths(ZipFilePSDriveInfo drive)
        {

            try {
                ZipArchive zipArchive = drive.LockArchive(ZipFileProviderStrings.DriveBuildFolderPaths);

                // Generate a list of items to create
                List<string> dirList = new List<string>();
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string fullName = entry.FullName;
                    if (Path.EndsInDirectorySeparator(fullName))
                    {
                        continue;
                    }

                    fullName = Path.GetDirectoryName(fullName) + Path.AltDirectorySeparatorChar;
                    fullName = fullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (String.IsNullOrEmpty(fullName))
                    {
                        continue;
                    }
                    var paths = enumFolderPaths(fullName);

                    foreach (string path in paths)
                    {
                        if (zipArchive.GetEntry(path) == null)
                        {
                            if (!dirList.Contains(path))
                            {
                                dirList.Add(path);
                            }
                        }
                    }
                }
                
                // Generate a list of directories
                foreach (string dir in dirList)
                {
                    zipArchive.CreateEntry(dir);
                }

            }
            catch(Exception e) {
                throw e;
            }
            finally {
                drive.UnlockArchive(ZipFileProviderStrings.DriveBuildFolderPaths);
            }
            /*
            //using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Update))
            {
                // Generate a list of items to create
                List<string> dirList = new List<string>();

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    string fullName = zipArchiveEntry.FullName;

                    // Check if this is NOT a directory
                    if (!Path.EndsInDirectorySeparator(fullName))
                    {

                        fullName = (
                                Path.GetDirectoryName(fullName) + Path.AltDirectorySeparatorChar
                            ).Replace(
                                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
                        );

                        if (String.IsNullOrEmpty(fullName))
                        {
                            continue;
                        }
                        
                        var paths = enumFolderPaths(fullName);

                        foreach (string path in paths)
                        {
                            if (zipArchive.GetEntry(path) == null)
                            {
                                if (!dirList.Contains(path))
                                {
                                    dirList.Add(path);
                                }
                            }
                        }

                    }

                }

                // Generate a list of directories
                foreach (string dir in dirList)
                {
                    zipArchive.CreateEntry(dir);
                }
            }
            */

        }

        private static IEnumerable<string> enumFolderPaths(string path)
        {
            int i = 0;
            while((i = path.IndexOf(Path.AltDirectorySeparatorChar, i+1)) > -1)
            {
                yield return path.Substring(0, i+1);
            }
        }

        #endregion

    }
    #endregion ZipFileItemInfo
}