//
// Microsoft.Web.Binding
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

namespace Microsoft.Web
{
	[ControlBuilder (typeof (BindingBuilder))]
	public class Binding : IScriptObject
	{
		public Binding ()
		{
		}

		bool automatic = true;
		public bool Automatic {
			get {
				return automatic;
			}
			set {
				automatic = value;
			}
		}

		string dataContext = "";
		public string DataContext {
			get {
				return dataContext;
			}
			set {
				dataContext = (value == null ? "" : value);
			}
		}

		string dataPath = "";
		public string DataPath {
			get {
				return dataPath;
			}
			set {
				dataPath = (value == null ? "" : value);
			}
		}

		BindingDirection direction = BindingDirection.In;
		public BindingDirection Direction {
			get {
				return direction;
			}
			set {
				direction = value;
			}
		}

		string id = "";
		public string ID {
			get {
				return id;
			}
			set {
				id = (value == null ? "" : value);
			}
		}

		string property = "";
		public string Property {
			get {
				return property;
			}
			set {
				property = (value == null ? "" : value);
			}
		}

		string propertyKey = "";
		public string PropertyKey {
			get {
				return propertyKey;
			}
			set {
				propertyKey = (value == null ? "" : value);
			}
		}

		TransformScriptEvent transform;
		public TransformScriptEvent Transform {
			get {
				if (transform == null)
					transform = new TransformScriptEvent (this);

				return transform;
			}
		}

		string transformerArgument = "";
		public string TransformerArgument {
			get {
				return transformerArgument;
			}
			set {
				transformerArgument = (value == null ? "" : value);
			}
		}

		public void RenderScript (ScriptTextWriter writer)
		{
			writer.WriteStartElement ("binding");

			if (Automatic == false)
				writer.WriteAttributeString ("automatic", Automatic.ToString());

			if (DataContext != "")
				writer.WriteAttributeString ("dataContext", DataContext);

			if (DataPath != "")
				writer.WriteAttributeString ("dataPath", DataPath);

			if (Direction != BindingDirection.In)
				writer.WriteAttributeString ("direction", Direction.ToString());

			if (ID != "")
				writer.WriteAttributeString ("id", ID);

			if (Property != "")
				writer.WriteAttributeString ("property", Property);

			if (PropertyKey != "")
				writer.WriteAttributeString ("propertyKey", PropertyKey);

			if (TransformerArgument != "")
				writer.WriteAttributeString ("transformerArgument", TransformerArgument);

			writer.WriteEndElement ();
		}

		public IScriptObject Owner {
			get {
				return null;
			}
		}

		ScriptTypeDescriptor IScriptObject.GetTypeDescriptor ()
		{
			ScriptTypeDescriptor d = new ScriptTypeDescriptor (this);

			d.AddEvent (new ScriptEventDescriptor ("transform", false));

			d.AddMethod (new ScriptMethodDescriptor ("evaluateIn", new string[0]));
			d.AddMethod (new ScriptMethodDescriptor ("evaluateOut", new string[0]));

			d.AddProperty (new ScriptPropertyDescriptor ("automatic", ScriptType.Boolean, false, "Automatic"));
			d.AddProperty (new ScriptPropertyDescriptor ("dataContext", ScriptType.Object, false, "DataContext"));
			d.AddProperty (new ScriptPropertyDescriptor ("dataPath", ScriptType.String, false, "DataPath"));
			d.AddProperty (new ScriptPropertyDescriptor ("direction", ScriptType.Enum, false, "Direction"));
			d.AddProperty (new ScriptPropertyDescriptor ("id", ScriptType.String, false, "ID"));
			d.AddProperty (new ScriptPropertyDescriptor ("property", ScriptType.String, false, "Property"));
			d.AddProperty (new ScriptPropertyDescriptor ("propertyKey", ScriptType.String, false, "PropertyKey"));
			d.AddProperty (new ScriptPropertyDescriptor ("transformerArgument", ScriptType.String, false, "TransformerArgument"));

			d.Close ();

			return d;
		}
	}

	class BindingBuilder : ControlBuilder
	{
		public override bool HasBody ()
		{
			return false;
		}
	}
}

#endif
