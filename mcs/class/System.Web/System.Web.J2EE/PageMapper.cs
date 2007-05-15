//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Xml;
using System.IO;
using System.Collections;
using System.Web.Compilation;
using System.Collections.Specialized;
using System.Threading;
using vmw.common;
using System.Reflection;

namespace System.Web.J2EE
{
	/// <summary>
	/// Class that allows reading assemblies.xml file for getting information about different types.
	/// </summary>
	public class PageMapper
	{
		//private static readonly string _fileListName = "/filelist.xml";
		private static readonly object LOCK_GETASSEMBLIESCACHEDDOCUMENT = new object();
		//private static readonly object LOCK_GETFROMMAPPATHCACHE = new object();


		static Assembly CurrentDomain_AssemblyResolve (object sender, ResolveEventArgs args)
		{
			Assembly resolvedAssembly = null;
			try
			{
				resolvedAssembly = GetCachedAssembly (HttpContext.Current, String.Concat (HttpContext.Current.Request.ApplicationPath, "/", args.Name));
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine (ex.ToString ());
#endif
				resolvedAssembly = null;
			}

			return resolvedAssembly;
		}

#if UNUSED

		public static string GetFromMapPathCache(string key)
		{
			Hashtable answer = null;
			lock(LOCK_GETFROMMAPPATHCACHE)
			{
				answer = (Hashtable) AppDomain.CurrentDomain.GetData(J2EEConsts.MAP_PATH_CACHE);
				if (answer == null)
				{
					answer = new Hashtable();
					CachedDocumentTypeStorage storage = (CachedDocumentTypeStorage)GetAssembliesCachedDocument();
					IDictionaryEnumerator e = storage.GetEnumerator();
					e.Reset();
					while (e.MoveNext())
					{					
						string currentFile = (string)((DictionaryEntry)e.Current).Key;
						answer[currentFile]= IAppDomainConfig.WAR_ROOT_SYMBOL + currentFile;
					}
					AppDomain.CurrentDomain.SetData(J2EEConsts.MAP_PATH_CACHE,answer);
				}
			}
			return (string)answer[key];
		}

		// UNUSED METHOD
		//The method was used by runtime to force file names casesensitivity
		// problem. The filelist.xml file should contain correct file names,
		// but currently it is unused
		public static void LoadFileList()
		{
			Hashtable hashTable = (Hashtable) AppDomain.CurrentDomain.GetData(J2EEConsts.FILE_LIST_FILE);
			if (hashTable == null)
			{
				XmlDocument doc;
				try
				{
					Stream fs = (Stream)IOUtils.getStream(_fileListName);
					if (fs == null)
					{
						AppDomain.CurrentDomain.SetData(J2EEConsts.FILE_LIST_FILE, new Hashtable());
						return;
					}

					doc = new XmlDocument();
					doc.Load(fs);
				}
				catch (Exception)
				{
//					Console.WriteLine("filelist.xml was not found!!!");
					AppDomain.CurrentDomain.SetData(J2EEConsts.FILE_LIST_FILE, new Hashtable());
					return;
				}
//				Console.WriteLine("filelist.xml was found!!!");
				if (doc != null && doc.DocumentElement.HasChildNodes)
				{
					hashTable = CollectionsUtil.CreateCaseInsensitiveHashtable();
					XmlNodeList nodeList = doc.DocumentElement.ChildNodes;
					for (int i = 0;i < nodeList.Count ; i++)
					{
						string fileName = nodeList.Item(i).InnerText;
						hashTable.Add(fileName,fileName);
					}
					AppDomain.CurrentDomain.SetData(J2EEConsts.FILE_LIST_FILE, hashTable);
				}
			}

		}
#endif
		private static ICachedXmlDoc GetAssembliesCachedDocument(HttpContext context)
		{
			ICachedXmlDoc doc = (ICachedXmlDoc) AppDomain.CurrentDomain.GetData (J2EEConsts.ASSEMBLIES_FILE);

			if (doc == null) {
				lock (LOCK_GETASSEMBLIESCACHEDDOCUMENT) {
					doc = (ICachedXmlDoc) AppDomain.CurrentDomain.GetData (J2EEConsts.ASSEMBLIES_FILE);
					if (doc == null) {
						doc = CreateDocument ();
						if (doc != null) {
							AppDomain.CurrentDomain.SetData (J2EEConsts.ASSEMBLIES_FILE, doc);

							AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler (CurrentDomain_AssemblyResolve);
							try {
								//Try to load the  global resources
								HttpContext.AppGlobalResourcesAssembly = GetCachedAssembly (context,  context.Request.ApplicationPath + "/app_globalresources");
							}
							catch (Exception ex) {
#if DEBUG
								Console.WriteLine (ex.ToString ());
#endif
							}
						}
					}
				}
			}

			return doc;
		}

