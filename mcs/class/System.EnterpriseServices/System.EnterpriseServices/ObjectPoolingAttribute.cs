// 
// System.EnterpriseServices.ObjectPoolingAttribute.cs
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
using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	[ComVisible(false)]
	public sealed class ObjectPoolingAttribute : Attribute, IConfigurationAttribute {

		#region Fields

		int creationTimeout;
		bool enabled;
		int minPoolSize;
		int maxPoolSize;

		#endregion // Fields

		#region Constructors

		public ObjectPoolingAttribute () 
			: this (true)
		{
		}

		public ObjectPoolingAttribute (bool enable)
		{
			this.enabled = enable;
		}

		public ObjectPoolingAttribute (int minPoolSize, int maxPoolSize)
			: this (true, minPoolSize, maxPoolSize)
		{
		}

		public ObjectPoolingAttribute (bool enable, int minPoolSize, int maxPoolSize)
		{
			this.enabled = enable;
			this.minPoolSize = minPoolSize;
			this.maxPoolSize = maxPoolSize;
		}

		#endregion // Constructors

		#region Properties

		public int CreationTimeout {
			get { return creationTimeout; }
			set { creationTimeout = value; }
		}

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		public int MaxPoolSize {
			get { return maxPoolSize; }
			set { maxPoolSize = value; }
		}

		public int MinPoolSize {
			get { return minPoolSize; }
			set { minPoolSize = value; }
		}

		#endregion // Properties

		#region Methods 

		[MonoTODO]
		public bool AfterSaveChanges (Hashtable info)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Apply (Hashtable info)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsValidTarget (string s)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
