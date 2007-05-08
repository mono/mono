//
// HttpSimpleMethodStubInfo.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
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

using System.Reflection;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols 
{
	enum FormatterKind
	{
		ParameterWriter,
		ParameterReader,
		ReturnReader,
		ReturnWriter
	}
	
	internal abstract class HttpSimpleMethodStubInfo : MethodStubInfo
	{
		public MimeFormatterInfo ParameterWriterType;
		public MimeFormatterInfo ParameterReaderType;
		public MimeFormatterInfo ReturnReaderType;
		public MimeFormatterInfo ReturnWriterType;
		
		public MimeFormatterInfo GetFormatterInfo (FormatterKind kind)
		{
			switch (kind)
			{
				case FormatterKind.ParameterWriter: return ParameterWriterType;
				case FormatterKind.ParameterReader: return ParameterReaderType;
				case FormatterKind.ReturnReader: return ReturnReaderType;
				case FormatterKind.ReturnWriter: return ReturnWriterType;
			}
			return null;
		}
		
		public HttpSimpleMethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source): base (parent, source)
		{
			object[] atts = source.CustomAttributeProvider.GetCustomAttributes (typeof(HttpMethodAttribute), true);
			if (atts.Length > 0)
			{
				HttpMethodAttribute at = (HttpMethodAttribute) atts[0];
				ParameterWriterType = new MimeFormatterInfo (at.ParameterFormatter);
				ReturnReaderType = new MimeFormatterInfo (at.ReturnFormatter);
			}
			
			if (ReturnReaderType == null) {
				if (source.IsVoid) ReturnReaderType = new MimeFormatterInfo (typeof(NopReturnReader));
				else ReturnReaderType = new MimeFormatterInfo (typeof(XmlReturnReader));
			}
		}
	}
	
	internal class MimeFormatterInfo
	{
		public MimeFormatterInfo (Type type) {
			Type = type;
		}
		
		public MimeFormatter Create () {
			return MimeFormatter.CreateInstance (Type, Initializer);
		}
		
		public Type Type;
		public object Initializer;
	}

	internal abstract class HttpSimpleTypeStubInfo : TypeStubInfo
	{
		public HttpSimpleTypeStubInfo (LogicalTypeInfo logicalTypeInfo): base (logicalTypeInfo)
		{
		}

		protected override void BuildTypeMethods ()
		{
			base.BuildTypeMethods ();
			
			BuildInitializers (FormatterKind.ParameterWriter);
			BuildInitializers (FormatterKind.ParameterReader);
			BuildInitializers (FormatterKind.ReturnReader);
			BuildInitializers (FormatterKind.ReturnWriter);
		}
		
		void BuildInitializers (FormatterKind formatter)
		{
			Hashtable types = new Hashtable ();

			foreach (HttpSimpleMethodStubInfo met in Methods)
				AddType (types, met.GetFormatterInfo (formatter).Type, met);
			
			foreach (DictionaryEntry ent in types)
			{
				Type t = (Type)ent.Key;
				ArrayList list = (ArrayList)ent.Value;
				LogicalMethodInfo[] mets = new LogicalMethodInfo [list.Count];
				for (int n=0; n<list.Count; n++) 
					mets[n] = ((MethodStubInfo)list[n]).MethodInfo;

				object[] inits = MimeFormatter.GetInitializers (t, mets);

				for (int n=0; n<list.Count; n++)
					((HttpSimpleMethodStubInfo)list[n]).GetFormatterInfo (formatter).Initializer = inits[n];
			}
		}

		void AddType (Hashtable types, Type type, HttpSimpleMethodStubInfo method)
		{
			ArrayList list = (ArrayList) types [type];
			if (list == null)
			{
				list = new ArrayList ();
				types [type] = list;
			}
			list.Add (method);
		}
	}
}
