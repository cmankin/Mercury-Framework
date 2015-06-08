using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Instrumentation
{
    /// <summary>
    /// A set of common system variables and associated functions.
    /// </summary>
    public static class SystemVariable
    {
        #region Variables
        /// <summary>
        /// Path variable for the AppData->Roaming folder.
        /// </summary>
        public const string AppData = "%APPDATA%";

        /// <summary>
        /// Path variable for the AppData->Local folder.
        /// </summary>
        public const string LocalAppData = "%LOCALAPPDATA%";

        /// <summary>
        /// Path variable for the All Users AppData folder.
        /// </summary>
        public const string AllUsersAppData = "%ALLUSERSAPPDATA%";

        /// <summary>
        /// Path variable for the Program Files folder.
        /// </summary>
        public const string ProgramFiles = "%PROGRAMFILES%";

        /// <summary>
        /// Path variable for the Program Files (x86) folder.
        /// </summary>
        public const string ProgramFilesX86 = "%PROGRAMFILES(x86)%";

        /// <summary>
        /// Path variable for the Program Files->Common Files folder.
        /// </summary>
        public const string CommonProgramFiles = "%COMMONPROGRAMFILES%";

        /// <summary>
        /// Path variable for the Program Files (x86)->Common Files folder.
        /// </summary>
        public const string CommonProgramFilesX86 = "%COMMONPROGRAMFILES(x86)%";

        /// <summary>
        /// Path variable for the user profile folder.
        /// </summary>
        public const string UserProfile = "%USERPROFILE%";

        /// <summary>
        /// Path variable for the system root folder.
        /// </summary>
        public const string SystemRoot = "%SYSTEMROOT%";

        /// <summary>
        /// Path variable for the windows folder.
        /// </summary>
        public const string WinDir = "%WINDIR%";
        #endregion

        /// <summary>
        /// Gets the expanded path from the specified system variable.
        /// </summary>
        /// <param name="pathVar">The path variable to expand.</param>
        /// <returns>The expanded path from the specified system variable or the original variable.</returns>
        public static string GetExpandedPathVariable(string pathVar)
        {
            string upper = pathVar.ToUpper();
            switch (upper)
            {
                case SystemVariable.AppData:
                    return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                case SystemVariable.LocalAppData:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.AllUsersAppData:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.ProgramFiles:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.ProgramFilesX86:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.CommonProgramFiles:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.CommonProgramFilesX86:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.SystemRoot:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.WinDir:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case SystemVariable.UserProfile:
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            return pathVar;
        }
    }
}
