//
// VsaReferenceItem.cs: Implements IVsaReference item
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using Microsoft.JScript.Vsa;
using Microsoft.Vsa;
using System;

namespace Microsoft.JScript {

	internal class VsaReferenceItem : VsaItem, IVsaReferenceItem {

		private string assemblyName;

		internal VsaReferenceItem (VsaEngine engine, string name, VsaItemFlag flag)
		{
			this.engine = engine;
			this.name = name;
			this.itemType = VsaItemType.Reference;	
			this.itemFlag = flag;
			this.dirty = true;
		}

		public string AssemblyName {
			get {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (engine.Running)
					throw new VsaException (VsaError.EngineRunning);
				else if (engine.Busy)
					throw new VsaException (VsaError.EngineBusy);

				throw new NotImplementedException ();
			}

			set { throw new NotImplementedException (); }
		}		
	}
}
