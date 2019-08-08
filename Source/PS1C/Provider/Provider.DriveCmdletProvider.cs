using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO;

namespace PS1C
{
    //public class Provider : FileSystemProvider, IContentCmdletProvider
    public partial class Provider : NavigationCmdletProvider, IContentCmdletProvider
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