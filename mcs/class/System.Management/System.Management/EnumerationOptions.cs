//
// System.Management.EnumerationOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class EnumerationOptions : ManagementOptions
	{
		public EnumerationOptions ()
		{
		}

		public EnumerationOptions (ManagementNamedValueCollection context,
					   TimeSpan timeout,
					   int blockSize,
					   bool rewindable,
					   bool returnImmediatley,
					   bool useAmendedQualifiers,
					   bool ensureLocatable,
					   bool prototypeOnly,
					   bool directRead,
					   bool enumerateDeep)
		{
		}

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		public int BlockSize {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool DirectRead {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool EnsureLocatable {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool EnumerateDeep {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool PrototypeOnly {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool ReturnImmediately {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool Rewindable {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool UseAmendedQualifiers {
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

