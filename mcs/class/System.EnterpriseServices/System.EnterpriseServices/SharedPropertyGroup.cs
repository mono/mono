// 
// System.EnterpriseServices.SharedPropertyGroup.cs
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

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[ComVisible (false)]
	public sealed class SharedPropertyGroup {

		#region Fields

		ISharedPropertyGroup propertyGroup;

		#endregion

		#region Constructors

		internal SharedPropertyGroup (ISharedPropertyGroup propertyGroup)
		{
			this.propertyGroup = propertyGroup;
		}

		#endregion // Constructors

		#region Methods

		public SharedProperty CreateProperty (string name, out bool fExists)
		{
			return new SharedProperty (propertyGroup.CreateProperty (name, out fExists));
		}

		public SharedProperty CreatePropertyByPosition (int position, out bool fExists)
		{
			return new SharedProperty (propertyGroup.CreatePropertyByPosition (position, out fExists));
		}

		public SharedProperty Property (string name)
		{
			return new SharedProperty (propertyGroup.Property (name));
		}

		public SharedProperty PropertyByPosition (int position)
		{
			return new SharedProperty (propertyGroup.PropertyByPosition (position));
		}

		#endregion // Methods
	}
}
