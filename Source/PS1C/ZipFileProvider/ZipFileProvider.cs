
using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace PS1C
{
    #region ZipFileProvider
    //[CmdletProvider(ZipFileProvider.ProviderName, ProviderCapabilities.Credentials | ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    [CmdletProvider(ZipFileProvider.ProviderName, ProviderCapabilities.ShouldProcess | ProviderCapabilities.ExpandWildcards )]

    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public const string ProviderName = "ZipFile";

        internal PSDriveInfo ZipFileDriveInfo {
            get {
                if (_psDriveInfo != null)
                {
                    return _psDriveInfo;
                }
                return PSDriveInfo;
            }
            private set {
                _psDriveInfo = value;
            }
        }

        private PSDriveInfo _psDriveInfo;


        /// <summary>
        /// Initializes a new instance of the FileSystemProvider class. Since this
        /// object needs to be stateless, the constructor does nothing.
        /// </summary>
        public ZipFileProvider()
        {

        }

        /// <summary>
        /// Converts all / in the path to \
        /// </summary>
        ///
        /// <param name="path">
        /// The path to normalize.
        /// </param>
        ///
        /// <returns>
        /// The path with all / normalized to \
        /// and resolve the path based off of its Root/Name
        /// </returns>
        private string NormalizePath(string path)
        {
            // [Bug] PSDriveInfo sometimes does not get instantiated with the provider
            // this causes stateful issues with complex providers.
            // Example Duplication of this issue
            //
            // ./<tabkey>
            // and 
            // Get-Item $FileName | Remove-Item
            //
            // Current Workaround searches all Drives with ProviderName
            // and checks relative path and overrides the path lookup.
            
            if (PSDriveInfo == null) {
                if (path.Contains(Path.VolumeSeparatorChar))
                {

                    SessionState.Drive.GetAllForProvider(ProviderName).ToList().ForEach( i => {
                        if ( (path.StartsWith(i.Root)) || (path.StartsWith(i.Name)) )
                        {
                            ZipFileDriveInfo = i;
                        }
                    });

                }
            }
            
            if (path.StartsWith(ZipFileDriveInfo.Root))
            {
                path = path.Remove(0, ZipFileDriveInfo.Root.Length+1);
            }
            else if (path.StartsWith($"{ZipFileDriveInfo.Name}:") )
            {
                path = path.Remove(0, ZipFileDriveInfo.Name.Length+2);
            }
            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return path;
        }

        /// <summary>
        /// Expand a provider path that contains wildcards to a list of provider paths that the
        /// path represents. Only called for providers that declare the ExpandWildcards capability.
        /// </summary>
        ///
        /// <param name="path">
        /// The path to expand. Expansion must be consistent with the wildcarding rules of PowerShell's WildcardPattern class.
        /// </param>
        /// 
        /// <returns>
        /// A list of provider paths that this path expands to. They must all exist.
        /// </returns>
        protected override string[] ExpandPath(string path)
        {
            path = NormalizePath(path);
            ZipFileItemInfo[] zipFileItemInfo = ZipFileItemInfo.GetFileItemInfo(ZipFileDriveInfo, path);
            return zipFileItemInfo.Select(i => i.FullName).ToArray();
        }

        /// <summary>
        /// Gets the dynamic parameters for get-childnames on the
        /// FileSystemProvider.
        /// We currently only support one dynamic parameter,
        /// "Attributes" that returns an enum evaluator for the
        /// given expression.
        /// </summary>
        ///
        /// <param name="path">
        /// If the path was specified on the command line, this is the path
        /// to the item for which to get the dynamic parameters.
        /// </param>
        ///
        /// <returns>
        /// An object that has properties and fields decorated with
        /// parsing attributes similar to a cmdlet class.
        /// </returns>
        protected override object GetChildNamesDynamicParameters(string path)
        {
            return null;
            // return new GetChildDynamicParameters();
        }

        /// <summary>
        /// Gets the dynamic parameters for get-childitems on the
        /// FileSystemProvider.
        /// We currently only support one dynamic parameter,
        /// "Attributes" that returns an enum evaluator for the
        /// given expression.
        /// </summary>
        ///
        /// <param name="path">
        /// If the path was specified on the command line, this is the path
        /// to the item for which to get the dynamic parameters.
        /// </param>
        ///
        /// <param name="recurse">
        /// Ignored.
        /// </param>
        ///
        /// <returns>
        /// An object that has properties and fields decorated with
        /// parsing attributes similar to a cmdlet class.
        /// </returns>
        protected override object GetChildItemsDynamicParameters(string path, bool recurse)
        {
            return null;
            // return new GetChildDynamicParameters();
        }

        /// <summary>
        /// Gets the dynamic parameters for Copy-Item on the FileSystemProvider.
        /// </summary>
        /// <param name="path">Source for the copy operation.</param>
        /// <param name="destination">Destination for the copy operation.</param>
        /// <param name="recurse">Whether to recurse.</param>
        /// <returns></returns>
        protected override object CopyItemDynamicParameters(string path, string destination, bool recurse)
        {
            return null;
            //return new CopyItemDynamicParameters();
        }

        #region DriveCmdletProvider members
        /// <summary>
        /// Determines if the specified drive can be mounted.
        /// </summary>
        ///
        /// <param name="drive">
        /// The drive that is going to be mounted.
        /// </param>
        ///
        /// <returns>
        /// The same drive that was passed in, if the drive can be mounted.
        /// null if the drive cannot be mounted.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// drive is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// drive root is null or empty.
        /// </exception>
		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
            // verify parameters

            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }

            if (String.IsNullOrEmpty(drive.Root))
            {
                throw PSTraceSource.NewArgumentException("drive.Root");
            }

			if (!File.Exists(drive.Root))
			{
				throw new Exception("file not found");
			}
			
            return base.NewDrive(drive);
		}

        #endregion DriveCmdletProvider members
        #region ItemCmdletProvider methods
        
        /// <summary>
        /// Retrieves the dynamic parameters required for the Get-Item cmdlet.
        /// </summary>
        /// <param name="path">The path of the file to process.</param>
        /// <returns>An instance of the FileSystemProviderGetItemDynamicParameters class that represents the dynamic parameters.</returns>
        protected override object GetItemDynamicParameters(string path)
        {
            // return new FileSystemProviderGetItemDynamicParameters();
            return null;
        }

        /// <summary>
        /// Determines if the specified path is syntactically and semantically valid.
        /// An example path looks like this
        ///     C:\WINNT\Media\chimes.wav.
        /// </summary>
        /// <param name="path">
        /// The fully qualified path to validate.
        /// </param>
        /// <returns>
        /// True if the path is valid, false otherwise.
        /// </returns>
        protected override bool IsValidPath(string path)
		{
            // Path passed should be fully qualified path.
            Console.WriteLine($"IsValidPath('{path}')");
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // Normalize the path
            path = NormalizePath(path);
            // path = EnsureDriveIsRooted(path);

            // Make sure the path is either drive rooted or UNC Path
            if (!IsAbsolutePath(path) && !Utils.PathIsUnc(path))
            {
                return false;
            }

            // Exceptions should only deal with exceptional circumstances,
            // but unfortunately, FileInfo offers no Try() methods that
            // let us check if we _could_ open the file.
            try
            {
                ZipFileItemInfo testFile = new ZipFileItemInfo(ZipFileDriveInfo, path);
            }
            catch (Exception e)
            {
                if ((e is ArgumentNullException) ||
                    (e is ArgumentException) ||
                    (e is System.Security.SecurityException) ||
                    (e is UnauthorizedAccessException) ||
                    (e is PathTooLongException) ||
                    (e is NotSupportedException))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
			return false;
		}

        /// <summary>
        /// Gets the item at the specified path.
        /// </summary>
        /// <param name="path">
        /// A fully qualified path representing a file or directory in the
        /// file system.
        /// </param>
        /// <returns>
        /// Nothing.  FileInfo and DirectoryInfo objects are written to the
        /// context's pipeline.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override void GetItem(string path)
        {

            path = NormalizePath(path);

            // Validate the argument
            bool isContainer = false;

            if (string.IsNullOrEmpty(path))
            {
                // The parameter was null, throw an exception
                throw PSTraceSource.NewArgumentException("path");
            }

            try
            {
                
                //ZipFileItemInfo result = new ZipFileItemInfo(ZipFileDriveInfo, path);
                ZipFileItemInfo[] result = ZipFileItemInfo.GetFileItemInfo(ZipFileDriveInfo, path);

                // FileSystemInfo result = GetFileSystemItem(path, ref isContainer, false);
                if (result != null)
                {
                    // Otherwise, return the item itself.
                    foreach (ZipFileItemInfo i in result) {
                        WriteItemObject(i, i.FullName, isContainer);
                    }
                    //
                    //WriteItemObject(result, path, )
                }
                else
                {
                    string error = StringUtil.Format(FileSystemProviderStrings.ItemNotFound, path);
                    Exception e = new IOException(error);
                    WriteError(new ErrorRecord(
                        e,
                        "ItemNotFound",
                        ErrorCategory.ObjectNotFound,
                        path));
                }
            }
            catch (IOException ioError)
            {
                // IOException contains specific message about the error occured and so no need for errordetails.
                ErrorRecord er = new ErrorRecord(ioError, "GetItemIOError", ErrorCategory.ReadError, path);
                WriteError(er);
            }
            catch (UnauthorizedAccessException accessException)
            {
                WriteError(new ErrorRecord(accessException, "GetItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
        }

        /// <summary>
        /// Invokes the item at the path using ShellExecute semantics.
        /// </summary>
        ///
        /// <param name="path">
        /// The item to invoke.
        /// </param>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override void InvokeDefaultAction(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            path = NormalizePath(path);

            string action = FileSystemProviderStrings.InvokeItemAction;

            string resource = StringUtil.Format(FileSystemProviderStrings.InvokeItemResourceFileTemplate, path);

            if (ShouldProcess(resource, action))
            {
                var invokeProcess = new System.Diagnostics.Process();
                invokeProcess.StartInfo.FileName = path;

                bool invokeDefaultProgram = false;


                if (IsItemContainer(path))
                {

                    // Path points to a directory. We have to use xdg-open/open on Linux/macOS.
                    invokeDefaultProgram = true;
                    path = ZipFileDriveInfo.Root;
                }
                else if (Path.GetExtension(path) == ".ps1") {
                    
                    ZipFileItemInfo[] zipFileItemInfoList = ZipFileItemInfo.GetFileItemInfo(ZipFileDriveInfo, path);
                    Object[] scriptargs = null;
                    foreach (ZipFileItemInfo zipFileItemInfo in zipFileItemInfoList)
                    {
                        string script = zipFileItemInfo.ReadToEnd();
                        ScriptBlock scriptBlock = ScriptBlock.Create(script);
                        var result = SessionState.InvokeCommand.InvokeScript(SessionState, scriptBlock, scriptargs);
                        WriteItemObject(result, zipFileItemInfo.FullName, false);
                    }
                }

                if (invokeDefaultProgram)
                {
                    const string quoteFormat = "\"{0}\"";
                                       
                    if (Platform.IsLinux) {
                        invokeProcess.StartInfo.FileName = "xdg-open";
                        invokeProcess.StartInfo.Arguments = path;
                    }
                    if (Platform.IsMacOS) {
                        invokeProcess.StartInfo.FileName = "open";
                        invokeProcess.StartInfo.Arguments = path;
                    }
                    if (Platform.IsWindows)
                    {
                        // Use ShellExecute when it's not a headless SKU
                        // 
                        invokeProcess.StartInfo.UseShellExecute = Platform.IsWindowsDesktop;
                        invokeProcess.StartInfo.FileName = path;
                    }
                    //if (NativeCommandParameterBinder.NeedQuotes(path))
                    {
                        // Assume true
                        path = string.Format(CultureInfo.InvariantCulture, quoteFormat, path);
                    }
                    invokeProcess.Start();
                }
            }
        } // InvokeDefaultAction
        
        #endregion ItemCmdletProvider methods

               #region ContainerCmdletProvider members
        
        #region GetChildItems
        /// <summary>
        /// Gets the child items of a given directory.
        /// </summary>
        ///
        /// <param name="path">
        /// The full path of the directory to enumerate.
        /// </param>
        ///
        /// <param name="recurse">
        /// If true, recursively enumerates the child items as well.
        /// </param>
        ///
        /// <param name="depth">
        /// Limits the depth of recursion; uint.MaxValue performs full recursion.
        /// </param>
        ///
        /// <returns>
        /// Nothing.  FileInfo and DirectoryInfo objects that match the filter are written to the
        /// context's pipeline.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override void GetChildItems(
            string path,
            bool recurse,
            uint depth)
        {
            GetPathItems(path, recurse, depth, false, ReturnContainers.ReturnMatchingContainers);
        } // GetChildItems

        #endregion GetChildItems

        #region GetChildNames
        /// <summary>
        /// Gets the path names for all children of the specified
        /// directory that match the given filter.
        /// </summary>
        ///
        /// <param name="path">
        /// The full path of the directory to enumerate.
        /// </param>
        ///
        /// <param name="returnContainers">
        /// Determines if all containers should be returned or only those containers that match the
        /// filter(s).
        /// </param>
        ///
        /// <returns>
        /// Nothing.  Child names are written to the context's pipeline.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override void GetChildNames(
            string path,
            ReturnContainers returnContainers)
        {
            GetPathItems(path, false, uint.MaxValue, true, returnContainers);
        } // GetChildNames

        #endregion GetChildNames
        protected override bool ConvertPath(
            string path,
            string filter,
            ref string updatedPath,
            ref string updatedFilter)
        {
            // In order to support Wildcards?
            // WriteWarning($"ConvertPath ({path}, {filter})");

            // Don't handle full paths, paths that the user is already trying to
            // filter, or paths they are trying to escape.
            if ((!string.IsNullOrEmpty(filter)) ||
                (path.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal)) ||
                (path.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal)) ||
                (path.Contains("`"))
                )
            {
                return false;
            }

            // We can never actually modify the PowerShell path, as the
            // Win32 filtering support returns items that match the short
            // filename OR long filename.
            //
            // This creates tons of seemingly incorrect matches, such as:
            //
            // *~*:   Matches any file with a long filename
            // *n*:   Matches all files with a long filename, but have been
            //        mapped to a [6][~n].[3] disambiguation bucket
            // *.abc: Matches all files that have an extension that begins
            //        with ABC, since their extension is truncated in the
            //        short filename
            // *.*:   Matches all files and directories, even if they don't
            //        have a dot in their name

            // Our algorithm here is pretty simple. The filesystem can handle
            // * and ? in PowerShell wildcards, just not character ranges [a-z].
            // We replace character ranges with the single-character wildcard, '?'.
            updatedPath = path;
            updatedFilter = System.Text.RegularExpressions.Regex.Replace(path, "\\[.*?\\]", "?");
            WriteWarning($"ConvertPath ({updatedPath}, {updatedFilter})");
            return true;
        }
        private void GetPathItems(
            string path,
            bool recurse,
            uint depth,
            bool nameOnly,
            ReturnContainers returnContainers)
        {
            // Verify parameters
            if (String.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            bool isDirectory = IsItemContainer(path);
            bool exists = ItemExists(path);
            
            path = NormalizePath(path);

            if (exists)
            {
                if (String.IsNullOrEmpty(path))
                {
                    path = $"{ZipFileDriveInfo.Name}:\\";
                }

                if (isDirectory)
                {
                    // Only the Root directory is looked at for this scenario. 
                    IEnumerable<ZipFileItemInfo> fileInfoItems = ZipFileItemInfo.GetFileItemInfo(ZipFileDriveInfo, "*");

                    // Sort the files
                    fileInfoItems = fileInfoItems.OrderBy(c => c.FullName, StringComparer.CurrentCultureIgnoreCase);

                    foreach (ZipFileItemInfo fileInfo in fileInfoItems)
                    {
                        if (nameOnly)
                        {
                            WriteItemObject(
                                fileInfo.Name,
                                fileInfo.FullName,
                                false);
                        }
                        else
                        {
                            WriteItemObject(fileInfo, fileInfo.FullName, false);
                        }
                    }
                    
                }
                else
                {
                    // Maybe the path is a file name so try a FileInfo instead
                    ZipFileItemInfo fileInfo = new ZipFileItemInfo(ZipFileDriveInfo, path);

                    if (nameOnly)
                    {
                        WriteItemObject(
                            fileInfo.Name,
                            fileInfo.FullName,
                            false);
                    }
                    else
                    {
                        WriteItemObject(fileInfo, fileInfo.FullName, false);
                    }

                }

            }
            else
            {
                String error = StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path);
                Exception e = new IOException(error);
                WriteError(new ErrorRecord(
                    e,
                    "ItemDoesNotExist",
                    ErrorCategory.ObjectNotFound,
                    path));
                return;
            }
        }

        #region RenameItem

        /// <summary>
        /// Renames a file or directory.
        /// </summary>
        ///
        /// <param name="path">
        /// The current full path to the file or directory.
        /// </param>
        ///
        /// <param name="newName">
        /// The new full path to the file or directory.
        /// </param>
        ///
        /// <returns>
        /// Nothing.  The renamed DirectoryInfo or FileInfo object is
        /// written to the context's pipeline.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        ///     newName is null or empty
        /// </exception>
        protected override void RenameItem(string path, string newName)
        {

            // Check the parameters
            if (String.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            path = NormalizePath(path);

            if (String.IsNullOrEmpty(newName))
            {
                throw PSTraceSource.NewArgumentException("newName");
            }

            // newName = NormalizePath(newName);

            // Clean up "newname" to fix some common usability problems:
            // Rename .\foo.txt .\bar.txt
            // Rename c:\temp\foo.txt c:\temp\bar.txt
            if (newName.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                newName.StartsWith("./", StringComparison.OrdinalIgnoreCase))
            {
                newName = newName.Remove(0, 2);
            }
            // else if (String.Equals(Path.GetDirectoryName(path), Path.GetDirectoryName(newName), StringComparison.OrdinalIgnoreCase))
            // {
            //     newName = Path.GetFileName(newName);
            // }

            //Check to see if the target specified is just filename. We dont allow rename to move the file to a different directory.
            //If a path is specified for the newName then we flag that as an error.
            // if (String.Compare(Path.GetFileName(newName), newName, StringComparison.OrdinalIgnoreCase) != 0)
            // {
            //     throw PSTraceSource.NewArgumentException("newName", FileSystemProviderStrings.RenameError);
            // }

            // Check to see if the target specified exists. 
            if (ItemExists(newName))
            {
                throw PSTraceSource.NewArgumentException("newName", FileSystemProviderStrings.RenameError);
            }
            
            try
            {           
                // Manually move this item since you cant have more than one stream open at a time.
                ZipFileItemInfo file = new ZipFileItemInfo(ZipFileDriveInfo, path);
                ZipFileItemInfo result;

                // Confirm the rename with the user

                string action = FileSystemProviderStrings.RenameItemActionFile;

                string resource = StringUtil.Format(FileSystemProviderStrings.RenameItemResourceFileTemplate, file.FullName, newName);


                if (ShouldProcess(resource, action))
                {
                    // Now move the file
                    // Validate Current PWD is not the Provider
                    //if ((!Path.IsPathFullyQualified(newName)) && (!SessionState.Path.CurrentLocation.Path.StartsWith(ZipFileDriveInfo.Name + ":")) )
                    //{
                    //    newName = Path.Join(SessionState.Path.CurrentLocation.Path, newName);
                    //}

                    file.MoveTo(newName);

                    result = file;
                    WriteItemObject(result, result.FullName, false);
                }
            }
            catch (ArgumentException argException)
            {
                WriteError(new ErrorRecord(argException, "RenameItemArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException ioException)
            {
                //IOException contains specific message about the error occured and so no need for errordetails.
                WriteError(new ErrorRecord(ioException, "RenameItemIOError", ErrorCategory.WriteError, path));
            }
            catch (UnauthorizedAccessException accessException)
            {
                WriteError(new ErrorRecord(accessException, "RenameItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
        }

        #endregion RenameItem

        #region NewItem
        /// <summary>
        /// Creates a file or directory with the given path.
        /// </summary>
        /// <param name="path">
        /// The path of the file or directory to create.
        /// </param>
        ///<param name="type">
        /// Specify "file" to create a file.
        /// Specify "directory" or "container" to create a directory.
        /// </param>
        /// <param name="value">
        /// If <paramref name="type" /> is "file" then this parameter becomes the content
        /// of the file to be created.
        /// </param>
        /// <returns>
        /// Nothing.  The new DirectoryInfo or FileInfo object is
        /// written to the context's pipeline.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        ///     type is null or empty.
        /// </exception>
        protected override void NewItem(
            string path,
            string type,
            object value)
        {
            //ItemType itemType = ItemType.Unknown;

            // Verify parameters
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            if (string.IsNullOrEmpty(type))
            {
                type = "file";
            }

            path = NormalizePath(path);

            // if (Force)
            // {
            //     if (!CreateIntermediateDirectories(path))
            //     {
            //         return;
            //     }
            // }
            if (ItemExists(path))
            {
                throw new Exception("File Exists");
            }

            if (type == "file")
            {
                ZipFileItemInfo newItem = new ZipFileItemInfo(ZipFileDriveInfo, path, true);
                if (value != null)
                {
                    using (StreamWriter writer = newItem.AppendText())
                    {
                        writer.Write(value.ToString());
                        writer.Flush();
                        writer.Dispose();
                    }
                }
            }

        }

        // Note: Omitted the following commands
        // WinCreateSymbolicLink
        // WinCreateHardLink
        // WinCreateJunction
        // CheckItemExists
        // ItemType
        // GetItemType
        // CreateDirectory
        // CreateIntermediateDirectories

        #endregion NewItem

        #region RemoveItem

        /// <summary>
        /// Removes the specified file or directory.
        /// </summary>
        /// <param name="path">
        /// The full path to the file or directory to be removed.
        /// </param>
        /// <param name="recurse">
        /// Specifies if the operation should also remove child items.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override void RemoveItem(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            path = NormalizePath(path);

            if (!ItemExists(path))
            {
                throw new Exception("Item not exists");
            }
            
            ZipFileItemInfo[] archiveItems = ZipFileItemInfo.GetFileItemInfo(ZipFileDriveInfo, path);
            foreach(ZipFileItemInfo archiveItem in archiveItems)
            {
                archiveItem.Delete();
            }

		}

        // Todo: Double check this whole region
        // Note: Omitted the following commands
        // RemoveItemDynamicParameters
        // RemoveDirectoryInfoItem
        // RemoveFileInfoItem
        // RemoveFileSystemItem

        #endregion RemoveItem

        #region ItemExists

        /// <summary>
        /// Determines if a file or directory exists at the specified path.
        /// </summary>
        ///
        /// <param name="path">
        /// The path of the item to check.
        /// </param>
        ///
        /// <returns>
        /// True if a file or directory exists at the specified path, false otherwise.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        ///

        protected override bool ItemExists(string path)
        {
            ErrorRecord error = null;

            bool result = ItemExists(path, out error);
            if (error != null)
            {
                WriteError(error);
            }

            return result;
        }

        /// <summary>
        /// Implementation of ItemExists for the provider. This implementation
        /// allows the caller to decide if it wants to WriteError or not based
        /// on the returned ErrorRecord
        /// </summary>
        ///
        /// <param name="path">
        /// The path of the object to check
        /// </param>
        ///
        /// <param name="error">
        /// An error record is returned in this parameter if there was an error.
        /// </param>
        ///
        /// <returns>
        /// True if an object exists at the specified path, false otherwise.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        ///

        private bool ItemExists(string path, out ErrorRecord error)
        {
            error = null;

            if (String.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            bool result = false;

            path = NormalizePath(path);
            
            if (String.IsNullOrEmpty(path))
            {
                return true;
            }
            try
            {
                bool notUsed;
                // Exception accessException;

                // First see if the file exists
                try {
                    //if ((new ZipFileItemInfo(ZipFileDriveInfo, path)) != null)
                    if (ZipFileItemInfo.GetFileItemInfo(ZipFileDriveInfo, path) != null)
                    {
                        result = true;
                    }
                }
                catch (IOException ioException)
                {
                    // File ZipFile Open and ZipFileItem Open throws the same errors, need to validate
                    // ZipFileItem existance.
                    if (ioException.Message != StringUtil.Format(FileSystemProviderStrings.ItemNotFound, path))
                    {
                        throw ioException;
                    }

                }
                catch (PSArgumentException psArgumentException)
                {

                }
                
                FileSystemItemProviderDynamicParameters itemExistsDynamicParameters =
                    DynamicParameters as FileSystemItemProviderDynamicParameters;

                // If the items see if we need to check the age of the file...
                if (result && itemExistsDynamicParameters != null)
                {
                    // DateTime lastWriteTime = File.GetLastWriteTime(path);

                    // if (itemExistsDynamicParameters.OlderThan.HasValue)
                    // {
                    //     result = lastWriteTime < itemExistsDynamicParameters.OlderThan.Value;
                    // }
                    // if (itemExistsDynamicParameters.NewerThan.HasValue)
                    // {
                    //     result = lastWriteTime > itemExistsDynamicParameters.NewerThan.Value;
                    // }
                }
            }
            catch (System.Security.SecurityException security)
            {
                error = new ErrorRecord(security, "ItemExistsSecurityError", ErrorCategory.PermissionDenied, path);
            }
            catch (ArgumentException argument)
            {
                error = new ErrorRecord(argument, "ItemExistsArgumentError", ErrorCategory.InvalidArgument, path);
            }
            catch (UnauthorizedAccessException unauthorized)
            {
                error = new ErrorRecord(unauthorized, "ItemExistsUnauthorizedAccessError", ErrorCategory.PermissionDenied, path);
            }
            catch (PathTooLongException pathTooLong)
            {
                error = new ErrorRecord(pathTooLong, "ItemExistsPathTooLongError", ErrorCategory.InvalidArgument, path);
            }
            catch (NotSupportedException notSupported)
            {
                error = new ErrorRecord(notSupported, "ItemExistsNotSupportedError", ErrorCategory.InvalidOperation, path);
            }

            return result;
        }
        
        #endregion ItemExists

        #region HasChildItems

        /// <summary>
        /// Determines if the given path is a directory, and has children.
        /// </summary>
        /// <param name="path">
        /// The full path to the directory.
        /// </param>
        /// <returns>
        /// True if the path refers to a directory that contains other
        /// directories or files.  False otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override bool HasChildItems(string path)
        {
            bool result = false;
            
            // verify parameters
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            path = NormalizePath(path);
            if (ItemExists(path))
            {
                return false;
            }

            return true;
        }

        #endregion HasChildItems
        
        #region CopyItem
        /// <summary>
        /// Copies an item at the specified path to the given destination.
        /// </summary>
        ///
        /// <param name="path">
        /// The path of the item to copy.
        /// </param>
        ///
        /// <param name="destinationPath">
        /// The path of the destination.
        /// </param>
        ///
        /// <param name="recurse">
        /// Specifies if the operation should also copy child items.
        /// </param>
        ///
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        ///     destination path is null or empty.
        /// </exception>
        ///
        /// <returns>
        /// Nothing.  Copied items are written to the context's pipeline.
        /// </returns>
        protected override void CopyItem(
            string path,
            string destinationPath,
            bool recurse)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            if (String.IsNullOrEmpty(destinationPath))
            {
                throw PSTraceSource.NewArgumentException("destinationPath");
            }

            path = NormalizePath(path);
            destinationPath = NormalizePath(destinationPath);

            Console.WriteLine("Todo: CopyItem");

            //PSSession fromSession = null;
            //PSSession toSession = null;

            // CopyItemDynamicParameters copyDynamicParameter = DynamicParameters as CopyItemDynamicParameters;

            //if (copyDynamicParameter != null)
            //{
            //    if (copyDynamicParameter.FromSession != null)
            //    {
            //        fromSession = copyDynamicParameter.FromSession;
            //    }
            //    else
            //    {
            //        toSession = copyDynamicParameter.ToSession;
            //    }
            //}

            //_excludeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(Exclude, WildcardOptions.IgnoreCase);

            // if the source and destination path are same (for a local copy) then flag it as error.
            //if ((toSession == null) && (fromSession == null) && InternalSymbolicLinkLinkCodeMethods.IsSameFileSystemItem(path, destinationPath))
            //{
            //    String error = StringUtil.Format(FileSystemProviderStrings.CopyError, path);
            //    Exception e = new IOException(error);
            //    e.Data[SelfCopyDataKey] = destinationPath;
            //    WriteError(new ErrorRecord(e, "CopyError", ErrorCategory.WriteError, path));
            //    return;
            //}
            // Copy-Item from session
            //if (fromSession != null)
            //{
            //    CopyItemFromRemoteSession(path, destinationPath, recurse, Force, fromSession);
            //}

            //else
            //{
            //    // Copy-Item to session
            //    if (toSession != null)
            //    {
            //        using (System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create())
            //        {
            //            ps.Runspace = toSession.Runspace;
            //            CopyItemLocalOrToSession(path, destinationPath, recurse, Force, ps);
            //        }
            //    }

                // Copy-Item local
            //    else
            //    {
            //        CopyItemLocalOrToSession(path, destinationPath, recurse, Force, null);
            //    }
            //}

            //_excludeMatcher.Clear();
            //_excludeMatcher = null;
        }
        #endregion CopyItem
        
        #endregion ContainerCmdletProvider members

        #region NavigationCmdletProvider

        #endregion NavigationCmdletProvider

        #region IContentCmdletProvider

        #endregion IContentCmdletProvider
    }
    #endregion ZipFileProvider
    #region Dynamic Parameters

    #endregion Dynamic Parameters

}