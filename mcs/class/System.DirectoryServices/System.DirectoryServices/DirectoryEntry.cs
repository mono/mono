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
// System.DirectoryServices.DirectoryEntry.cs
//
// Authors:
//   Sunil Kumar (sunilk@novell.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	 Boris Kirzner (borisk@mainsoft.com)
//
// (C)  Novell Inc.
//

using System.ComponentModel;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;
using System.Globalization;
using System.DirectoryServices.Design;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.InteropServices;

namespace System.DirectoryServices
{
	
	/// <summary>
	///Encapsulates a node or object in the Ldap Directory hierarchy.
	/// </summary>
	[TypeConverter (typeof (DirectoryEntryConverter))]
	public class DirectoryEntry : Component	
	{
		private static readonly string DEFAULT_LDAP_HOST = "System.DirectoryServices.DefaultLdapHost";
		private static readonly string DEFAULT_LDAP_PORT = "System.DirectoryServices.DefaultLdapPort";

		private LdapConnection _conn = null;
		private AuthenticationTypes _AuthenticationType=AuthenticationTypes.None;
		private DirectoryEntries _Children;
		private string _Fdn = null;
		private string _Path="";
		private string _Name=null;
		private DirectoryEntry _Parent=null;
		private string _Username;
		private string _Password;
		//private string _Nativeguid;
		private PropertyCollection _Properties = null;
		private string _SchemaClassName=null;
		private bool _Nflag = false;
		private bool _usePropertyCache=true;
		private bool _inPropertiesLoading;

		/// <summary>
		/// Returns entry's Fully distinguished name.
		/// </summary>
		internal string Fdn
		{
			get	{
				if (_Fdn == null) {
					LdapUrl lUrl = new LdapUrl (ADsPath);
					string fDn=lUrl.getDN();
					if(fDn != null)
						_Fdn = fDn;
					else
						_Fdn=String.Empty;
				}
				return _Fdn;
			}
		}

		/// <summary>
		///  Returns the connection object used to communicate with
		/// Ldap server
		/// </summary>
		internal LdapConnection conn
		{
			get			{
				if( _conn == null)
					InitBlock();

				return _conn;
			}
			set			{
				_conn=value;
			}
		}

		/// <summary>
		/// Flag to check whether the entry is to be cerated or it already
		/// exists.
		/// </summary>
		internal bool Nflag
		{
			get			{
				return _Nflag;
			}
			set			{
				_Nflag = value;
			}
		}

		/// <summary> Initializes the Connection and other properties.
		/// 
		/// </summary>
		private void InitBlock()
		{
			try			{
				_conn= new LdapConnection ();
				LdapUrl lUrl = new LdapUrl (ADsPath);
				_conn.Connect(lUrl.Host,lUrl.Port);
				_conn.Bind(Username,Password, (Novell.Directory.Ldap.AuthenticationTypes)AuthenticationType);
			}
			catch(LdapException ex)			{
				throw ex;
			}
			catch(Exception e)			{
				throw e;
			}
		}

		/// <summary>
		/// Initializes the Entry specific properties e.g entry DN etc.
		/// </summary>
		void InitEntry()
		{			
			LdapUrl lUrl = new LdapUrl (ADsPath);
			string dn = lUrl.getDN();
			if (dn != null ) {
				if (String.Compare (dn,"rootDSE",true) == 0)
					InitToRootDse (lUrl.Host,lUrl.Port);
				else {
				DN userDn = new DN (dn);
				String[] lRdn = userDn.explodeDN(false);
				_Name = (string)lRdn[0];
				_Parent = new DirectoryEntry(conn);
				_Parent.Path = GetLdapUrlString (lUrl.Host,lUrl.Port,userDn.Parent.ToString ());
				}
			}
			else			{
				_Name=lUrl.Host+":"+lUrl.Port;
				_Parent = new DirectoryEntry(conn);
				_Parent.Path = "Ldap:";
			}
		}