		private static String NormalizeName(string url)
		{
#if NET_2_0
			url = System.Web.Util.UrlUtils.RemoveDoubleSlashes(url);
#endif 
			if (url.StartsWith(IAppDomainConfig.WAR_ROOT_SYMBOL))
				url = url.Substring(IAppDomainConfig.WAR_ROOT_SYMBOL.Length);
			return url;
		}
		private static ICachedXmlDoc CreateDocument()
		{
			return new CachedDocumentTypeStorage();
		}

		public static Type GetObjectType (HttpContext context, string url)
		{
			return GetCachedType(context, NormalizeName(url), true);
		}

		public static Type GetObjectType (HttpContext context, string url, bool throwException) {
			return GetCachedType (context, NormalizeName (url), throwException);
		}

		public static Assembly GetObjectAssembly (HttpContext context, string url)
		{
			return GetCachedAssembly (context, NormalizeName (url));
		}
		public static string GetAssemblyResource (HttpContext context, string url)
		{
			return GetCachedResource (context, NormalizeName (url));
		}
		private static string GetCachedResource (HttpContext context, string url)
		{
			ICachedXmlDoc doc = PageMapper.GetAssembliesCachedDocument(context);
			return doc.GetAssemblyResourceName (context, url);
		}
		private static Assembly GetCachedAssembly (HttpContext context, string url)
		{
			ICachedXmlDoc doc = PageMapper.GetAssembliesCachedDocument(context);
			return doc.GetAssembly (context, url);
		}
		private static Type GetCachedType (HttpContext context, string url) {
			return GetCachedType (context, url, true);
		}
		private static Type GetCachedType (HttpContext context, string url, bool throwException)
		{
			ICachedXmlDoc doc = PageMapper.GetAssembliesCachedDocument(context);						
			Type t = doc.GetType(context, url);
			if (t == null && throwException)
				throw new HttpException(404,"The requested resource (" + url + ") is not available.");

			return t;
		}

		#region ICachedXmlDoc interface
		interface ICachedXmlDoc
		{
			Type GetType (HttpContext context, string key);
			Assembly GetAssembly (HttpContext context, string key);
			string GetAssemblyResourceName (HttpContext context, string key);
		}
		#endregion

		#region CachedDocumentTypeStorage class
		class CachedDocumentTypeStorage : ICachedXmlDoc
		{
			private static readonly object _fuse = new object();
			public static readonly ICachedXmlDoc DEFAULT_DOC =
				new CachedDocumentTypeStorage(0);

			private static readonly int DEFAULT_PAGES_NUMBER = 25;

			private Hashtable _table;

			private CachedDocumentTypeStorage(int initTableSize)
			{
				_table = Hashtable.Synchronized(new Hashtable(initTableSize));
			}

			public CachedDocumentTypeStorage() :
				this(DEFAULT_PAGES_NUMBER)
			{}

			string ICachedXmlDoc.GetAssemblyResourceName (HttpContext context, string o)
			{
				return GetMetaByURL(context, o).Resource;
			}
			Type ICachedXmlDoc.GetType (HttpContext context, string o)
			{
				return GetMetaByURL(context, o).Type;
			}
			Assembly ICachedXmlDoc.GetAssembly (HttpContext context, string o)
			{
				return GetMetaByURL(context, o).Assembly;
			}

			internal IDictionaryEnumerator GetEnumerator()
			{
				return _table.GetEnumerator();				
			}	

