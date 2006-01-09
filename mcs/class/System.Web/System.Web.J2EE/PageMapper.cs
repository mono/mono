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

namespace System.Web.J2EE
{
	/// <summary>
	/// Class that allows reading assemblies.xml file for getting information about different types.
	/// </summary>
	public class PageMapper
	{
		private static readonly string _fileListName = "/filelist.xml";
		private static readonly object LOCK_GETASSEMBLIESCACHEDDOCUMENT = new object();
		private static readonly object LOCK_GETFROMMAPPATHCACHE = new object();

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

		private static ICachedXmlDoc GetAssembliesCachedDocument()
		{
			lock(LOCK_GETASSEMBLIESCACHEDDOCUMENT)
			{
				ICachedXmlDoc doc = (ICachedXmlDoc) AppDomain.CurrentDomain.GetData(J2EEConsts.ASSEMBLIES_FILE);
				if (doc == null)
				{
					doc = CreateDocument();
					if (doc != null)
						AppDomain.CurrentDomain.SetData(J2EEConsts.ASSEMBLIES_FILE, doc);
				}

				return doc;
			}
		}

		private static ICachedXmlDoc CreateDocument()
		{
			return new CachedDocumentTypeStorage();
		}

		public static Type GetObjectType(string url)
		{
			return GetCachedType(url);
		}

		private static Type GetCachedType(string url)
		{
			ICachedXmlDoc doc = PageMapper.GetAssembliesCachedDocument();
			
			if (url.StartsWith(IAppDomainConfig.WAR_ROOT_SYMBOL))
				url = url.Substring(IAppDomainConfig.WAR_ROOT_SYMBOL.Length);
			
			Type t = doc.Get(url);

			if (t == null)
				throw new HttpException(404,"The requested resource (" + url + ") is not available.");

			return t;
		}

		#region ICachedXmlDoc interface
		interface ICachedXmlDoc
		{
			Type Get(string key);
			//bool ContainsKey(object key);
		}
		#endregion

		#region CachedDocumentTypeStorage class
		class CachedDocumentTypeStorage : ICachedXmlDoc
		{
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

			Type ICachedXmlDoc.Get(string o)
			{
				return GetTypeByURL(o);
			}

			internal IDictionaryEnumerator GetEnumerator()
			{
				return _table.GetEnumerator();				
			}	

			public Type GetTypeByURL(string url)
			{
				string lwUrl = url.ToLower();
				lock (_table)
				{
					object retVal = _table[lwUrl];
					if (retVal == null)
					{
						PageCompiler compiler = new PageCompiler(url);
						retVal = compiler.GetCachedType();
						_table[lwUrl] = retVal;
					}
				
					return (Type)retVal;
				}
			}
		}
		#endregion

	}

	public class PageCompiler
	{
		private static readonly string PAGE_XPATH = "preserve";
		private static readonly string ASSEM_ATTRIB_NAME = "assem";
		private static readonly string TYPE_ATTRIB_NAME = "type";
		private static string _parser = null;

		private Type _type = null;
		private string _xmlDescriptor = null;
		private string _url = null;
		private string _session = null;

		public PageCompiler(string url)
		{
			_url = url;
			_xmlDescriptor = GetDescFromUrl();
			_session = DateTime.Now.Ticks.ToString();
		}

		public Type GetCachedType()
		{
			if (_type != null)
				return _type;
			
			string typeName = null;
		
			//if the desciptor exists in the war - get the type
			string descPath = String.Join("/", new string[]{"assemblies", _xmlDescriptor});

			try
			{
#if DEBUG
				Console.WriteLine(descPath);
#endif
				Stream fs = (Stream)IOUtils.getStream("/" + descPath);
				if (fs != null)
				{
					typeName = GetTypeFromDescStream(fs);
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine(ex);
#endif
				//desc not in the war
				typeName = null;
			}

			if (typeName != null)
			{
				_type = Type.GetType(typeName);
				return _type;
			}
			
			string fileName = Path.GetFileName(_url);
			if (fileName.ToLower() != "global.asax"
				&& fileName.ToLower() != "defaultwsdlhelpgenerator.aspx")
			{
				string fullFileName = HttpContext.Current.Request.MapPath(_url);
				if ( File.Exists(fullFileName) ) {
					//type not found - run aspxparser
					string[] command = GetParserCmd();
					if (J2EEUtils.RunProc(command) != 0)
						throw GetCompilerError();
				}
				else {
					string message = "The requested resource (" + _url + ") is not available.";
					throw new HttpException(404, message);
				}
			}
			//if the desciptor exists in the real app dir - get the type
			try
			{
				StreamReader sr = new StreamReader(HttpContext.Current.Request.MapPath("/" + descPath));
				typeName = GetTypeFromDescStream(sr.BaseStream);
				sr.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw ex;
			}

			if (typeName != null)
			{
				_type = Type.GetType(typeName);
				return _type;
			}

			return null;
		}

		private string GetTypeFromDescStream(Stream fs)
		{
			if (fs != null)
			{
				XmlDocument descXml = new XmlDocument();
				descXml.Load(fs);
				string assem = descXml.SelectSingleNode(PAGE_XPATH).Attributes[ASSEM_ATTRIB_NAME].Value;
				string shortType = descXml.SelectSingleNode(PAGE_XPATH).Attributes[TYPE_ATTRIB_NAME].Value;
				string typeName = String.Format("{0}, {1}",shortType,assem);
				fs.Close();
				return typeName;
			}

			return null;
		}

		private string[] GetParserCmd()
		{
			string[] cmd = new string[5];
			cmd[0] = GetParser();
			cmd[1] = "/aspxFiles:" + _url.Trim('/').Replace('/','\\');
			cmd[2] = "/session:" + _session;
			cmd[3] = "/appDir:" + (string)AppDomain.CurrentDomain.GetData(IAppDomainConfig.APP_PHYS_DIR);
			cmd[4] = "/compilepages";
			return cmd;
		}

		private string GetParser()
		{
			if (_parser == null)
			{
				StreamReader sr =
					File.OpenText(HttpContext.Current.Request.MapPath("/AspxParser.params"));
				_parser = sr.ReadLine();
				sr.Close();
			}

			return _parser;
		}

		private string GetDescFromUrl()
		{
			string fileName = Path.GetFileName(_url);
			
			if (fileName.ToLower() == "global.asax")
				return "global.asax.xml";

			string id = GetIdFromUrl(_url);
			string[] descName = new string[3] {fileName, id, ".xml"} ;
			return string.Concat(descName).ToLower();
		}

		private string GetIdFromUrl(string path)
		{
			path = path.Trim('/');
			string fileName = Path.GetFileName(path);
			string id = string.Empty;
			if (path.Length > fileName.Length)
				id = "." + path.Substring(0,path.Length - fileName.Length).Replace('/','_');
			return id;	
		}

		private Exception GetCompilerError()
		{
			string _errFile = HttpContext.Current.Request.MapPath("/" + _session + ".vmwerr");
			
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
