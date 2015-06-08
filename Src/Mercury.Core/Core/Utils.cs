using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace Mercury
{
    /// <summary>
    /// A set of library utilities.
    /// </summary>
    internal class Utils
    {
        /// <summary>
        /// Gets the culture info of the current thread.
        /// </summary>
        /// <returns>The culture info of the current thread.</returns>
        internal static CultureInfo GetCultureInfo()
        {
            return Thread.CurrentThread.CurrentCulture;
        }
    }
}
