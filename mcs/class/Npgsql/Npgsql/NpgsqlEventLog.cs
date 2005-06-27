// created on 07/06/2002 at 09:34

// Npgsql.NpgsqlEventLog.cs
//
// Author:
//	Dave Page (dpage@postgresql.org)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System.IO;
using System.Text;
using System.Diagnostics;
using System;
using System.Resources;

namespace Npgsql
{

    /// <summary>
    /// The level of verbosity of the NpgsqlEventLog
    /// </summary>
    public enum LogLevel {
        /// <summary>
        /// Don't log at all
        /// </summary>
        None = 0,
        /// <summary>
        /// Only log the most common issues
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Log everything
        /// </summary>
        Debug = 2
    }


    /// <summary>
    /// This class handles all the Npgsql event and debug logging
    /// </summary>
    public class NpgsqlEventLog
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlEventLog";
        private static   String       logfile;
        private static   LogLevel     level;
        private static   Boolean echomessages;

        private static ResourceManager LogResMan;

        private NpgsqlEventLog()
        {}
        // static constructor
        static NpgsqlEventLog()
        {
            LogResMan = new ResourceManager(typeof(NpgsqlEventLog));
        }

        ///<summary>
        /// Sets/Returns the level of information to log to the logfile.
        /// </summary>
        /// <value>The current <see cref="Npgsql.LogLevel">LogLevel</see></value>
        public static LogLevel Level {
            get
            {
                LogPropertyGet(LogLevel.Debug, CLASSNAME, "LogLevel");
                return level;
            }
            set
            {
                LogPropertySet(LogLevel.Debug, CLASSNAME, "LogLevel", value);
                level = value;
            }
        }

        ///<summary>
        /// Sets/Returns the filename to use for logging.
        /// </summary>
        /// <value>The filename of the current Log file.</value>
        public static String LogName {
            get
            {
                LogPropertyGet(LogLevel.Debug, CLASSNAME, "LogName");
                return logfile;
            }
            set
            {
                LogPropertySet(LogLevel.Normal, CLASSNAME, "LogName", value);
                logfile = value;
            }
        }

        ///<summary>
        /// Sets/Returns whether Log messages should be echoed to the console
        /// </summary>
        /// <value><b>true</b> if Log messages are echoed to the console, otherwise <b>false</b></value>
        public static Boolean EchoMessages {
            get
            {
                LogPropertyGet(LogLevel.Debug, CLASSNAME, "EchoMessages");
                return echomessages;
            }
            set
            {
                LogPropertySet(LogLevel.Normal, CLASSNAME, "EchoMessages", value);
                echomessages = value;
            }
        }

