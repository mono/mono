/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// System.DirectoryServices.DirectoryEntries.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C) Novell Inc.
//

using System.Collections;
using Novell.Directory.Ldap;

namespace System.DirectoryServices
{
	
	/// <summary>
	///Contains the children (child entries) of an entry in
	/// a Ldap Directory
	/// </summary>
	public class DirectoryEntries : IEnumerable
	{
		private LdapConnection _Conn=null;
		private string _Bpath=null;
		private string _Buser=null;
		private string _Bpass=null;
		private string _Basedn=null;
		private ArrayList m_oValues=null;


		/// <summary> Initializes the Connection and other properties.
		/// 
		/// </summary>
		private void InitBlock()
		{
			try			{
				LdapUrl lUrl=new LdapUrl(_Bpath);
				_Conn = new LdapConnection();
				_Conn.Connect(lUrl.Host,lUrl.Port);
				_Conn.Bind(_Buser,_Bpass);
			}
			catch(LdapException ex)			{
				throw ex;
			}
			catch(Exception e)				{
				throw e;
			}
		}

		internal string Basedn
		{
			get										{
				if( _Basedn == null)				{
					LdapUrl lurl=new LdapUrl(_Bpath);
					string bdn = lurl.getDN();
					if( bdn != null)
						_Basedn = bdn;
					else
						_Basedn = "";
				}
				return _Basedn;
			}
		}
				
		/// <summary> Contains the Path of the Container under which
		/// the entries belongs to.
		/// </summary>
		internal string Bpath
		{
			get			{
				return _Bpath;
			}
			set			{
				_Bpath=value;
			}
		}

		/// <summary> Returns the connection object used to communicate with
		/// Ldap server
		/// </summary>
		internal LdapConnection Conn
		{
			get						{
				if( _Conn == null)	{
					InitBlock();
				}
				return _Conn;
			}
			set						{
				_Conn=value;
			}
		}

		/// <summary> Constructs a collection of all the child entries of
		/// an entry
		/// </summary>
		/// <param name="path"> Path of the entry
		/// </param>
		/// <param name="uname"> Username to Bind as while authenticating to ldap
		/// server</param>
		/// <param name="passwd"> Password of the user</param>
		internal DirectoryEntries(string path, string uname, string passwd)
		{
			_Bpath = path;
			_Buser = uname;
			_Bpass = passwd;
		}

		/// <summary> Constructs a collection of all the child entries of
		/// a entry
		/// </summary>
		/// <param name="path"> Path of the entry
		/// </param>
		/// <param name="lc"> connection object used to connect to ldap server
		/// </param>
		internal DirectoryEntries(string path,  LdapConnection lc)
		{
			_Bpath = path;
			_Conn = lc;
		}

		public SchemaNameCollection SchemaFilter {
			[MonoTODO]
			get { throw new NotImplementedException ("System.DirectoryServices.DirectoryEntries.SchemaFilter"); }
		}

		public  IEnumerator GetEnumerator()
		{
			m_oValues= new ArrayList();
			string[] attrs={"objectClass"};
			LdapSearchResults lsc= Conn.Search(	Basedn,
												LdapConnection.SCOPE_ONE,
												"objectClass=*",
												attrs,
												false);

			LdapUrl Burl=new LdapUrl(_Bpath);
			string host=Burl.Host;
			int port=Burl.Port;

			while (lsc.hasMore())			{
				LdapEntry nextEntry = null;
				try					{
					nextEntry = lsc.next();
				}
				catch(LdapException e) 		{
					// Exception is thrown, go for next entry
					continue;
				}
				DirectoryEntry dEntry=new DirectoryEntry(Conn);
				string eFdn=nextEntry.DN;
				LdapUrl curl=new LdapUrl(host,port,eFdn);
				dEntry.Path=curl.ToString();
				m_oValues.Add((DirectoryEntry) dEntry);
			}
			return m_oValues.GetEnumerator();
		}

		/// <summary> Creates a request to create a new entry in the container.
		/// 
		/// </summary>
		/// <param name="name"> RDN of the entry to be created
		/// </param>
		/// <param name="schemaClassName"> StructuralClassName of the entry to be
		/// created.
		/// </param>
		public DirectoryEntry Add(	string name,string schemaClassName)
		{
			DirectoryEntry ent=new DirectoryEntry(Conn);
			LdapUrl Burl=new LdapUrl(_Bpath);
			string baseDn = Burl.getDN();
			string eFdn=((baseDn != null && baseDn.Length != 0) ? (name + "," + baseDn) : name);
			LdapUrl curl=new LdapUrl(Burl.Host,Burl.Port,eFdn);
			ent.Path=curl.ToString();
			ent.Nflag = true;
			return ent;
		}

		/// <summary>
		/// Deletes a child DirectoryEntry from this collection
		/// </summary>
		/// <param name="entry">The DirectoryEntry to delete</param>
		public void Remove(	DirectoryEntry entry )
		{
			LdapUrl Burl=new LdapUrl(_Bpath);
			string eFDN = entry.Name + "," + Burl.getDN();
			Conn.Delete( eFDN);
		}

		/// <summary>
		/// Returns the child with the specified name.
		/// </summary>
		/// <param name="filter">relative distinguised name of the child
		/// </param>
		/// <returns>Child entry with the specified name </returns>
		public DirectoryEntry Find(string name)
		{
			DirectoryEntry child=CheckEntry(name);
			return child;
		}

		/// <summary>
		/// Returns the child with the specified name and of the specified type.
		/// </summary>
		/// <param name="filter">relative distinguised name of the child
		/// </param>
		/// <param name="otype"> Type of the child i.e strutcuralObjectClass
		/// name of the child </param>
		/// <returns>Child entry with the specified name and type</returns>
		public DirectoryEntry Find(string name, string schemaClassName)
		{
			DirectoryEntry child=CheckEntry(name);

			if( child != null)			{
				if(child.Properties["objectclass"].ContainsCaselessStringValue(schemaClassName))
					return child;
				else
					throw new SystemException("An unknown directory object was requested");
			}
			return child;
		}

		/// <summary>
		/// Checks whether the entry with the specified Relative distinguised name
		/// exists or not.
		/// </summary>
		/// <param name="rdn"> Relative distinguished name of the entry</param>
		/// <returns>DirectoryEntry object of Entry if entry exists,
		/// Null if entry doesn't exist </returns>
		private DirectoryEntry CheckEntry(string rdn)
		{
			string Ofdn=null;
			DirectoryEntry cEntry=null;

			Ofdn=rdn+","+Basedn;
			string[] attrs={"objectClass"};
			try										{
				LdapSearchResults lsc= Conn.Search(	Ofdn,
													LdapConnection.SCOPE_BASE,
													"objectClass=*",
													attrs,
													false);
				while(lsc.hasMore())				{
					LdapEntry nextEntry = null;
					try								{
						nextEntry = lsc.next();
						cEntry =  new DirectoryEntry(Conn);
						LdapUrl Burl=new LdapUrl(_Bpath);
						LdapUrl curl=new LdapUrl(Burl.Host,Burl.Port,Ofdn);
						cEntry.Path=curl.ToString();
					}
					catch(LdapException e) 			{
						// Exception is thrown, go for next entry
						throw e;
					}
					break;
				}

			}
			catch(LdapException le)
			{
				if(le.ResultCode == LdapException.NO_SUCH_OBJECT)	{
					return null;
				}
				else		{
					throw le;
				}
			}
			catch(Exception e)		{
				throw e;
			}
			return cEntry;
		}

	}

}

