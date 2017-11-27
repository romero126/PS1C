using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
namespace PS1C
{
    [CmdletProvider("PS1C", ProviderCapabilities.Credentials | ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
	//public class Provider : FileSystemProvider, IContentCmdletProvider
	public class Provider : NavigationCmdletProvider, IContentCmdletProvider
    public sealed partial class Provider : NavigationCmdletProvider,
                                                     IContentCmdletProvider,
                                                     IPropertyCmdletProvider
	{


	}
}
 