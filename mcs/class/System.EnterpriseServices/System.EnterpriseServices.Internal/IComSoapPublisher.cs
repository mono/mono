// System.EnterpriseServices.Internal.IComSoapPublisher.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro Sánchez Acosta
//
using System;

using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
	//[Guid("")]
	public interface IComSoapPublisher
	{
		void CreateMailBox (string RootMailServer, string MailBox, out string SmtpName, out string Domain,
         			       out string PhysicalPath, out string Error);

		void CreateVirtualRoot (string Operation, string FullUrl, out string BaseUrl, out string VirtualRoot,
					       out string PhysicalPath, out string Error);

		void DeleteMailBox (string RootMailServer, string MailBox, out string Error);

		void DeleteVirtualRoot (string RootWebServer, string FullUrl, out string Error);

		void GacInstall (string AssemblyPath);

		void GacRemove (string AssemblyPath);

		void GetAssemblyNameForCache (string TypeLibPath, out string CachePath);

		string GetTypeNameFromProgId (string AssemblyPath, string ProgId);

		void ProcessClientTlb (string ProgId, string SrcTlbPath, string PhysicalPath, string VRoot, string BaseUrl,
        			       string Mode, string Transport, out string AssemblyName, out string TypeName,
				       out string Error);

		void ProcessServerTlb (string ProgId, string SrcTlbPath, string PhysicalPath, string Operation, 
					out string AssemblyName, out string TypeName, out string Error);

		void RegisterAssembly (string AssemblyPath);

		void UnRegisterAssembly(string AssemblyPath);
	}
}
