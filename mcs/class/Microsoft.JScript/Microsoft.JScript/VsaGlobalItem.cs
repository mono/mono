//
// VsaGlobalItem.cs: Implements of IVsaGlobalItem.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using Microsoft.Vsa;
using Microsoft.JScript.Vsa;
using System;

namespace Microsoft.JScript {

	internal class VsaGlobalItem : VsaItem, IVsaGlobalItem {

		internal VsaGlobalItem (VsaEngine engine, string name, VsaItemFlag flag)
			: base (engine, name, VsaItemType.AppGlobal, flag)
		{
			this.dirty = true;
		}

		//
		// Still not implemented on .Net 1.1
		//
		public bool ExposeMembers {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string TypeString {
			get {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (engine.Running)
					throw new VsaException (VsaError.EngineRunning);
				else if (engine.Busy)
					throw new VsaException (VsaError.EngineBusy);

				return System.Enum.GetName (typeof (VsaItemType), type);
			}

			set {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (engine.Running)
					throw new VsaException (VsaError.EngineRunning);
				else if (engine.Busy)
					throw new VsaException (VsaError.EngineBusy);
				
				type = (VsaItemType) System.Enum.Parse (typeof (VsaItemType), value);
			}
		}
	}
}
