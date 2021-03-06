// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Win32.SafeHandles;

using Dbg = System.Management.Automation;
using PS1C;

namespace System.Management.Automation.Internal
{
#if !UNIX
    #region AlternateDataStreamUtilities

    /// <summary>
    /// Represents alternate stream data retrieved from a file.
    /// </summary>
    public class AlternateStreamData
    {
        /// <summary>
        /// The name of the file that holds this stream.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The name of this stream.
        /// </summary>
        public string Stream { get; set; }

        /// <summary>
        /// The length of this stream.
        /// </summary>
        public long Length { get; set; }
    }

    /// <summary>
    /// Provides access to alternate data streams on a file.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes",
        Justification = "Needed by both the FileSystem provider and Unblock-File cmdlet.")]
    public static class AlternateDataStreamUtilities
    {
        /// <summary>
        /// List all of the streams on a file.
        /// </summary>
        /// <param name="path">The fully-qualified path to the file.</param>
        /// <returns>The list of streams (and their size) in the file.</returns>
        internal static List<AlternateStreamData> GetStreams(string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            List<AlternateStreamData> alternateStreams = new List<AlternateStreamData>();

            AlternateStreamNativeData findStreamData = new AlternateStreamNativeData();

            SafeFindHandle handle = NativeMethods.FindFirstStreamW(
                path, NativeMethods.StreamInfoLevels.FindStreamInfoStandard,
                findStreamData, 0);
            if (handle.IsInvalid) throw new Win32Exception();

            try
            {
                do
                {
                    // Remove the leading ':'
                    findStreamData.Name = findStreamData.Name.Substring(1);

                    // And trailing :$DATA (as long as it's not the default data stream)
                    string dataStream = ":$DATA";
                    if (!string.Equals(findStreamData.Name, dataStream, StringComparison.OrdinalIgnoreCase))
                    {
                        findStreamData.Name = findStreamData.Name.Replace(dataStream, string.Empty);
                    }

                    AlternateStreamData data = new AlternateStreamData();
                    data.Stream = findStreamData.Name;
                    data.Length = findStreamData.Length;
                    data.FileName = path.Replace(data.Stream, string.Empty);
                    //data.FileName = data.FileName.Trim(Utils.Separators.Colon);
                    data.FileName = data.FileName.Trim(';');
                    alternateStreams.Add(data);
                    findStreamData = new AlternateStreamNativeData();
                }
                while (NativeMethods.FindNextStreamW(handle, findStreamData));

                int lastError = Marshal.GetLastWin32Error();
                if (lastError != NativeMethods.ERROR_HANDLE_EOF)
                    throw new Win32Exception(lastError);
            }
            finally { handle.Dispose(); }

            return alternateStreams;
        }

        /// <summary>
        /// Creates a file stream on a file.
        /// </summary>
        /// <param name="path">The fully-qualified path to the file.</param>
        /// <param name="streamName">The name of the alternate data stream to open.</param>
        /// <param name="mode">The FileMode of the file.</param>
        /// <param name="access">The FileAccess of the file.</param>
        /// <param name="share">The FileShare of the file.</param>
        /// <returns>A FileStream that can be used to interact with the file.</returns>
        internal static FileStream CreateFileStream(string path, string streamName, FileMode mode, FileAccess access, FileShare share)
        {
            if (!TryCreateFileStream(path, streamName, mode, access, share, out var stream))
            {
                string errorMessage = StringUtil.Format(
                    FileSystemProviderStrings.AlternateDataStreamNotFound, streamName, path);
                throw new FileNotFoundException(errorMessage, $"{path}:{streamName}");
            }

            return stream;
        }

        /// <summary>
        /// Tries to create a file stream on a file.
        /// </summary>
        /// <param name="path">The fully-qualified path to the file.</param>
        /// <param name="streamName">The name of the alternate data stream to open.</param>
        /// <param name="mode">The FileMode of the file.</param>
        /// <param name="access">The FileAccess of the file.</param>
        /// <param name="share">The FileShare of the file.</param>
        /// <param name="stream">A FileStream that can be used to interact with the file.</param>
        /// <returns>True if the stream was successfully created, otherwise false.</returns>
        internal static bool TryCreateFileStream(string path, string streamName, FileMode mode, FileAccess access, FileShare share, out FileStream stream)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (streamName == null)
            {
                throw new ArgumentNullException(nameof(streamName));
            }

            if (mode == FileMode.Append)
            {
                mode = FileMode.OpenOrCreate;
            }

            var resultPath = $"{path}:{streamName}";
            SafeFileHandle handle = NativeMethods.CreateFile(resultPath, access, share, IntPtr.Zero, mode, 0, IntPtr.Zero);

            if (handle.IsInvalid)
            {
                stream = null;
                return false;
            }

            stream = new FileStream(handle, access);
            return true;
        }

        /// <summary>
        /// Removes an alternate data stream.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="streamName">The name of the alternate data stream to delete.</param>
        internal static void DeleteFileStream(string path, string streamName)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (streamName == null) throw new ArgumentNullException("streamName");

            string adjustedStreamName = streamName.Trim();
            if (adjustedStreamName.IndexOf(':') != 0)
            {
                adjustedStreamName = ":" + adjustedStreamName;
            }

            string resultPath = path + adjustedStreamName;

            File.Delete(resultPath);
        }

        internal static void SetZoneOfOrigin(string path, SecurityZone securityZone)
        {
            using (FileStream fileStream = CreateFileStream(path, "Zone.Identifier", FileMode.Create, FileAccess.Write, FileShare.None))
            using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.Unicode))
            {
                textWriter.WriteLine("[ZoneTransfer]");
                textWriter.WriteLine("ZoneId={0}", (int)securityZone);
            }

            // an alternative is to use IAttachmentExecute interface as described here:
            // http://joco.name/2010/12/22/windows-antivirus-api-in-net-and-a-com-interop-crash-course/
            // the code above seems cleaner and more robust than the IAttachmentExecute approach
        }

        internal static class NativeMethods
        {
            internal const int ERROR_HANDLE_EOF = 38;
            internal enum StreamInfoLevels { FindStreamInfoStandard = 0 }

            [DllImport(PinvokeDllNames.CreateFileDllName, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern SafeFileHandle CreateFile(string lpFileName,
                FileAccess dwDesiredAccess, FileShare dwShareMode,
                IntPtr lpSecurityAttributes, FileMode dwCreationDisposition,
                int dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport(PinvokeDllNames.FindFirstStreamDllName, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "AlternateStreamNativeData.Name")]
            internal static extern SafeFindHandle FindFirstStreamW(
                string lpFileName, StreamInfoLevels InfoLevel,
                [In, Out, MarshalAs(UnmanagedType.LPStruct)]
                AlternateStreamNativeData lpFindStreamData, uint dwFlags);

            [DllImport(PinvokeDllNames.FindNextStreamDllName, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "AlternateStreamNativeData.Name")]
            internal static extern bool FindNextStreamW(
                SafeFindHandle hndFindFile,
                [In, Out, MarshalAs(UnmanagedType.LPStruct)]
                AlternateStreamNativeData lpFindStreamData);
        }

        internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeFindHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                return FindClose(this.handle);
            }

            [DllImport(PinvokeDllNames.FindCloseDllName)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FindClose(IntPtr handle);
        }

        /// <summary>
        /// Represents alternate stream data retrieved from a file.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal class AlternateStreamNativeData
        {
            /// <summary>
            /// The length of this stream.
            /// </summary>
            public long Length;

            /// <summary>
            /// The name of this stream.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)]
            public string Name;
        }
    }

    #endregion
