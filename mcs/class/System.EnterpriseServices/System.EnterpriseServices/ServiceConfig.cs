// System.EnterpriseServices.ServiceConfig.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
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
#if NET_2_0
using System.Transactions;
#endif

namespace System.EnterpriseServices {

	[MonoTODO]
	[ComVisible(false)]
	public sealed class ServiceConfig {

		#region Constructors

		[MonoTODO]
		public ServiceConfig ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		[MonoTODO]
		public BindingOption Binding {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

#if NET_2_0
		[MonoTODO]
		public Transaction BringYourOwnSystemTransaction {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO]
		public ITransaction BringYourOwnTransaction {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool COMTIIntrinsicsEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IISIntrinsicsEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public InheritanceOption Inheritance {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public TransactionIsolationLevel IsolationLevel {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Guid PartitionId {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public PartitionOption PartitionOption {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string SxsDirectory {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string SxsName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public SxsOption SxsOption {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public SynchronizationOption Synchronization {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ThreadPoolOption ThreadPool {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string TipUrl {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string TrackingAppName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string TrackingComponentName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool TrackingEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public TransactionOption Transaction {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string TransactionDescription {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int TransactionTimeout {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}
