using Microsoft.PowerShell.Commands;
using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PS1C
{
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {

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
                    }

                }
            }

        }

        #endregion


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
        #endregion

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

        #endregion
    }
}