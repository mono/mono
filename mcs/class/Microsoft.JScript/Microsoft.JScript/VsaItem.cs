//
// VsaItem.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using Microsoft.Vsa;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	public abstract class VsaItem : IVsaItem {

		protected bool dirty;
		protected VsaItemType type;
		protected VsaItemFlag flag;
		protected string name;
		protected VsaEngine engine;

		internal VsaItem (VsaEngine engine, string name, VsaItemType type, VsaItemFlag flag)
		{
			this.engine = engine;
			this.name = name;
			this.type = type;
			this.flag = flag;						
		}

		public virtual bool IsDirty {
			get {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else return dirty;
			}

			set {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);

				dirty = value;
			}
		}

		public virtual string Name {
			get {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (engine.Running)
					throw new VsaException (VsaError.EngineRunning);
				
				return name;
			}
			
			set {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (engine.Running)
					throw new VsaException (VsaError.EngineRunning);
				
				name = value;
			}
		} 		

		public VsaItemType ItemType {
			get {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else return type;
			}
		}

		public virtual Object GetOption (string name)
		{
			if (engine.Closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (engine.Busy)
				throw new VsaException (VsaError.EngineBusy);

			object opt = engine.GetOption (name);

			return opt;
		}

		public virtual void SetOption (string name, object value)
		{
			if (engine.Closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (engine.Busy)
				throw new VsaException (VsaError.EngineBusy);
			else if (engine.Running)
				throw new VsaException (VsaError.EngineRunning);
			
			engine.SetOption (name, value);
		}
	}
}
