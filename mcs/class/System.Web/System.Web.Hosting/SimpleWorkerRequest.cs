// 
// System.Web.Hosting
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	(class signature from Bob Smith <bob@thestuff.net> (C) )
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
using System;
using System.IO;
using System.Text;

namespace System.Web.Hosting
{
	[MonoTODO("Implement security demands on the path usage functions (and review)")]
	public class SimpleWorkerRequest : HttpWorkerRequest
	{
		private string _Page;
		private string _Query;
		private string _PathInfo = String.Empty;
		private string _AppVirtualPath;
		private string _AppPhysicalPath;
		private string _AppInstallPath;
		private TextWriter _Output;
		private bool _HasInstallInfo;

		private SimpleWorkerRequest ()
		{
		}

		public SimpleWorkerRequest (string Page, string Query, TextWriter Output)
		{
			_Page = Page;

			_Query = Query;
			AppDomain current = AppDomain.CurrentDomain;
			object o = current.GetData (".appPath");
			if (o == null)
				throw new HttpException ("Cannot get .appPath");
			_AppPhysicalPath = o.ToString ();

			o = current.GetData (".hostingVirtualPath");
			if (o == null)
				throw new HttpException ("Cannot get .hostingVirtualPath");
			_AppVirtualPath = CheckAndAddVSlash (o.ToString ());

			o = current.GetData (".hostingInstallDir");
			if (o == null)
				throw new HttpException ("Cannot get .hostingInstallDir");
			_AppInstallPath = o.ToString ();
			_Output = Output;

			if (_AppPhysicalPath == null)
				throw new HttpException ("Invalid app domain");

			_HasInstallInfo = true;

			ExtractPagePathInfo();
		}

		public SimpleWorkerRequest (string AppVirtualPath,
					    string AppPhysicalPath,
					    string Page,
					    string Query,
					    TextWriter Output)
		{
			if (AppDomain.CurrentDomain.GetData (".appPath") == null)
				throw new HttpException ("Invalid app domain");

			_Page = Page;
			_Query = Query;
			_AppVirtualPath = AppVirtualPath;
			_AppPhysicalPath = CheckAndAddSlash (AppPhysicalPath);
			_Output = Output;
			_HasInstallInfo = false;

			ExtractPagePathInfo();
		}

		[MonoTODO("Implement security")]
		public override string MachineInstallDirectory
		{
			get {
				if (_HasInstallInfo)
					return _AppInstallPath;

				return null;
			}
		}

		[MonoTODO("Get config path from Web.Config class")]
		public override string MachineConfigPath
		{
			get {
				return "MachineConfigPath"; //FIXME
			}
		}

		public override void EndOfRequest ()
		{
		}

		public override void FlushResponse (bool finalFlush)
		{
		}

		public override string GetAppPath ()
		{
			return _AppVirtualPath;
		}

		public override string GetAppPathTranslated ()
		{
			return _AppPhysicalPath;
		}

		public override string GetFilePath ()
		{
                        return CreatePath (false);
		}

		public override string GetFilePathTranslated ()
		{
                        string page = _Page;

			if (Path.DirectorySeparatorChar != '/')
                        {
                                page = _Page.Replace ('/', Path.DirectorySeparatorChar);
                        }

			return (Path.Combine (_AppPhysicalPath, page));
		}

		public override string GetHttpVerbName ()
		{
			return "GET";
		}

		public override string GetHttpVersion ()
		{
			return "HTTP/1.0";
		}

		public override string GetLocalAddress ()
		{
			return "127.0.0.1";
		}

		public override int GetLocalPort ()
		{
			return 80;
		}

		public override string GetPathInfo ()
		{
			return _PathInfo;
		}

		public override string GetQueryString ()
		{
			return _Query;
		}

		public override string GetRawUrl ()
		{
                        string path = CreatePath (true);
                        if (null != _Query && _Query.Length > 0)
				return path + "?" + _Query;

			return path;
		}

		public override string GetRemoteAddress()
		{
			return "127.0.0.1";
		}

		public override int GetRemotePort()
		{
			return 0;
		}

		public override string GetServerVariable(string name)
		{
			return String.Empty;
		}

		public override string GetUriPath()
		{
			return CreatePath (true);
		}

		public override IntPtr GetUserToken()
		{
			return IntPtr.Zero;
		}

		public override string MapPath (string path)
		{
			string sPath = _AppPhysicalPath.Substring (0, _AppPhysicalPath.Length - 1);
			if (path != null && path.Length > 0 && path [0] != '/')
				return sPath;
 
			char sep = Path.DirectorySeparatorChar;
			if (path.StartsWith(_AppVirtualPath)) {
				if (sep == '/')
					return _AppPhysicalPath + path.Substring (_AppVirtualPath.Length);
				else
					return _AppPhysicalPath + path.Substring (_AppVirtualPath.Length).Replace ('/', sep);
			}

			return null;
		}

		public override void SendKnownResponseHeader (int index, string value)
		{
		}

		public override void SendResponseFromFile (IntPtr handle, long offset, long length)
		{
		}

		public override void SendResponseFromFile (string filename, long offset, long length)
		{
		}

		public override void SendResponseFromMemory (byte [] data, int length)
		{
			_Output.Write (Encoding.Default.GetChars (data, 0, length));
		}

		public override void SendStatus(int statusCode, string statusDescription)
		{
		}

		public override void SendUnknownResponseHeader(string name, string value)
		{
		}

		// Create's a path string
		private string CheckAndAddSlash(string sPath)
		{
			if (null == sPath)
				return null;

			if (!sPath.EndsWith ("" + Path.DirectorySeparatorChar))
				return sPath + Path.DirectorySeparatorChar;

			return sPath;
		}

		// Creates a path string
		private string CheckAndAddVSlash(string sPath)
		{
			if (null == sPath)
				return null;

			if (!sPath.EndsWith ("/"))
				return sPath + "/";

			return sPath;
		}

		// Create's a path string
                private string CreatePath (bool bIncludePathInfo)
                {
                        string sPath = Path.Combine (_AppVirtualPath, _Page);

                        if (bIncludePathInfo)
                        {
                                sPath += _PathInfo;
                        }

                        return sPath;
		}

                //  "The extra path information, as given by the client. In
                //  other words, scripts can be accessed by their virtual
                //  pathname, followed by extra information at the end of this
                //  path. The extra information is sent as PATH_INFO."
                private void ExtractPagePathInfo ()
                {
                        if (_Page == null || _Page == String.Empty)
                        {
                                return;
                        }

                        string FullPath = GetFilePathTranslated();

                        int PathInfoLength = 0;

                        string LastFile = String.Empty;

                        while (PathInfoLength < _Page.Length)
                        {
                                if (LastFile.Length > 0)
                                {
                                        // increase it by the length of the file plus 
                                        // a "/"
                                        //
                                        PathInfoLength += LastFile.Length + 1;
                                }

                                if (File.Exists (FullPath) == true)
                                {
                                        break;
                                }

                                if (Directory.Exists (FullPath) == true)
                                {
                                        PathInfoLength -= (LastFile.Length + 1);
                                        break;
                                }

                                LastFile = Path.GetFileName (FullPath);
                                FullPath = Path.GetDirectoryName (FullPath);
                        }

                        if (PathInfoLength > _Page.Length)
                        {
                                return;
                        }

                        _PathInfo = _Page.Substring (_Page.Length - PathInfoLength);
                        _Page = _Page.Substring (0, _Page.Length - PathInfoLength);
                }
	}
}

