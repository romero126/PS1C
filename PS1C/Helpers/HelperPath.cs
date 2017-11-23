using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace PS1C
{
	class HelperPath
	{

		public string Drive;
		private PSDriveInfo DriveInfo;
		private string PathData;
		public HelperPath(string path, PSDriveInfo drive) {
			this.DriveInfo = drive;
			this.PathData = path;
		}
		/// <summary>
		/// Helper to format path
		/// Gets the parent drive from the string. 
		/// Input:	C:\directory\zipfile.zip\dir\dir\file.txt
		/// Output:	rootdir:\dir\dir\file.txt
		/// </summary>
		/// <param name="path">The path of the file to read / monitor</param>

		public string Path {
			get { return PathData.Replace(DriveInfo.Root, DriveInfo.Name + ":"); }
		}
		/// <summary>
		/// Helper to format drive data
		/// Gets the parent drive from the string. 
		/// Input:	rootdir:\dir\dir\file.txt
		/// Output:	rootdir:\dir\dir\
		/// </summary>
		/// <param name="path">The path of the file to read / monitor</param>
		public string Parent(string path) {
			return path.Substring(0, path.LastIndexOf("\\")+1);
		}
		/// <summary>
		///	Helper to format output of drive path.
		/// Returns output of rootdir:\
		/// </summary>
		public string DriveName {
			get {
				return DriveInfo.Name + ":\\";
			}
		}

		/// <summary>
		/// Helper to format output of drive path
		/// Converts ZipFile Path to Default drive:\path
		/// </summary>
		/// <param name="path">The path of the </param>
		public string UnixPath {
				get { return PathData.Replace(DriveInfo.Root, "").Replace("\\", "/"); }
		}

	}
}
/*









*/