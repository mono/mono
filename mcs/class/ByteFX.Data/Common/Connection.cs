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
	public class Connection : Component
	{
		protected StringDictionary	m_ConnSettings = new StringDictionary();
		protected string			m_ConnString;
		internal  ConnectionState	m_State;


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
			m_State = ConnectionState.Closed;
		}


		[Browsable(true)]
		public string DataSource
		{
			get
			{
				String s = m_ConnSettings["data source"];
				if (s == null)
					s = m_ConnSettings["server"];
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
				string s = m_ConnSettings["user id"];
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
				string pwd = m_ConnSettings["password"];
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
				if (m_ConnSettings == null)
					return 30;
				return Convert.ToInt32(m_ConnSettings["connection timeout"]);
			}
		}
		
		[Browsable(true)]
		public string Database
		{
			get	
			{	
				// Returns an initial database as set in the connection string.
				// An empty string indicates not set - do not return a null reference.
				return m_ConnSettings["database"];
			}
		}

		[Browsable(false)]
		public ConnectionState State
		{
			get { return m_State; }
		}
	}
}
