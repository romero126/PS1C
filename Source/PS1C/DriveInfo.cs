using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.IO;
namespace PS1C
{

	internal class DriveInfo : PSDriveInfo
	{
        public DriveInfo(PSDriveInfo driveInfo) : base(driveInfo)
		{

		}
    }

}