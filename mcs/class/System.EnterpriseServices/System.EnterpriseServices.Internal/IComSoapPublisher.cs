// System.EnterpriseServices.Internal.IComSoapPublisher.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// Copyright (C) 2002 Alejandro Sánchez Acosta
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("d8013eee-730b-45e2-ba24-874b7242c425")]
	public interface IComSoapPublisher
	{
		[DispId(6)]
		void CreateMailBox (string RootMailServer, string MailBox, out string SmtpName, out string Domain,
         			       out string PhysicalPath, out string Error);
		[DispId(4)]
		void CreateVirtualRoot (string Operation, string FullUrl, out string BaseUrl, out string VirtualRoot,
					       out string PhysicalPath, out string Error);
		[DispId(7)]
		void DeleteMailBox (string RootMailServer, string MailBox, out string Error);
		[DispId(5)]
		void DeleteVirtualRoot (string RootWebServer, string FullUrl, out string Error);
		[DispId(13)]
		void GacInstall (string AssemblyPath);
		[DispId(14)]
		void GacRemove (string AssemblyPath);
		[DispId(15)]
		void GetAssemblyNameForCache (string TypeLibPath, out string CachePath);
		[DispId(10)]
		string GetTypeNameFromProgId (string AssemblyPath, string ProgId);
		[DispId(9)]
		void ProcessClientTlb (string ProgId, string SrcTlbPath, string PhysicalPath, string VRoot, string BaseUrl,
        			       string Mode, string Transport, out string AssemblyName, out string TypeName,
				       out string Error);
		[DispId(8)]
		void ProcessServerTlb (string ProgId, string SrcTlbPath, string PhysicalPath, string Operation, 
					out string AssemblyName, out string TypeName, out string Error);
		[DispId(11)]
		void RegisterAssembly (string AssemblyPath);
		[DispId(12)]
		void UnRegisterAssembly (string AssemblyPath);
	}
#endif
}
