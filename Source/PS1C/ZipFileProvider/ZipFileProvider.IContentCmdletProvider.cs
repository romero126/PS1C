using PS1C.Archive;
using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;
using System.Text;
using System.Resources;
using System.Globalization;
using PS1C.Archive;

namespace PS1C
{
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        #region IContentCmdletProvider
        
        /// <summary>
        /// Creates an instance of the FileSystemContentStream class, opens
        /// the specified file for reading, and returns the IContentReader interface
        /// to it.
        /// </summary>
        /// <param name="path">
        /// The path of the file to be opened for reading.
        /// </param>
        /// <returns>
        /// An IContentReader for the specified file.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        public IContentReader GetContentReader(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            path = NormalizePath(path);

            // Defaults for the file read operation
            string delimiter = "\n";

            // Encoding encoding = Encoding.Default;
            Encoding encoding = ClrFacade.GetDefaultEncoding();

            bool streamTypeSpecified = false;
            bool usingByteEncoding = false;
            bool delimiterSpecified = false;
            bool isRawStream = false;

            // Get the dynamic parameters.
            // They override the defaults specified above.
            if (DynamicParameters != null)
            {
                StreamContentReaderDynamicParameters dynParams = DynamicParameters as StreamContentReaderDynamicParameters;
                if (dynParams != null)
                {
                    // -raw is not allowed when -first,-last or -wait is specified
                    // this call will validate that and throws.
                    ValidateParameters(dynParams.Raw);

                    isRawStream = dynParams.Raw;

                    // Get the delimiter
                    delimiterSpecified = dynParams.DelimiterSpecified;
                    if (delimiterSpecified)
                        delimiter = dynParams.Delimiter;

                    // Get the stream type
                    usingByteEncoding = dynParams.AsByteStream;
                    streamTypeSpecified = dynParams.WasStreamTypeSpecified;

                    if (usingByteEncoding && streamTypeSpecified)
                    {
                        WriteWarning(FileSystemProviderStrings.EncodingNotUsed);
                    }

                    if (streamTypeSpecified)
                    {
                        encoding = dynParams.Encoding;
                    }

                }
            }
            StreamContentReaderWriter stream = null;

            ZipFileItemInfo archiveFile = GetItemHelper(path);
            //Archive.FileStream archiveStream = archiveFile.Open(FileMode.Append);

