using System;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public static class PathUtilsStrings
    {

       internal static string UtilityFileExistsNoClobber                                                                           =   "File {0} already exists and {1} was specified."; 
       internal static string OutFile_ReadWriteFileNotFileSystemProvider                                                           =   "Cannot open file because the current provider ({0}) cannot open a file."; 
       internal static string OutFile_MultipleFilesNotSupported                                                                    =   "Cannot perform operation because the path resolved to more than one file. This command cannot operate on multiple files."; 
       internal static string OutFile_DidNotResolveFile                                                                            =   "Cannot perform operation because the wildcard path {0} did not resolve to a file."; 
       internal static string OutFile_WriteToFileEncodingUnknown                                                                   =   "Unknown encoding {0}; valid values are {1}."; 
       internal static string ExportPSSession_ErrorDirectoryExists                                                                 =   "The directory '{0}' already exists.  Use the -Force parameter if you want to overwrite the directory and files within the directory."; 
       internal static string ExportPSSession_CannotCreateOutputDirectory                                                          =   "Cannot create the module {0} due to the following: {1}. Use a different argument for the -OutputModule parameter and retry."; 
       internal static string ExportPSSession_ScriptGeneratorVersionMismatch                                                       =   "The module cannot be loaded because it has been generated with an incompatible version of the {0} cmdlet. Generate the module with the {0} cmdlet from the current session, and try loading the module again.";

    }
}

