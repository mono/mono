// created on 12/5/2002 at 23:10

// Npgsql.NpgsqlException.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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

using System;
using System.Resources;
using System.IO;
using System.Text;
using System.Collections;

namespace Npgsql
{
    [Serializable]
    public class NpgsqlException : Exception
    {
        private IList errors;


        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlException";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlException));

        private NpgsqlException()
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, "<no message>");

        }
        
        internal NpgsqlException(IList errors) : base(((NpgsqlError)errors[0]).ToString())
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, Message);
            this.errors = errors;
        }

        /*internal NpgsqlException(String message) : base(message)
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, message);
        }*/

        internal NpgsqlException(String message, Exception inner)
                : base(message, inner)
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, message + " (" + inner.Message + ")");
        }

        internal NpgsqlException(String message, IList errors) : base(message)
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, message);
            this.errors = errors;
        }
        
        /// <summary>
        /// Provide access to the entire list of errors provided by the PostgreSQL backend.
        /// </summary>
        public NpgsqlError this[Int32 Index]
        {
            get
            {
                return (NpgsqlError)errors[Index];
            }
        }
        
        
        /// <summary>
        /// Severity code.  All versions.
        /// </summary>
        public String Severity
        {
            get
            {
                return (errors != null) ? this[0].Severity : String.Empty;
            }
        }

        /// <summary>
        /// Error code.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Code
        {
            get
            {
                return (errors != null) ? this[0].Code : String.Empty;
            }
        }

        /// <summary>
        /// Detailed error message.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Detail
        {
            get
            {
                return (errors != null) ? this[0].Detail : String.Empty;
            }
        }

        /// <summary>
        /// Suggestion to help resolve the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Hint
        {
            get
            {
                return (errors != null) ? this[0].Hint : String.Empty;
            }
        }

        /// <summary>
        /// Position (one based) within the query string where the error was encounterd.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Position
        {
            get
            {
                return (errors != null) ? this[0].Position : String.Empty;
            }
        }

        /// <summary>
        /// Trace back information.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Where
        {
            get
            {
                return (errors != null) ? this[0].Where : String.Empty;
            }
        }

        /// <summary>
        /// Source file (in backend) reporting the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String File
        {
            get
            {
                return (errors != null) ? this[0].File : String.Empty;
            }
        }

        /// <summary>
        /// Source file line number (in backend) reporting the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Line
        {
            get
            {
                return (errors != null) ? this[0].Line : String.Empty;
            }
        }

        /// <summary>
        /// Source routine (in backend) reporting the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Routine
        {
            get
            {
                return (errors != null) ? this[0].Routine : String.Empty;
            }
        }
        
        /// <summary>
        /// Returns the entire list of errors provided by the PostgreSQL backend.
        /// </summary>
        public IList Errors
        {
            get
            {
                return errors;
            }
        }
        
        private void AppendString(StringWriter Stream, string Format, string Str)
        {
            if (Str.Length > 0) {
                Stream.WriteLine(Format, Str);
            }
				}

        public override String ToString()
        {
            StringWriter    S = new StringWriter();

            S.WriteLine("{0}:", this.GetType().FullName);

            foreach (NpgsqlError PgError in Errors) 
            {
                AppendString(S, "{0}", PgError.Message);
                AppendString(S, "Severity: {0}", PgError.Severity);
                AppendString(S, "Code: {0}", PgError.Code);
                AppendString(S, "Hint: {0}", PgError.Hint);
			}

            S.Write(StackTrace);

            return S.ToString();
            
        }

    }



}
