//
// System.Data.OleDb.OleDbConnection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Data.Common;
using System.Data.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using Mainsoft.Data.Configuration;

using System.Globalization;

using java.sql;
using javax.sql;
using javax.naming;

namespace Mainsoft.Data.Jdbc.Providers
{
	public class GenericProvider : IConnectionProvider
	{
		#region JdbcUrlConnector

		sealed class JdbcUrlConnector {
			#region Consts

			private static readonly Regex JdbcUrlPatternRegex= new Regex (@"\$\{(?<VALUE>[^$\{\}]*)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

			#endregion // Consts

			#region Fields

			readonly IConnectionStringDictionary _keyMapper;
			readonly GenericProvider			_provider;
			readonly ArrayList _excludedKeys = new ArrayList();

			#endregion // Fields

			#region Constructors

			internal JdbcUrlConnector (GenericProvider provider, IConnectionStringDictionary keyMapper) {
				_provider = provider;
				_keyMapper = keyMapper;
			}

			#endregion // Constructors


			#region Methods 

			internal java.sql.Connection Connection {
				get {
					java.util.Properties properties = new java.util.Properties ();

					string url = (string)_keyMapper["JdbcURL"];
					if (url == null) {
						string jdbcUrlPattern = (string)_provider.ProviderInfo [ConfigurationConsts.JdbcUrlPattern];
						if (jdbcUrlPattern == null || jdbcUrlPattern.Length == 0) {
							//throw ExceptionHelper.JdbcUrlPatternNotFound ((string)_provider.ProviderInfo [ConfigurationConsts.Name]);
							object [] args = new object [] {_provider.ProviderInfo [ConfigurationConsts.Name]};
							throw new ArgumentException (String.Format("Provider '{0}' is not configured with valid JDBC URL pattern.",args));
						}
						MatchEvaluator evaluator = new MatchEvaluator (ReplaceEvaluator);
						url = JdbcUrlPatternRegex.Replace (jdbcUrlPattern, evaluator);
					}
					else {
						// hack for backward comatibility:
						// if the connection string contains 'Provider',
						// the following mapping will hold:
						// 'User', 'User Id' --> 'user'
						// 'Password' --> 'password'
						if (_keyMapper["Provider"] != null) {

							const string USER = "User";
							const string USERID = "User Id";
							const string PASSWORD = "Password";

							string user = (string)_keyMapper[USER];
							if (user != null) {
								properties.put("user", user);
								_excludedKeys.Add(USER);
							}
							else {
								user = (string)_keyMapper[USERID];
								if (user != null) {
									properties.put("user", user);
									_excludedKeys.Add(USERID);
								}
							}

							string password = (string)_keyMapper[PASSWORD];
							if (password != null) {
								properties.put("password", password);
								_excludedKeys.Add(PASSWORD);
							}
						}
					}

					if (_provider._excludedKeys != null)
						_excludedKeys.AddRange(_provider._excludedKeys);

					foreach(string key in _provider.KeyMapping.Keys) {
						object value = _keyMapper [key];
						if (value == null)
							continue;
						bool contains = false;
						for (int i = 0; i < _excludedKeys.Count; i++) {
							if (String.Compare((string)_excludedKeys[i], key,
								true, CultureInfo.InvariantCulture) == 0) {
								contains = true;
								break;
							}
						}
						if (!contains) {
							properties.put (key, value);
							_excludedKeys.Add(key);
						}
					}

					for (int i = 0; i < _excludedKeys.Count; i++) {
						string value = _keyMapper.GetConnectionStringKey((string)_excludedKeys[i]);
						if (value != null)
							_excludedKeys[i] = value;
					}

					foreach(string key in _keyMapper.Keys) {
						object value = _keyMapper [key];
						if (value == null)
							continue;
						bool contains = false;
						for (int i = 0; i < _excludedKeys.Count; i++) {
							if (String.Compare((string)_excludedKeys[i], key,
								true, CultureInfo.InvariantCulture) == 0) {
								contains = true;
								break;
							}
						}
						if (!contains) {
							if (_provider._unsupportedKeys != null)
								for (int i = 0; i < _provider._unsupportedKeys.Length; i++)
									if (String.Compare ((string) _provider._unsupportedKeys [i], key,
										true, CultureInfo.InvariantCulture) == 0)
										throw new NotSupportedException (
											String.Format ("The parameter '{0}' is not supported.", key));

							properties.put (key, value);
						}
					}

					Driver d = ActivateJdbcDriver ();
					// TBD : add DriverManager.setLoginTimeout	
					if (d != null)
						return d.connect (url, properties);

					return DriverManager.getConnection (url, properties);
				}
			}

			private string ReplaceEvaluator (Match m) {
				Group g = m.Groups["VALUE"];

				if (!g.Success)
					return String.Empty;

				string usedKey = g.Value.Trim();

				string value = (string)_keyMapper [usedKey];
				if (value == null)
					throw new ArgumentException(
						String.Format("Missing parameter {0}", g.Value),
						"ConnectionString");

				_excludedKeys.Add(usedKey);
				return value;
			}

			private Driver ActivateJdbcDriver () {
				string driver = (string) _keyMapper["JdbcDriverClassName"];
				if (driver == null)
					driver = (string) _provider.ProviderInfo [ConfigurationConsts.JdbcDriverClassName];

				if (driver != null && driver.Length != 0) {
					try {
						java.lang.ClassLoader contextLoader = (java.lang.ClassLoader) AppDomain.CurrentDomain.GetData ("GH_ContextClassLoader");
						if (contextLoader != null)
							return (Driver) contextLoader.loadClass (driver).newInstance ();
						return (Driver) java.lang.Class.forName (driver).newInstance ();
					}
					catch (java.lang.ClassNotFoundException e) {
						throw new TypeLoadException (e.Message, e);
					}
					catch (java.lang.InstantiationException e) {
						throw new MemberAccessException (e.Message, e);
					}
					catch (java.lang.IllegalAccessException e) {
						throw new MissingMethodException (e.Message, e);
					}
				}

				return null;
			}

			#endregion // Methods
		}

