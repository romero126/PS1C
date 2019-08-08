using System;
 

namespace PS1C.Archive
{

    public class FileAttributes {

    }
    public class DirectoryInfo {
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
        DirectoryInfo   Parent;                         // {get;}
        DirectoryInfo   Root;                           // {get;}

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
    }

    public class ZipFile
    {

    }
}
