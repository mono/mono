//
// System.Runtime.Remoting.MetadataServices.MetaData
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//		Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
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

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Net;
#if !TARGET_JVM
using System.CodeDom.Compiler;
using Microsoft.CSharp;
#endif

namespace System.Runtime.Remoting.MetadataServices
{
	public class MetaData 
	{
		internal const string WsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
		internal const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";
		internal const string SchemaNamespace = "http://www.w3.org/2001/XMLSchema";
		internal const string SchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		internal const string SudsNamespace = "http://www.w3.org/2000/wsdl/suds";
		internal const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		internal const string SoapNamespace = "http://schemas.xmlsoap.org/wsdl/soap/";
		
		public MetaData() 
		{
		}

#if !TARGET_JVM
		[MonoTODO ("strong name")]
		public static void ConvertCodeSourceFileToAssemblyFile (
				   string codePath,
				   string assemblyPath,
				   string strongNameFilename)
		{
			CSharpCodeProvider prov = new CSharpCodeProvider ();
			ICodeCompiler comp = prov.CreateCompiler ();
			CompilerParameters pars = new CompilerParameters ();
			pars.OutputAssembly = assemblyPath;
			CompilerResults cr = comp.CompileAssemblyFromFile(pars, codePath);
			CheckResult (cr);
		}

		[MonoTODO ("strong name")]
		public static void ConvertCodeSourceStreamToAssemblyFile (
				   ArrayList outCodeStreamList,
				   string assemblyPath,
				   string strongNameFilename)
		{
			CSharpCodeProvider prov = new CSharpCodeProvider ();
			ICodeCompiler comp = prov.CreateCompiler ();
			CompilerParameters pars = new CompilerParameters ();
			pars.OutputAssembly = assemblyPath;
			CompilerResults cr  = comp.CompileAssemblyFromFileBatch (pars, (string[]) outCodeStreamList.ToArray(typeof(string)));
			CheckResult (cr);
		}
		
		static void CheckResult (CompilerResults cr)
		{
			if (cr.Errors.Count > 0)
			{
				foreach (string s in cr.Output)
					Console.WriteLine (s);
					
				string errs = "";
				foreach (CompilerError error in cr.Errors)
					if (error.FileName != "")
						errs += error.ToString () + "\n";
				throw new Exception ("There where errors during compilation of the assembly:\n" + errs);
			}
		}
		public static void ConvertSchemaStreamToCodeSourceStream (
				   bool clientProxy, 
				   string outputDirectory, 
				   Stream inputStream, 
				   ArrayList outCodeStreamList)
		{
			ConvertSchemaStreamToCodeSourceStream (clientProxy, outputDirectory, inputStream, outCodeStreamList, null, null);
		}

		public static void ConvertSchemaStreamToCodeSourceStream (
				   bool clientProxy, 
				   string outputDirectory, 
				   Stream inputStream, 
				   ArrayList outCodeStreamList, 
				   string proxyUrl)
		{
			ConvertSchemaStreamToCodeSourceStream (clientProxy, outputDirectory, inputStream, outCodeStreamList, proxyUrl, null);
		}

		public static void ConvertSchemaStreamToCodeSourceStream (
				   bool clientProxy, 
				   string outputDirectory, 
				   Stream inputStream, 
				   ArrayList outCodeStreamList, 
				   string proxyUrl, 
				   string proxyNamespace)
		{
			MetaDataCodeGenerator cg = new MetaDataCodeGenerator ();
			
			MemoryStream memStream = new MemoryStream ();
			CopyStream (inputStream, memStream);
			memStream.Position = 0;
			cg.GenerateCode (clientProxy, outputDirectory, memStream, outCodeStreamList, proxyUrl, proxyNamespace);
		}
#endif

		public static void ConvertTypesToSchemaToFile (ServiceType [] types, SdlType sdlType, string path)
		{
			FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
			ConvertTypesToSchemaToStream (types, sdlType, fs);
			fs.Close ();
		}

		public static void ConvertTypesToSchemaToFile (Type [] types, SdlType sdlType, string path)
		{
			FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
			ConvertTypesToSchemaToStream (types, sdlType, fs);
			fs.Close ();
		}

		public static void ConvertTypesToSchemaToStream (Type [] types, SdlType sdlType, Stream outputStream)
		{
			ServiceType[] st = new ServiceType [types.Length];
			for (int n=0; n<types.Length; n++)
				st [n] = new ServiceType (types[n]);

			ConvertTypesToSchemaToStream (st, sdlType, outputStream);
		}

		public static void ConvertTypesToSchemaToStream (ServiceType [] serviceTypes, SdlType sdlType, Stream outputStream)
		{
			MetaDataExporter exporter = new MetaDataExporter ();
			MemoryStream memStream = new MemoryStream ();
			
			StreamWriter sw = new StreamWriter (memStream);
			XmlTextWriter tw = new XmlTextWriter (sw);

			exporter.ExportTypes (serviceTypes, sdlType, tw);
			tw.Flush ();
			
			memStream.Position = 0;
			CopyStream (memStream, outputStream);
		}
		
		public static void RetrieveSchemaFromUrlToFile (string url, string path)
		{
			FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
			RetrieveSchemaFromUrlToStream (url, fs);
			fs.Close ();
		}

		public static void RetrieveSchemaFromUrlToStream (string url, Stream outputStream)
		{
			WebRequest req = WebRequest.Create (url);
			Stream st = req.GetResponse().GetResponseStream();
			CopyStream (st, outputStream);
			st.Close ();
		}

		public static void SaveStreamToFile (Stream inputStream, string path)
		{
			FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
			CopyStream (inputStream, fs);
			fs.Close ();
		}
		
		static void CopyStream (Stream inputStream, Stream outputStream)
		{
			byte[] buffer = new byte [1024*5];
			int nr = 0;
			
			while ((nr = inputStream.Read (buffer, 0, buffer.Length)) > 0)
				outputStream.Write (buffer, 0, nr);
		}
	}
}
