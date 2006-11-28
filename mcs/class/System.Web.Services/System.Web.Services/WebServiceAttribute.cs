 // 
// System.Web.Services.WebServiceAttribute.cs
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
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
#else
	[AttributeUsage (AttributeTargets.Class, Inherited = true)]
#endif
	public sealed class WebServiceAttribute : Attribute {

		#region Fields

		public const string DefaultNamespace = "http://tempuri.org/";
		string description;
		string name;
		string ns;

		#endregion // Fields

		#region Constructors

		
		public WebServiceAttribute ()
		{
			description = String.Empty;
			name = String.Empty;
			ns = DefaultNamespace;
		}
		
		#endregion // Constructors

		#region Properties

		public string Description { 	
			get { return description; }
			set { description = value; }
		}
	
		public string Name {
			get { return name; }
			set { name = value; }
		}
	
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		#endregion // Properties
	}
}
