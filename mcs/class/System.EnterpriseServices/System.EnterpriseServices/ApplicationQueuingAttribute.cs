// 
// System.EnterpriseServices.ApplicationQueuingAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class ApplicationQueuingAttribute : Attribute {

		#region Fields

		bool enabled;
		int maxListenerThreads;
		bool queueListenerEnabled;

		#endregion // Fields

		#region Constructors

		public ApplicationQueuingAttribute ()
		{
			enabled = true;
			queueListenerEnabled = false;
			maxListenerThreads = 0;
		}

		#endregion // Constructors

		#region Properties

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		public int MaxListenerThreads {
			get { return maxListenerThreads; }
			set { maxListenerThreads = value; }
		}

		public bool QueueListenerEnabled {
			get { return queueListenerEnabled; }
			set { queueListenerEnabled = value; }
		}

		#endregion // Properties
	}
}
