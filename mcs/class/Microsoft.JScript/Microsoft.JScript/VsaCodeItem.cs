//
// VsaCodeItem.cs: Implements IVsaCodeItem
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
