//
// System.Runtime.Remoting.MetadataServices.MetaData
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//		Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Net;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

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

		public static void ConvertTypesToSchemaToFile (ServiceType[] servicetypes, SdlType sdltype, string path)
		{
			FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
			ConvertTypesToSchemaToStream (servicetypes, sdltype, fs);
			fs.Close ();
		}

		public static void ConvertTypesToSchemaToFile (Type[] types, SdlType sdltype, string path)
		{
			FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
			ConvertTypesToSchemaToStream (types, sdltype, fs);
			fs.Close ();
		}

		public static void ConvertTypesToSchemaToStream (Type[] types, SdlType sdltype, Stream stream)
		{
			ServiceType[] st = new ServiceType [types.Length];
			for (int n=0; n<types.Length; n++)
				st [n] = new ServiceType (types[n]);
				
			ConvertTypesToSchemaToStream (st, sdltype, stream);
		}

		public static void ConvertTypesToSchemaToStream (ServiceType[] servicetypes, SdlType sdltype, Stream stream)
		{
			MetaDataExporter exporter = new MetaDataExporter ();
			MemoryStream memStream = new MemoryStream ();
			
			StreamWriter sw = new StreamWriter (memStream);
			XmlTextWriter tw = new XmlTextWriter (sw);
			
			exporter.ExportTypes (servicetypes, sdltype, tw);
			tw.Flush ();
			
			memStream.Position = 0;
			CopyStream (memStream, stream);
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
