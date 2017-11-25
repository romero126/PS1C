using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1C.Zip
{
	class Directory
	{
		public string Name;
		public string FullName;
		public string FullPath;
		public string ArchivePath;
		public string Drive;

		public Directory(string archivepath, string drive, string pwd, string name) {
			this.Name = name.Replace("/", "\\");
			this.FullName = drive + pwd + name;
			this.FullPath = pwd + name;
			this.FullName = this.FullName.Replace("/", "\\");
			this.ArchivePath = archivepath;
			this.Drive = drive;
		}
		public Directory()
		{

		}
	}
}