        // Event/Debug Logging
        // This method should be private and only used by the internal methods that support localization.
        /// <summary>
        /// Writes a string to the Npgsql event log if msglevel is bigger then <see cref="Npgsql.NpgsqlEventLog.Level">NpgsqlEventLog.Level</see>
        /// </summary>
        /// <remarks>
        /// This method is obsolete and should no longer be used.
        /// It is likely to be removed in future versions of Npgsql
        /// </remarks>
        /// <param name="message">The message to write to the event log</param>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        private static void LogMsg(String message, LogLevel msglevel)
        {
            if (msglevel > level)
                return;

            Process proc = Process.GetCurrentProcess();

            if (echomessages)
            {
                Console.WriteLine(message);
            }

            if (logfile != null)
            {
                if (logfile != "")
                {

                    StreamWriter writer = new StreamWriter(logfile, true);

                    // The format of the logfile is
                    // [Date] [Time]  [PID]  [Level]  [Message]
                    writer.WriteLine(System.DateTime.Now + "\t" + proc.Id + "\t" + msglevel + "\t" + message);
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Writes a string to the Npgsql event log if msglevel is bigger then <see cref="Npgsql.NpgsqlEventLog.Level">NpgsqlEventLog.Level</see>
        /// </summary>
        /// <param name="resman">The <see cref="System.Resources.ResourceManager">ResourceManager</see> to get the localized resources</param>
        /// <param name="ResourceString">The name of the resource that should be fetched by the <see cref="System.Resources.ResourceManager">ResourceManager</see></param>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="Parameters">The additional parameters that shall be included into the log-message (must be compatible with the string in the resource):</param>
        internal static void LogMsg(ResourceManager resman, string ResourceString, LogLevel msglevel, params Object[] Parameters)
        {
            if (msglevel > level)
                return;

            string message = resman.GetString(ResourceString);

            if (message == null) {
                message = String.Format("Unable to find resource string {0} for class {1}", ResourceString, resman.BaseName);
            } else if (Parameters.Length > 0) {
                message = String.Format(message, Parameters);
            }

            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling the Get-part of an Indexer to the log file.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Indexer</param>
        /// <param name="IndexerParam">The parameter given to the Indexer</param>
        internal static void LogIndexerGet(LogLevel msglevel, string ClassName, object IndexerParam)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Indexer_Get"), ClassName, IndexerParam);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling the Set-part of an Indexer to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Indexer</param>
        /// <param name="IndexerParam">The parameter given to the Indexer</param>
        /// <param name="value">The value the Indexer is set to</param>
        internal static void LogIndexerSet(LogLevel msglevel, string ClassName, object IndexerParam, object value)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Indexer_Set"), ClassName, IndexerParam, value);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling the Get-part of a Property to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Property</param>
        /// <param name="PropertyName">The name of the Property</param>
        internal static void LogPropertyGet(LogLevel msglevel, string ClassName, string PropertyName)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Property_Get"), ClassName, PropertyName);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling the Set-part of a Property to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Property</param>
        /// <param name="PropertyName">The name of the Property</param>
        /// <param name="value">The value the Property is set to</param>
        internal static void LogPropertySet(LogLevel msglevel, string ClassName, string PropertyName, object value)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Property_Set"), ClassName, PropertyName, value);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling a Method without Arguments to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Method</param>
        /// <param name="MethodName">The name of the Method</param>
        internal static void LogMethodEnter(LogLevel msglevel, string ClassName, string MethodName)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Method_0P_Enter"), ClassName, MethodName);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling a Method with one Argument to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Method</param>
        /// <param name="MethodName">The name of the Method</param>
        /// <param name="MethodParameter">The value of the Argument of the Method</param>
        internal static void LogMethodEnter(LogLevel msglevel, string ClassName, string MethodName, object MethodParameter)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Method_1P_Enter"), ClassName, MethodName, MethodParameter);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling a Method with two Arguments to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Method</param>
        /// <param name="MethodName">The name of the Method</param>
        /// <param name="MethodParameter1">The value of the first Argument of the Method</param>
        /// <param name="MethodParameter2">The value of the second Argument of the Method</param>
        internal static void LogMethodEnter(LogLevel msglevel, string ClassName, string MethodName, object MethodParameter1, object MethodParameter2)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Method_2P_Enter"), ClassName, MethodName, MethodParameter1, MethodParameter2);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling a Method with three Arguments to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Method</param>
        /// <param name="MethodName">The name of the Method</param>
        /// <param name="MethodParameter1">The value of the first Argument of the Method</param>
        /// <param name="MethodParameter2">The value of the second Argument of the Method</param>
        /// <param name="MethodParameter3">The value of the third Argument of the Method</param>
        internal static void LogMethodEnter(LogLevel msglevel, string ClassName, string MethodName, object MethodParameter1, object MethodParameter2, object MethodParameter3)
        {
            if (msglevel > level)
                return;
            string message = String.Format(LogResMan.GetString("Method_3P_Enter"), ClassName, MethodName, MethodParameter1, MethodParameter2, MethodParameter3);
            LogMsg(message, msglevel);
        }

        /// <summary>
        /// Writes the default log-message for the action of calling a Method with more than three Arguments to the logfile.
        /// </summary>
        /// <param name="msglevel">The minimum <see cref="Npgsql.LogLevel">LogLevel</see> for which this message should be logged.</param>
        /// <param name="ClassName">The name of the class that contains the Method</param>
        /// <param name="MethodName">The name of the Method</param>
        /// <param name="MethodParameters">A <see cref="System.Object">Object</see>-Array with zero or more Ojects that are Arguments of the Method.</param>
        internal static void LogMethodEnter(LogLevel msglevel, string ClassName, string MethodName, params object[] MethodParameters)
        {
            if (msglevel > level)
                return;
            string message = String.Empty;
            switch (MethodParameters.Length)
            {
            case 4:
                message = String.Format(LogResMan.GetString("Method_4P_Enter"), ClassName, MethodName,
                                        MethodParameters[0], MethodParameters[1], MethodParameters[2], MethodParameters[3]);
                break;
            case 5:
                message = String.Format(LogResMan.GetString("Method_5P_Enter"), ClassName, MethodName,
                                        MethodParameters[0], MethodParameters[1], MethodParameters[2], MethodParameters[3], MethodParameters[4]);
                break;
            case 6:
                message = String.Format(LogResMan.GetString("Method_6P_Enter"), ClassName, MethodName,
                                        MethodParameters[0], MethodParameters[1], MethodParameters[2], MethodParameters[3], MethodParameters[4], MethodParameters[5]);
                break;
            default:
                // should always be true - but who knows ;-)
                if (MethodParameters.Length > 6)
                    message = String.Format(LogResMan.GetString("Method_6P+_Enter"), ClassName, MethodName, MethodParameters[0], MethodParameters[1]);
                break;
            }
            LogMsg(message, msglevel);
        }

    }
}
