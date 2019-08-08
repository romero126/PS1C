using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PS1C
{
    //public class Provider : FileSystemProvider, IContentCmdletProvider
    public partial class Provider : NavigationCmdletProvider, IContentCmdletProvider
    {

    }
}