 // 
// System.Web.Services.WebServiceBindingAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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

namespace System.Web.Services {
#if NET_2_0
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
#else
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
#endif
	public sealed class WebServiceBindingAttribute : Attribute {

		#region Fields

		string location;
		string name;
		string ns;
		
#if NET_2_0
		bool emitConformanceClaims;
		
		WsiProfiles conformsTo;
#endif

		#endregion // Fields

		#region Constructors
		
		public WebServiceBindingAttribute ()
			: this (String.Empty, String.Empty, String.Empty)
		{
		}

		public WebServiceBindingAttribute (string name)
			: this (name, String.Empty, String.Empty)
		{
		}

		public WebServiceBindingAttribute (string name, string ns)
			: this (name, ns, String.Empty)
		{
		}

		public WebServiceBindingAttribute (string name, string ns, string location)
		{
			this.name = name;
			this.ns = ns;
			this.location = location;
		}
		
		#endregion // Constructors

		#region Properties

		public string Location { 	
			get { return location; }
			set { location = value; }
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}
		
#if NET_2_0

		public bool EmitConformanceClaims {
			get { return emitConformanceClaims; }
			set { emitConformanceClaims = value; }
		}	
		
		public WsiProfiles ConformsTo { 
			get { return conformsTo; } 
			set { conformsTo = value; }
		}
#endif



		#endregion // Properties
	}
}
