// 
// System.Web.Services.Protocols.MimeFormatter.cs
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

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class MimeFormatter {

		#region Constructors

		protected MimeFormatter () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public static MimeFormatter CreateInstance (Type type, object initializer)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			ob.Initialize (initializer);
			return ob;
		}

		public abstract object GetInitializer (LogicalMethodInfo methodInfo);

		public static object GetInitializer (Type type, LogicalMethodInfo methodInfo)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			return ob.GetInitializer (methodInfo);
		}

		public virtual object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			object[] initializers = new object [methodInfos.Length];
			for (int n=0; n<methodInfos.Length; n++)
				initializers [n] = GetInitializer (methodInfos[n]);
				
			return initializers;
		}

		public static object[] GetInitializers (Type type, LogicalMethodInfo[] methodInfos)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			return ob.GetInitializers (methodInfos);
		}

		public abstract void Initialize (object initializer);	

		#endregion // Methods
	}
}
