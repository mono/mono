//
// Microsoft.Web.UI.DragDropList
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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.Web.UI
{
	public class DragDropList : Behavior
	{
		public DragDropList ()
		{
		}

		protected override void AddAttributesToElement (ScriptTextWriter writer)
		{
			base.AddAttributesToElement (writer);

			// MS raises a NRE when this is called from
			// our tests.  speculation: they're accessing
			// Browser or Page to figure out if they
			// should be rendering attributes.
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("acceptedDataTypes", ScriptType.Array, false, "AcceptedDataTypes"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("dataType", ScriptType.String, false, "DataType"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("dragMode", ScriptType.Enum, false, "DragMode"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("direction", ScriptType.Enum, false, "Direction"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("dropCueTemplate", ScriptType.Object, false));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("emptyTemplate", ScriptType.Object, false));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("floatContainerTemplate", ScriptType.Object, false));
		}

		string acceptedDataTypes = null;
		public string AcceptedDataTypes {
			get {
				return acceptedDataTypes;
			}
			set {
				acceptedDataTypes = value;
			}
		}

		string dataType = null;
		public string DataType {
			get {
				return dataType;
			}
			set {
				dataType = value;
			}
		}

		RepeatDirection direction = RepeatDirection.Vertical;
		public RepeatDirection Direction {
			get {
				return direction;
			}
			set {
				direction = value;
			}
		}

		DragMode dragMode = DragMode.Copy;
		public Microsoft.Web.UI.DragMode DragMode {
			get {
				return dragMode;
			}
			set {
				dragMode = value;
			}
		}

		string floatContainerCssClass = null;
		public string FloatContainerCssClass {
			get {
				return floatContainerCssClass;
			}
			set {
				floatContainerCssClass = value;
			}
		}

		HtmlTextWriterTag floatContainerTag = HtmlTextWriterTag.Div;
		public HtmlTextWriterTag FloatContainerTag {
			get {
				return floatContainerTag;
			}
			set {
				floatContainerTag = value;
			}
		}

		public override string TagName {
			get {
				return "dragDropList";
			}
		}
	}
}

#endif
