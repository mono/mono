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
// System.DirectoryServices.DirectorySearcher.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

using System.ComponentModel;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;
using System.Collections.Specialized;

namespace System.DirectoryServices
{
	
	/// <summary>
	///Performs queries against Ldap directory.
	/// </summary>
    public class DirectorySearcher : Component	
	{

		private DirectoryEntry _SearchRoot=null;
		private bool _CacheResults=true;
		private TimeSpan _ClientTimeout = new TimeSpan(-1);
		private string _Filter="(objectClass=*)";
		private int _PageSize=0;
		private StringCollection _PropertiesToLoad=new StringCollection();
		private bool _PropertyNamesOnly=false;
		private ReferralChasingOption _ReferralChasing=
						System.DirectoryServices.ReferralChasingOption.External;
		private SearchScope _SearchScope=
						System.DirectoryServices.SearchScope.Subtree;
		private TimeSpan _ServerPageTimeLimit=new TimeSpan(-1);
		private int _SizeLimit=1000;
		private LdapConnection _conn = null;
		private string _Host=null;
		private int _Port=389;
		private SearchResultCollection _SrchColl=null;
		private bool emptycoll=true;

		internal SearchResultCollection SrchColl 
		{
			get
			{
				if (emptycoll)
				{
					_SrchColl =  new SearchResultCollection();
					DoSearch();
					emptycoll=false;
				}
				return _SrchColl;
			}
		}

