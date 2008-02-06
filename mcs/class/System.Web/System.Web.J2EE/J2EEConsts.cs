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

namespace System.Web.J2EE
{
	internal sealed class J2EEConsts
	{
		public const string SESSION_STATE = "GH_SESSION_STATE";
	
		public const string CLASS_LOADER = "GH_ContextClassLoader";
		public const string SERVLET_CONFIG = "GH_ServletConfig";
		public const string RESOURCE_LOADER = "GH_ResourceLoader";
	
		public const string APP_DOMAIN = "AppDomain";
	
		public const string Enable_Session_Persistency = "EnableSessionPersistency";
	    
		//Used to save assemblies.xml file per application.
		public const string ASSEMBLIES_FILE = "AssembliesXml";
	
		//Used to save FileList.xml file per application.
		public const string FILE_LIST_FILE = "FileListXml";
	
		public const string MAP_PATH_CACHE = "MapPathCache";
	
		//Used to save servlet request of current Servlet.
		public const string SERVLET_REQUEST = "GH_ServletRequest";
		//Used to save servlet response of current Servlet.
		public const string SERVLET_RESPONSE = "GH_ServletResponse";
		//Used to save current Servlet.
		public const string CURRENT_SERVLET = "GH_Servlet";
	
		public const string DESERIALIZER_CONST = "GH_DeserializeWorkAround";
	//Used to control file system access in web app context
		public const string FILESYSTEM_ACCESS = "WebFileSystemAccess";
		public const string ACCESS_FULL = "Full";
		public const string ACCESS_VIRTUAL = "Virtual";

		public const string ACTION_URL_PREFIX = "ActionURL:";
		public const string RENDER_URL_PREFIX = "RenderURL:";
	}
}
