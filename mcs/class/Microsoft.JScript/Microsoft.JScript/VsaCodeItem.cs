//
// VsaCodeItem.cs: Implements IVsaCodeItem
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using Microsoft.Vsa;
using Microsoft.JScript.Vsa;
using System.CodeDom;
using System;

namespace Microsoft.JScript {

	internal class VsaCodeItem : VsaItem, IVsaCodeItem {

		private string sourceText;

		internal VsaCodeItem (VsaEngine engine, string name, VsaItemFlag flag)
			: base (engine, name, VsaItemType.Code, flag)
		{
			this.dirty = true;
		}
		
		public CodeObject CodeDOM {
			get { throw new NotImplementedException (); }
		}

		public string SourceText {
			get {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);

				return sourceText;
			}

			set {
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (engine.Running)
					throw new VsaException (VsaError.EngineRunning);

				sourceText = value;
			}
		}

		public void AddEventSource (string eventSourceName, string eventSourceType)
		{
			throw new NotImplementedException ();
		}

		public void AppendSourceText (string text)
		{
			if (engine.Closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (engine.Busy)
				throw new VsaException (VsaError.EngineBusy);
			else if (engine.Running)
				throw new VsaException (VsaError.EngineRunning);

			sourceText += text;
		}

		public void RemoveEventSource (string eventSourceName)
		{
			throw new NotImplementedException ();
		}
	}
}
