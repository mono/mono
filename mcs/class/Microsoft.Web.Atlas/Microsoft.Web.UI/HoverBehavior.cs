//
// Microsoft.Web.UI.HoverBehavior
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;

namespace Microsoft.Web.UI
{
	public class HoverBehavior : Behavior
	{
		public HoverBehavior ()
		{
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			/* XXX should make this use Hover/Unhover, but Owner == null is causing exceptions */
			typeDescriptor.AddEvent (new ScriptEventDescriptor ("hover", true));
			typeDescriptor.AddEvent (new ScriptEventDescriptor ("unhover", true));

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("hoverElement", ScriptType.Object, false, "HoverElementID"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("unhoverDelay", ScriptType.Number, false, "UnhoverDelay"));
		}

		ScriptEvent hover;
		public ScriptEvent Hover {
			get {
				if (hover == null)
					hover = new ScriptEvent (Owner, "hover", true);

				return hover;
			}
		}

		string hoverElementID = "";
		public string HoverElementID {
			get {
				return hoverElementID;
			}
			set {
				hoverElementID = (value == null ? "" : value);
			}
		}


		ScriptEvent unhover;
		public ScriptEvent Unhover {
			get {
				if (unhover == null)
					unhover = new ScriptEvent (Owner, "unhover", true);

				return unhover;
			}
		}

		int unhoverDelay = 0;
		public int UnhoverDelay {
			get {
				return unhoverDelay;
			}
			set {
				unhoverDelay = value;
			}
		}

		public override string TagName {
			get {
				return "hoverBehavior";
			}
		}
	}
}

#endif
