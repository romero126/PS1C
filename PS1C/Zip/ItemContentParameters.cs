using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

namespace PS1C.Zip
{
	public class ItemContentParameters
	{
		[Parameter(HelpMessage = "Handle the input/output as binary data")]
		public SwitchParameter IsBinary { get; set; }
		[Parameter(HelpMessage = "Handle the input/output as Raw data")]
		public SwitchParameter Raw { get; set; }
		[Parameter(HelpMessage = "Handle the input/output as Raw data")]
		public SwitchParameter Wait { get; set; }
	}
}
