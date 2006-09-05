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
 *	Copyright (c) 2004 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/overview/*'/>
	public sealed class FbConnectionStringBuilder
	{
		#region Private	fields

		private Hashtable options;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="ConnectionString"]/*'/>
		public string ConnectionString
		{
			get { return this.ToString(); }
			set { this.Load(value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="UserID"]/*'/>
		public string UserID
		{
			get { return this.GetString("user id"); }
			set { this.SetValue("user id", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Password"]/*'/>
		public string Password
		{
			get { return this.GetString("password"); }
			set { this.SetValue("password", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="DataSource"]/*'/>
		public string DataSource
		{
			get { return this.GetString("data source"); }
			set { this.SetValue("data source", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Port"]/*'/>
		public int Port
		{
			get { return this.GetInt32("port number"); }
			set { this.SetValue("port number", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Database"]/*'/>
		public string Database
		{
			get { return this.GetString("database"); }
			set { this.SetValue("database", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="PacketSize"]/*'/>
		public short PacketSize
		{
			get { return this.GetInt16("packet size"); }
			set { this.SetValue("packet size", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Role"]/*'/>
		public string Role
		{
			get { return this.GetString("role name"); }
			set
			{
				if (value == null)
				{
					value = String.Empty;
				}
				this.SetValue("role name", value);
			}
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Dialect"]/*'/>
		public byte Dialect
		{
			get { return this.GetByte("dialect"); }
			set { this.SetValue("dialect", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Charset"]/*'/>
		public string Charset
		{
			get { return this.GetString("charset"); }
			set { this.SetValue("charset", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="ConnectionTimeout"]/*'/>
		public int ConnectionTimeout
		{
			get { return this.GetInt32("connection timeout"); }
			set { this.SetValue("connection timeout", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="Pooling"]/*'/>
		public bool Pooling
		{
			get { return this.GetBoolean("pooling"); }
			set { this.SetValue("pooling", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="ConnectionLifeTime"]/*'/>
		public long ConnectionLifeTime
		{
			get { return this.GetInt64("connection lifetime"); }
			set { this.SetValue("connection lifetime", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="MinPoolSize"]/*'/>
		public int MinPoolSize
		{
			get { return this.GetInt32("min pool size"); }
			set { this.SetValue("min pool size", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="MaxPoolSize"]/*'/>
		public int MaxPoolSize
		{
			get { return this.GetInt32("max pool size"); }
			set { this.SetValue("max pool size", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="FetchSize"]/*'/>
		public int FetchSize
		{
			get { return this.GetInt32("fetch size"); }
			set { this.SetValue("fetch size", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="ServerType"]/*'/>
		public int ServerType
		{
			get { return this.GetInt32("server type"); }
			set { this.SetValue("server type", value); }
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/property[@name="IsolationLevel"]/*'/>
		public IsolationLevel IsolationLevel
		{
			get { return this.GetIsolationLevel("isolation level"); }
			set { this.SetValue("isolation level", value.ToString()); }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/constructor[@name="ctor"]/*'/>
		public FbConnectionStringBuilder() : this(null)
		{
		}

		/// <include file='Doc/en_EN/FbConnectionStringBuilder.xml'	path='doc/class[@name="FbConnectionStringBuilder"]/constructor[@name="ctor(System.String)"]/*'/>
		public FbConnectionStringBuilder(string connectionString)
		{
			this.options = new Hashtable();

			if (connectionString != null && connectionString.Length > 0)
			{
				this.Load(connectionString);
			}
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overrided method, returns the Firebird connection string.
		/// </summary>
		/// <returns>The Firebird connection string.</returns>
		public override string ToString()
		{
			StringBuilder cs = new StringBuilder();

			IDictionaryEnumerator e = this.options.GetEnumerator();
			while (e.MoveNext())
			{
				if (e.Value != null)
				{
					if (cs.Length > 0)
					{
						cs.Append(";");
					}
					string key = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Key.ToString());
					cs.AppendFormat(CultureInfo.CurrentCulture, "{0}={1}", key, e.Value);
				}
			}

			return cs.ToString();
		}

		#endregion

		#region Private	Methods

		private void Load(string connectionString)
		{
			string[]	keyPairs = connectionString.Split(';');
			Hashtable	synonyms = FbConnectionString.Synonyms;

			if (this.options != null)
			{
				this.options.Clear();
			}

			foreach (string keyPair in keyPairs)
			{
				string[] values = keyPair.Split('=');

				if (values.Length == 2 &&
					values[0] != null && values[0].Length > 0 &&
					values[1] != null && values[1].Length > 0)
				{
					values[0] = values[0].ToLower(CultureInfo.CurrentCulture).Trim();

					if (synonyms.Contains(values[0]))
					{
						string key = (string)synonyms[values[0]];
						this.options[key] = values[1].Trim();
					}
				}
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
				return Convert.ToByte(this.options[key], CultureInfo.InvariantCulture);
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

		private void SetValue(string key, object value)
		{
			if (this.options.Contains(key))
			{
				this.options[key] = value;
			}
			else
			{
				this.options.Add(key, value);
			}
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

		#endregion
	}
}