		/// <summary>
		/// Initializes a new instance of the DirectoryEntry class
		/// </summary>
		public DirectoryEntry()
		{
		}

		/// <summary>
		/// Initializes a new instance of the DirectoryEntry class that binds
		///  to the specified native Active Directory object.
		/// </summary>
		/// <param name="adsObject"> native active directory object</param>
		public DirectoryEntry(object adsObject)
		{
			 throw new NotImplementedException();
		}

		/// <summary>
		/// Initializes a new instance of the DirectoryEntry class that binds
		///  this instance to the node in Ldap Directory located at the
		///  specified path.
		/// </summary>
		/// <param name="path"> Path of the entry i.e Ldap URL specifying 
		/// entry path</param>
		public DirectoryEntry(string path)
		{
			_Path=path;
		}

		/// <summary>
		/// Initializes a new instance of the DirectoryEntry class. The Path,
		///  Username, and Password properties are set to the specified values.
		/// </summary>
		/// <param name="path">Path of the entry i.e Ldap URL specifying 
		/// entry path</param>
		/// <param name="username">user name to use when authenticating the client
		/// </param>
		/// <param name="password">password to use when authenticating the client
		/// </param>
		public DirectoryEntry(string path,string username,string password)
		{
			_Path=path;
			_Username=username;
			_Password=password;
		}

		/// <summary>
		/// Initializes a new instance of the DirectoryEntry class. The Path,
		///  Username, and Password properties are set to the specified values.
		/// </summary>
		/// <param name="path">Path of the entry i.e Ldap URL specifying 
		/// entry path</param>
		/// <param name="username">user name to use when authenticating the client
		/// </param>
		/// <param name="password">password to use when authenticating the client
		/// </param>
		/// <param name="authenticationType"> type of authentication to use</param>
		public DirectoryEntry(
				string path,
				string username,
				string password,
				AuthenticationTypes authenticationType)
		{
			_Path=path;
			_Username=username;
			_Password=password;
			_AuthenticationType=authenticationType;
		}

		/// <summary>
		/// Creates the entry object
		/// </summary>
		/// <param name="lconn">Connection object used to communicate with
		/// Ldap server</param>
		internal DirectoryEntry(LdapConnection lconn)
		{
			conn = lconn;
		}

		/// <summary>
		/// Returns Type of authentication to use while Binding to Ldap server
		/// </summary>
		[DSDescription ("Type of authentication to use while Binding to Ldap server")]
		[DefaultValue (AuthenticationTypes.None)]
		public AuthenticationTypes AuthenticationType 
		{
			get 
			{
				return _AuthenticationType;
			}
			set 
			{
				_AuthenticationType = value;
			}
		}

