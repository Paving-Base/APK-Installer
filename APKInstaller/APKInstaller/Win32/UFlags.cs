using System;

namespace APKInstaller.Win32
{
    [Flags]
    internal enum UFlags
    {
        /// <summary>
        ///     The hIcon member is valid.
        /// </summary>
        Icon = 2,

        /// <summary>
        ///     The uCallbackMessage member is valid.
        /// </summary>
        Message = 1,

        /// <summary>
        ///     The szTip member is valid.
        /// </summary>
        ToolTip = 4,

        /// <summary>
        ///     The dwState and dwStateMask members are valid.
        /// </summary>
        State = 8,

        /// <summary>
        ///     Use a balloon ToolTip instead of a standard ToolTip. The szInfo, uTimeout, szInfoTitle, and dwInfoFlags members are
        ///     valid.
        /// </summary>
        Balloon = 0x10
    }
}
