//
// ExternalTypeHelpers.cs (for Moonlight profile)
//
// Authors:
//	Andreia Gaita  <avidigal@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace System.Runtime.Serialization.Json {

	struct ExternalTypeFuncs {
		public Func<object, string> Serialize;
		public Func<string, object> Deserialize;
	}

	internal sealed class ExternalTypeMap : TypeMap {

		static Dictionary<Type, ExternalTypeFuncs> types;
		Type type;

		static ExternalTypeMap ()
		{
			types = new Dictionary<Type, ExternalTypeFuncs>();
		}

		public ExternalTypeMap (Type type) : base(type, null, null)
		{
			this.type = type;
		}

		public static void AddExternalType (Type type, Func<object, string> ser, Func<string, object> deser)
		{
			if (!types.ContainsKey (type))
				types[type] = new ExternalTypeFuncs () { Serialize=ser, Deserialize=deser };
		}

		public static bool HasType (Type type)
		{
			Type t = type;
			while (t != typeof(object)) {
				if (types.ContainsKey (t)) {
					if (!types.ContainsKey (type))
						types[type] = types[t];
					return true;
				}
				t = t.BaseType;
			}

			return false;
		}

		public override void Serialize (JsonSerializationWriter jsw, object graph, string t)
		{
			string ret = types[type].Serialize (graph);
			if (ret.Length > 0) {
				if (ret[0] == '{') {
					t = "object";
					ret = ret.Substring (1, ret.Length - 2);
				} else if (ret[0] == '[') {
					t = "array";
					ret = ret.Substring (1, ret.Length - 2);
				}
			}
			jsw.Writer.WriteAttributeString ("type", t);
			jsw.Writer.WriteRaw (ret);
		}

		public override object Deserialize (JsonSerializationReader jsr)
		{
			return types[type].Deserialize (jsr.Reader.ReadInnerXml ());
		}
	}
}