            try
            {
                // Users can't both read as bytes, and specify a delimiter
                if (delimiterSpecified)
                {
                    if (usingByteEncoding)
                    {
                        Exception e =
                            new ArgumentException(FileSystemProviderStrings.DelimiterError, "delimiter");
                        WriteError(new ErrorRecord(
                            e,
                            "GetContentReaderArgumentError",
                            ErrorCategory.InvalidArgument,
                            path));
                    }
                    else
                    {
                        //Console.WriteLine("Get-Content : Delimiter");
                        stream = new ZipFileContentStream(archiveFile, FileMode.Append, delimiter, encoding, usingByteEncoding, this, isRawStream);
                    }
                }
                else
                {
                    //Console.WriteLine("Get-Content : Default");
                    stream = new ZipFileContentStream(archiveFile, FileMode.Append, encoding, usingByteEncoding, this, isRawStream);
                }
            }
            catch (PathTooLongException pathTooLong)
            {
                WriteError(new ErrorRecord(pathTooLong, "GetContentReaderPathTooLongError", ErrorCategory.InvalidArgument, path));
            }
            catch (FileNotFoundException fileNotFound)
            {
                WriteError(new ErrorRecord(fileNotFound, "GetContentReaderFileNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (DirectoryNotFoundException directoryNotFound)
            {
                WriteError(new ErrorRecord(directoryNotFound, "GetContentReaderDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (ArgumentException argException)
            {
                WriteError(new ErrorRecord(argException, "GetContentReaderArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException ioException)
            {
                // IOException contains specific message about the error occured and so no need for errordetails.
                WriteError(new ErrorRecord(ioException, "GetContentReaderIOError", ErrorCategory.ReadError, path));
            }
            catch (System.Security.SecurityException securityException)
            {
                WriteError(new ErrorRecord(securityException, "GetContentReaderSecurityError", ErrorCategory.PermissionDenied, path));
            }
            catch (UnauthorizedAccessException unauthorizedAccess)
            {
                WriteError(new ErrorRecord(unauthorizedAccess, "GetContentReaderUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
            catch (Exception e)
            {
                WriteError(
                    new ErrorRecord(e, "Unhandled Error", ErrorCategory.InvalidArgument , path)
                );
            }

            if (stream == null)
            {
                throw new Exception("Invalid stream");
            }

            return stream;
        }

		public object GetContentReaderDynamicParameters(string path)
		{
            return new StreamContentReaderDynamicParameters();
		}


        /// <summary>
        /// Creates an instance of the FileSystemContentStream class, opens
        /// the specified file for writing, and returns the IContentReader interface
        /// to it.
        /// </summary>
        /// <param name="path">
        /// The path of the file to be opened for writing.
        /// </param>
        /// <returns>
        /// An IContentWriter for the specified file.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        /// </exception>
        public IContentWriter GetContentWriter(string path)
        {

            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }

            path = NormalizePath(path);

            // If this is true, then the content will be read as bytes
            bool usingByteEncoding = false;
            bool streamTypeSpecified = false;
            Encoding encoding = ClrFacade.GetDefaultEncoding();
            FileMode filemode = FileMode.OpenOrCreate;
            bool suppressNewline = false;

            // Get the dynamic parameters
            if (DynamicParameters != null)
            {

                // [BUG] Regardless of override DynamicParameters is of type FileSystemContentWriterDynamicParameters
                // StreamContentWriterDynamicParameters dynParams = DynamicParameters as StreamContentWriterDynamicParameters;
                FileSystemContentWriterDynamicParameters dynParams = DynamicParameters as FileSystemContentWriterDynamicParameters;
                                                                 
                if (dynParams != null)
                {
                    usingByteEncoding = dynParams.AsByteStream;
                    streamTypeSpecified = dynParams.WasStreamTypeSpecified;

                    if (usingByteEncoding && streamTypeSpecified)
                    {
                        WriteWarning(FileSystemProviderStrings.EncodingNotUsed);
                    }

                    if (streamTypeSpecified)
                    {
                        encoding = dynParams.Encoding;
                    }

                    suppressNewline = dynParams.NoNewline.IsPresent;
                }
            }

            StreamContentReaderWriter stream = null;

            ZipFileItemInfo archiveFile;
            if (ItemExists(path))
            {
                archiveFile = GetItemHelper(path);
            }
            else {
                // Set-Item should create an item if not exists.
                archiveFile = NewItemHelper(path);
            }

            try
            {
                stream = new ZipFileContentStream(archiveFile, FileMode.Append, encoding, usingByteEncoding, this, false, suppressNewline);
            }
            catch (PathTooLongException pathTooLong)
            {
                WriteError(new ErrorRecord(pathTooLong, "GetContentWriterPathTooLongError", ErrorCategory.InvalidArgument, path));
            }
            catch (FileNotFoundException fileNotFound)
            {
                WriteError(new ErrorRecord(fileNotFound, "GetContentWriterFileNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (DirectoryNotFoundException directoryNotFound)
            {
                WriteError(new ErrorRecord(directoryNotFound, "GetContentWriterDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (ArgumentException argException)
            {
                WriteError(new ErrorRecord(argException, "GetContentWriterArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException ioException)
            {
                // IOException contains specific message about the error occured and so no need for errordetails.
                WriteError(new ErrorRecord(ioException, "GetContentWriterIOError", ErrorCategory.WriteError, path));
            }
            catch (System.Security.SecurityException securityException)
            {
                WriteError(new ErrorRecord(securityException, "GetContentWriterSecurityError", ErrorCategory.PermissionDenied, path));
            }
            catch (UnauthorizedAccessException unauthorizedAccess)
            {
                WriteError(new ErrorRecord(unauthorizedAccess, "GetContentWriterUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }

            return stream;
        }
        public object GetContentWriterDynamicParameters(string path)
		{
			return new StreamContentWriterDynamicParameters();
		}

		public void ClearContent(string path)
		{
            StreamContentReaderWriter stream = null;
            ZipFileItemInfo archiveFile = GetItemHelper(path);
            archiveFile.ClearContent();

		}

        public object ClearContentDynamicParameters(string path)
		{
            return new StreamContentClearContentDynamicParameters();
		}
        #endregion

        /// <summary>
        /// -raw is not allowed when -first,-last or -wait is specified
        /// this call will validate that and throws.
        /// </summary>
        private void ValidateParameters(bool isRawSpecified)
        {
            if (isRawSpecified)
            {
                if (this.Context_MyInvocation.BoundParameters.ContainsKey("TotalCount"))
                {
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "TotalCount");
                    throw new PSInvalidOperationException(message);
                }
            

                if (this.Context_MyInvocation.BoundParameters.ContainsKey("Tail"))
                {
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Tail");
                    throw new PSInvalidOperationException(message);
                }

                if (this.Context_MyInvocation.BoundParameters.ContainsKey("Delimiter"))
                {
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Delimiter");
                    throw new PSInvalidOperationException(message);
                }
            }
        }

    }
}