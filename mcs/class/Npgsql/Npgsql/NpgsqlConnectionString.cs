// created on 6/21/2004

// Npgsql.NpgsqlConnecionString.cs
//
// Author:
//	Glen Parker (glenebob@nwlink.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Resources;

namespace Npgsql
{
    /// <summary>
    /// Represents a connection string.
    /// </summary>
    internal sealed class NpgsqlConnectionString : IEnumerable
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlConnectionString";
        private static System.Resources.ResourceManager resman;

        private String                                 connection_string = null;
        private ListDictionary                         connection_string_values;

        static NpgsqlConnectionString()
        {
            resman = new System.Resources.ResourceManager(typeof(NpgsqlConnectionString));
        }

        private NpgsqlConnectionString(NpgsqlConnectionString Other)
        {
            connection_string = Other.connection_string;
            connection_string_values = new ListDictionary(CaseInsensitiveComparer.Default);
            foreach (DictionaryEntry DE in Other.connection_string_values)
            {
                connection_string_values.Add(DE.Key, DE.Value);
            }
        }

        private NpgsqlConnectionString(ListDictionary Values)
        {
            connection_string_values = Values;
        }

        /// <summary>
        /// Return an exact copy of this NpgsqlConnectionString.
        /// </summary>
        public NpgsqlConnectionString Clone()
        {
            return new NpgsqlConnectionString(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return connection_string_values.GetEnumerator();
        }

        /// <summary>
        /// This method parses a connection string and returns a new NpgsqlConnectionString object.
        /// </summary>
        public static NpgsqlConnectionString ParseConnectionString(String CS)
        {
            ListDictionary new_values = new ListDictionary(CaseInsensitiveComparer.Default);
            String[] pairs;
            String[] keyvalue;

            if (CS == null)
                CS = String.Empty;

            // Get the key-value pairs delimited by ;
            pairs = CS.Split(';');

            // Now, for each pair, get its key=value.
            foreach(String sraw in pairs)
            {
                String s = sraw.Trim();
                String Key = "", Value = "";

                // Just ignore these.
                if (s == "")
                {
                    continue;
                }

                // Split this chunk on the first CONN_ASSIGN only.
                keyvalue = s.Split(new Char[] {'='}, 2);

                // Keys always get trimmed and uppercased.
                Key = keyvalue[0].Trim().ToUpper();

                // Make sure the key is even there...
                if (Key.Length == 0)
                {
                    throw new ArgumentException(resman.GetString("Exception_WrongKeyVal"), "<BLANK>");
                }

                // We don't expect keys this long, and it might be about to be put
                // in an error message, so makes sure it is a sane length.
                if (Key.Length > 20)
                {
                    Key = Key.Substring(0, 20);
                }

                // Check if there is a key-value pair.
                if (keyvalue.Length != 2)
                {
                    throw new ArgumentException(resman.GetString("Exception_WrongKeyVal"), Key);
                }

                // Values always get trimmed.
                Value = keyvalue[1].Trim();

                // Substitute the real key name if this is an alias key (ODBC stuff for example)...
                String      AliasKey = (string)ConnectionStringKeys.Aliases[Key];

                if (AliasKey != null)
                {
                    Key = AliasKey;
                }

                // Add the pair to the dictionary..
                new_values.Add(Key, Value);
            }

            return new NpgsqlConnectionString(new_values);
        }

        /// <summary>
        /// Case insensative accessor for indivual connection string values.
        /// </summary>
        public String this[String Key]
        {
            get
            {
                return (String)connection_string_values[Key];
            }
            set
            {
                connection_string_values[Key] = value;
                connection_string = null;
            }
        }

        /// <summary>
        /// Report whether a value with the provided key name exists in this connection string.
        /// </summary>
        public Boolean Contains(String Key)
        {
            return connection_string_values.Contains(Key);
        }

        /// <summary>
        /// Return a clean string representation of this connection string.
        /// </summary>
        public override String ToString()
        {
            if (connection_string == null)
            {
                StringBuilder      S = new StringBuilder();

                foreach (DictionaryEntry DE in this)
                {
                    S.AppendFormat("{0}={1};", DE.Key, DE.Value);
                }

                connection_string = S.ToString();
            }

            return connection_string;
        }

        /// <summary>
        /// Return a string value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// </summary>
        public String ToString(String Key)
        {
            return ToString(Key, "");
        }

        /// <summary>
        /// Return a string value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// </summary>
        public String ToString(String Key, String Default)
        {
            if (! connection_string_values.Contains(Key))
            {
                return Default;
            }

            return Convert.ToString(connection_string_values[Key]);
        }

        /// <summary>
        /// Return an integer value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// Throw an appropriate exception if the value cannot be coerced to an integer.
        /// </summary>
        public Int32 ToInt32(String Key)
        {
            return ToInt32(Key, 0);
        }

        /// <summary>
        /// Return an integer value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// Throw an appropriate exception if the value cannot be coerced to an integer.
        /// </summary>
        public Int32 ToInt32(String Key, Int32 Min, Int32 Max)
        {
            return ToInt32(Key, Min, Max, 0);
        }

        /// <summary>
        /// Return an integer value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// Throw an appropriate exception if the value cannot be coerced to an integer.
        /// </summary>
        public Int32 ToInt32(String Key, Int32 Default)
        {
            if (! connection_string_values.Contains(Key))
            {
                return Default;
            }

            try
            {
                return Convert.ToInt32(connection_string_values[Key]);
            }
            catch (Exception E)
            {
                throw new ArgumentException(String.Format(resman.GetString("Exception_InvalidIntegerKeyVal"), Key), Key, E);
            }
        }

        /// <summary>
        /// Return an integer value from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value cannot be coerced to an integer.
        /// </summary>
        public Int32 ToInt32(String Key, Int32 Min, Int32 Max, Int32 Default)
        {
            Int32   V;

            V = ToInt32(Key, Default);

            if (V < Min)
            {
                throw new ArgumentException(String.Format(resman.GetString("Exception_IntegerKeyValMin"), Key, Min), Key);
            }
            if (V > Max)
            {
                throw new ArgumentException(String.Format(resman.GetString("Exception_IntegerKeyValMax"), Key, Max), Key);
            }

            return V;
        }

        /// <summary>
        /// Return a boolean value from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value is not recognized as a boolean.
        /// </summary>
        public Boolean ToBool(String Key)
        {
            return ToBool(Key, false);
        }

        /// <summary>
        /// Return a boolean value from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value is not recognized as a boolean.
        /// </summary>
        public Boolean ToBool(String Key, Boolean Default)
        {
            if (! connection_string_values.Contains(Key))
            {
                return Default;
            }

            switch (connection_string_values[Key].ToString().ToLower())
            {
            case "t" :
            case "true" :
            case "y" :
            case "yes" :
                return true;

            case "f" :
            case "false" :
            case "n" :
            case "no" :
                return false;

            default :
                throw new ArgumentException(String.Format(resman.GetString("Exception_InvalidBooleanKeyVal"), Key), Key);

            }
        }

        /// <summary>
        /// Return a ProtocolVersion from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value is not recognized as
        /// integer 2 or 3.
        /// </summary>
        public ProtocolVersion ToProtocolVersion(String Key)
        {
            if (! connection_string_values.Contains(Key))
            {
                return ProtocolVersion.Version3;
            }

            switch (ToInt32(Key))
            {
            case 2 :
                return ProtocolVersion.Version2;

            case 3 :
                return ProtocolVersion.Version3;

            default :
                throw new ArgumentException(String.Format(resman.GetString("Exception_InvalidProtocolVersionKeyVal"), Key), Key);

            }
        }
    }


