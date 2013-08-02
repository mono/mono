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

#if !FULL_AOT_RUNTIME
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeCustomAttribute
	{
		public static CodeCustomAttribute Create (Type attributeType)
		{
			return Create (attributeType, Type.EmptyTypes, new object [0], new string [0], new object [0]);
		}

		public static CodeCustomAttribute Create (Type attributeType, Type [] ctorArgTypes, object [] ctorArgs, string [] namedArgNames, object [] namedArgValues)
		{
			MemberInfo [] members = new MemberInfo [namedArgNames.Length];
			for (int i = 0; i < namedArgNames.Length; i++) {
				members [i] = attributeType.GetField (namedArgNames [i]);
				if (members [i] == null)
					members [i] = attributeType.GetProperty (namedArgNames [i]);
				if (members [i] == null)
					throw new ArgumentException (String.Format ("Named argument {0} was not found in attribute type {1}", namedArgNames [i], attributeType));
			}

			CodeLiteral [] args = new CodeLiteral [ctorArgs.Length];
			for (int i = 0; i < args.Length; i++)
				args [i] = new CodeLiteral (ctorArgs [i]);

			CodeLiteral [] nargs = new CodeLiteral [namedArgValues.Length];
			for (int i = 0; i < nargs.Length; i++)
				nargs [i] = new CodeLiteral (namedArgValues [i]);

			return Create (attributeType, ctorArgTypes, args, members, nargs);
		}

		public static CodeCustomAttribute Create (Type attributeType, Type [] ctorArgTypes, CodeLiteral [] ctorArgs, MemberInfo [] members, CodeLiteral [] values)
		{
			ArrayList props = new ArrayList ();
			ArrayList pvalues  = new ArrayList ();
			ArrayList fields = new ArrayList ();
			ArrayList fvalues = new ArrayList ();
			for (int i = 0; i < members.Length; i++) {
				if (members [i] == null)
					throw new ArgumentException (String.Format ("MemberInfo at {0} was null for type {1}.", i, attributeType));
				if (members [i] is PropertyInfo) {
					props.Add ((PropertyInfo) members [i]);
					pvalues.Add (values [i].Value);
				} else {
					fields.Add ((FieldInfo) members [i]);
					fvalues.Add (values [i].Value);
				}
			}

			ConstructorInfo ci = attributeType.GetConstructor (ctorArgTypes);
			CustomAttributeBuilder cab = new CustomAttributeBuilder (
				ci, ctorArgs,
				(PropertyInfo []) props.ToArray (typeof (PropertyInfo)), pvalues.ToArray (),
				(FieldInfo []) fields.ToArray (typeof (FieldInfo)), fvalues.ToArray ());

			CodeCustomAttribute attr = new CodeCustomAttribute (
				cab, attributeType, ci, ctorArgs, members, values);

			return attr;
		}

		CustomAttributeBuilder customAttributeBuilder;
		Type type;
		ConstructorInfo constructor;
		CodeLiteral [] ctorArgs;
		MemberInfo [] members;
		CodeLiteral [] namedArgValues;

		public CodeCustomAttribute (
			CustomAttributeBuilder attributeBuilder,
			Type type,
			ConstructorInfo constructor,
			CodeLiteral [] ctorArgs,
			MemberInfo [] namedArgMembers,
			CodeLiteral [] namedArgValues)
		{
			this.type = type;
			this.constructor = constructor;
			this.customAttributeBuilder = attributeBuilder;
			this.ctorArgs = ctorArgs;
			this.members = namedArgMembers;
			this.namedArgValues = namedArgValues;
		}

		public CustomAttributeBuilder Builder {
			get { return customAttributeBuilder; }
		}

		public string PrintCode ()
		{
			StringWriter sw = new StringWriter ();
			CodeWriter cw = new CodeWriter (sw);
			PrintCode (cw);
			return sw.ToString ();
		}
		
		public void PrintCode (CodeWriter cw)
		{
			cw.Write ("[").Write (type.Name).Write ("(");
			if (ctorArgs.Length > 0) {
				for (int i = 0; i < ctorArgs.Length - 1; i++) {
					ctorArgs [i].PrintCode (cw);
					cw.Write (", ");
				}
				ctorArgs [ctorArgs.Length - 1].PrintCode (cw);
			}
			if (members.Length > 0) {
				if (ctorArgs.Length > 0)
					cw.Write (", ");
				for (int i = 0; i < members.Length; i++) {
					cw.Write (members [i].Name).Write (" = ");
					namedArgValues [i].PrintCode (cw);
					if (i < members.Length - 1)
						cw.Write (", ");
				}
			}
			cw.Write (" )]");
			cw.EndLine ();
		}
	}
}
#endif
