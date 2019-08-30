
using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace PS1C
{
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        #region ContainerCmdletProvider members
        
        #region GetChildItems

        #endregion GetChildItems

        #region GetChildNames
        #endregion GetChildNames

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

            newName = NormalizePath(newName);

            // Clean up "newname" to fix some common usability problems:
            // Rename .\foo.txt .\bar.txt
            // Rename c:\temp\foo.txt c:\temp\bar.txt
            if (newName.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                newName.StartsWith("./", StringComparison.OrdinalIgnoreCase))
            {
                newName = newName.Remove(0, 2);
            }
            //else if (String.Equals(Path.GetDirectoryName(path), Path.GetDirectoryName(newName), StringComparison.OrdinalIgnoreCase))
            //{
            //    newName = Path.GetFileName(newName);
            //}

            // Check to see if the target specified exists. 
            if (ItemExists(newName))
            {
                throw PSTraceSource.NewArgumentException("newName", FileSystemProviderStrings.RenameError);
            }
            
            
            // Manually move this item since you cant have more than one stream open at a time.
            ZipFileItemInfo entry = new ZipFileItemInfo(PSDriveInfo, path);

            entry.MoveTo(newName);

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
                ZipFileItemInfo newItem = NewItemHelper(path);
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
            ZipFileItemInfo ArchiveItem = GetItemHelper(path);
            ArchiveItem.Delete();
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

                    if ((new ZipFileItemInfo(PSDriveInfo, path)) != null)
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

                FileSystemItemProviderDynamicParameters itemExistsDynamicParameters =
                    DynamicParameters as FileSystemItemProviderDynamicParameters;

                // If the items see if we need to check the age of the file...
                if (result && itemExistsDynamicParameters != null)
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(path);

                    if (itemExistsDynamicParameters.OlderThan.HasValue)
                    {
                        result = lastWriteTime < itemExistsDynamicParameters.OlderThan.Value;
                    }
                    if (itemExistsDynamicParameters.NewerThan.HasValue)
                    {
                        result = lastWriteTime > itemExistsDynamicParameters.NewerThan.Value;
                    }
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
        
        #endregion CopyItem
        
        #endregion ContainerCmdletProvider members

    }
}