			//rewamped for perfomance reasons
			//It looks like issue is not important in development mode,
			//but only will became important in production mode when dynamyc compilation will be enabled
			//Anyway, locking whole table and compiling under lock looks odd
			//spivak.December 07 2006
			public MetaProvider GetMetaByURL(HttpContext context, string url)
			{

#if !NO_GLOBAL_LOCK_ON_COMPILE
				string lwUrl = url.ToLowerInvariant();
				lock (_table)
				{
					object retVal = _table[lwUrl];
					if (retVal == null)
					{
						retVal = PageCompiler.GetCompiler(context, url);
						_table[lwUrl] = retVal;
					}
				
					return (MetaProvider)retVal;
				}

#else
				string lwUrl = url.ToLower();
				if (!_table.ContainsKey(lwUrl))
				{
					lock (_table.SyncRoot)
					{
						if (_table.ContainsKey(lwUrl))
							goto Fused;
						_table[lwUrl] = _fuse;
					}
					try
					{
						MetaProvider meta = PageCompiler.GetCompiler(url);
						lock (_table.SyncRoot)
						{
							return (MetaProvider)(_table[lwUrl] = meta);
						}
					}
					catch(Exception e)
					{
						_table.Remove(lwUrl);
					}
				}				
			Fused:
				
				while (_table[lwUrl] == _fuse) 
					Thread.Sleep(10);

				return !_table.ContainsKey(lwUrl)? PageCompiler.Error: (MetaProvider)_table[lwUrl];
#endif
			}
		}
		

		#endregion
	}

	public interface  MetaProvider
	{
		Type Type { get;}
		Assembly Assembly {get;}
		string Resource { get;}
	}
	public class PageCompiler : MetaProvider
	{
		private static readonly string PAGE_XPATH = "preserve";
		private static readonly string ASSEM_ATTRIB_NAME = "assem";
		private static readonly string TYPE_ATTRIB_NAME = "type";
		private static string _parser = null;

		private Type _type = null;
		private string _typeName = null;
		private Assembly _assembly = null;
		private string _origAssemblyName = null;
		private string _xmlDescriptor = null;
		private string _url = null;
		private string _session = null;
		readonly private HttpContext _context;

		PageCompiler(HttpContext context, string url)
		{
			_url = url;
			_context = context;
			_xmlDescriptor = GetDescFromUrl();
			_session = DateTime.Now.Ticks.ToString();
			LoadTypeAndAssem();
		}

		public static PageCompiler GetCompiler(HttpContext context, string url)
		{
			return new PageCompiler(context, url);
		}