    /// <summary>
    /// Know connection string keys.
    /// </summary>
    internal abstract class ConnectionStringKeys
    {
        public static readonly String Host             = "SERVER";
        public static readonly String Port             = "PORT";
        public static readonly String Protocol         = "PROTOCOL";
        public static readonly String Database         = "DATABASE";
        public static readonly String UserName         = "USER ID";
        public static readonly String Password         = "PASSWORD";
        public static readonly String SSL              = "SSL";
        public static readonly String Encoding         = "ENCODING";
        public static readonly String Timeout          = "TIMEOUT";

        // These are for the connection pool
        public static readonly String Pooling          = "POOLING";
        public static readonly String MinPoolSize      = "MINPOOLSIZE";
        public static readonly String MaxPoolSize      = "MAXPOOLSIZE";

        // A list of aliases for some of the above values.  If one of these aliases is
        // encountered when parsing a connection string, it's real key name will
        // be used instead.  These will be reflected if ToString() is used to inspect
        // the string.
        private static ListDictionary _aliases;

        static ConnectionStringKeys()
        {
            _aliases = new ListDictionary();

            // Aliases to help catch common errors.
            _aliases.Add("DB", Database);
            _aliases.Add("HOST", Host);
            _aliases.Add("USER", UserName);
            _aliases.Add("USERID", UserName);
            _aliases.Add("USER NAME", UserName);
            _aliases.Add("USERNAME", UserName);
            _aliases.Add("PSW", Password);

            // Aliases to make migration from ODBC easier.
            _aliases.Add("UID", UserName);
            _aliases.Add("PWD", Password);
        }

        public static IDictionary Aliases
        {
            get
            {
                return _aliases;
            }
        }
    }

    /// <summary>
    /// Connection string default values.
    /// </summary>
    internal abstract class ConnectionStringDefaults
    {
        // Connection string defaults
        public static readonly Int32 Port              = 5432;
        public static readonly String Encoding         = "SQL_ASCII";
        public static readonly Boolean Pooling         = true;
        public static readonly Int32 MinPoolSize       = 1;
        public static readonly Int32 MaxPoolSize       = 20;
        public static readonly Int32 Timeout           = 15; // Seconds
    }
}
