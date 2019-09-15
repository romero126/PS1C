using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
    #region ZipFileItemInfo
    public class ZipFileItemInfo : System.IO.FileSystemInfo
    {
        //Public Extension info
        
        //public DateTime        CreationTime;                   // {get;set;}
        //public DateTime        CreationTimeUtc;                // {get;set;}
        public PSDriveInfo Drive {
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
        public override bool Exists {
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
                return String.Format("{0}:\\{1}", Drive.Name, archiveEntry.FullName).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

        public string FullArchiveName {
            get {
                return archiveEntry.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
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
                return archiveEntry.LastWriteTime.DateTime;
            }
            set {
                // Todo: Fix writetime so it updates the archive as well
                archiveEntry.LastWriteTime = new DateTimeOffset(value);
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
                return archiveEntry.Length;
            }
        }
        public long CompressedLength {
            get {
                return archiveEntry.CompressedLength;
            }
        }
        public override string Name {
            get {
                if (IsContainer)
                {
                    return Path.GetFileName(Path.TrimEndingDirectorySeparator(archiveEntry.FullName));
                }
                return archiveEntry.Name;
            }
        }
        private ZipArchiveEntry archiveEntry;

        public ZipArchive Archive {
            get {
                if (archiveEntry.Archive.Entries.Count == 0)
                {
                    return null;
                }
                return archiveEntry.Archive;
            }
        }

        public FileInfo FileSystemContainer {
            get {
                return new FileInfo(Drive.Root);
            }
        }

        public bool IsContainer {
            get {
                return Path.EndsInDirectorySeparator(archiveEntry.FullName);
            }
        }

        public ZipFileItemInfo(ZipArchiveEntry item, PSDriveInfo drive)
        {
            Drive = drive;
            archiveEntry = item;
        }

        public ZipFileItemInfo(PSDriveInfo drive, string path) : this(drive, path, false)
        {

        }

        public ZipFileItemInfo(PSDriveInfo drive, string path, bool createEntry)
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

            using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Update))
            {
                // Quick Archive
                archiveEntry = zipArchive.GetEntry(path);

                if (archiveEntry == null)
                {
                    if (createEntry == true)
                    {
                        // Create an entry if not exists
                        zipArchive.CreateEntry(path);
                        archiveEntry = zipArchive.GetEntry(path);
                        if (archiveEntry == null)
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
        }
        
        // Search 
        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(PSDriveInfo drive, string path, bool directory, bool file)
        {
            path = path.TrimStart(Path.AltDirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {

                    if ( Path.GetDirectoryName(path) != Path.GetDirectoryName( Path.TrimEndingDirectorySeparator(zipArchiveEntry.FullName) ) )
                    {
                        continue;
                    }

                    if (wildcardPattern.IsMatch(Path.TrimEndingDirectorySeparator( zipArchiveEntry.FullName )))
                    {
                        bool isDirectory = Path.EndsInDirectorySeparator(zipArchiveEntry.FullName);

                        if ((directory && isDirectory) || (file && !isDirectory))
                        {
                            yield return new ZipFileItemInfo(zipArchiveEntry, drive);
                        }

                    }
                }
            }
        }
        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(PSDriveInfo drive)
        {
            using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    yield return new ZipFileItemInfo(zipArchiveEntry, drive);
                }
            }
        }

        public static IEnumerable<ZipFileItemInfo> GetZipFileItemInfo(PSDriveInfo drive, string path)
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
        public static ZipFileItemInfo[] GetFileItemInfo2(PSDriveInfo drive, string path)
        {
            List<ZipFileItemInfo> results = new List<ZipFileItemInfo>();
            
            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            
            using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Read))
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
        public static bool ItemExists(PSDriveInfo drive, string path, bool directory)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            IEnumerable<ZipFileItemInfo> items = GetZipFileItemInfo(drive);

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
        

        
        public override void Delete()
        {

            using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(archiveEntry.FullName);
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
                destFileName = $"{destFileName}{archiveEntry.Name}";
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

            using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {   
                ZipArchiveEntry thisEntry = zipArchive.GetEntry(archiveEntry.FullName);

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
            using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {
                ZipArchiveEntry thisEntry = zipArchive.GetEntry(archiveEntry.FullName);
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
        public ZipFileItemStream Open(FileMode mode)
        {
            return new ZipFileItemStream(Drive.Root, archiveEntry.FullName, mode);
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
            return Open(FileMode.Open);
        }

        public StreamReader OpenText()
        {
            return new StreamReader(Open(FileMode.Open));
        }
        public ZipFileItemStream OpenWrite()
        {
            return Open(FileMode.Append);
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

        // Gets the Directory Information Stack
        public static IEnumerable<ZipFileItemInfo> GetDirectories(PSDriveInfo drive, string path, int recurse, bool directory)
        {
            List<string> DirList = new List<string>();

            WildcardPattern wildcardPattern = WildcardPattern.Get(path, WildcardOptions.CultureInvariant | WildcardOptions.Compiled);

            using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    if (wildcardPattern.IsMatch(zipArchiveEntry.FullName))
                    {
                        yield return new ZipFileItemInfo(zipArchiveEntry, drive);
                    }
                }
            }
        }

        public static void buildFolderPaths(PSDriveInfo drive)
        {

            using (ZipArchive zipArchive = ZipFile.Open(drive.Root, ZipArchiveMode.Update))
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