//
// VsaItem.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
