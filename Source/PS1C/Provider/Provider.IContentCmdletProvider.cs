using PS1C.Archive;
using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.PowerShell.Commands;
using System.Text;
using System.Resources;
using System.Globalization;


namespace PS1C
{
    //public class Provider : FileSystemProvider, IContentCmdletProvider
    public partial class Provider : NavigationCmdletProvider, IContentCmdletProvider
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
                //throw new PSArgumentException("path");
            }

            //path = NormalizePath(path);

            // Defaults for the file read operation
            string delimiter = "\n";

            Encoding encoding = Encoding.Default;
            bool waitForChanges = false;

            bool streamTypeSpecified = false;
            bool usingByteEncoding = false;
            bool delimiterSpecified = false;
            bool isRawStream = false;
            string streamName = null;

            // Get the dynamic parameters.
            // They override the defaults specified above.
            if (DynamicParameters != null)
            {
                StreamContentReaderDynamicParameters dynParams = DynamicParameters as StreamContentReaderDynamicParameters;
                if (dynParams != null)
                {
                    // -raw is not allowed when -first,-last or -wait is specified
                    // this call will validate that and throws.
                    //ValidateParameters(dynParams.Raw);

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
                        WriteWarning(ProviderStrings.EncodingNotUsed);
                    }

                    if (streamTypeSpecified)
                    {
                        encoding = dynParams.Encoding;
                    }

                }
            }
            StreamContentReaderWriter stream = null;

            Archive.FileInfo archiveFile = GetItemHelper(path);
            //Archive.FileStream archiveStream = archiveFile.Open(FileMode.Append);

            try
            {
                // Users can't both read as bytes, and specify a delimiter
                if (delimiterSpecified)
                {
                    if (usingByteEncoding)
                    {
                        Exception e =
                            new ArgumentException(ProviderStrings.DelimiterError, "delimiter");
                        WriteError(new ErrorRecord(
                            e,
                            "GetContentReaderArgumentError",
                            ErrorCategory.InvalidArgument,
                            path));
                    }
                    else
                    {
                        stream = new ArchiveContentStream(archiveFile, FileMode.Append, encoding, usingByteEncoding, this, isRawStream);
                        //stream = new StreamContentReaderWriter(archiveStream as System.IO.Stream, encoding, usingByteEncoding, this, isRawStream);
                    }
                }
                else
                {
                    stream = new ArchiveContentStream(archiveFile, FileMode.Append, encoding, usingByteEncoding, this, isRawStream);
                    //stream = new StreamContentReaderWriter(archiveStream as System.IO.Stream, encoding, usingByteEncoding, this, isRawStream);
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

            //path = NormalizePath(path);

            // If this is true, then the content will be read as bytes
            bool usingByteEncoding = false;
            bool streamTypeSpecified = false;
            Encoding encoding = ClrFacade.GetDefaultEncoding();
            FileMode filemode = FileMode.OpenOrCreate;
            string streamName = null;
            bool suppressNewline = false;

            // Get the dynamic parameters
            if (DynamicParameters != null)
            {
                StreamContentWriterDynamicParameters dynParams =
                    DynamicParameters as StreamContentWriterDynamicParameters;

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
            Archive.FileInfo archiveFile = GetItemHelper(path);
            //Archive.FileStream archiveStream = archiveFile.Open(FileMode.Append);
            // Todo:
            // Set-Item should create an item as well

            try
            {
                stream = new ArchiveContentStream(archiveFile, FileMode.Append, encoding, usingByteEncoding, this, false, suppressNewline);
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
            WriteVerbose("[Todo]: Provider GetContentWriterDynamicParameters(string path)");
			return new StreamContentWriterDynamicParameters();
		}

		public void ClearContent(string path)
		{
            WriteVerbose("[Todo]: Provider ClearContent(string path)");
		}

        public object ClearContentDynamicParameters(string path)
		{
            WriteVerbose("[Todo]: Provider ClearContentDynamicParameters(string path)");
            return new FileSystemClearContentDynamicParameters();
		}
        #endregion





    }
}