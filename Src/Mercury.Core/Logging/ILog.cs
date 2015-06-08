using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging
{
    /// <summary>
    /// An interface for logging nessages.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Gets the name of this log.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initializes this log with the specified log name.
        /// </summary>
        /// <param name="logName">The name of this log.</param>
        void Initialize(string logName);

        /// <summary>
        /// Writes the specified debug message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        void Debug(Func<string> message);

        /// <summary>
        /// Writes the specified debug message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        void Debug(int id, Func<string> message);

        /// <summary>
        /// Writes the debug message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// Writes the debug message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Debug(int id, string format, params object[] args);

        /// <summary>
        /// Writes the specified information message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        void Info(Func<string> message);

        /// <summary>
        /// Writes the specified information message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        void Info(int id, Func<string> message);

        /// <summary>
        /// Writes the information message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Writes the information message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Info(int id, string format, params object[] args);

        /// <summary>
        /// Writes the specified warning message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        void Warn(Func<string> message);

        /// <summary>
        /// Writes the specified warning message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        void Warn(int id, Func<string> message);

        /// <summary>
        /// Writes the warning message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Writes the warning message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Warn(int id, string format, params object[] args);

        /// <summary>
        /// Writes the specified error message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        void Error(Func<string> message);

        /// <summary>
        /// Writes the specified error message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        void Error(int id, Func<string> message);

        /// <summary>
        /// Writes the error message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// Writes the error message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Error(int id, string format, params object[] args);

        /// <summary>
        /// Writes the specified critical error message.
        /// </summary>
        /// <param name="message">A function generating the message to write.</param>
        void Critical(Func<string> message);

        /// <summary>
        /// Writes the specified critical error message with the specified event identifier.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="message">A function generating the message to write.</param>
        void Critical(int id, Func<string> message);

        /// <summary>
        /// Writes the critical error message using the specified argument array and format. 
        /// </summary>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Critical(string format, params object[] args);

        /// <summary>
        /// Writes the critical error message using the specified event identifier, argument array, and format.
        /// </summary>
        /// <param name="id">The event identifier for this message.</param>
        /// <param name="format">The composite format string to write.</param>
        /// <param name="args">The argument array to format.</param>
        void Critical(int id, string format, params object[] args);
    }

    /// <summary>
    /// Ensures a default constructor for the logger type.
    /// </summary>
    /// <typeparam name="T">The type of the logger to construct.</typeparam>
    public interface ILog<T> where T : new()
    {
    }
}
