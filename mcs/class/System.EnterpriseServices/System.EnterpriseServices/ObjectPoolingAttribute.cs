// 
// System.EnterpriseServices.ObjectPoolingAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ObjectPoolingAttribute : Attribute {

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
