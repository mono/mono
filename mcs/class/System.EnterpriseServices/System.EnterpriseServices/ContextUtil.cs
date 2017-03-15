// 
// System.EnterpriseServices.ContextUtil.cs
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
using System.Transactions;

namespace System.EnterpriseServices {
	public sealed class ContextUtil {

		#region Fields

		static bool deactivateOnReturn;
		static TransactionVote myTransactionVote;

		#endregion // Fields

		#region Constructors

		internal ContextUtil ()
		{
		}

		#endregion // Constructors

		#region Properties

		public static Guid ActivityId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		public static Guid ApplicationId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Guid ApplicationInstanceId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Guid ContextId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static bool DeactivateOnReturn {
			get { return deactivateOnReturn; }
			set { deactivateOnReturn = value; }
		}

		public static bool IsInTransaction {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static bool IsSecurityEnabled {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static TransactionVote MyTransactionVote {
			get { return myTransactionVote; }
			set { myTransactionVote = value; }
		}

		public static Guid PartitionId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static object Transaction {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Transaction SystemTransaction {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Guid TransactionId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DisableCommit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void EnableCommit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetNamedProperty (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsCallerInRole (string role)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsDefaultContext ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetAbort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetComplete ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetNamedProperty (string name, object value)
		{
			throw new NotImplementedException ();
		}
		#endregion // Methods
	}
}
