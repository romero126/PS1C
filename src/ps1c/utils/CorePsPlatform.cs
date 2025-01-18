// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32;
//using Microsoft.Win32.Registry;
using Microsoft.Win32.SafeHandles;

namespace PS1C
{

    /// <summary>
    /// These are platform abstractions and platform specific implementations.
    /// </summary>
    public static class Platform
    {
        private static string _tempDirectory = null;

        /// <summary>
        /// True if the current platform is Linux.
        /// </summary>
        public static bool IsLinux
        {
            get
            {
                #if NET6_0_OR_GREATER
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
                #else
                return false;
                #endif
            }
        }

        /// <summary>
        /// True if the current platform is macOS.
        /// </summary>
        public static bool IsMacOS
        {
            get
            {
                #if NET6_0_OR_GREATER
                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
                #else
                return false;
                #endif
            }
        }

        /// <summary>
        /// True if the current platform is Windows.
        /// </summary>
        public static bool IsWindows
        {
            get
            {
                #if NET6_0_OR_GREATER
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                #else
                return true;
                #endif
            }
        }

        /// <summary>
        /// True if the underlying system is NanoServer.
        /// </summary>
        public static bool IsNanoServer
        {
            get
            {
#if UNIX
                return false;
#else
                if (_isNanoServer.HasValue) { return _isNanoServer.Value; }

                _isNanoServer = false;
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Server\ServerLevels"))
                {
                    if (regKey != null)
                    {
                        object value = regKey.GetValue("NanoServer");
                        if (value != null && regKey.GetValueKind("NanoServer") == RegistryValueKind.DWord)
                        {
                            _isNanoServer = (int)value == 1;
                        }
                    }
                }

                return _isNanoServer.Value;
#endif
            }
        }

        /// <summary>
        /// True if the underlying system is IoT.
        /// </summary>
        public static bool IsIoT
        {
            get
            {
#if UNIX
                return false;
#else
                if (_isIoT.HasValue) { return _isIoT.Value; }

                _isIoT = false;
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (regKey != null)
                    {
                        object value = regKey.GetValue("ProductName");
                        if (value != null && regKey.GetValueKind("ProductName") == RegistryValueKind.String)
                        {
                            _isIoT = string.Equals("IoTUAP", (string)value, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }

                return _isIoT.Value;
#endif
            }
        }

        /// <summary>
        /// True if underlying system is Windows Desktop.
        /// </summary>
        public static bool IsWindowsDesktop
        {
            get
            {
#if UNIX
                return false;
#else
                if (_isWindowsDesktop.HasValue) { return _isWindowsDesktop.Value; }

                _isWindowsDesktop = !IsNanoServer && !IsIoT;
                return _isWindowsDesktop.Value;
#endif
            }
        }

#if UNIX
        // Gets the location for cache and config folders.
        internal static readonly string CacheDirectory = Platform.SelectProductNameForDirectory(Platform.XDG_Type.CACHE);
        internal static readonly string ConfigDirectory = Platform.SelectProductNameForDirectory(Platform.XDG_Type.CONFIG);
#else
        // Gets the location for cache and config folders.
        internal static readonly string CacheDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\PowerShell";
        internal static readonly string ConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\PowerShell";

        private static bool? _isNanoServer = null;
        private static bool? _isIoT = null;
        private static bool? _isWindowsDesktop = null;
#endif
    }

}