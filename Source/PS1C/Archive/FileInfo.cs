using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;

namespace PS1C.Archive
{
    public class FileInfo : System.IO.FileSystemInfo
    {


        //Public Extension info
        
        //public DateTime        CreationTime;                   // {get;set;}
        //public DateTime        CreationTimeUtc;                // {get;set;}
        public PSDriveInfo Drive {
            get;
            private set;
        }
        public DirectoryInfo   Directory;                      // {get;}
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
        public FileInfo(ZipArchiveEntry item, PSDriveInfo drive)
        {
            Drive = drive;
            archiveEntry = item;
        }

        
        //Methods
        //AppendText                Method         System.IO.StreamWriter AppendText()
        

        //CopyTo                    Method         System.IO.FileInfo CopyTo(string destFileName), System.IO.FileInfo CopyTo(s...
        
        //Create                    Method         System.IO.FileStream Create()
        
        //CreateObjRef              Method         System.Runtime.Remoting.ObjRef CreateObjRef(type requestedType)
        
        //CreateText                Method         System.IO.StreamWriter CreateText()
        
        //Decrypt                   Method         void Decrypt()
        
        //Delete                    Method         void Delete()
        
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
        public FileStream Open(FileMode mode)
        {
            
            return new FileStream(Drive.Root, archiveEntry.FullName, mode);
        }

        public FileStream Open(FileMode mode, FileAccess access)
        {
            // Quick map
            throw new Exception("Not Implemented Exception");
        }
        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new Exception("Not Implemented Exception");
        }

        //OpenRead                  Method         System.IO.FileStream OpenRead()
        public FileStream OpenRead()
        {
            return Open(FileMode.Open);
        }

        //OpenText                  Method         System.IO.StreamReader OpenText()
        //
        public StreamReader OpenText()
        {
            throw new Exception("Not Implemented Exception");
        }

        //OpenWrite                 Method         System.IO.FileStream OpenWrite()
        public FileStream OpenWrite()
        {
            return Open(FileMode.Append);
        }

        //Refresh                   Method         void Refresh()
        //Replace                   Method         System.IO.FileInfo Replace(string destinationFileName, string destinationBa...
        //SetAccessControl          Method         void SetAccessControl(System.Security.AccessControl.FileSecurity fileSecurity)
    
        public override void Delete()
        {
            
        }

    }
}