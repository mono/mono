// System.EnterpriseServices.Internal.Publish.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("d8013eef-730b-45e2-ba24-874b7242c425")]
	public class Publish : IComSoapPublisher {

		[MonoTODO]
		public Publish ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CreateMailBox (string RootMailServer, string MailBox, out string SmtpName, out string Domain, out string PhysicalPath, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CreateVirtualRoot (string Operation, string FullUrl, out string BaseUrl, out string VirtualRoot, out string PhysicalPath, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DeleteMailBox (string RootMailServer, string MailBox, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DeleteVirtualRoot (string RootWebServer, string FullUrl, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GacInstall (string AssemblyPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GacRemove (string AssemblyPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GetAssemblyNameForCache (string TypeLibPath, out string CachePath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetClientPhysicalPath (bool CreateDir)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetTypeNameFromProgId (string AssemblyPath, string ProgId)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ParseUrl (string FullUrl, out string BaseUrl, out string VirtualRoot)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ProcessClientTlb (string ProgId, string SrcTlbPath, string PhysicalPath, string VRoot, string BaseUrl, string Mode, string Transport, out string AssemblyName, out string TypeName, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ProcessServerTlb (string ProgId, string SrcTlbPath, string PhysicalPath, string Operation, out string strAssemblyName, out string TypeName, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RegisterAssembly (string AssemblyPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void UnRegisterAssembly (string AssemblyPath)
		{
			throw new NotImplementedException ();
		}
	}
#endif
}
