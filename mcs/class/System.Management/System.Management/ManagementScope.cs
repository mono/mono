//
// System.Management.ManagementScope
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class ManagementScope : ICloneable
	{
		[MonoTODO]
		public ManagementScope ()
		{
		}

		[MonoTODO]
		public ManagementScope (ManagementPath path)
		{
		}

		[MonoTODO]
		public ManagementScope (string path)
		{
		}

		[MonoTODO]
		public ManagementScope (string path, ConnectionOptions options)
		{
		}

		[MonoTODO]
		public ManagementScope (ManagementPath path, ConnectionOptions options)
		{
		}

		[MonoTODO]
		public ManagementScope Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Connect ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		public bool IsConnected {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public ConnectionOptions Options {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public ManagementPath Path {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