#endif

    #region CopyFileFromRemoteUtils

    internal static class CopyFileRemoteUtils
    {
        private const string functionToken = "function ";
        private const string nameToken = "Name";
        private const string definitionToken = "Definition";

        #region PSCopyToSessionHelper

        internal const string PSCopyToSessionHelperName = @"PSCopyToSessionHelper";
        private static string s_driveMaxSizeErrorFormatString = FileSystemProviderStrings.DriveMaxSizeError;
        private static string s_PSCopyToSessionHelperDefinition = StringUtil.Format(PSCopyToSessionHelperDefinitionFormat, @"[ValidateNotNullOrEmpty()]", s_driveMaxSizeErrorFormatString);
        private static string s_PSCopyToSessionHelperDefinitionRestricted = StringUtil.Format(PSCopyToSessionHelperDefinitionFormat, @"[ValidateUserDrive()]", s_driveMaxSizeErrorFormatString);
        private const string PSCopyToSessionHelperDefinitionFormat = @"
        param (
            [Parameter(ParameterSetName=""PSCopyFileToRemoteSession"")]
            [Parameter(ParameterSetName=""PSCopyAlternateStreamToRemoteSession"")]
            {0}

            [string] $copyToFilePath,

            [Parameter(ParameterSetName=""PSCopyFileToRemoteSession"", Mandatory=$false)]
            [Parameter(ParameterSetName=""PSCopyAlternateStreamToRemoteSession"")]
            [ValidateNotNullOrEmpty()]
            [string] $b64Fragment,

            [Parameter(ParameterSetName=""PSCopyFileToRemoteSession"")]
            [switch] $createFile = $false,

            [Parameter(ParameterSetName=""PSCopyFileToRemoteSession"")]
            [switch] $emptyFile = $false,

            [Parameter(ParameterSetName=""PSCopyAlternateStreamToRemoteSession"", Mandatory=$true)]
            [ValidateNotNullOrEmpty()]
            [string] $streamName,

            [Parameter(ParameterSetName=""PSTargetSupportsAlternateStreams"", Mandatory=$true)]
            {0}

            [string] $supportAltStreamPath,

            [Parameter(ParameterSetName=""PSSetFileMetadata"", Mandatory=$true)]
            {0}

            [string] $metaDataFilePath,

            [Parameter(ParameterSetName=""PSSetFileMetadata"", Mandatory=$true)]
            [ValidateNotNull()]
            [hashtable] $metaDataToSet,

            [Parameter(ParameterSetName=""PSRemoteDestinationPathIsFile"", Mandatory=$true)]
            {0}

            [string] $isFilePath,

            [Parameter(ParameterSetName=""PSGetRemotePathInfo"", Mandatory=$true)]
            {0}

            [string] $remotePath,

            [Parameter(ParameterSetName=""PSCreateDirectoryOnRemoteSession"", Mandatory=$true)]
            {0}

            [string] $createDirectoryPath,

            [Parameter(ParameterSetName=""PSCreateDirectoryOnRemoteSession"")]
            [switch] $force
        )

        # Checks if path drive specifies max size and if max size is exceeded
        #
        function CheckPSDriveSize
        {{
            param (
                [System.Management.Automation.PathInfo] $resolvedPath,
                [int] $fragmentLength
            )

            if (($null -ne $resolvedPath.Drive) -and ($null -ne $resolvedPath.Drive.MaximumSize))
            {{
                $maxUserSize = $resolvedPath.Drive.MaximumSize
                $dirSize = 0
                Microsoft.PowerShell.Management\Get-ChildItem -LiteralPath ($resolvedPath.Drive.Name + "":"") -Recurse | ForEach-Object {{
                    Microsoft.PowerShell.Management\Get-Item -LiteralPath $_.FullName -Stream * | ForEach-Object {{ $dirSize += $_.Length }}
                }}

                if (($dirSize + $fragmentLength) -gt $maxUserSize)
                {{
                    $msg = ""{1}"" -f $maxUserSize
                    throw $msg
                }}
            }}
        }}

        # Return a hashtable with the following members:
        #    BytesWritten - the number of bytes written to a file
        #
        function PSCopyFileToRemoteSession
        {{
            param(
                [string] $copyToFilePath,
                [string] $b64Fragment,
                [switch] $createFile = $false,
                [switch] $emptyFile = $false
            )

            $op = @{{
                BytesWritten = $null
            }}

            $wstream = $null

            try
            {{
                $filePathExists = Microsoft.PowerShell.Management\Test-Path -Path $copyToFilePath

                if ($createFile -or (! $filePathExists))
                {{
                    # If the file already exists, try to delete it.
                    if ($filePathExists)
                    {{
                        Microsoft.PowerShell.Management\Remove-Item -Path $copyToFilePath -Force -ea SilentlyContinue
                    }}

                    # Create the new file.
                    $fileInfo = Microsoft.PowerShell.Management\New-Item -Path $copyToFilePath -Type File -Force

                    if ($emptyFile)
                    {{
                        # Handle the empty file scenario.
                        $op['BytesWritten'] = 0
                        return $op
                    }}
                }}

                # Resolve path in case it is a PSDrive
                $resolvedPath = Microsoft.PowerShell.Management\Resolve-Path -literal $copyToFilePath

                # Decode
                $fragment = [System.Convert]::FromBase64String($b64Fragment)

                # Check if drive specifies max size and if max size is exceeded
                CheckPSDriveSize $resolvedPath $fragment.Length

                # Write fragment
                $wstream = Microsoft.PowerShell.Utility\New-Object -TypeName IO.FileStream -ArgumentList ($resolvedPath.ProviderPath), ([System.IO.FileMode]::Append)
                $wstream.Write($fragment, 0, $fragment.Length)

                $op['BytesWritten'] = $fragment.Length
            }}
            catch
            {{
                if ($_.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                }}
            }}
            finally
            {{
                if ($null -ne $wstream)
                {{
                    $wstream.Dispose()
                }}
            }}

            return $op
        }}

        # Returns a hashtable with the following members:
        #    BytesWritten - number of bytes written to an alternate file stream
        #
        function PSCopyFileAlternateStreamToRemoteSession
        {{
            param (
                [string] $copyToFilePath,
                [string] $b64Fragment,
                [string] $streamName
            )

            $op = @{{
                BytesWritten = $null
            }}

            try
            {{
                # Resolve path in case it is a PSDrive
                $resolvedPath = Microsoft.PowerShell.Management\Resolve-Path -literal $copyToFilePath

                # Decode
                $fragment = [System.Convert]::FromBase64String($b64Fragment)

                # Check if drive specifies max size and if max size is exceeded
                CheckPSDriveSize $resolvedPath $fragment.Length

                # Write the stream
                Microsoft.PowerShell.Management\Add-Content -Path ($resolvedPath.ProviderPath) -Value $fragment -Encoding Byte -Stream $streamName -ErrorAction Stop
                $op['BytesWritten'] = $fragment.Length
            }}
            catch
            {{
                Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
            }}

            return $op
        }}

        # Returns a hashtable with the following member:
        #    TargetSupportsAlternateStreams - boolean to keep track of whether the target supports Alternate data streams.
        #
        function PSTargetSupportsAlternateStreams
        {{
            param (
                [string] $supportAltStreamPath
            )

            $result = @{{
                TargetSupportsAlternateStreams = $false
            }}

            # Resolve path in case it is a PSDrive
            $resolvedPath = Microsoft.PowerShell.Management\Resolve-Path -literal $supportAltStreamPath

            $targetDrive = [IO.Path]::GetPathRoot($resolvedPath.ProviderPath)
            if (-not $targetDrive)
            {{
                return $result
            }}

            # Check if the target drive is NTFS
            $driveFormat = 'NTFS'
            foreach ($drive in [System.IO.DriveInfo]::GetDrives())
            {{
                if (($drive.Name -eq $targetDrive) -and ($drive.DriveFormat -eq $driveFormat))
                {{
                    # Now, check if the target supports Add-Command -Stream. This functionality was introduced in version 3.0.
                    $addContentCmdlet = Microsoft.PowerShell.Core\Get-Command Microsoft.PowerShell.Management\Add-Content -ErrorAction SilentlyContinue
                    if ($addContentCmdlet.Parameters.Keys -contains 'Stream')
                    {{
                        $result['TargetSupportsAlternateStreams'] = $true
                        break
                    }}
                }}
            }}

            return $result
        }}

        # Sets the metadata for the given file.
        #
        function PSSetFileMetadata
        {{
            param (
                [string] $metaDataFilePath,
                [hashtable] $metaDataToSet
            )

            $item = Microsoft.PowerShell.Management\get-item $metaDataFilePath -ea SilentlyContinue -Force

            if ($item)
            {{
                # LastWriteTime
                if ($metaDataToSet['LastWriteTimeUtc'])
                {{
                    $item.LastWriteTimeUtc = $metaDataToSet['LastWriteTimeUtc']
                }}

                if ($metaDataToSet['LastWriteTime'])
                {{
                    $item.LastWriteTime = $metaDataToSet['LastWriteTime']
                }}

                # Attributes
                if ($metaDataToSet['Attributes'])
                {{
                    $item.Attributes = $metaDataToSet['Attributes']
                }}
            }}
        }}

        # Returns a hashtable with the following member:
        #    IsFileInfo - boolean to keep track of whether the given path is a remote file.
        #    IsDirectoryInfo - boolean to keep track of whether the given path is a remote directory.
        #    ParentIsDirectoryInfo - boolean to keep track of whether the given parent path is a remote directory.
        #
        function PSGetRemotePathInfo
        {{
            param (
                [string] $remotePath
            )

            try
            {{

                try
                {{
                    $parentPath = Microsoft.PowerShell.Management\Split-Path $remotePath
                }}
                # catch everything and ignore the error.
                catch {{}}

                $result = @{{
                    IsFileInfo = (Microsoft.PowerShell.Management\Test-Path $remotePath -PathType Leaf)
                    IsDirectoryInfo = (Microsoft.PowerShell.Management\Test-Path $remotePath -PathType Container)
                }}

                if ($parentPath)
                {{
                    $result['ParentIsDirectoryInfo'] = (Microsoft.PowerShell.Management\Test-Path $parentPath -PathType Container)
                }}

            }}
            catch
            {{
                if ($_.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                }}
            }}

            return $result
        }}

        # Returns a hashtable with the following information:
        #  - IsFileInfotrue bool to keep track if the given destination is a FileInfo type.
        function PSRemoteDestinationPathIsFile
        {{
            param (
                [string] $isFilePath
            )

            $op = @{{
                IsFileInfo = $null
            }}

            try
            {{
                $op['IsFileInfo'] = (Microsoft.PowerShell.Management\Test-Path $isFilePath -PathType Leaf)
            }}
            catch
            {{
                if ($_.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                }}
            }}

            return $op
        }}

        # Return a hash table in the following format:
        #      DirectoryPath is the directory to be created.
        #      PathExists is a bool to to keep track of whether the directory already exist.
        #
        # 1) If DirectoryPath already exists:
        #     a) If -Force is specified, force create the directory. Set DirectoryPath to the created directory path.
        #     b) If not -Force is specified, then set PathExists to $true.
        # 2) If DirectoryPath does not exist, create it. Set DirectoryPath to the created directory path.
        function PSCreateDirectoryOnRemoteSession
        {{
            param (
                [string] $createDirectoryPath,
                [switch] $force = $false
            )

            $op = @{{
                DirectoryPath = $null
                PathExists = $false
            }}

            try
            {{
                if (Microsoft.PowerShell.Management\Test-Path $createDirectoryPath)
                {{
                    # -Force is specified, then force create the directory.
                    if ($force)
                    {{
                        Microsoft.PowerShell.Management\New-Item $createDirectoryPath -ItemType Directory -Force | Out-Null
                        $op['DirectoryPath'] = $createDirectoryPath
                    }}
                    else
                    {{
                        $op['PathExists'] = $true
                    }}
                }}
                else
                {{
                    Microsoft.PowerShell.Management\New-Item $createDirectoryPath -ItemType Directory | Out-Null
                    $op['DirectoryPath'] = $createDirectoryPath
                }}
            }}
            catch
            {{
                if ($_.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                }}
            }}

            return $op
        }}

        #
        # Call helper function based on bound parameter set
        #
        $params = $PSCmdlet.MyInvocation.BoundParameters
        switch ($PSCmdlet.ParameterSetName)
        {{
            ""PSCopyFileToRemoteSession""
            {{
                return PSCopyFileToRemoteSession @params
            }}

            ""PSCopyAlternateStreamToRemoteSession""
            {{
                return PSCopyFileAlternateStreamToRemoteSession @params
            }}

            ""PSTargetSupportsAlternateStreams""
            {{
                return PSTargetSupportsAlternateStreams @params
            }}

            ""PSSetFileMetadata""
            {{
                return PSSetFileMetadata @params
            }}

            ""PSRemoteDestinationPathIsFile""
            {{
                return PSRemoteDestinationPathIsFile @params
            }}

            ""PSGetRemotePathInfo""
            {{
                return PSGetRemotePathInfo @params
            }}

            ""PSCreateDirectoryOnRemoteSession""
            {{
                return PSCreateDirectoryOnRemoteSession @params
            }}
        }}
        ";

        private static string s_PSCopyToSessionHelper = functionToken + PSCopyToSessionHelperName + @"
        {
        " + s_PSCopyToSessionHelperDefinition + @"
        }
        ";

        private static Hashtable s_PSCopyToSessionHelperFunction = new Hashtable() {
            {nameToken, PSCopyToSessionHelperName},
            {definitionToken, s_PSCopyToSessionHelperDefinitionRestricted}
        };

        #endregion

        #region PSCopyFromSessionHelper

        internal const string PSCopyFromSessionHelperName = @"PSCopyFromSessionHelper";
        private static string s_PSCopyFromSessionHelperDefinition = StringUtil.Format(PSCopyFromSessionHelperDefinitionFormat, @"[ValidateNotNullOrEmpty()]");
        private static string s_PSCopyFromSessionHelperDefinitionRestricted = StringUtil.Format(PSCopyFromSessionHelperDefinitionFormat, @"[ValidateUserDrive()]");
        private const string PSCopyFromSessionHelperDefinitionFormat = @"
        param (
            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"", Mandatory=$true)]
            {0}

            [string] $copyFromFilePath,

            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"", Mandatory=$true)]
            [ValidateRange(0, [long]::MaxValue)]
            [long] $copyFromStart,

            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"", Mandatory=$true)]
            [ValidateRange(0, [long]::MaxValue)]
            [long] $copyFromNumBytes,

            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"")]
            [switch] $force,

            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"")]
            [switch] $isAlternateStream,

            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"")]
            [ValidateNotNullOrEmpty()]
            [string] $streamName,

            [Parameter(ParameterSetName=""PSSourceSupportsAlternateStreams"", Mandatory=$true)]
            {0}

            [string] $supportAltStreamPath,

            [Parameter(ParameterSetName=""PSGetFileMetadata"", Mandatory=$true)]
            {0}

            [string] $getMetaFilePath,

            [Parameter(ParameterSetName=""PSGetPathItems"", Mandatory=$true)]
            {0}

            [string] $getPathItems,

            [Parameter(ParameterSetName=""PSGetPathDirAndFiles"", Mandatory=$true)]
            {0}

            [string] $getPathDir
        )

        # A hash table with the following members is returned:
        #   - moreAvailable bool to keep track of whether there is more data available
        #   - b64Fragment to track of the number of bytes.
        #   - ExceptionThrown bool to keep track if an exception was thrown
        function PSCopyFileFromRemoteSession
        {{
            param(
                [string] $copyFromFilePath,
                [long] $copyFromStart,
                [long] $copyFromNumbytes,
                [switch] $force = $false,
                [switch] $isAlternateStream = $false,
                [string] $streamName
            )

            $finalResult = @{{
                b64Fragment = $null
                moreAvailable = $null
                ExceptionThrown = $false
            }}

            function PerformCopyFileFromRemoteSession
            {{
                param(
                    [string] $filePath,
                    [long] $start,
                    [long] $numBytes,
                    [switch] $isAlternateStream,
                    [string] $streamName
                )

                $op = @{{
                    b64Fragment = $null
                    moreAvailable = $false
                }}

                # Ensure bytes read is less than Max allowed
                $maxBytes = 10 * 1024 * 1024
                $numBytes = [Math]::Min($numBytes, $maxBytes)

                $rstream = $null
                try
                {{
                    if ($isAlternateStream)
                    {{
                        $content = Microsoft.PowerShell.Management\Get-Content $filePath -stream $streamName -Encoding Byte -Raw
                        $rstream = [System.IO.MemoryStream]::new($content)
                    }}
                    else
                    {{
                        $rstream = [System.IO.File]::OpenRead($filePath)
                    }}

                    # Create a new array to hold the file content
                    if ($start -lt $rstream.Length)
                    {{
                        $o = $rstream.Seek($start, 0)
                        $toRead = [Math]::Min($numBytes, $rstream.Length - $start)
                        $fragment = Microsoft.PowerShell.Utility\New-Object byte[] $toRead
                        $readsoFar = 0
                        while ($readsoFar -lt $toRead)
                        {{
                            $read = $rstream.Read($fragment, $readSoFar, $toRead - $readsoFar)
                            $readsoFar += $read
                        }}

                        $op['b64Fragment'] = [System.Convert]::ToBase64String($fragment)
                        if (($start + $readsoFar) -lt $rstream.Length)
                        {{
                            $op['moreAvailable'] = $true
                        }}
                    }}

                    $op
                }}
                finally
                {{
                    if ($null -ne $rstream)
                    {{
                        $rstream.Dispose()
                    }}
                }}
            }}

            function WriteException
            {{
                param ($ex)

                if ($ex.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $ex.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $ex.Exception
                }}

                $finalResult.ExceptionThrown = $true
            }}

            # Resolve path in case it is a PSDrive
            $resolvedFilePath = (Microsoft.PowerShell.Management\Resolve-Path -literal $copyFromFilePath).ProviderPath

            $unAuthorizedAccessException = $null
            $result = $null

            $isReadOnly = $false
            $isHidden = $false
            try
            {{
                $result = PerformCopyFileFromRemoteSession -filePath $resolvedFilePath -start $copyFromStart -numBytes $copyFromNumBytes -isAlternateStream:$isAlternateStream -streamName $streamName
                $finalResult.b64Fragment =  $result.b64Fragment
                $finalResult.moreAvailable =  $result.moreAvailable
            }}
            catch [System.UnauthorizedAccessException]
            {{
                $unAuthorizedAccessException = $_
                if ($force)
                {{
                    $exception = $null
                    try
                    {{
                        # Disable the readonly and hidden attributes and try again
                        $item = Microsoft.PowerShell.Management\Get-Item $resolvedFilePath

                        if ($item.Attributes.HasFlag([System.IO.FileAttributes]::Hidden))
                        {{
                            $isHidden = $true
                            $item.Attributes = $item.Attributes -band (-bnot ([System.IO.FileAttributes]::Hidden))
                        }}

                        if ($item.Attributes.HasFlag([System.IO.FileAttributes]::ReadOnly))
                        {{
                            $isReadOnly = $true
                            $item.Attributes = $item.Attributes -band (-bnot ([System.IO.FileAttributes]::ReadOnly))
                        }}

                        $result = PerformCopyFileFromRemoteSession -filePath $resolvedFilePath -start $copyFromStart -numBytes $copyFromNumBytes -isAlternateStream:$isAlternateStream
                        $finalResult.b64Fragment =  $result.b64Fragment
                        $finalResult.moreAvailable =  $result.moreAvailable
                    }}
                    catch
                    {{
                        $e = $_
                        if (($e.Exception.InnerException -is [System.IO.FileNotFoundException]) -or
                            ($e.Exception.InnerException -is [System.IO.DirectoryNotFoundException]) -or
                            ($e.Exception.InnerException -is [System.Security.SecurityException] ) -or
                            ($e.Exception.InnerException -is [System.ArgumentException]) -or
                            ($e.Exception.InnerException -is [System.IO.IOException]))
                        {{
                            # Write out the original error since we failed to force the copy
                            WriteException $unAuthorizedAccessException
                        }}
                        else
                        {{
                            WriteException $e
                        }}

                        $finalResult.ExceptionThrown = $true
                    }}
                }}
                else
                {{
                    $finalResult.ExceptionThrown = $true
                    WriteException $unAuthorizedAccessException
                }}
            }}
            catch
            {{
                WriteException $_
            }}
            finally
            {{
                if ($isReadOnly)
                {{
                    $item.Attributes = $item.Attributes -bor [System.IO.FileAttributes]::ReadOnly
                }}

                if ($isHidden)
                {{
                    $item.Attributes = $item.Attributes -bor [System.IO.FileAttributes]::Hidden
                }}
            }}

            return $finalResult
        }}

        # Returns a hashtable with the following members:
        #    SourceSupportsAlternateStreams - boolean to keep track of whether the source supports Alternate data streams.
        #    Streams - the list of alternate streams
        #
        function PSSourceSupportsAlternateStreams
        {{
            param ([string]$supportAltStreamPath)

            $result = @{{
                SourceSupportsAlternateStreams = $false
                Streams = @()
            }}

            # Check if the source supports 'Get-Content -Stream'. This functionality was introduced in version 3.0.
            $getContentCmdlet = Microsoft.PowerShell.Core\Get-Command Microsoft.PowerShell.Management\Get-Content -ErrorAction SilentlyContinue
            if ($getContentCmdlet.Parameters.Keys -notcontains 'Stream')
            {{
                return $result
            }}

            $result['SourceSupportsAlternateStreams'] = $true

            # Check if the file has any alternate data streams.
            $item = Microsoft.PowerShell.Management\Get-Item -Path $supportAltStreamPath -Stream * -ea SilentlyContinue
            if (-not $item)
            {{
                return $result
            }}

            foreach ($streamName in $item.Stream)
            {{
                if ($streamName -ne ':$DATA')
                {{
                    $result['Streams'] += $streamName
                }}
            }}

            return $result
        }}

        # Returns a hash table with metadata info about the file for the given path.
        #
        function PSGetFileMetadata
        {{
            param ($getMetaFilePath)

            if (-not (Microsoft.PowerShell.Management\Test-Path $getMetaFilePath))
            {{
                return
            }}

            $item = Microsoft.PowerShell.Management\Get-Item $getMetaFilePath -Force -ea SilentlyContinue
            if ($item)
            {{
                $metadata = @{{}}

                # Attributes
                $attributes = @($item.Attributes.ToString().Split(',').Trim())
                if ($attributes.Count -gt 0)
                {{
                    $metadata.Add('Attributes', $attributes)
                }}

                # LastWriteTime
                $metadata.Add('LastWriteTime', $item.LastWriteTime)
                $metadata.Add('LastWriteTimeUtc', $item.LastWriteTimeUtc)

                return $metadata
            }}
        }}

        # Converts file system path to PSDrive path
        #    Returns converted path or original path if not conversion is needed.
        function ConvertToPSDrivePath
        {{
            param (
                [System.Management.Automation.PSDriveInfo] $driveInfo,
                [string] $pathToConvert
            )

            if (!($driveInfo) -or !($driveInfo.Name) -or !($driveInfo.Root))
            {{
                return $pathToConvert
            }}

            if (! ($driveInfo.Root.StartsWith($driveInfo.Name)))
            {{
                return $pathToConvert.ToUpper().Replace($driveInfo.Root.ToUpper(), (($driveInfo.Name) + "":""))
            }}

            return $pathToConvert
        }}

        ## A hashtable is returned in the following format:
        ##  Exists - Boolean to keep track if the given path exists.
        ##  Items  - The items that Get-Item -Path $path resolves to.
        function PSGetPathItems
        {{
            param (
                [string] $getPathItems
            )

            $op = @{{
                Exists = $null
                Items = $null
            }}

            try
            {{
                if (-not (Microsoft.PowerShell.Management\Test-Path $getPathItems))
                {{
                    $op['Exists'] = $false
                    return $op
                }}

                $items = @(Microsoft.PowerShell.Management\Get-Item -Path $getPathItems | Microsoft.PowerShell.Core\ForEach-Object {{
                    @{{
                        FullName = ConvertToPSDrivePath $_.PSDrive $_.FullName;
                        Name = $_.Name;
                        FileSize = $_.Length; IsDirectory = $_ -is [System.IO.DirectoryInfo]
                     }}
                }})
                $op['Exists'] = $true
                $op['Items'] = $items

                return $op
            }}
            catch
            {{
                if ($_.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                }}
            }}

            return $op
        }}

        # Return a hashtable with the following members:
        # Files - Array with file fullnames, and their sizes
        # Directories - Array of child directory fullnames
        function PSGetPathDirAndFiles
        {{
            param (
                [string] $getPathDir
            )

            $result = @()

            $op = @{{
                Files = $null
                Directories = $null
            }}

            try
            {{
                $item = Microsoft.PowerShell.Management\Get-Item $getPathDir

                if ($item -isnot [System.IO.DirectoryInfo])
                {{
                    return $op
                }}

                $files = @(Microsoft.PowerShell.Management\Get-ChildItem -Path $getPathDir -File | Microsoft.PowerShell.Core\ForEach-Object {{
                    @{{ FileName = $_.Name;
                        FilePath = (ConvertToPSDrivePath $_.PSDrive $_.FullName);
                        FileSize = $_.Length
                     }}
                }})

                $directories = @(Microsoft.PowerShell.Management\Get-ChildItem -Path $getPathDir -Directory | Microsoft.PowerShell.Core\ForEach-Object {{
                    @{{ Name = $_.Name;
                        FullName = (ConvertToPSDrivePath $_.PSDrive $_.FullName)
                     }}
                }})

                if ($files.count -gt 0)
                {{
                    $op['Files'] = $files
                }}

                if ($directories.count -gt 0)
                {{
                    $op['Directories'] = $directories
                }}
            }}
            catch
            {{
                if ($_.Exception.InnerException)
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                }}
                else
                {{
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                }}
            }}

            return $op
        }}

        #
        # Call helper function based on bound parameter set
        #
        $params = $PSCmdlet.MyInvocation.BoundParameters
        switch ($PSCmdlet.ParameterSetName)
        {{
            ""PSCopyFileFromRemoteSession""
            {{
                return PSCopyFileFromRemoteSession @params
            }}

            ""PSSourceSupportsAlternateStreams""
            {{
                PSSourceSupportsAlternateStreams @params
            }}

            ""PSGetFileMetadata""
            {{
                PSGetFileMetadata @params
            }}

            ""PSGetPathItems""
            {{
                PSGetPathItems @params
            }}

            ""PSGetPathDirAndFiles""
            {{
                PSGetPathDirAndFiles @params
            }}
        }}
        ";

        internal static string PSCopyFromSessionHelper = functionToken + PSCopyFromSessionHelperName + @"
        {
        " + s_PSCopyFromSessionHelperDefinition + @"
        }
        ";

        private static Hashtable s_PSCopyFromSessionHelperFunction = new Hashtable() {
            {nameToken, PSCopyFromSessionHelperName},
            {definitionToken, s_PSCopyFromSessionHelperDefinitionRestricted}
        };

        #endregion

        #region PSCopyRemoteUtils

        internal const string PSCopyRemoteUtilsName = @"PSCopyRemoteUtils";
        internal static string PSCopyRemoteUtilsDefinition = StringUtil.Format(PSCopyRemoteUtilsDefinitionFormat, @"[ValidateNotNullOrEmpty()]", PSValidatePathFunction);
        private static string s_PSCopyRemoteUtilsDefinitionRestricted = StringUtil.Format(PSCopyRemoteUtilsDefinitionFormat, @"[ValidateUserDrive()]", PSValidatePathFunction);
        private const string PSCopyRemoteUtilsDefinitionFormat = @"
        param (
            [Parameter(ParameterSetName=""PSRemoteDirectoryExist"", Mandatory=$true)]
            {0}

            [string] $dirPathExists,

            [Parameter(ParameterSetName=""PSValidatePath"", Mandatory=$true)]
            {0}

            [string] $pathToValidate,

            [Parameter(ParameterSetName=""PSValidatePath"")]
            [switch] $sourceIsRemote
        )

        # Returns a hashtable with the following member:
        #    Exists - boolean to keep track of whether the given path exists for a remote directory.
        #
        function PSRemoteDirectoryExist
        {{
            param (
                [string] $dirPathExists
            )

            $result = @{{ Exists = (Microsoft.PowerShell.Management\Test-Path $dirPathExists -PathType Container) }}

            return $result
        }}

        {1}

        #
        # Call helper function based on bound parameter set
        #
        $params = $PSCmdlet.MyInvocation.BoundParameters
        switch ($PSCmdlet.ParameterSetName)
        {{
            ""PSRemoteDirectoryExist""
            {{
                return PSRemoteDirectoryExist @params
            }}

            ""PSValidatePath""
            {{
                return PSValidatePath @params
            }}
        }}
        ";

        private const string PSValidatePathFunction = functionToken + "PSValidatePath" + @"
        {
        " + PSValidatePathDefinition + @"
        }
        ";

        internal const string PSValidatePathDefinition = @"
        # Return hashtable in the following format:
        #   Exists - boolean to keep track if the given path exists
        #   Root - the root for the given path. If wildcards are used, it returns the first drive root.
        #   IsAbsolute - boolean to keep track of whether the given path is absolute
        param (
            [string] $pathToValidate,
            [switch] $sourceIsRemote
        )

        function SafeGetDriveRoot
        {
            param (
                [System.Management.Automation.PSDriveInfo] $driveInfo
            )

            if (! ($driveInfo.Root.StartsWith($driveInfo.Name)))
            {
                return (($driveInfo.Name) + "":"")
            }
            else
            {
                $driveInfo.Root
            }
        }

        $result = @{
            Exists = $null
            Root = $null
            IsAbsolute = $null
        }

        # Validate if the path is absolute
        $result['IsAbsolute'] = (Microsoft.PowerShell.Management\Split-Path $pathToValidate -IsAbsolute)
        if (-not $result['IsAbsolute'])
        {
            return $result
        }

        # Check if the given path exists.
        $result['Exists'] = (Microsoft.PowerShell.Management\Test-Path $pathToValidate)

        # If $pathToValidate is a remote source, and it does not exist, return.
        if ($sourceIsRemote -and (-not $result['Exists']))
        {
            return $result
        }

        # If the path does not exist, check if we can find its root.
        if (-not (Microsoft.PowerShell.Management\Test-Path $pathToValidate))
        {
            $possibleRoot = $null

            try
            {
                $possibleRoot = [System.IO.Path]::GetPathRoot($pathToValidate)
            }
            # Catch everything and ignore the error.
            catch {}

            if (-not $possibleRoot)
            {
                return $result
            }

            # Now use this path to find its root.
            $pathToValidate = $possibleRoot
        }

        # Get the root path using Get-Item
        $item = Microsoft.PowerShell.Management\Get-Item $pathToValidate -ea SilentlyContinue
        if (($null -ne $item) -and ($item[0].PSProvider.Name -eq 'FileSystem'))
        {
            $result['Root'] = SafeGetDriveRoot $item[0].PSDrive
            return $result
        }

        # If this fails, try to get them via Get-PSDrive
        $fileSystemDrives = @(Microsoft.PowerShell.Management\Get-PSDrive -PSProvider FileSystem -ea SilentlyContinue)

        # If this fails, try to get them via Get-PSProvider
        if ($fileSystemDrives.Count -eq 0)
        {
            $fileSystemDrives = @((Microsoft.PowerShell.Management\Get-PSProvider -PSProvider FileSystem -ea SilentlyContinue).Drives)
        }

        foreach ($drive in  $fileSystemDrives)
        {
            if ($pathToValidate.StartsWith($drive.Root))
            {
                $result['Root'] = SafeGetDriveRoot $drive
                break
            }
        }

        return $result
        ";

        internal static string PSCopyRemoteUtils = functionToken + PSCopyRemoteUtilsName + @"
        {
        " + PSCopyRemoteUtilsDefinition + @"
        }
        ";

        internal static Hashtable PSCopyRemoteUtilsFunction = new Hashtable() {
            {nameToken, PSCopyRemoteUtilsName},
            {definitionToken, s_PSCopyRemoteUtilsDefinitionRestricted}
        };

        #endregion

        internal static string AllCopyToRemoteScripts = s_PSCopyToSessionHelper + PSCopyRemoteUtils;
        internal static IEnumerable<Hashtable> GetAllCopyToRemoteScriptFunctions()
        {
            yield return s_PSCopyToSessionHelperFunction;
            yield return PSCopyRemoteUtilsFunction;
        }

        internal static string AllCopyFromRemoteScripts = PSCopyFromSessionHelper + PSCopyRemoteUtils;
        internal static IEnumerable<Hashtable> GetAllCopyFromRemoteScriptFunctions()
        {
            yield return s_PSCopyFromSessionHelperFunction;
            yield return PSCopyRemoteUtilsFunction;
        }
    }

    #endregion
}

