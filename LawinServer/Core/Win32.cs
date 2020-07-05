using System;
using System.Runtime.InteropServices;

namespace LawinServer
{
    internal static class Win32
    {
        /// <summary>
        /// Adds or removes an application-defined HandlerRoutine function from the list of handler functions for the calling process.
        /// </summary>
        /// <param name="HandlerRoutine">A pointer to the application-defined HandlerRoutine function to be added or removed.</param>
        /// <param name="Add">If this parameter is TRUE, the handler is added; if it is FALSE, the handler is removed.</param>
        /// <returns>If the function succeeds, the return value is nonzero, else zero.</returns>
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler HandlerRoutine, bool Add);

        /// <summary>
        /// An application-defined function used with the SetConsoleCtrlHandler function.
        /// </summary>
        /// <param name="dwCtrlType">The type of control signal received by the handler.</param>
        /// <returns>If the function handles the control signal, it should return TRUE. If it returns FALSE, the next handler function in the list of handlers for this process is used.</returns>
        public delegate bool SetConsoleCtrlEventHandler(CtrlType dwCtrlType);

        /// <summary>
        /// The type of control signal received by the handler.
        /// </summary>
        public enum CtrlType
        {
            /// <summary>
            /// A CTRL+C signal was received.
            /// </summary>
            CTRL_C_EVENT = 0,

            /// <summary>
            /// A CTRL+BREAK signal was received.
            /// </summary>
            CTRL_BREAK_EVENT = 1,

            /// <summary>
            /// A signal that the system sends to all processes attached to a console when the user closes the console.
            /// </summary>
            CTRL_CLOSE_EVENT = 2,

            /// <summary>
            /// A signal that the system sends to all console processes when a user is logging off.
            /// </summary>
            CTRL_LOGOFF_EVENT = 5,

            /// <summary>
            /// A signal that the system sends when the system is shutting down. 
            /// </summary>
            CTRL_SHUTDOWN_EVENT = 6
        }

        /// <summary>
        /// Set an internet option.
        /// </summary>
        /// <param name="hInternet">Handle on which to set information.</param>
        /// <param name="dwOption">Internet option to be set.</param>
        /// <param name="lpBuffer">Pointer to a buffer that contains the option setting.</param>
        /// <param name="dwBufferLength">Size of the lpBuffer buffer.</param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, InternetOptions dwOption, IntPtr lpBuffer, uint dwBufferLength);

        /// <summary>
        /// The following option flags are used with the InternetQueryOption and InternetSetOption functions.
        /// </summary>
        internal enum InternetOptions : int
        {
            /// <summary>
            /// Causes the proxy data to be reread from the registry for a handle.
            /// </summary>
            INTERNET_OPTION_REFRESH = 37,

            /// <summary>
            /// Notifies the system that the registry settings have been changed so that it verifies the settings on the next call to InternetConnect.
            /// </summary>
            INTERNET_OPTION_SETTINGS_CHANGED = 39
        }
    }
}