using System;

using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO;
using System.Linq;

namespace PS1C
{

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


        private PSDriveInfo _psDriveInfo;


        /// <summary>
        /// Initializes a new instance of the FileSystemProvider class. Since this
        /// object needs to be stateless, the constructor does nothing.
        /// </summary>


        internal PSDriveInfo ZipFileDriveInfo {
            get {
                if (_psDriveInfo != null)
                {
                    return _psDriveInfo;
                }
                return PSDriveInfo;
            }
        }




    }

}