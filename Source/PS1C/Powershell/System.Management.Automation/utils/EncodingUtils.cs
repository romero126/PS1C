// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using System.Management.Automation.Internal;

using PS1C;

namespace System.Management.Automation
{
    internal static class EncodingConversion
    {
        internal const string Unknown = "unknown";
        internal const string String = "string";
        internal const string Unicode = "unicode";
        internal const string BigEndianUnicode = "bigendianunicode";
        internal const string Ascii = "ascii";
        internal const string Utf8 = "utf8";
        internal const string Utf8NoBom = "utf8NoBOM";
        internal const string Utf8Bom = "utf8BOM";
        internal const string Utf7 = "utf7";
        internal const string Utf32 = "utf32";
        internal const string Default = "default";
        internal const string OEM = "oem";
        internal static readonly string[] TabCompletionResults = {
                Ascii, BigEndianUnicode, OEM, Unicode, Utf7, Utf8, Utf8Bom, Utf8NoBom, Utf32
            };

        internal static Dictionary<string, Encoding> encodingMap = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase)
        {
            { Ascii, System.Text.Encoding.ASCII },
            { BigEndianUnicode, System.Text.Encoding.BigEndianUnicode },
            { Default, System.Text.Encoding.Default },
            { OEM, System.Text.Encoding.Default },
            { Unicode, System.Text.Encoding.Unicode },
            { Utf7, System.Text.Encoding.UTF7 },
            { Utf8, System.Text.Encoding.UTF8 },
            { Utf8Bom, System.Text.Encoding.UTF8 },
            { Utf8NoBom, System.Text.Encoding.UTF8 },
            { Utf32, System.Text.Encoding.UTF32 },
            { String, System.Text.Encoding.Unicode },
            { Unknown, System.Text.Encoding.Unicode },
        };

        /// <summary>
        /// Retrieve the encoding parameter from the command line
        /// it throws if the encoding does not match the known ones.
        /// </summary>
        /// <returns>A System.Text.Encoding object (null if no encoding specified).</returns>
        internal static Encoding Convert(Cmdlet cmdlet, string encoding)
        {
            if (string.IsNullOrEmpty(encoding))
            {
                // no parameter passed, default to UTF8
                return ClrFacade.GetDefaultEncoding();
            }

            Encoding foundEncoding;
            if (encodingMap.TryGetValue(encoding, out foundEncoding))
            {
                return foundEncoding;
            }

            // error condition: unknown encoding value
            string validEncodingValues = string.Join(", ", TabCompletionResults);
            string msg = StringUtil.Format(PathUtilsStrings.OutFile_WriteToFileEncodingUnknown, encoding, validEncodingValues);

            ErrorRecord errorRecord = new ErrorRecord(
                PSTraceSource.NewArgumentException("Encoding"),
                "WriteToFileEncodingUnknown",
                ErrorCategory.InvalidArgument,
                null);

            errorRecord.ErrorDetails = new ErrorDetails(msg);
            cmdlet.ThrowTerminatingError(errorRecord);

            return null;
        }
    }

}