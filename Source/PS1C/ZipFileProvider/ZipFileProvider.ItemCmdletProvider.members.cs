
using Microsoft.PowerShell.Commands;
using System;
using System.IO;
using System.Globalization;
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
                        //var result = SessionState.InvokeCommand.InvokeScript(script, false, System.Management.Automation.Runspaces.PipelineResultTypes.All, null, scriptargs);
                        ScriptBlock scriptBlock = ScriptBlock.Create(script);
                        var result = SessionState.InvokeCommand.InvokeScript(SessionState, scriptBlock, scriptargs);
                        //var result = SessionState.InvokeCommand.InvokeScript(script, scriptargs);
                        WriteItemObject(result, zipFileItemInfo.FullName, false);
                    }
                    //SessionState.InvokeCommand.InvokeScript(script, scriptargs);
                }


                // else
                // {
                //     try
                //     {
                //         // Try Process.Start first. This works for executables on Win/Unix platforms
                //         invokeProcess.Start();
                //     }
                //     catch (Win32Exception ex) when (ex.NativeErrorCode == 13)
                //     {
                //         // Error code 13 -- Permission denied
                //         // The file is possibly not an executable. We try xdg-open/open on Linux/macOS.
                //         invokeDefaultProgram = true;
                //     }
                // }

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
        
        #endregion ItemCmdletProvider members

        
    }
}