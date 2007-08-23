// 
// System.Web.Services.Protocols.ValueCollectionParameterReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Xml;

namespace System.Web.Services.Protocols {
	public abstract class ValueCollectionParameterReader : MimeParameterReader {

		ParameterInfo[] parameters;

		#region Constructors

		protected ValueCollectionParameterReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			if (IsSupported (methodInfo)) return methodInfo.Parameters;
			else return null;
		}

		public override void Initialize (object o)
		{
			parameters = (ParameterInfo[]) o;
		}

		public static bool IsSupported (LogicalMethodInfo methodInfo)
		{
			foreach (ParameterInfo param in methodInfo.Parameters)
				if (!IsSupported (param)) return false;
			return true;
		}

		public static bool IsSupported (ParameterInfo paramInfo)
		{
			Type type = paramInfo.ParameterType;
			if (type.IsByRef || paramInfo.IsOut) return false;
			if (type.IsArray) return IsSupportedPrimitive (type.GetElementType());
			else return IsSupportedPrimitive (type);
		}
		
		internal static bool IsSupportedPrimitive (Type type)
		{
			return ( type.IsPrimitive || 
					 type.IsEnum ||
					 type == typeof(string) ||
					 type == typeof(DateTime) ||
					 type == typeof(Decimal)
					 );
		}

		protected object[] Read (NameValueCollection collection)
		{
			object[] res = new object [parameters.Length];
			for (int n=0; n<res.Length; n++)
			{
				ParameterInfo pi = parameters [n];

				if (pi.ParameterType.IsArray) {
					string[] values = collection.GetValues (pi.Name);
					if (values == null)
						throw new InvalidOperationException ("Missing parameter: " + pi.Name);
					Type elemType = pi.ParameterType.GetElementType ();
					Array a = Array.CreateInstance (elemType, values.Length);
					for (int i = 0; i < values.Length; i++) {
						try {
							a.SetValue (StringToObj (elemType, values [i]), i);							
						} catch (Exception ex) {
							string error = "Cannot convert '" + values [i] + "' to " + elemType + "\n";
							error += "Parameter name: " + pi.Name + " --> " + ex.Message;
							throw new InvalidOperationException (error);
						}
					}
					res [n] = a;
				} else {
					string val = collection [pi.Name];
					if (val == null)
						throw new InvalidOperationException ("Missing parameter: " + pi.Name);
					try
					{
						res [n] = StringToObj (pi.ParameterType, val);
					}
					catch (Exception ex)
					{
						string error = "Cannot convert '" + val + "' to " + pi.ParameterType + "\n";
						error += "Parameter name: " + pi.Name + " --> " + ex.Message;
						throw new InvalidOperationException (error);
					}
				}	
			}
			return res;
		}
		#endregion // Methods
	}
}
