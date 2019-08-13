using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

using Microsoft.PowerShell.Commands;
using System.Diagnostics.CodeAnalysis;


namespace PS1C
{
	//[CmdletProvider("PS1C", ProviderCapabilities.ShouldProcess)]
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {

        /// <summary>
        /// Invokes the item at the path using ShellExecute semantics.
        /// </summary>
        /// <param name="path">
        /// The item to invoke.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        protected override void InvokeDefaultAction(string path)
        {
            WriteWarning("This is not implimented correctly");

            if (string.IsNullOrEmpty(path))
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
#if UNIX
                bool useShellExecute = false;
                if (Directory.Exists(path))
                {
                    // Path points to a directory. We have to use xdg-open/open on Linux/macOS.
                    useShellExecute = true;
                }
                else
                {
                    try
                    {
                        // Try Process.Start first. This works for executables on Win/Unix platforms
                        invokeProcess.Start();
                    }
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 13)
                    {
                        // Error code 13 -- Permission denied
                        // The file is possibly not an executable. We try xdg-open/open on Linux/macOS.
                        useShellExecute = true;
                    }
                }

                if (useShellExecute)
                {
                    invokeProcess.StartInfo.UseShellExecute = true;
                    invokeProcess.Start();
                }
#else
                // Use ShellExecute when it's not a headless SKU
                invokeProcess.StartInfo.UseShellExecute = Platform.IsWindowsDesktop;
                invokeProcess.Start();
#endif
            }
        }
    }

}