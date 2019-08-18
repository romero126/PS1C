using System;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PS1C
{
	//[CmdletProvider("PS1C", ProviderCapabilities.ShouldProcess)]
    //[CmdletProvider(ZipFileProvider.ProviderName, ProviderCapabilities.Credentials | ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    [CmdletProvider(ZipFileProvider.ProviderName, ProviderCapabilities.Credentials | ProviderCapabilities.ShouldProcess)]


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

        /// <summary>
        /// Initializes a new instance of the FileSystemProvider class. Since this
        /// object needs to be stateless, the constructor does nothing.
        /// </summary>

        // Workaround for internal class objects
        internal InvocationInfo Context_MyInvocation {
            get {
                return (InvocationInfo)SessionState.PSVariable.Get("MyInvocation").Value;
            }

        }

    }

}