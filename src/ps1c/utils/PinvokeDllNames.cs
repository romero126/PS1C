// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace PS1C
{
    /// <summary>
    /// PinvokeDllNames contains the DLL names to be use for PInvoke in FullCLR/CoreCLR powershell.
    ///
    /// * When adding a new DLL name here, make sure that you add both the FullCLR and CoreCLR version
    ///   of it. Add the comment '/*COUNT*/' with the new DLL name, and make sure the 'COUNT' is the
    ///   same for both FullCLR and CoreCLR DLL names.
    /// </summary>
    internal static class PinvokeDllNames
    {
        internal const string GetCPInfoDllName = "api-ms-win-core-localization-l1-2-0.dll";
    }
}