		/// <summary>
		/// Gets a DirectoryEntries containing the child entries of this node
		///  in the Ldap Directory hierarchy.
		/// </summary>
		/// <value>A DirectoryEntries containing the child entries of this node
		///  in the Ldap Directory hierarchy.</value>
		///  <remarks>
		///  The child entries are only the immediate children of this node.
		///  Use this property to find, retrieve, or create a directory entry
		///  in the hierarchy. This property is a collection that, along with 
		///  usual iteration capabilities, provides an Add method through which
		///  you add a node to the collection directly below the parent node
		///  that you are currently bound to. When adding a node to the 
		///  collection, you must specify a name for the new node and the name of 
		///  a schema template that you want to associate with the node. For 
		///  example, you might want to use a schema titled "Computer" to add 
		///  new computers to the hierarchy.
		///  </remarks>
		[DSDescription ("Child entries of this node")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public DirectoryEntries Children 
		{
			get 
			{
				_Children = new DirectoryEntries(ADsPath, conn);
				return _Children;
			}
		}

		/// <summary>
		/// Gets the globally unique identifier (GUID) of the DirectoryEntry
		/// </summary>
		/// <value>The globally unique identifier of the DirectoryEntry.</value>
		/// <remarks>
		/// Not implemented yet.		
		/// </remarks>
		[DSDescription ("A globally unique identifier for this DirectoryEntry")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MonoTODO]
		public Guid Guid 
		{
			get 
			{
				throw new NotImplementedException();
			}

		}

		/// <summary>
		/// Gets the name of the object as named with the underlying directory
		///  service
		/// </summary>
		/// <value>The name of the object as named with the underlying directory
		///  service</value>
		/// <remarks>This name, along with SchemaClassName, distinguishes this
		///  entry from its siblings and must be unique amongst its siblings 
		///  in each instance of DirectoryEntry.</remarks>
		[DSDescription ("The name of the object as named with the underlying directory")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string Name 
		{
			get								{
				if(_Name==null)				{
					if(CheckEntry(conn,ADsPath))
						InitEntry();
					else
						throw new SystemException("There is no such object on the server");
				}
				return _Name;
			}
		}

		/// <summary>
		/// Gets this entry's parent in the Ldap Directory hierarchy.
		/// </summary>
		/// <value>This entry's parent in the Active Directory hierarc</value>
		[DSDescription ("This entry's parent in the Ldap Directory hierarchy.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public DirectoryEntry Parent 
		{
			get			{
				if(_Parent==null)				{
					if(CheckEntry(conn,ADsPath))
						InitEntry();
					else
						throw new SystemException("There is no such object on the server");
				}
				return _Parent;
			}
		}

		/// <summary>
		/// Gets the globally unique identifier of the DirectoryEntry, as 
		/// returned from the provider
		/// </summary>
		/// <value>
		/// The globally unique identifier of the DirectoryEntry, as returned 
		/// from the provider.
		/// </value>
		/// <remarks>
		/// Not implemented yet.
		/// </remarks>
		[DSDescription ("The globally unique identifier of the DirectoryEntry, as returned from the provider")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MonoTODO]
		public string NativeGuid 
		{
			get			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the native Active Directory Service Interfaces (ADSI) object.
		/// </summary>
		/// <remarks>
		/// Not implemented yet
		[DSDescription ("The native Active Directory Service Interfaces (ADSI) object.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public object NativeObject 
		{
			[MonoTODO]
			get			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Determines if a cache should be used.
		/// </summary>
		[DSDescription ("Determines if a cache should be used.")]
		[DefaultValue (true)]
		public bool UsePropertyCache
		{
			get 
			{
				return _usePropertyCache;
			}
			set 
			{
				_usePropertyCache = value;
			}
		}

		/// <summary>
		/// Gets or sets the password to use when authenticating the client.
		/// </summary>
		/// <value>
		/// The password to use when authenticating the client.
		/// </value>
		/// <remarks>
		/// You can set the Username and password in order to specify alternate 
		/// credentials with which to access the information in Ldap Directory. 
		/// Any other DirectoryEntry objects retrieved from this instance (for 
		/// example, through Children) are automatically created with the same 
		/// alternate credentials.
		/// </remarks>
		[DSDescription ("The password to use when authenticating the client.")]
		[DefaultValue (null)]
		[Browsable (false)]
		public string Password 
		{
			get		{
				return _Password;
			}
			set			{
				_Password = value;
			}

		}

		/// <summary>
		/// Gets or sets the user name to use when authenticating the client.
		/// </summary>
		/// <value>
		/// The user name to use when authenticating the client.
		/// </value>
		/// <remarks>
		/// You can set the user name and Password in order to specify alternate 
		/// credentials with which to access the information in Ldap Directory. 
		/// Any other DirectoryEntry objects retrieved from this instance (for 
		/// example, through Children) are automatically created with the same 
		/// alternate 
		/// </remarks>
		[DSDescription ("The user name to use when authenticating the client.")]
		[DefaultValue (null)]
		[Browsable (false)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Username 
		{
			get			{
				return _Username ;
			}
			set			{
				_Username = value;
			}

		}

		/// <summary>
		/// Gets or sets the path for this DirectoryEntry.
		/// </summary>
		/// <value>
		/// The path of this DirectoryEntry. The default is an empty string ("").
		/// </value>
		/// <remarks>
		/// The Path property uniquely identifies this entry in a networked 
		/// environment. This entry can always be retrieved using this Path.
		/// 
		/// Setting the Path retrieves a new entry from the directory store; it 
		/// does not change the path of the currently bound entry.
		/// 
		/// The classes associated with the DirectoryEntry component can be used 
		/// with any of the  Directory service providers. Some of the current 
		/// providers are Internet Information Services (IIS), Lightweight Directory 
		/// Access Protocol (Ldap), Novell NetWare Directory Service (NDS), and WinNT.
		/// 
		/// Currently we Support only Ldap provider.
		/// e.g Ldap://[hostname]:[port number]/[ObjectFDN]
		/// </remarks>
		[DSDescription ("The path for this DirectoryEntry.")]
		[DefaultValue ("")]
		[RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Path 
		{
			get			{
				return _Path;
			}
			set			{
				if (value == null)
					_Path = String.Empty;
				else
					_Path = value;
			}
		}

		internal string ADsPath
		{
			get	{
				if (Path == null || Path == String.Empty) {
					DirectoryEntry rootDse = new DirectoryEntry ();
					rootDse.InitToRootDse (null,-1);
					string namingContext = (string) rootDse.Properties ["defaultNamingContext"].Value;
					if ( namingContext == null )
						namingContext = (string) rootDse.Properties ["namingContexts"].Value;

					LdapUrl actualUrl= new LdapUrl (DefaultHost,DefaultPort,namingContext);
					return actualUrl.ToString ();
				}
				return Path;
			}
		}

		/// <summary>
		/// Gets a PropertyCollection of properties set on this object.
		/// </summary>
		/// <value>
		/// A PropertyCollection of properties set on this object.
		/// </value>
		[DSDescription ("Properties set on this object.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public PropertyCollection Properties
		{
			get			{
				return GetProperties (true);
			}
		}

		/// <summary>
		/// Gets the name of the schema used for this DirectoryEntry
		/// </summary>
		/// <value>
		/// The name of the schema used for this DirectoryEntry.
		/// </value>
		[DSDescription ("The name of the schema used for this DirectoryEntry.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string SchemaClassName 
		{
			get			{
				if(_SchemaClassName==null)				{
						_SchemaClassName = FindAttrValue("structuralObjectClass");
				}
				return _SchemaClassName;
			}
		}

		/// <summary>
		/// Gets the current schema directory entry.
		/// </summary>
		/// <remarks>
		/// Not implemented yet
		[DSDescription ("The current schema directory entry.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public DirectoryEntry SchemaEntry 
		{
			[MonoTODO]
			get			{
				throw new NotImplementedException();
			}
		}

		private string DefaultHost
		{
			get {
				string defaultHost = (string) AppDomain.CurrentDomain.GetData (DEFAULT_LDAP_HOST);

				if (defaultHost == null) {
					NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig ("mainsoft.directoryservices/settings");
					if (config != null) 
						defaultHost = config ["servername"];

					if (defaultHost == null) 
						defaultHost = "localhost";

					AppDomain.CurrentDomain.SetData (DEFAULT_LDAP_HOST,defaultHost);
				}
				return defaultHost;
			}
		}

		private int DefaultPort
		{
			get {
				string defaultPortStr = (string) AppDomain.CurrentDomain.GetData (DEFAULT_LDAP_PORT);

				if (defaultPortStr == null) {
					NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig ("mainsoft.directoryservices/settings");
					if (config != null)
						defaultPortStr = config ["port"];

					if (defaultPortStr == null) 
						defaultPortStr = "389";

					AppDomain.CurrentDomain.SetData (DEFAULT_LDAP_PORT,defaultPortStr);
				}
				return Int32.Parse (defaultPortStr);
			}
		}

		private void InitToRootDse(string host,int port)
		{
			if ( host == null )
				host = DefaultHost;
			if ( port < 0 )
				port = DefaultPort;	
		
			LdapUrl rootPath = new LdapUrl (host,port,String.Empty);
			string [] attrs = new string [] {"+","*"};
			DirectoryEntry rootEntry = new DirectoryEntry (rootPath.ToString (),this.Username,this.Password,this.AuthenticationType);
			DirectorySearcher searcher = new DirectorySearcher (rootEntry,null,attrs,SearchScope.Base);

			SearchResult result = searcher.FindOne ();			
			// copy properties from search result
			PropertyCollection pcoll = new PropertyCollection ();
			foreach (string propertyName in result.Properties.PropertyNames) {
				System.Collections.IEnumerator enumerator = result.Properties [propertyName].GetEnumerator ();
				if (enumerator != null)
					while (enumerator.MoveNext ())
						if (String.Compare (propertyName,"ADsPath",true) != 0)
							pcoll [propertyName].Add (enumerator.Current);
			}			
			this.SetProperties (pcoll);
			this._Name = "rootDSE";
		}

		private void SetProperties(PropertyCollection pcoll)
		{
			_Properties = pcoll;
		}

		/// <summary>
		/// Returns entry properties.
		/// </summary>
		/// <param name="forceLoad">Specifies whenever to force the properties load from the server if local property cache is empty.</param>
		/// <returns></returns>
		private PropertyCollection GetProperties(bool forceLoad)
		{
			if (_Properties == null) {
				// load properties into a different collection 
				// to preserve original collection state if exception occurs
				PropertyCollection properties = new PropertyCollection (this);
				if (forceLoad && !Nflag)				
					LoadProperties (properties,null);

				_Properties = properties ;
			}			
			return _Properties;
		}

		/// <summary>
		/// Loads the values of the specified properties into the property cache.
		/// </summary>
		/// <param name="propertyNames">An array of the specified properties.</param>
		private void LoadProperties(PropertyCollection properties,string[] propertyNames)
		{
			_inPropertiesLoading = true;
			try	{
				LdapSearchResults lsc=conn.Search (Fdn,LdapConnection.SCOPE_BASE,"objectClass=*",propertyNames,false);
				if (lsc.hasMore ()) {
					LdapEntry nextEntry = lsc.next ();
					string [] lowcasePropertyNames = null;
					int length = 0;
					if (propertyNames != null) {
						length = propertyNames.Length;
						lowcasePropertyNames = new string [length];
						for(int i=0; i < length; i++)
							lowcasePropertyNames [i] = propertyNames [i].ToLower ();
					}
					foreach (LdapAttribute attribute in nextEntry.getAttributeSet ())	{
						string attributeName = attribute.Name;
						if ((propertyNames == null) || (Array.IndexOf (lowcasePropertyNames,attributeName.ToLower ()) != -1)) {
							properties [attributeName].Value = null;
							properties [attributeName].AddRange (attribute.StringValueArray);
							properties [attributeName].Mbit=false;
						}
					}
				}
			}
			finally {
				_inPropertiesLoading = false;
			}
		}

		/// <summary>
		/// Searches an entry in the Ldap directory and returns the attribute value
		/// </summary>
		/// <param name="attrName">attribute whose value is required</param>
		/// <returns> value of the attribute stored in Ldap directory</returns>
		private string FindAttrValue(string attrName)
		{
			string aValue=null;
			string[] attrs={attrName};

			LdapSearchResults lsc=conn.Search(	Fdn,
												LdapConnection.SCOPE_BASE,
												"objectClass=*",
												attrs,
												false);
			while(lsc.hasMore())			{
				LdapEntry nextEntry = null;
				try 						{
					nextEntry = lsc.next();
				}
				catch(LdapException e)		{
					// Exception is thrown, go for next entry
					throw e;
				}
				LdapAttribute attribute = nextEntry.getAttribute(attrName);
				aValue = attribute.StringValue;
				break;
			}
			return aValue;
		}

		/// <summary>
		/// Modifies an entry in the Ldap directory with the input LdapModification
		/// values.
		/// </summary>
		/// <param name="mods">Array consisting of the entry attribute name and the
		/// attribute  values to be modified.</param>
		private void ModEntry(LdapModification[] mods)
		{

			try						{
				conn.Modify(Fdn,mods);
			}
			catch(LdapException le)	{
				throw le;
			}
		}

		/// <summary>
		/// Checks whether the entry exists in the Ldap directory or not
		/// </summary>
		/// <param name="lconn">
		/// Connection used to communicate with directory
		/// </param>
		/// <param name="epath">
		/// path of the entry
		/// </param>
		/// <returns>
		///		true of the entry exists in the Ldap directory
		///		false if entry doesn't exists
		/// </returns>
		private static bool CheckEntry(LdapConnection lconn, string epath)
		{
			LdapUrl lUrl=new LdapUrl(epath);
			string eDn=lUrl.getDN();
			if(eDn==null)
			{
				eDn = String.Empty;
			}
			// rootDSE is a "virtual" entry that always exists
			else if (String.Compare (eDn,"rootDSE",true) == 0)
				return true;

			string[] attrs={"objectClass"};
			try
			{
				LdapSearchResults lsc=lconn.Search(	eDn,
					LdapConnection.SCOPE_BASE,
					"objectClass=*",
					attrs,
					false);
				while(lsc.hasMore())
				{
					LdapEntry nextEntry = null;
					try 
					{
						nextEntry = lsc.next();
					}
					catch(LdapException e) 
					{
						// Exception is thrown, go for next entry
						throw e;
					}
					break;
				}

			}
			catch(LdapException le)
			{
				if(le.ResultCode == LdapException.NO_SUCH_OBJECT)
				{
					return false;
				}
				else
				{
					throw le;
				}
			}
			catch(Exception e)
			{
				throw e;
			}
			return true;
		}

		/// <summary>
		/// Closes the DirectoryEntry and releases any system resources associated 
		/// with this component.
		/// </summary>
		/// <remarks>
		/// Following a call to Close, any operations on the DirectoryEntry might 
		/// raise exceptions.
		/// </remarks>
		public void Close()
		{
			if (_conn != null && _conn.Connected) {
				_conn.Disconnect();
			}
		}

		/// <summary>
		/// Creates a copy of this entry as a child of the specified parent.
		/// </summary>
		/// <param name="newParent">The parent DirectoryEntry. 	</param>
		/// <returns>A copy of this entry as a child of the specified parent.
		[MonoTODO]
		public DirectoryEntry CopyTo(DirectoryEntry newParent)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deletes this entry and its entire subtree from the Active Directory 
		/// hierarchy.
		/// </summary>
		/// <remarks>
		/// CAUTION   The entry and its entire subtree are deleted from the 
		/// Ldap Directory hierarchy.
		/// </remarks>
		public void DeleteTree()
		{
			System.Collections.IEnumerator ienum = Children.GetEnumerator();
			while(ienum.MoveNext())
			{
				DirectoryEntry de=(DirectoryEntry)ienum.Current;
				conn.Delete(de.Fdn);
			}
			conn.Delete(Fdn);
		}

		/// <summary>
		/// Searches the directory store at the specified path to see whether 
		/// an entry exists
		/// </summary>
		/// <param name="path">
		/// The path at which to search the directory store. 
		/// </param>
		/// <returns>
		/// true if an entry exists in the directory store at the specified 
		/// path; otherwise, false.
		/// </returns>
		public static bool Exists(string path)
		{
			LdapConnection aconn=new LdapConnection();
			LdapUrl lurl=new LdapUrl(path);
			aconn.Connect(lurl.Host,lurl.Port);
			aconn.Bind("","");
			if(CheckEntry(aconn,path))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Moves this entry to the specified parent.
		/// </summary>
		/// <param name="pentry">
		/// The parent to which you want to move this entry
		/// </param>
		public void MoveTo(DirectoryEntry newParent)
		{
			string oldParentFdn = Parent.Fdn;
			conn.Rename(Fdn, Name, newParent.Fdn, true);
			// TBD : threat multiple name instance in path
			Path = Path.Replace(oldParentFdn,newParent.Fdn);
			RefreshEntry();			
		}

		/// <summary>
		/// Moves this entry to the specified parent and changes its name to 
		/// the value of the newName parameter.
		/// </summary>
		/// <param name="newParent"> The parent to which you want to move 
		/// this entry
		/// </param>
		/// <param name="newName">
		/// The new name of this entry. 
		/// </param>
		public void MoveTo(	DirectoryEntry newParent,
							string newName	)
		{
			string oldParentFdn = Parent.Fdn;
			conn.Rename(Fdn, newName, newParent.Fdn, true);
			// TBD : threat multiple name instance in path
			Path = Path.Replace(oldParentFdn,newParent.Fdn).Replace(Name,newName);
			RefreshEntry();	
		}

		/// <summary>
		/// Changes the name of this entry.
		/// </summary>
		/// <param name="newName">
		/// The new name of the entry. 
		/// </param>
		/// <remarks>
		/// Note   This will also affect the path used to refer to this entry.
		/// </remarks>
		public void Rename(	string newName	)
		{
			string oldName = Name;
			conn.Rename( Fdn, newName, true);
			// TBD : threat multiple name instance in path
			Path = Path.Replace(oldName,newName);
			RefreshEntry();	
		}

		/// <summary>
		/// Calls a method on the native Active Directory.
		/// </summary>
		/// <param name="methodName">The name of the method to invoke. 
		/// </param>
		/// <param name="args">
		/// An array of type Object that contains the arguments of the method 
		/// to invoke. 
		/// </param>
		/// <returns>The return value of the invoked method</returns>
		/// <remarks>
		/// Not implemented.
		[MonoTODO]
		public object Invoke(string methodName,
			params object[] args)
		{
			throw new NotImplementedException();
		}

#if NET_2_0
		/// <summary>
		/// Gets a property value from the native Active Directory Entry.
		/// </summary>
		/// <param name="propertyName">The name of the property to get. 
		/// </param>
		/// <returns>The value of the property</returns>
		/// <remarks>
		/// Not implemented yet.
		[ComVisibleAttribute (false)]
		[MonoNotSupported ("")]
		public object InvokeGet (string propertyName)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Sets a property value on the native Active Directory Entry.
		/// </summary>
		/// <param name="propertyName">The name of the property to get. 
		/// </param>
		/// <param name="args">
		/// An array of type Object that contains the arguments of the property 
		/// beeing set. 
		/// </param>
		/// <remarks>
		/// Not implemented yet.
		[ComVisibleAttribute (false)]
		[MonoNotSupported ("")]
		public void InvokeSet (string propertyName, params object [] args)
		{
			throw new NotImplementedException ();
		}
#endif

		/// <summary>
		/// Creates a copy of this entry, as a child of the specified parent, with 
		/// the specified new name.
		/// </summary>
		/// <param name="newParent">The parent DirectoryEntry. 	</param>
		/// <param name="newName"> The name of the copy of this entry. 
		/// </param>
		/// <returns>A renamed copy of this entry as a child of the specified parent.
		[MonoTODO]
		public DirectoryEntry CopyTo( DirectoryEntry newParent,
			string newName	)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves any changes to the entry in the Ldap Directory store.
		/// </summary>
		/// <remarks>
		/// By default, changes to properties are done locally to a cache, and 
		/// property values to be read are cached after the first read. For more 
		/// information, see UsePropertyCache.
		/// Changes made to the cache include changes to the properties as well as 
		/// calls to Add (if this is the newly created entry).
		/// </remarks>
		public void CommitChanges()
		{
			if(UsePropertyCache) 
			{
				CommitEntry();
			}
		}

		private void CommitEntry()
		{
			PropertyCollection properties = GetProperties(false);
			if(!Nflag)
			{
				System.Collections.ArrayList modList = new System.Collections.ArrayList();
				foreach (string attribute in properties.PropertyNames)
				{
					LdapAttribute attr=null;
					if (properties [attribute].Mbit)
					{
						switch (properties [attribute].Count) {
							case 0:
								attr = new LdapAttribute (attribute, new string [0]);
								modList.Add (new LdapModification (LdapModification.DELETE, attr));
								break;
							case 1:
								string val = (string) properties [attribute].Value;
								attr = new LdapAttribute (attribute, val);
								modList.Add (new LdapModification (LdapModification.REPLACE, attr));
								break;
							default:
								object [] vals = (object [])properties [attribute].Value;
								string [] aStrVals = new string [properties [attribute].Count];
								Array.Copy (vals, 0, aStrVals, 0, properties [attribute].Count);
								attr = new LdapAttribute (attribute, aStrVals);
								modList.Add (new LdapModification (LdapModification.REPLACE, attr));
								break;
						}
						properties [attribute].Mbit=false;
					}
				}
				if (modList.Count > 0) {
					LdapModification[] mods = new LdapModification[modList.Count]; 	
					Type mtype = typeof (LdapModification);
					mods = (LdapModification[])modList.ToArray(mtype);
					ModEntry(mods);
				}
			}
			else
			{
				LdapAttributeSet attributeSet = new LdapAttributeSet();
				foreach (string attribute in properties.PropertyNames)
				{
					if (properties [attribute].Count == 1)
					{
						string val = (string) properties [attribute].Value;
						attributeSet.Add(new LdapAttribute(attribute, val));                
					}
					else
					{
						object[] vals = (object []) properties [attribute].Value;
						string[] aStrVals = new string [properties [attribute].Count];
						Array.Copy (vals,0,aStrVals,0,properties [attribute].Count);
						attributeSet.Add( new LdapAttribute( attribute , aStrVals));
					}
				}
				LdapEntry newEntry = new LdapEntry( Fdn, attributeSet );
				conn.Add( newEntry );
				Nflag = false;
			}
		}

		internal void CommitDeferred()
		{
			if (!_inPropertiesLoading && !UsePropertyCache && !Nflag) 
			{
				CommitEntry();
			}
		}

		void RefreshEntry()
		{
			_Properties = null;
			_Fdn = null;
			_Name = null;
			_Parent = null;
			_SchemaClassName = null;
			InitEntry();
		}

		/// <summary>
		/// Loads the values of the specified properties into the property cache.
		/// </summary>
		public void RefreshCache ()
		{
			// note that GetProperties must be called with false, elswere infinite loop will be caused
			PropertyCollection properties = new PropertyCollection ();
			LoadProperties(properties, null);
			SetProperties (properties);
		}

		/// <summary>
		/// Loads the values of the specified properties into the property cache.
		/// </summary>
		/// <param name="propertyNames">An array of the specified properties. </param>
		public void RefreshCache (string[] propertyNames)
		{
			// note that GetProperties must be called with false, elswere infinite loop will be caused
			LoadProperties(GetProperties(false),propertyNames);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Close ();
			}
			base.Dispose (disposing);
		}

		internal static string GetLdapUrlString(string host, int port, string dn)
		{
			LdapUrl lUrl;
			if (port == LdapConnection.DEFAULT_PORT)
				lUrl = new LdapUrl (host,0,dn);
			else
				lUrl = new LdapUrl (host,port,dn);
			return lUrl.ToString();
		}
	}
}
