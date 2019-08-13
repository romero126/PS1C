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
		
        protected override bool IsValidPath(string path)
		{
			WriteVerbose("[Todo]: Provider IsValidPath(string path)");
			return true;
		}
        
        #endregion
    }
}