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

        /// <summary>
        /// Determines if the specified drive can be mounted.
        /// </summary>
        ///
        /// <param name="drive">
        /// The drive that is going to be mounted.
        /// </param>
        ///
        /// <returns>
        /// The same drive that was passed in, if the drive can be mounted.
        /// null if the drive cannot be mounted.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// drive is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// drive root is null or empty.
        /// </exception>
		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
            // verify parameters

            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }

            if (String.IsNullOrEmpty(drive.Root))
            {
                throw PSTraceSource.NewArgumentException("drive.Root");
            }

			if (!File.Exists(drive.Root))
			{
				throw new Exception("file not found");
			}
			
            return base.NewDrive(drive);
		}
        #endregion
    }
}