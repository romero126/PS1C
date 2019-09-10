using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;
using System.Linq;
using System.Diagnostics;
namespace PS1C
{

    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {


        // Workaround for internal class objects
        internal InvocationInfo Context_MyInvocation {
            get {
                return (InvocationInfo)SessionState.PSVariable.Get("MyInvocation").Value;
            }
        }


        internal void OutStackTrace()
        {
            StackTrace st = new StackTrace(true);
            for(int i =0; i< st.FrameCount; i++ )
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                StackFrame sf = st.GetFrame(i);
                WriteInformation(
                    new InformationRecord(
                        String.Format(
                            "    at {0}: {1} {2}",
                            sf.GetFileLineNumber(),
                            sf.GetMethod().DeclaringType,
                            sf.GetMethod()
                        ),
                        "this"
                    )
                );
            }
        }

		#region ItemCmdletProvider
		
        // Placeholder commands...

        // Note: we don't use IO.Path.IsPathRooted as this deals with "invalid" i.e. unnormalized paths
        private static bool IsAbsolutePath(string path)
        {
            Console.WriteLine($"IsAbsolutePath: {path}");
            return false;
        }


        #endregion
    }
}