		private void InitBlock()
		{
			_conn = new LdapConnection();
			LdapUrl lUrl=new LdapUrl(SearchRoot.Path);
			_Host=lUrl.Host;
			_Port=lUrl.Port;
			_conn.Connect(_Host,_Port);
			_conn.Bind(SearchRoot.Username,SearchRoot.Password);

		}
		/// <summary>
		/// Gets or sets the node in the Ldap Directory hierarchy where the 
		/// search starts.
		/// </summary>
		/// <value>
		/// The DirectoryEntry in the Ldap Directory hierarchy where the 
		/// search starts. The default is a null reference 
		/// </value>
		public DirectoryEntry SearchRoot 
		{
			get
			{
				return _SearchRoot;
			}
			set
			{
				_SearchRoot = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the result is 
		/// cached on the client computer.
		/// </summary>
		/// <value>
		/// true if the result is cached on the client computer; otherwise, 
		/// false. The default is true
		/// </value>
		/// <remarks>
		/// If the search returns a large result set, it is better to set 
		/// this property to false.
		/// </remarks>
		public bool CacheResults 
		{
			get
			{
				return _CacheResults;
			}
			set
			{
				CacheResults = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum amount of time that the client waits for 
		/// the server to return results. If the server does not respond 
		/// within this time, the search is aborted and no results are 
		/// returned.
		/// </summary>
		/// <value>
		/// A TimeSpan that represents the maximum amount of time (in seconds) 
		/// for the client to wait for the server to return results. The 
		/// default is -1, which means to wait indefinitely.
		/// </value>
		/// <remarks>
		/// If the ServerTimeLimit is reached before the client times out, 
		/// the server returns its results and the client stops waiting. The 
		/// maximum server time limit is 120 seconds.
		/// </remarks>
		public TimeSpan ClientTimeout 
		{
			get
			{
				return _ClientTimeout;
			}
			set
			{
				_ClientTimeout = value;
			}
		}

		/// <summary>
		/// Gets or sets the Lightweight Directory Access Protocol (Ldap) 
		/// format filter string.
		/// </summary>
		/// <value>
		/// The search filter string in Ldap format, such as 
		/// "(objectClass=user)". The default is "(objectClass=*)", which 
		/// retrieves all objects.
		/// </value>
		/// <remarks>
		/// The filter uses the following guidelines: 
		/// 1. The string must be enclosed in parentheses. 
		///	2. Expressions can use the relational operators: <, <=, =, >=, 
		///	and >. An example is "(objectClass=user)". Another example is 
		///	"(lastName>=Davis)". 
		/// 3. Compound expressions are formed with the prefix operators & 
		/// and |. Anexampleis"(&(objectClass=user)(lastName= Davis))".
		/// Anotherexampleis"(&(objectClass=printer)(|(building=42)
		/// (building=43)))". 
		/// </remarks>
		public string Filter 
		{
			get
			{
				return _Filter;
			}
			set
			{
				_Filter = value;
			}
		}

		/// <summary>
		/// Gets or sets the page size in a paged search.
		/// </summary>
		/// <value>
		/// The maximum number of objects the server can return in a paged 
		/// search. The default is zero, which means do not do a paged search.
		/// </value>
		/// <remarks>
		/// After the server has found a PageSize object, it will stop 
		/// searching and return the results to the client. When the client 
		/// requests more data, the server will restart the search where it 
		/// left off.
		/// </remarks>
		public int PageSize 
		{
			get
			{
				return _PageSize;
			}
			set
			{
				_PageSize =  value;
			}
		}

		/// <summary>
		/// Gets the set of properties retrieved during the search.
		/// </summary>
		/// <value>
		/// The set of properties retrieved during the search. The default is 
		/// an empty StringCollection, which retrieves all properties.
		/// </value>
		/// <remarks>
		/// To retrieve specific properties, add them to this collection 
		/// before you begin the search. For example, searcher.
		/// PropertiesToLoad.Add("phone");. 
		/// </remarks>
		public StringCollection PropertiesToLoad 
		{
			get
			{
				return _PropertiesToLoad;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the search retrieves only the 
		/// names of attributes to which values have been assigned.
		/// </summary>
		/// <value>
		/// true if the search obtains only the names of attributes to which 
		/// values have been assigned; false if the search obtains the names 
		/// and values for all the requested attributes. The default is false.
		/// </value>
		public bool PropertyNamesOnly 
		{
			get
			{
				return _PropertyNamesOnly;
			}
			set
			{
				_PropertyNamesOnly = value;
			}
		}

		/// <summary>
		/// Gets or sets how referrals are chased.
		/// </summary>
		/// <value>
		/// One of the ReferralChasingOption values. The default is External.
		/// </value>
		/// <remarks>
		/// If the root search is not specified in the naming context of the 
		/// server or when the search results cross a naming context (for 
		/// example, when you have child domains and search in the parent 
		/// domain), the server sends a referral message to the client that 
		/// the client can choose to ignore or chase.
		/// </remarks>
		public ReferralChasingOption ReferralChasing 
		{
			get
			{
				return _ReferralChasing;
			}
			set
			{
				_ReferralChasing = value;
			}
		}

		/// <summary>
		/// Gets or sets the scope of the search that is observed by the 
		/// server.
		/// </summary>
		/// <value>
		/// One of the SearchScope values. The default is Subtree.
		/// </value>
		public SearchScope SearchScope 
		{
			get
			{
				return _SearchScope;
			}
			set
			{
				_SearchScope =  value;
			}
		}

		/// <summary>
		/// Gets or sets the time limit the server should observe to search an 
		/// individual page of results (as opposed to the time limit for the 
		/// entire search).
		/// </summary>
		/// <value>
		/// A TimeSpan that represents the amount of time the server should 
		/// observe to search a page of results. The default is -1, which 
		/// means to search indefinitely.
		/// </value>
		/// <remarks>
		/// When the time limit is reached, the server stops searching and 
		/// returns the result obtained up to that point, along with a cookie
		///  containing the information about where to resume searching.
		///  A negative value means to search indefinitely.
		///  		Note:   This property only applies to searches where PageSize 
		///  		is set to a value that is not the default of -1.
		/// </remarks>
		public TimeSpan ServerPageTimeLimit 
		{
			get
			{
				return _ServerPageTimeLimit;
			}
			set
			{
				_ServerPageTimeLimit = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of objects the server returns in 
		/// a search.
		/// </summary>
		/// <value>
		/// The maximum number of objects the server returns in a search. The
		/// default of zero means to use the server-determined default size 
		/// limit of 1000 entries.
		/// </value>
		/// <remarks>
		/// The server stops searching after the size limit is reached and 
		/// returns the results accumulated up to that point.
		/// 		Note:   If you set SizeLimit to a value that is larger 
		/// 		than the server-determined default of 1000 entries, the 
		/// 		server-determined default is used.
		/// </remarks>
		public int SizeLimit 
		{
			get
			{
				return _SizeLimit;
			}
			set
			{
				_SizeLimit =  value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// SearchRoot, Filter, PropertiesToLoad, and SearchScope set to the 
		/// default values.
		/// </summary>
		public DirectorySearcher()
		{
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// Filter, PropertiesToLoad, and SearchScope set to the default 
		/// values. SearchRoot is set to the specified value.
		/// </summary>
		/// <param name="searchRoot">
		/// The node in the Active Directory hierarchy where the search starts. 
		/// The SearchRoot property is initialized to this value. 
		/// </param>
		public DirectorySearcher(DirectoryEntry searchRoot)
		{
			_SearchRoot = searchRoot;
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// SearchRoot, PropertiesToLoad, and SearchScope set to the default 
		/// values. Filter is set to the specified value.
		/// </summary>
		/// <param name="filter">
		/// The search filter string in Lightweight Directory Access Protocol 
		/// (Ldap) format. The Filter property is initialized to this value. 
		/// </param>
		public DirectorySearcher(string filter)
		{
			_Filter = filter;
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// PropertiesToLoad and SearchScope set to the default values. 
		/// SearchRoot and Filter are set to the specified values.
		/// </summary>
		/// <param name="searchRoot">
		/// The node in the Active Directory hierarchy where the search starts. 
		/// The SearchRoot property is initialized to this value. 
		/// </param>
		/// <param name="filter">
		/// The search filter string in Lightweight Directory Access Protocol 
		/// (Ldap) format. The Filter property is initialized to this value. 
		/// </param>
		public DirectorySearcher(	DirectoryEntry searchRoot,
									string filter	)
		{
			_SearchRoot = searchRoot;
			_Filter = filter;
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// SearchRoot and SearchScope set to the default values. Filter and 
		/// PropertiesToLoad are set to the specified values.
		/// </summary>
		/// <param name="filter">
		/// The search filter string in Lightweight Directory Access Protocol 
		/// (Ldap) format. The Filter property is initialized to this value. 
		/// </param>
		/// <param name="propertiesToLoad">
		/// The set of properties to retrieve during the search. The 
		/// PropertiesToLoad property is initialized to this value. 
		/// </param>
		public DirectorySearcher(	string filter,
									string[] propertiesToLoad	)
		{
			_Filter = filter;
			PropertiesToLoad.AddRange(propertiesToLoad);
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// SearchScope set to its default value. SearchRoot, Filter, and 
		/// PropertiesToLoad are set to the specified values.
		/// </summary>
		/// <param name="searchRoot">
		/// The node in the Active Directory hierarchy where the search starts. 
		/// The SearchRoot property is initialized to this value
		/// </param>
		/// <param name="filter">
		/// The search filter string in Lightweight Directory Access Protocol 
		/// (Ldap) format. The Filter property is initialized to this value. 
		/// </param>
		/// <param name="propertiesToLoad">
		/// The set of properties retrieved during the search. The 
		/// PropertiesToLoad property is initialized to this value. 
		/// </param>
		public DirectorySearcher(	DirectoryEntry searchRoot,
									string filter,
									string[] propertiesToLoad	)
		{
			_SearchRoot = searchRoot;
			_Filter = filter;
			PropertiesToLoad.AddRange(propertiesToLoad);
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with 
		/// SearchRoot set to its default value. Filter, PropertiesToLoad, 
		/// and SearchScope are set to the specified values
		/// </summary>
		/// <param name="filter">
		/// The search filter string in Lightweight Directory Access Protocol 
		/// (Ldap) format. The Filter property is initialized to this value.
		/// </param>
		/// <param name="propertiesToLoad">
		/// The set of properties to retrieve during the search. The 
		/// PropertiesToLoad property is initialized to this value. 
		/// </param>
		/// <param name="scope">
		/// The scope of the search that is observed by the server. The 
		/// SearchScope property is initialized to this value. 
		/// </param>
		public DirectorySearcher(	string filter,
									string[] propertiesToLoad,
									SearchScope scope )
		{
			_SearchScope = scope;
			_Filter = filter;
			PropertiesToLoad.AddRange(propertiesToLoad);
		}

		/// <summary>
		/// Initializes a new instance of the DirectorySearcher class with the
		/// SearchRoot, Filter, PropertiesToLoad, and SearchScope properties 
		/// set to the specified values
		/// </summary>
		/// <param name="searchRoot">
		/// The node in the Active Directory hierarchy where the search starts. 
		/// The SearchRoot property is initialized to this value. 
		/// </param>
		/// <param name="filter">
		/// The search filter string in Lightweight Directory Access Protocol 
		/// (Ldap) format. The Filter property is initialized to this value.
		/// </param>
		/// <param name="propertiesToLoad">
		/// The set of properties to retrieve during the search. The 
		/// PropertiesToLoad property is initialized to this value. 
		/// </param>
		/// <param name="scope">
		/// The scope of the search that is observed by the server. The 
		/// SearchScope property is initialized to this value. 
		/// </param>
		public DirectorySearcher(	DirectoryEntry searchRoot,
									string filter,
									string[] propertiesToLoad,
									SearchScope scope )
		{
			_SearchRoot = searchRoot;
			_SearchScope = scope;
			_Filter = filter;
			PropertiesToLoad.AddRange(propertiesToLoad);

		}

		/// <summary>
		/// Executes the Search and returns only the first entry found
		/// </summary>
		/// <returns> 
		/// A SearchResult that is the first entry found during the Search
		/// </returns>
		public SearchResult FindOne()
		{
			return SrchColl[0];
		}

		/// <summary>
		/// Executes the Search and returns a collection of the entries that are found
		/// </summary>
		/// <returns> 
		/// A SearchResultCollection collection of entries from the director
		/// </returns>
		public SearchResultCollection FindAll()
		{
			return SrchColl;
		}

		private void DoSearch()
		{
			InitBlock();
			String[] attrs= new String[PropertiesToLoad.Count];
			PropertiesToLoad.CopyTo(attrs,0);
			LdapSearchResults lsc=_conn.Search(	SearchRoot.Fdn,
												LdapConnection.SCOPE_SUB,
												Filter,
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
					Console.WriteLine("Error: " + e.LdapErrorMessage);
					// Exception is thrown, go for next entry
					throw e;
				}
				DirectoryEntry de = new DirectoryEntry(_conn);
				PropertyCollection pcoll = new PropertyCollection();
//				de.SetProperties();
				de.Path="LDAP://" + _Host+ ":" + _Port + "/" + nextEntry.DN;
				LdapAttributeSet attributeSet = nextEntry.getAttributeSet();
				System.Collections.IEnumerator ienum=attributeSet.GetEnumerator();
				if(ienum!=null)							
				{
					while(ienum.MoveNext())				
					{
						LdapAttribute attribute=(LdapAttribute)ienum.Current;
						string attributeName = attribute.Name;
						pcoll[attributeName].AddRange(attribute.StringValueArray);
//						de.Properties[attributeName].AddRange(attribute.StringValueArray);
//						de.Properties[attributeName].Mbit=false;
					}
				}
//				_SrchColl.Add(new SearchResult(de,PropertiesToLoad));
				_SrchColl.Add(new SearchResult(de,pcoll));
			}
		return;
		}

		protected override void Dispose(bool disposing)
		{
			throw new NotImplementedException();
		}

	}
}

