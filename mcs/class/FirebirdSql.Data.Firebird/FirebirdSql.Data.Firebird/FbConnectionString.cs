/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2004-2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	internal sealed class FbConnectionString
	{
		#region Static Fields

		public static readonly Hashtable Synonyms = GetSynonyms();

		#endregion

		#region Static Methods

		// This	is somethig	that should	be needed in .NET 2.0
		// for use with	the	DbConnectionOptions	or DbConnectionString classes.
		private static Hashtable GetSynonyms()
		{
			Hashtable synonyms = new Hashtable();

			synonyms.Add("data source", "data source");
			synonyms.Add("datasource", "data source");
			synonyms.Add("server", "data source");
			synonyms.Add("host", "data source");
			synonyms.Add("port", "port number");
			synonyms.Add("port number", "port number");
			synonyms.Add("database", "database");
			synonyms.Add("user id", "user id");
			synonyms.Add("uid", "user id");
			synonyms.Add("user", "user id");
			synonyms.Add("user name", "user id");
			synonyms.Add("password", "password");
			synonyms.Add("user password", "password");
			synonyms.Add("dialect", "dialect");
			synonyms.Add("charset", "charset");
			synonyms.Add("pooling", "pooling");
			synonyms.Add("max pool size", "max pool size");
			synonyms.Add("min pool size", "min pool size");
			synonyms.Add("character set", "charset");
			synonyms.Add("connection lifetime", "connection lifetime");
			synonyms.Add("timeout", "connection timeout");
			synonyms.Add("connection timeout", "connection timeout");
			synonyms.Add("packet size", "packet size");
			synonyms.Add("role", "role name");
			synonyms.Add("role name", "role name");
			synonyms.Add("fetch size", "fetch size");
			synonyms.Add("fetchsize", "fetch size");
			synonyms.Add("server type", "server type");
			synonyms.Add("servertype", "server type");
			synonyms.Add("isolation level", "isolation level");

			return synonyms;
		}

		#endregion

		#region Private	fields

		private Hashtable options;
		private bool isServiceConnectionString;

		#endregion

		#region Properties

		public string UserID
		{
			get { return this.GetString("user id"); }
		}

		public string Password
		{
			get { return this.GetString("password"); }
		}

		public string DataSource
		{
			get { return this.GetString("data source"); }
		}

		public int Port
		{
			get { return this.GetInt32("port number"); }
		}

		public string Database
		{
			get { return this.GetString("database"); }
		}

		public short PacketSize
		{
			get { return this.GetInt16("packet size"); }
		}

		public string Role
		{
			get { return this.GetString("role name"); }
		}

		public byte Dialect
		{
			get { return this.GetByte("dialect"); }
		}

		public string Charset
		{
			get { return this.GetString("charset"); }
		}

		public int ConnectionTimeout
		{
			get { return this.GetInt32("connection timeout"); }
		}

		public bool Pooling
		{
			get { return this.GetBoolean("pooling"); }
		}

		public long ConnectionLifeTime
		{
			get { return this.GetInt64("connection lifetime"); }
		}

		public int MinPoolSize
		{
			get { return this.GetInt32("min pool size"); }
		}

		public int MaxPoolSize
		{
			get { return this.GetInt32("max pool size"); }
		}

		public int FetchSize
		{
			get { return this.GetInt32("fetch size"); }
		}

		public int ServerType
		{
			get { return this.GetInt32("server type"); }
		}

		public IsolationLevel IsolationLevel
		{
			get { return this.GetIsolationLevel("isolation level"); }
		}

		#endregion

		#region Constructors

		public FbConnectionString()
		{
			this.SetDefaultOptions();
		}

		public FbConnectionString(FbConnectionStringBuilder csb)
			: this(csb.ToString())
		{
		}

		public FbConnectionString(string connectionString)
		{
			this.Load(connectionString);
		}

		internal FbConnectionString(bool isServiceConnectionString)
		{
			this.isServiceConnectionString = isServiceConnectionString;
			this.SetDefaultOptions();
		}

		#endregion

		#region Public methods

		public void Load(string connectionString)
		{
			this.SetDefaultOptions();

			if (connectionString != null && connectionString.Length > 0)
			{
				Hashtable synonyms = GetSynonyms();
				MatchCollection keyPairs = Regex.Matches(connectionString, @"([\w\s\d]*)\s*=\s*([^;]*)");

				foreach (Match keyPair in keyPairs)
				{
					if (keyPair.Groups.Count == 3)
					{
						string[] values = new string[] 
						{
							keyPair.Groups[1].Value.Trim(),
							keyPair.Groups[2].Value.Trim()
						};

						if (values.Length == 2 &&
							values[0] != null && values[0].Length > 0 &&
							values[1] != null && values[1].Length > 0)
						{
							values[0] = values[0].ToLower(CultureInfo.CurrentCulture);

							if (synonyms.Contains(values[0]))
							{
								string key = (string)synonyms[values[0]];
								this.options[key] = values[1].Trim();
							}
						}
					}
				}

				if (this.Database != null && this.Database.Length > 0)
				{
					this.ParseConnectionInfo(this.Database);
				}
			}
		}

		public void Validate()
		{
			if ((this.UserID == null || this.UserID.Length == 0) ||
				(this.Password == null || this.Password.Length == 0) ||
				((this.Database == null || this.Database.Length == 0) && !this.isServiceConnectionString) ||
				((this.DataSource == null || this.DataSource.Length == 0) && this.ServerType != 1) ||
				(this.Charset == null || this.Charset.Length == 0) ||
				this.Port == 0 ||
				(this.ServerType != 0 && this.ServerType != 1) ||
				(this.MinPoolSize > this.MaxPoolSize))
			{
				throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			}
			else
			{
				if (this.Dialect < 1 || this.Dialect > 3)
				{
					throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
				}
				if (this.PacketSize < 512 || this.PacketSize > 32767)
				{
					throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "'Packet Size' value of {0} is not valid.\r\nThe value should be an integer >= 512 and <= 32767.", this.PacketSize));
				}

				this.CheckIsolationLevel();
			}
		}

		#endregion

		#region Private	Methods

		private void SetDefaultOptions()
		{
			if (this.options == null)
			{
				this.options = new Hashtable();
			}

			this.options.Clear();

			// Add default key pairs values
			this.options.Add("data source", "");
			this.options.Add("port number", 3050);
			this.options.Add("user id", "SYSDBA");
			this.options.Add("password", "masterkey");
			this.options.Add("role name", String.Empty);
			this.options.Add("database", String.Empty);
			this.options.Add("charset", "None");
			this.options.Add("dialect", 3);
			this.options.Add("packet size", 8192);
			this.options.Add("pooling", true);
			this.options.Add("connection lifetime", 0);
			this.options.Add("min pool size", 0);
			this.options.Add("max pool size", 100);
			this.options.Add("connection timeout", 15);
			this.options.Add("fetch size", 200);
			this.options.Add("server type", 0);
			this.options.Add("isolation level", IsolationLevel.ReadCommitted.ToString());
		}

		private void ParseConnectionInfo(string connectInfo)
		{
			string database = null;
			string dataSource = null;
			int portNumber = -1;

			// allows standard syntax //host:port/....
			// and old fb syntax host/port:....
			connectInfo = connectInfo.Trim();
			char hostSepChar;
			char portSepChar;

			if (connectInfo.StartsWith("//"))
			{
				connectInfo = connectInfo.Substring(2);
				hostSepChar = '/';
				portSepChar = ':';
			}
			else
			{
				hostSepChar = ':';
				portSepChar = '/';
			}

			int sep = connectInfo.IndexOf(hostSepChar);
			if (sep == 0 || sep == connectInfo.Length - 1)
			{
				throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			}
			else if (sep > 0)
			{
				dataSource = connectInfo.Substring(0, sep);
				database = connectInfo.Substring(sep + 1);
				int portSep = dataSource.IndexOf(portSepChar);

				if (portSep == 0 || portSep == dataSource.Length - 1)
				{
					throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
				}
				else if (portSep > 0)
				{
					portNumber = Int32.Parse(dataSource.Substring(portSep + 1), CultureInfo.InvariantCulture);
					dataSource = dataSource.Substring(0, portSep);
				}
				else if (portSep < 0 && dataSource.Length == 1)
				{
					if (this.DataSource == null || this.DataSource.Length == 0)
					{
						dataSource = "localhost";
					}
					else
					{
						dataSource = null;
					}

					database = connectInfo;
				}
			}
			else if (sep == -1)
			{
				database = connectInfo;
			}

			this.options["database"] = database;
			if (dataSource != null)
			{
				this.options["data source"] = dataSource;
			}
			if (portNumber != -1)
			{
				this.options["port"] = portNumber;
			}
		}

		private string GetString(string key)
		{
			if (this.options.Contains(key))
			{
				return (string)this.options[key];
			}
			return null;
		}

		private bool GetBoolean(string key)
		{
			if (this.options.Contains(key))
			{
				return Boolean.Parse(this.options[key].ToString());
			}
			return false;
		}

		private byte GetByte(string key)
		{
			if (this.options.Contains(key))
			{
				return Convert.ToByte(this.options[key], CultureInfo.CurrentCulture);
			}
			return 0;
		}

		private short GetInt16(string key)
		{
			if (this.options.Contains(key))
			{
				return Convert.ToInt16(this.options[key], CultureInfo.InvariantCulture);
			}
			return 0;
		}

		private int GetInt32(string key)
		{
			if (this.options.Contains(key))
			{
				return Convert.ToInt32(this.options[key], CultureInfo.InvariantCulture);
			}
			return 0;
		}

		private long GetInt64(string key)
		{
			if (this.options.Contains(key))
			{
				return Convert.ToInt64(this.options[key], CultureInfo.InvariantCulture);
			}
			return 0;
		}

		private IsolationLevel GetIsolationLevel(string key)
		{
			if (this.options.Contains(key))
			{
				string il = this.options[key].ToString().ToLower(CultureInfo.CurrentCulture);
				switch (il)
				{
					case "readcommitted":
						return IsolationLevel.ReadCommitted;

					case "readuncommitted":
						return IsolationLevel.ReadUncommitted;

					case "repeatableread":
						return IsolationLevel.RepeatableRead;

					case "serializable":
						return IsolationLevel.Serializable;

					case "chaos":
						return IsolationLevel.Chaos;

					case "unspecified":
						return IsolationLevel.Unspecified;
				}
			}

			return IsolationLevel.ReadCommitted;
		}

		private void CheckIsolationLevel()
		{
			string il = this.options["isolation level"].ToString().ToLower(CultureInfo.CurrentCulture);
			switch (il)
			{
				case "readcommitted":
				case "readuncommitted":
				case "repeatableread":
				case "serializable":
				case "chaos":
				case "unspecified":
					break;

				default:
					throw new ArgumentException("Specified Isolation Level is not valid.");
			}
		}

		#endregion
	}
}
