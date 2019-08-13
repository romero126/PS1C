using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO;

namespace PS1C
{
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        #region DriveCmdletProvider
		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			if (!File.Exists(drive.Root))
			{
				throw new Exception("file not found");
			}
			
            return base.NewDrive(drive);
		}
        #endregion
    }
}