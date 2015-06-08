using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Mercury.Instrumentation;

namespace Mercury.Logging
{
    /// <summary>
    /// An extension of the System.Diagnostics.TextWriterTraceListener 
    /// that allows path variables in the file path to be expanded and 
    /// ensures that all directories in the path exist.
    /// </summary>
    public class TextWriterTraceListenerEx
        : TextWriterTraceListener
    {
        /// <summary>
        /// Initializes a default instance of the TextWriterTraceListenerEx class.
        /// </summary>
        public TextWriterTraceListenerEx()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TextWriterTraceListenerEx 
        /// class with the specified file path.
        /// </summary>
        /// <param name="path">The file path of the file to write to.</param>
        public TextWriterTraceListenerEx(string path)
            : this(path, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TextWriterTraceListenerEx 
        /// class with the specified file path and name.
        /// </summary>
        /// <param name="path">The file path of the file to write to.</param>
        /// <param name="name">The name of the new instance.</param>
        public TextWriterTraceListenerEx(string path, string name)
            : base(InstrumentationUtil.EnsurePath(path), name)
        {
        }
    }
}