		Type MetaProvider.Type
		{
			get{
				return _type;
			}
		}
		Assembly MetaProvider.Assembly
		{
			get{
				return _assembly;
			}
		}
		string MetaProvider.Resource
		{
			get
			{
				return _origAssemblyName != null ? _origAssemblyName + ".ghres" : "dll.ghres";
			}
		}
		private void LoadTypeAndAssem()
		{
			if (_assembly == null)
			{
				string typeName = GetCachedTypeName();
#if DEBUG
				Console.WriteLine ("Loading type:" + typeName);
#endif
				if (typeName != null)
				{
					if ((_type = Type.GetType(typeName)) != null)
						_assembly = _type.Assembly;
					else
						_assembly = Assembly.Load(_origAssemblyName);
				}
			}
		}
		private bool InternalCompile()
		{
			string fileName = VirtualPathUtility.GetFileName (_url);

			string fullFileName = (fileName.ToLower () == "global.asax") ? _url : _context.Request.MapPath (_url);
#if DEBUG
			Console.WriteLine("fullFileName=" + fullFileName);
#endif
			//type not found - run aspxparser
			if (false/*File.Exists(fullFileName) || Directory.Exists(fullFileName)*/) //dynamic compilation currently is not supported
			{
				string[] command = GetParserCmd(fileName.ToLower() == "global.asax");
				if (J2EEUtils.RunProc(command) != 0)
					throw GetCompilerError();

				return true;
			}
			else
			{
				return false;
				//string message = "The requested resource (" + _url + ") is not available.";
				//throw new HttpException(404, message);
			}
		}
		private string GetDescriptorPath()
		{
			return String.Join("/", new string[] { "assemblies", _xmlDescriptor });
		}
		private string GetTypeNameFromAppFolder()
		{
			try
			{
				using (StreamReader sr = new StreamReader(_context.Request.MapPath("~/" + GetDescriptorPath())))
				{
					return GetTypeFromDescStream(sr.BaseStream);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw ex;
			}
		}
		internal string GetTypeFromResources()
		{
			string typeName = null;

			//if the desciptor exists in the war - get the type
			string descPath = GetDescriptorPath();

			try
			{
#if DEBUG
				Console.WriteLine(descPath);
#endif
				using (Stream fs = (Stream)IOUtils.getStreamRecursive("/" + descPath))
				{
					if (fs != null)
					{
						return GetTypeFromDescStream(fs);
					}
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine(ex);
#endif
			}
			return null;
		}
		internal string GetCachedTypeName()
		{			
			string typeName = GetTypeFromResources();
			if (typeName == null)
			{
				//spawn dynamic compilation and lookup typename from created folder
				if (InternalCompile())
					typeName = GetTypeNameFromAppFolder();
			}
			return typeName;
		}
		private string GetTypeName()
		{
			return String.Format("{0}, {1}", _typeName, _origAssemblyName); 
		}
		private bool LoadMetaFromXmlStream(Stream fs)
		{
			if (fs != null)
			{
				try
				{
					XmlDocument descXml = new XmlDocument();
					descXml.Load(fs);
					_origAssemblyName = descXml.SelectSingleNode(PAGE_XPATH).Attributes[ASSEM_ATTRIB_NAME].Value;
					_typeName = descXml.SelectSingleNode(PAGE_XPATH).Attributes[TYPE_ATTRIB_NAME].Value;
					return true;
				}
				catch
				{
#if DEBUG
					Console.WriteLine("Failed to load typename from stream");
#endif
				}
			}
			return false;
		}

		private string GetTypeFromDescStream(Stream fs)
		{
			if (LoadMetaFromXmlStream(fs))
				return GetTypeName();
			return null;
		}

		private string[] GetParserCmd(bool globalAsax)
		{
            string[] cmd = null;			
            if (globalAsax)
            {
                cmd = new string[4];
                cmd[3] = "/buildglobalasax";
            }
            else
            {
                cmd = new string[5];
                cmd[3] = "/aspxFiles:" + _url;
                cmd[4] = "/compilepages";
            }
            cmd[0] = GetParser();
            cmd[1] = "/session:" + _session;
            cmd[2] = "/appDir:" + (string)AppDomain.CurrentDomain.GetData(IAppDomainConfig.APP_PHYS_DIR);
			return cmd;
		}

		private string GetParser()
		{
			if (_parser == null)
			{
				StreamReader sr =
					File.OpenText (_context.Request.MapPath ("~/AspxParser.params"));
				_parser = sr.ReadLine();
				sr.Close();
			}

			return _parser;
		}

		private string GetDescFromUrl()
		{
			string fileName = VirtualPathUtility.GetFileName (_url);
			
			if (fileName.ToLower() == "global.asax")
				return "global.asax.xml";

			string id = GetIdFromUrl(_url);
			string[] descName = new string[3] {fileName, id, ".xml"} ;
			return string.Concat(descName).ToLowerInvariant();
		}

		private string GetIdFromUrl(string path)
		{
			string fileName = VirtualPathUtility.GetFileName(path);
			string id = string.Empty;

			if (VirtualPathUtility.IsAbsolute (path))
				path = path.Substring (_context.Request.ApplicationPath.Length + 1);

			if (path.Length > fileName.Length)
				id = "." + path.Substring(0,path.Length - fileName.Length).Replace('/','_');
			return id;	
		}

		private Exception GetCompilerError()
		{
			string _errFile = _context.Request.MapPath ("~/" + _session + ".vmwerr");
			
			if (!File.Exists(_errFile))
				throw new FileNotFoundException("Internal Error",_errFile);

			StreamReader sr = new StreamReader(_errFile);
			string message = string.Empty, line = null, file = null, lineInFile = "0";

			while ((line = sr.ReadLine()) != null)
			{
				if (line.StartsWith("Message: "))
					message = line.Substring("Message: ".Length);
				else if (line.StartsWith("File: "))
					file = line.Substring("File: ".Length);
				else if (line.StartsWith("Line: "))
					lineInFile = line.Substring("Line: ".Length);
			}

			sr.Close();

			if (file != null)
			{
				Location loc = new Location(null);
				loc.Filename = file;
				loc.BeginLine = int.Parse(lineInFile);
				return new ParseException(loc,message);
			}

			if (message.IndexOf(typeof(FileNotFoundException).Name) != -1 &&
				message.IndexOf(_url.Trim('\\','/').Replace('/','\\')) != -1)
				message = "The requested resource (" + _url + ") is not available.";
			return new HttpException(404,(message !=  null ? message : string.Empty));
		}
	}
}
