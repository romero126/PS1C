using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
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
                return String.Format("{0}:\\{1}", Drive.Name, archiveEntry.FullName);
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
                return archiveEntry.Name;
            }
        }
        private ZipArchiveEntry archiveEntry;

        public ZipArchive Archive {
            get {
                return archiveEntry.Archive;
            }
        }
        public ZipFileItemInfo(ZipArchiveEntry item, PSDriveInfo drive)
        {
            Drive = drive;
            archiveEntry = item;
        }

        
        //Methods
        public StreamWriter AppendText()
        {
            return new StreamWriter( OpenWrite() );
        }

        //CopyTo                    Method         System.IO.FileInfo CopyTo(string destFileName), System.IO.FileInfo CopyTo(s...
        
        //Create                    Method         System.IO.FileStream Create()
        
        //CreateObjRef              Method         System.Runtime.Remoting.ObjRef CreateObjRef(type requestedType)
        
        //CreateText                Method         System.IO.StreamWriter CreateText()
        public StreamWriter CreateText()
        {
            return new StreamWriter( OpenWrite() );
        }
        
        //Decrypt                   Method         void Decrypt()
        
        //Delete                    Method         void Delete()
        public override void Delete()
        {

            using (ZipArchive zipArchive = ZipFile.Open(Drive.Root, ZipArchiveMode.Update))
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(archiveEntry.FullName);
                zipArchiveEntry.Delete();
            }

        }
        
        //Encrypt                   Method         void Encrypt()
        
        //Equals                    Method         bool Equals(System.Object obj)
        
        //GetAccessControl          Method         System.Security.AccessControl.FileSecurity GetAccessControl(), System.Secur...
        
        //GetHashCode               Method         int GetHashCode()
        
        //GetLifetimeService        Method         System.Object GetLifetimeService()
        
        //GetObjectData             Method         void GetObjectData(System.Runtime.Serialization.SerializationInfo info, Sys...
        
        //GetType                   Method         type GetType()
        
        //InitializeLifetimeService Method         System.Object InitializeLifetimeService()
        
        //MoveTo                    Method         void MoveTo(string destFileName)

        //Open                      Method         System.IO.FileStream Open(System.IO.FileMode mode), System.IO.FileStream Op...
        public ZipFileItemStream Open(FileMode mode)
        {
            
            return new ZipFileItemStream(Drive.Root, archiveEntry.FullName, mode);
        }

        public ZipFileItemStream Open(FileMode mode, FileAccess access)
        {
            throw new Exception("Not Implemented Exception");
        }
        public ZipFileItemStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new Exception("Not Implemented Exception");
        }
        public ZipFileItemStream OpenRead()
        {
            return Open(FileMode.Open);
        }

        public StreamReader OpenText()
        {
            throw new Exception("Not Implemented Exception");
        }
        public ZipFileItemStream OpenWrite()
        {
            return Open(FileMode.Append);
        }

        //Refresh                   Method         void Refresh()
        //Replace                   Method         System.IO.FileInfo Replace(string destinationFileName, string destinationBa...
        //SetAccessControl          Method         void SetAccessControl(System.Security.AccessControl.FileSecurity fileSecurity)
    
        internal void ClearContent()
        {
            ZipFileItemStream fileStream = Open(FileMode.Append);
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.SetLength(0);
            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
        }


    }
}