		#endregion // JdbcUrlBuilder

		#region DataSourceCache

		private sealed class DataSourceCache : AbstractDbMetaDataCache {
			internal DataSource GetDataSource(string dataSourceName,string namingProviderUrl,string namingFactoryInitial) {
				Hashtable cache = Cache;

				DataSource ds = cache[dataSourceName] as DataSource;

				if (ds != null) {
					return ds;
				}

				Context ctx = null;
				
				java.util.Properties properties = new java.util.Properties();

				if ((namingProviderUrl != null) && (namingProviderUrl.Length > 0)) {
					properties.put("java.naming.provider.url",namingProviderUrl);
				}
				
				if ((namingFactoryInitial != null) && (namingFactoryInitial.Length > 0)) {
					properties.put("java.naming.factory.initial",namingFactoryInitial);
				}

				ctx = new InitialContext(properties);
 
				try {
					ds = (DataSource)ctx.lookup(dataSourceName);
				}
				catch(javax.naming.NameNotFoundException e) {
					// possible that is a Tomcat bug,
					// so try to lookup for jndi datasource with "java:comp/env/" appended
					ds = (DataSource)ctx.lookup("java:comp/env/" + dataSourceName);
				}

				cache[dataSourceName] = ds;
				return ds;
			}
		}

		#endregion // DatasourceCache

		#region Fields

		private static DataSourceCache _dataSourceCache = new DataSourceCache();

		private readonly IDictionary _providerInfo;
		private NameValueCollection _keyMapping;
		private string[] _excludedKeys;
		private string[] _unsupportedKeys;

		#endregion // Fields

		#region Constructors

		public GenericProvider(IDictionary providerInfo)
		{
			_providerInfo = providerInfo;
			_keyMapping = null;
		}

		#endregion // Constructors

		#region Properties

		protected IDictionary ProviderInfo
		{
			get { return _providerInfo; }
		}

		private NameValueCollection KeyMapping
		{
			get
			{
				if (_keyMapping == null)
					InitKeyMapping ();

				return _keyMapping;
			}
		}

		#endregion // Properties

		#region Methods

		public virtual java.sql.Connection GetConnection (IConnectionStringDictionary conectionStringBuilder)
		{
			string dataSourceJndi = (string) conectionStringBuilder.GetValue ("jndi-datasource-name");

			if (dataSourceJndi != null && dataSourceJndi.Length > 0) {

				string namingProviderUrl = (string) conectionStringBuilder.GetValue ("naming-provider-url");
				string namingFactoryInitial = (string) conectionStringBuilder.GetValue ("naming-factory-initial");
				DataSource ds = _dataSourceCache.GetDataSource(dataSourceJndi,namingProviderUrl,namingFactoryInitial);
				return ds.getConnection();
			}

			JdbcUrlConnector connector = new JdbcUrlConnector (this, conectionStringBuilder);
			return connector.Connection;
		}
			
		public virtual IConnectionStringDictionary GetConnectionStringBuilder (string connectionString)
		{
			return new ConnectionStringDictionary(connectionString, KeyMapping);
		}

		private void InitKeyMapping ()
		{
			lock (this) {
				if (_keyMapping != null)
					return;

				_keyMapping = new NameValueCollection (StringComparer.OrdinalIgnoreCase);

				// create key mappings collection
				string keyMappingsStr = (string) _providerInfo [ConfigurationConsts.KeyMapping];
				if (keyMappingsStr != null) {
					string [] keyMappings = keyMappingsStr.Split (ConfigurationConsts.SemicolonArr);
					foreach (string keyMapping in keyMappings) {
						if (keyMapping.Length == 0)
							continue;
						int equalsIndex = keyMapping.IndexOf ('=');
						string key = keyMapping.Substring (0, equalsIndex).Trim ();
						string [] mappings = keyMapping.Substring (equalsIndex + 1).Trim ().Split (ConfigurationConsts.CommaArr);
						foreach (string mapping in mappings)
							_keyMapping.Add (key, mapping.Trim ());
					}
				}

				string keyMappingExcludesStr = (string) _providerInfo [ConfigurationConsts.KeyMappingExcludes];
				if (keyMappingExcludesStr != null) {
					_excludedKeys = keyMappingExcludesStr.Split (ConfigurationConsts.CommaArr);
					for (int i = 0; i < _excludedKeys.Length; i++)
						_excludedKeys [i] = _excludedKeys [i].Trim ();
				}

				string keyMappingUnsupportedStr = (string) _providerInfo [ConfigurationConsts.KeyMappingUnsupported];
				if (keyMappingUnsupportedStr != null) {
					_unsupportedKeys = keyMappingUnsupportedStr.Split (ConfigurationConsts.CommaArr);
					for (int i = 0; i < _unsupportedKeys.Length; i++)
						_unsupportedKeys [i] = _unsupportedKeys [i].Trim ();
				}
			}
		}

		#endregion // Methods
	}
}
