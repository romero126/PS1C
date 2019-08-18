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
		#region ItemCmdletProvider
		
        // Placeholder commands...

        // Note: we don't use IO.Path.IsPathRooted as this deals with "invalid" i.e. unnormalized paths
        private static bool IsAbsolutePath(string path)
        {
            return true;
        }
        #endregion
    }
}