// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
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
using System.ComponentModel;
using System.Collections.Specialized;
using System.Data;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Connection.
	/// </summary>
	[ToolboxItem(false)]
	[System.ComponentModel.DesignerCategory("Code")]
	public class Connection : Component
	{
		internal ConnectionString	connString;
		internal ConnectionState	state;
		internal IDataReader		reader = null;


		public Connection(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
			Init();
		}

		public Connection()
		{
			Init();
		}

		protected void Init() 
		{
			state = ConnectionState.Closed;
		}


		[Browsable(true)]
		public string DataSource
		{
			get
			{
				String s = connString["data source"];
				if (s == null || s.Length == 0)
					return "localhost";
				return s;
			}
		}

		[Browsable(false)]
		public string User
		{
			get
			{
				string s = connString["user id"];
				if (s == null)
					s = "";
				return s;
			}
		}

		[Browsable(false)]
		public string Password
		{
			get
			{
				string pwd = connString["password"];
				if (pwd == null)
					pwd = "";
				return pwd;
			}
		}

		[Browsable(true)]
		public int ConnectionTimeout
		{
			get
			{
				// Returns the connection time-out value set in the connection
				// string. Zero indicates an indefinite time-out period.
				if (connString == null)
					return 30;
				return connString.GetIntOption("connection timeout", 30);
			}
		}
		
		[Browsable(true)]
		public string Database
		{
			get	
			{	
				// Returns an initial database as set in the connection string.
				// An empty string indicates not set - do not return a null reference.
				return connString["database"];
			}
		}

		[Browsable(false)]
		public ConnectionState State
		{
			get { return state; }
		}

		internal IDataReader Reader
		{
			get { return reader; }
			set { reader = value; }
		}
	}
}
