using System;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PS1C
{
	[CmdletProvider("PS1C", ProviderCapabilities.ShouldProcess)]
    //public class Provider : FileSystemProvider, IContentCmdletProvider
    public partial class Provider : NavigationCmdletProvider, IContentCmdletProvider
    {

    }
}