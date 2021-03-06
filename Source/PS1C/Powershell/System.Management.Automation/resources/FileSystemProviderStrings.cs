using System;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public static class FileSystemProviderStrings
    {

       internal static string InvokeItemAction                                                                                     =   "Invoke Item"; 
       internal static string InvokeItemResourceFileTemplate                                                                       =   "Item: {0}"; 
       internal static string RemoveItemActionFile                                                                                 =   "Remove File"; 
       internal static string RemoveItemActionDirectory                                                                            =   "Remove Directory"; 
       internal static string CopyItemActionFile                                                                                   =   "Copy File"; 
       internal static string CopyItemResourceFileTemplate                                                                         =   "Item: {0} Destination: {1}"; 
       internal static string CopyItemActionDirectory                                                                              =   "Copy Directory"; 
       internal static string RenameItemActionFile                                                                                 =   "Rename File"; 
       internal static string RenameItemActionDirectory                                                                            =   "Rename Directory"; 
       internal static string RenameItemResourceFileTemplate                                                                       =   "Item: {0} Destination: {1}"; 
       internal static string MoveItemActionFile                                                                                   =   "Move File"; 
       internal static string MoveItemActionDirectory                                                                              =   "Move Directory"; 
       internal static string MoveItemResourceFileTemplate                                                                         =   "Item: {0} Destination: {1}"; 
       internal static string SetPropertyActionFile                                                                                =   "Set Property File"; 
       internal static string SetPropertyActionDirectory                                                                           =   "Set Property Directory"; 
       internal static string SetPropertyResourceTemplate                                                                          =   "Item: {0} Property: {1} Value: {2}"; 
       internal static string ClearPropertyActionFile                                                                              =   "Clear Property File"; 
       internal static string ClearPropertyActionDirectory                                                                         =   "Clear Property Directory"; 
       internal static string ClearPropertyResourceTemplate                                                                        =   "Item: {0} Property: {1}"; 
       internal static string NewItemActionFile                                                                                    =   "Create File"; 
       internal static string NewItemActionDirectory                                                                               =   "Create Directory"; 
       internal static string NewItemActionTemplate                                                                                =   "Destination: {0}"; 
       internal static string ClearContentActionFile                                                                               =   "Clear Content"; 
       internal static string ClearContentesourceTemplate                                                                          =   "Item: {0}"; 
       internal static string ItemNotFound                                                                                         =   "Could not find item {0}."; 
       internal static string CannotRemoveItem                                                                                     =   "Cannot remove item {0}: {1}"; 
       internal static string CannotRestoreAttributes                                                                              =   "Cannot restore attributes on item {0}: {1}"; 
       internal static string ItemDoesNotExist                                                                                     =   "An object at the specified path {0} does not exist."; 
       internal static string DirectoryNotEmpty                                                                                    =   "Directory {0} cannot be removed because it is not empty."; 
       internal static string UnknownType                                                                                          =   "The type is not a known type for the file system. Only \"jfile\"j,\"jdirectory\"j or \"jsymboliclink\"j can be specified."; 
       internal static string PathOutSideBasePath                                                                                  =   "Cannot process the path because the specified path refers to an item that is outside the basePath."; 
       internal static string DriveRootError                                                                                       =   "The specified drive root \"j{0}\"j either does not exist, or it is not a folder."; 
       internal static string DirectoryExist                                                                                       =   "An item with the specified name {0} already exists."; 
       internal static string BasePathLengthError                                                                                  =   "The path length is too short. The character length of a path cannot be less than the character length of the basePath."; 
       internal static string DelimiterError                                                                                       =   "A delimiter cannot be specified when reading the stream one byte at a time."; 
       internal static string CopyError                                                                                            =   "Cannot overwrite the item {0} with itself."; 
       internal static string RenameError                                                                                          =   "Cannot rename the specified target, because it represents a path or device name."; 
       internal static string PropertyNotFound                                                                                     =   "The property {0} does not exist or was not found."; 
       internal static string PermissionError                                                                                      =   "You do not have sufficient access rights to perform this operation or the item is hidden, system, or read only."; 
       internal static string AttributesNotSupported                                                                               =   "The attribute cannot be set because attributes are not supported. Only the following attributes can be set: Archive, Hidden, Normal, ReadOnly, or System."; 
       internal static string CannotClearProperty                                                                                  =   "The property cannot be cleared because the property is not supported. Only the Attributes property can be cleared."; 
       internal static string TargetCannotContainDeviceName                                                                        =   "Cannot process path '{0}' because the target represents a reserved device name."; 
       internal static string EncodingNotUsed                                                                                      =   "Encoding not used when '-AsByteStream' specified."; 
       internal static string ByteEncodingError                                                                                    =   "Cannot proceed with byte encoding. When using byte encoding the content must be of type byte."; 
       internal static string FileNotFound                                                                                         =   "Cannot process the file because the file {0} was not found."; 
       internal static string DirectoryDisplayGrouping                                                                             =   "Directory: "; 
       internal static string ReadBackward_Encoding_NotSupport                                                                     =   "Cannot detect the encoding of the file. The specified encoding {0} is not supported when the content is read in reverse."; 
       internal static string AlternateDataStreamNotFound                                                                          =   "Could not open the alternate data stream '{0}' of the file '{1}'."; 
       internal static string StreamAction                                                                                         =   "Stream '{0}' of file '{1}'."; 
       internal static string RawAndWaitCannotCoexist                                                                              =   "The Raw and Wait parameters cannot be specified in the same command."; 
       internal static string InvalidDriveName                                                                                     =   "To use the Persist switch parameter, the drive name must be supported by the operating system (for example, drive letters A-Z)."; 
       internal static string PersistNotSupported                                                                                  =   "When you use the Persist parameter, the root must be a file system location on a remote computer."; 
       internal static string NoFirstLastWaitForRaw                                                                                =   "The '{0}' and '{1}' parameters cannot be specified in the same command."; 
       internal static string SubstitutePathTooLong                                                                                =   "The substitute path for the DOS device '{0}' is too long. It exceeds the maximum total path length (32,767 characters) that is valid for the Windows API."; 
       internal static string ItemNotDirectory                                                                                     =   "A directory is required for the operation. The item '{0}' is not a directory."; 
       internal static string NewItemActionJunction                                                                                =   "Create Junction"; 
       internal static string NewItemActionSymbolicLink                                                                            =   "Create Symbolic Link"; 
       internal static string ElevationRequired                                                                                    =   "Administrator privilege required for this operation."; 
       internal static string NewItemActionHardLink                                                                                =   "Create Hard Link"; 
       internal static string ItemNotFile                                                                                          =   "A file is required for the operation. The item '{0}' is not a file."; 
       internal static string HardLinkNotSupported                                                                                 =   "Hard links are not supported for the specified path."; 
       internal static string SymbolicLinkNotSupported                                                                             =   "Symbolic links are not supported for the specified path."; 
       internal static string CopyItemRemotelyProgressActivity                                                                     =   "Copying {0} to {1}"; 
       internal static string CopyItemRemoteDestinationIsFile                                                                      =   "Destination path {0} is a file that already exists on the target destination."; 
       internal static string CopyItemRemotelyFailed                                                                               =   "Failed to copy file {0} to remote target destination."; 
       internal static string CopyItemRemotelyStatusDescription                                                                    =   "From {0} to {1}"; 
       internal static string CopyItemRemotelyDestinationIsFile                                                                    =   "Cannot copy a directory '{0}' to file '{0}'"; 
       internal static string CopyItemRemotelyFailedToGetDirectoryChildItems                                                       =   "Failed to get directory {0} child items."; 
       internal static string CopyItemRemotelyFailedToReadFile                                                                     =   "Failed to read remote file '{0}'."; 
       internal static string CopyItemRemotelyFailedToValidateDestination                                                          =   "Failed to validate remote destination '{0}'."; 
       internal static string CopyItemRemotelyFailedToValidateIfDestinationIsFile                                                  =   "Cannot validate if remote destination {0} is a file."; 
       internal static string CopyItemRemotelyOperationNotSupported                                                                =   "Remote copy with {0} is not supported."; 
       internal static string CopyItemRemotelyFailedToCreateDirectory                                                              =   "Failed to create directory '{0}' on remote destination."; 
       internal static string DriveMaxSizeError                                                                                    =   "Maximum size for drive has been exceeded: {0}."; 
       internal static string SymlinkItemExists                                                                                    =   "Cannot create symbolic link because the path {0} already exists."; 
       internal static string AlreadyListedDirectory                                                                               =   "Skip already-visited directory {0}.";

    }
}

