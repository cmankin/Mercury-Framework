using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Instrumentation
{
    /// <summary>
    /// A utility class for logging.
    /// </summary>
    internal class InstrumentationUtil
    {
        /// <summary>
        /// Creates the appropriate directories for the specified path and returns the full file path.
        /// </summary>
        /// <param name="filePath">The path to ensure.</param>
        /// <returns>The full file path.</returns>
        internal static string EnsurePath(string filePath)
        {
            string fullPath = GetExpandedFilePath(filePath);
            FileInfo fi = new FileInfo(fullPath);
            fi.Directory.Create();
            return fullPath;
        }

        /// <summary>
        /// Returns the file path with select system path variables expanded.
        /// </summary>
        /// <param name="filePath">The file path to expand.</param>
        /// <returns>The file path with select system path variables expanded or the original file path.</returns>
        internal static string GetExpandedFilePath(string filePath)
        {
            string fullPath = filePath;
            string upper = filePath.ToUpper();
            if (upper.Contains(SystemVariable.AppData))
                fullPath = fullPath.ReplaceText(SystemVariable.AppData, GetExpandedPathVariable(SystemVariable.AppData));
            if (upper.Contains(SystemVariable.AllUsersAppData))
                fullPath = fullPath.ReplaceText(SystemVariable.AllUsersAppData, GetExpandedPathVariable(SystemVariable.AllUsersAppData));
            else if (upper.Contains(SystemVariable.CommonProgramFiles))
                fullPath = fullPath.ReplaceText(SystemVariable.CommonProgramFiles, GetExpandedPathVariable(SystemVariable.CommonProgramFiles));
            else if (upper.Contains(SystemVariable.CommonProgramFilesX86))
                fullPath = fullPath.ReplaceText(SystemVariable.CommonProgramFilesX86, GetExpandedPathVariable(SystemVariable.CommonProgramFilesX86));
            else if (upper.Contains(SystemVariable.LocalAppData))
                fullPath = fullPath.ReplaceText(SystemVariable.LocalAppData, GetExpandedPathVariable(SystemVariable.LocalAppData));
            else if (upper.Contains(SystemVariable.ProgramFiles))
                fullPath = fullPath.ReplaceText(SystemVariable.ProgramFiles, GetExpandedPathVariable(SystemVariable.ProgramFiles));
            else if (upper.Contains(SystemVariable.ProgramFilesX86))
                fullPath = fullPath.ReplaceText(SystemVariable.ProgramFilesX86, GetExpandedPathVariable(SystemVariable.ProgramFilesX86));
            else if (upper.Contains(SystemVariable.SystemRoot))
                fullPath = fullPath.ReplaceText(SystemVariable.SystemRoot, GetExpandedPathVariable(SystemVariable.SystemRoot));
            else if (upper.Contains(SystemVariable.UserProfile))
                fullPath = fullPath.ReplaceText(SystemVariable.UserProfile, GetExpandedPathVariable(SystemVariable.UserProfile));
            else if (upper.Contains(SystemVariable.WinDir))
                fullPath = fullPath.ReplaceText(SystemVariable.WinDir, GetExpandedPathVariable(SystemVariable.WinDir));
            return fullPath;
        }

        /// <summary>
        /// Wrapper for a call to the SystemVariable.GetExpandedPathVariable() method.
        /// </summary>
        /// <param name="pathVar">The path variable to expnad.</param>
        /// <returns>The expanded path variable or the original variable.</returns>
        protected static string GetExpandedPathVariable(string pathVar)
        {
            return SystemVariable.GetExpandedPathVariable(pathVar);
        }
    }
}
