//
// ReflectionTypeDisplayer.cs: Displays type information as a tree
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class ReflectionTypeDisplayer : IndentingTypeDisplayer {

		public ReflectionTypeDisplayer (TextWriter writer)
			: base (writer)
		{
		}

		private sealed class ResultInfo : IComparable {
			public string Result;
			// array of ResultInfo
			public ArrayList Subresults;

			public ResultInfo ()
			{
				Result = null;
				Subresults = null;
			}

			public ResultInfo (string result)
			{
				Result = result;
				Subresults = null;
			}

			public int CompareTo (object obj)
			{
				ResultInfo rhs = obj as ResultInfo;
				if (rhs == null)
					throw new ArgumentException ();
				return Result.CompareTo (rhs.Result);
			}
		}

		private void PrintResultInfo (ArrayList results)
		{
			foreach (ResultInfo ri in results) {
				WriteLine (ri.Result);
				if (ri.Subresults != null) {
					using (Indenter n1 = GetIndenter()) {
						PrintResultInfo (ri.Subresults);
					}
				}
			}
		}

		private sealed class MemberInfoNameComparer : IComparer {

			public int Compare (object a, object b)
			{
				if (a == null && b == null)
					return 0;

				MemberInfo x = a as MemberInfo;
				MemberInfo y = b as MemberInfo;
				if (a == null && b == null)
					throw new ArgumentException ();
				return x.Name.CompareTo (y.Name);
			}
		}

		private static readonly IComparer NameComparer = new MemberInfoNameComparer();

		private void PrintTypeProperties (Type type, 
				object instance, 
				ArrayList results, 
				ArrayList seen, 
				int depth)
		{
			if (depth-- < 0)
				return;
			ArrayList properties = new ArrayList ();
			properties.AddRange (type.GetProperties ());
			properties.Sort (NameComparer);
			object[] args = new object[]{};

			foreach (PropertyInfo p in properties) {
				ResultInfo ri = new ResultInfo ();
				object r = null;

				char t = '-';
				try {
					r = type.InvokeMember (p.Name,
						BindingFlags.Public | BindingFlags.Instance | 
							BindingFlags.GetProperty,
						null, 
						instance, 
						args);
					Type rType = r.GetType();
					if (rType.IsEnum)
						r = GetEnumDescription (r.GetType(), r);
					else if (rType.GetProperties().Length > 0) {
						t = '+';
						if (seen.IndexOf (r) == -1) {
              t = '\\';
							seen.Add (r);
							// We test IsSubclassOf to prevent infinite recursion.
							ri.Subresults = new ArrayList ();
							PrintTypeProperties (rType, r, ri.Subresults, seen, depth);
						}
					}
				} catch (Exception e) {
					r = String.Format ("<exception: {0}>", e.Message);
				} finally {
					ri.Result = String.Format ("{0} {1}={2}", t.ToString(), p.Name, r);
					results.Add (ri);
				}
			}
      results.Add (new ResultInfo (String.Format ("- ToString()={0}",
				instance.ToString())));
		}

		private void PrintTypeProperties (Type type, object instance)
		{
			if (VerboseOutput) {
				ArrayList results = new ArrayList ();
				PrintTypeProperties (type, instance, results, new ArrayList(), 
					MaxDepth - Writer.IndentLevel);
				PrintResultInfo (results);
			}
		}

		private void PrintTypeProperties (Type type, MemberInfo instance)
		{
			if (VerboseOutput) {
				PrintTypeProperties (type, (object) instance);
				object[] attrs = instance.GetCustomAttributes (true);
				if (attrs.Length > 0) {
					WriteLine ("\\ GetCustomAttributes(true):");
					using (Indenter n1 = GetIndenter ()) {
						foreach (object a in attrs) {
							WriteLine (string.Format ("\\ {0}", a));
							using (Indenter n2 = GetIndenter ())
								PrintTypeProperties (a.GetType(), a);
						}
					}
				}
			}
		}

		protected override void OnIndentedType (TypeEventArgs e)
		{
			WriteLine (GetTypeHeader (e.Type));

			if (ShowTypeProperties) {
				using (Indenter n1 = GetIndenter()) {
					WriteLine ("System.Type Properties:");
					using (Indenter n2 = GetIndenter())
						PrintType (e.Type);
				}
			}
		}

		protected void PrintType (Type i)
		{
			PrintTypeProperties (i.GetType(), i);
		}

		protected override void OnIndentedInterfaces (InterfacesEventArgs e)
		{ 
			WriteLine ("Interfaces:");
			using (Indenter n1 = GetIndenter()) {
				if (e.Interfaces.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (Type i in e.Interfaces) {
					WriteLine (i);
					if (VerboseOutput) {
						using (Indenter n2 = GetIndenter()) {
							PrintType (i);
						}
					}
				}
			}
		}

		protected void PrintFieldInfo (FieldInfo f)
		{
			if (VerboseOutput) {
				PrintTypeProperties (f.GetType(), f);
				if ((f.Attributes & FieldAttributes.HasDefault) != 0) {
					WriteLine ("- Default Value={0}", FieldValue (f));
				}
				else if ((f.Attributes & FieldAttributes.Static) != 0) {
 					WriteLine ("- Static Value={0}", FieldValue (f));
				}
			}
		}

		protected override void OnIndentedFields (FieldsEventArgs e)
		{
			WriteLine ("Fields:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Fields.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (FieldInfo f in e.Fields) {
					WriteLine (f);
					using (Indenter n3 = GetIndenter()) {
						PrintFieldInfo (f);
					}
				}
			}
		}

		protected void PrintPropertyInfo (PropertyInfo p)
		{
			if (VerboseOutput) {
				PrintTypeProperties (p.GetType(), p);
			}
		}

		protected override void OnIndentedProperties (PropertiesEventArgs e)
		{
			WriteLine ("Properties:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Properties.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (PropertyInfo p in e.Properties) {
					WriteLine (p);
					using (Indenter n3 = GetIndenter()) {
						PrintPropertyInfo (p);
					}
				}
			}
		}

		protected void PrintEventInfo (EventInfo i)
		{
			if (VerboseOutput) {
				PrintTypeProperties (i.GetType(), i);
			}
		}

		protected override void OnIndentedEvents (EventsEventArgs e)
		{
			WriteLine ("Events:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Events.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (EventInfo i in e.Events) {
					WriteLine (i);
					using (Indenter n3 = GetIndenter()) {
						PrintEventInfo (i);
					}
				}
			}
		}

		private void PrintMethodBase (MethodBase m)
		{
			if (VerboseOutput) {
				PrintTypeProperties (m.GetType(), m);
				WriteLine ("- GetMethodImplementationFlags()={0}",
					GetEnumDescription (typeof(MethodImplAttributes), 
						m.GetMethodImplementationFlags ()));
				ParameterInfo[] parms = m.GetParameters ();
				if (parms.Length == 0) {
					WriteLine ("- GetParameters(): (none)");
				} else {
					WriteLine ("\\ GetParameters():");
					using (Indenter n1 = GetIndenter ()) {
						foreach (ParameterInfo pi in parms) {
							PrintTypeProperties (pi.GetType(), pi);
						}
					}
				}
			}
		}

		protected void PrintConstructorInfo (ConstructorInfo c)
		{
			if (VerboseOutput) {
				PrintMethodBase (c);
			}
		}

		protected override void OnIndentedConstructors (ConstructorsEventArgs e)
		{
			WriteLine ("Constructors:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Constructors.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (ConstructorInfo c in e.Constructors) {
					WriteLine (c);
					using (Indenter n3 = GetIndenter()) {
						PrintConstructorInfo (c);
					}
				}
			}
		}

		protected void PrintMethodInfo (MethodInfo m)
		{
			if (VerboseOutput) {
				PrintMethodBase (m);
			}
		}

		protected override void OnIndentedMethods (MethodsEventArgs e)
		{
			WriteLine ("Methods:");
			using (Indenter n2 = GetIndenter()) {
				if (e.Methods.Length == 0) {
					WriteLine ("(none)");
					return;
				}
				foreach (MethodInfo m in e.Methods) {
					if ((m.Attributes & MethodAttributes.SpecialName) == 0) {
						WriteLine (m);
						using (Indenter n3 = GetIndenter()) {
							PrintMethodInfo (m);
						}
					}
				}
			}
		}
	}
}

