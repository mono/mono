// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;

namespace System.Data.Common
{
	internal partial class DbConnectionOptions
	{
		// SxS notes:
		// * this method queries "DataDirectory" value from the current AppDomain.
		//   This string is used for to replace "!DataDirectory!" values in the connection string, it is not considered as an "exposed resource".
		// * This method uses GetFullPath to validate that root path is valid, the result is not exposed out.
		[ResourceExposure(ResourceScope.None)]
		[ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
		internal static string ExpandDataDirectory(string keyword, string value, ref string datadir)
		{
			string fullPath = null;
			if ((null != value) && value.StartsWith(DataDirectory, StringComparison.OrdinalIgnoreCase))
			{
				string rootFolderPath = datadir;
				if (null == rootFolderPath)
				{
					// find the replacement path
					object rootFolderObject = AppDomain.CurrentDomain.GetData("DataDirectory");
					rootFolderPath = (rootFolderObject as string);
					if ((null != rootFolderObject) && (null == rootFolderPath))
					{
						throw ADP.InvalidDataDirectory();
					}
					else if (string.IsNullOrEmpty(rootFolderPath))
					{
						rootFolderPath = AppDomain.CurrentDomain.BaseDirectory;
					}
					if (null == rootFolderPath)
					{
						rootFolderPath = "";
					}
					// cache the |DataDir| for ExpandDataDirectories
					datadir = rootFolderPath;
				}

				// We don't know if rootFolderpath ends with '\', and we don't know if the given name starts with onw
				int fileNamePosition = DataDirectory.Length;	// filename starts right after the '|datadirectory|' keyword
				bool rootFolderEndsWith = (0 < rootFolderPath.Length) && rootFolderPath[rootFolderPath.Length - 1] == '\\';
				bool fileNameStartsWith = (fileNamePosition < value.Length) && value[fileNamePosition] == '\\';

				// replace |datadirectory| with root folder path
				if (!rootFolderEndsWith && !fileNameStartsWith)
				{
					// need to insert '\'
					fullPath = rootFolderPath + '\\' + value.Substring(fileNamePosition);
				}
				else if (rootFolderEndsWith && fileNameStartsWith)
				{
					// need to strip one out
					fullPath = rootFolderPath + value.Substring(fileNamePosition + 1);
				}
				else
				{
					// simply concatenate the strings
					fullPath = rootFolderPath + value.Substring(fileNamePosition);
				}

				// verify root folder path is a real path without unexpected "..\"
				if (!ADP.GetFullPath(fullPath).StartsWith(rootFolderPath, StringComparison.Ordinal))
				{
					throw ADP.InvalidConnectionOptionValue(keyword);
				}
			}
			return fullPath;
		}

		internal string ExpandDataDirectories(ref string filename, ref int position)
		{
			string value = null;
			StringBuilder builder = new StringBuilder(_usersConnectionString.Length);
			string datadir = null;

			int copyPosition = 0;
			bool expanded = false;

			for (NameValuePair current = _keyChain; null != current; current = current.Next)
			{
				value = current.Value;

				// remove duplicate keyswords from connectionstring
				//if ((object)this[current.Name] != (object)value) {
				//	expanded = true;
				//	copyPosition += current.Length;
				//	continue;
				//}

				// There is a set of keywords we explictly do NOT want to expand |DataDirectory| on
				if (_useOdbcRules)
				{
					switch (current.Name)
					{
						case DbConnectionOptionKeywords.Driver:
						case DbConnectionOptionKeywords.Pwd:
						case DbConnectionOptionKeywords.UID:
							break;
						default:
							value = ExpandDataDirectory(current.Name, value, ref datadir);
							break;
					}
				}
				else
				{
					switch (current.Name)
					{
						case DbConnectionOptionKeywords.Provider:
						case DbConnectionOptionKeywords.DataProvider:
						case DbConnectionOptionKeywords.RemoteProvider:
						case DbConnectionOptionKeywords.ExtendedProperties:
						case DbConnectionOptionKeywords.UserID:
						case DbConnectionOptionKeywords.Password:
						case DbConnectionOptionKeywords.UID:
						case DbConnectionOptionKeywords.Pwd:
							break;
						default:
							value = ExpandDataDirectory(current.Name, value, ref datadir);
							break;
					}
				}
				if (null == value)
				{
					value = current.Value;
				}
				if (_useOdbcRules || (DbConnectionOptionKeywords.FileName != current.Name))
				{
					if (value != current.Value)
					{
						expanded = true;
						AppendKeyValuePairBuilder(builder, current.Name, value, _useOdbcRules);
						builder.Append(';');
					}
					else
					{
						builder.Append(_usersConnectionString, copyPosition, current.Length);
					}
				}
				else
				{
					// strip out 'File Name=myconnection.udl' for OleDb
					// remembering is value for which UDL file to open
					// and where to insert the strnig
					expanded = true;
					filename = value;
					position = builder.Length;
				}
				copyPosition += current.Length;
			}

			if (expanded)
			{
				value = builder.ToString();
			}
			else
			{
				value = null;
			}
			return value;
		}

		internal bool HasBlankPassword {
			get {
				if (!ConvertValueToIntegratedSecurity()) {
					if (_parsetable.ContainsKey(KEY.Password)) {
						return ADP.IsEmpty((string)_parsetable[KEY.Password]);
					} else
					if (_parsetable.ContainsKey(SYNONYM.Pwd)) {
						return ADP.IsEmpty((string)_parsetable[SYNONYM.Pwd]); // MDAC 83097
					} else {
						return ((_parsetable.ContainsKey(KEY.User_ID) && !ADP.IsEmpty((string)_parsetable[KEY.User_ID])) || (_parsetable.ContainsKey(SYNONYM.UID) && !ADP.IsEmpty((string)_parsetable[SYNONYM.UID])));
					}
				}
				return false;
			}
		}
	}


	internal static class DbConnectionOptionKeywords
	{
		// Odbc
		internal const string Driver = "driver";
		internal const string Pwd = "pwd";
		internal const string UID = "uid";

		// OleDb
		internal const string DataProvider = "data provider";
		internal const string ExtendedProperties = "extended properties";
		internal const string FileName = "file name";
		internal const string Provider = "provider";
		internal const string RemoteProvider = "remote provider";

		// common keywords (OleDb, OracleClient, SqlClient)
		internal const string Password = "password";
		internal const string UserID = "user id";
	}
}