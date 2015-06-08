using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Mercury
{
    /// <summary>
    /// Core extensions to a System.String.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a string containing a specified number of characters from the left side of a string.
        /// </summary>
        /// <param name="value">The string from which the characters are returned.</param>
        /// <param name="length">The number of characters to return.  If 0, a zero-length string ("") is returned.</param>
        /// <returns>A string containing a specified number of characters from the left side of a string.</returns>
        public static string Left(this string value, int length)
        {
            if (length == 0)
                return string.Empty;
            return value.Mid(0, length);
        }

        /// <summary>
        /// Returns a string containing a specified number of characters from the right side of a string.
        /// </summary>
        /// <param name="value">The string from which the characters are returned.</param>
        /// <param name="length">The number of characters to return.  If 0, a zero-length string ("") is returned.</param>
        /// <returns>A string containing a specified number of characters from the right side of a string.</returns>
        public static string Right(this string value, int length)
        {
            if (length == 0)
                return string.Empty;

            int index = (value.Length - length);
            if (index < 0)
                index = 0;

            return value.Mid(index, length);
        }

        /// <summary>
        /// Returns a string containing a specified number of characters 
        /// from a string beginning at the specified start index.  This 
        /// function is zero-based.
        /// </summary>
        /// <param name="value">The string from which the characters are returned.</param>
        /// <param name="start">Zero-based character position in string.</param>
        /// <param name="length">The number of characters to return.</param>
        /// <returns>A string containing a specified number of characters 
        /// from a string beginning at the specified start index.</returns>
        public static string Mid(this string value, int start, int length = 0)
        {
            if (start < 0)
                throw new ArgumentException(Properties.Strings.Mid_Function_Start_Error);
            if (length < 0)
                throw new ArgumentException(Properties.Strings.String_Function_Length_Error);

            if (start > value.Length)
                return string.Empty;

            int actualLength = length;
            if (length == 0 || (start + length > value.Length))
                actualLength = (value.Length - start);

            return value.Substring(start, actualLength);
        }

        /// <summary>
        /// Returns a string in which a specified substring has been replaced with another substring 
        /// a specified number of times.  Evaluation of the strings is case-insensitive.
        /// </summary>
        /// <param name="value">The string containing the substring to replace.</param>
        /// <param name="find">The substring being searched for.</param>
        /// <param name="replacement">The replacement value.</param>
        /// <param name="count">Optional. Number of substring substitutions to perform.  If 
        /// omitted, the default value is -1, which means "make all possible substitutions."</param>
        /// <returns></returns>
        public static string ReplaceText(this string value,string find, string replacement, int count = -1)
        {
            string result = value;
            try
            {
                if (count < -1)
                    throw new ArgumentOutOfRangeException("newValue");
                if (!string.IsNullOrEmpty(find) && count != 0)
                {
                    if (count == -1)
                        count = value.Length;
                    result = StringExtensions.ReplaceInternal(value, find, replacement, count);
                }
            }
            catch
            {
                throw;
            }
            return result;
        }

        private static string ReplaceInternal(string expression, string find, string replacement, int count)
        {
            int length = expression.Length;
            int findLength = find.Length;
            StringBuilder builder = new StringBuilder(length);

            // Set comparison options
            CompareInfo info = Utils.GetCultureInfo().CompareInfo;
            CompareOptions options = (CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

            checked
            {
                int i = 0;
                int num = 0;
                while (i < length)
                {
                    if (num == count)
                    {
                        builder.Append(expression.Substring(i));
                        break;
                    }

                    int findIdx = info.IndexOf(expression, find, i, options);
                    if (findIdx < 0)
                    {
                        builder.Append(expression.Substring(i));
                        break;
                    }
                    builder.Append(expression.Substring(i, findIdx - i));
                    builder.Append(replacement);
                    num++;
                    i = findIdx + findLength;
                }
                return builder.ToString();
            }
        }


        /// <summary>
        /// Returns a value indicating whether the specified 
        /// string value contains only numeric data.
        /// </summary>
        /// <param name="value">The string value to test.</param>
        /// <returns>True if the string contains only numeric data; otherwise, false.</returns>
        public static bool IsNumeric(this string value)
        {
            double result;
            if (double.TryParse(value, out result))
                return true;
            return false;
        }

        /// <summary>
        /// Formats a string using the specified format.  Wraps string.Format().
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>A string formatted with the specified argument array.</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        /// <summary>
        /// Appends a new line character to the string.
        /// </summary>
        /// <param name="current">The string on which to append a new line character.</param>
        /// <returns>A string on which a new line character has been appended.</returns>
        public static string AppendNewLine(this string current)
        {
            return string.Format("{0}{1}", current, Environment.NewLine);
        }

        /// <summary>
        /// Appends a tab character to the string.
        /// </summary>
        /// <param name="current">The string on which to append a tab character.</param>
        /// <returns>A string on which a tab character has been appended.</returns>
        public static string AppendTab(this string current)
        {
            return string.Format("{0}{1}", current, '\t');
        }
    }
}
