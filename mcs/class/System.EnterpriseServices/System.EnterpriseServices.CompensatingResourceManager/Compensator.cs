// 
// System.EnterpriseServices.CompensatingResourceManager.Compensator.cs
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
using System.EnterpriseServices;

namespace System.EnterpriseServices.CompensatingResourceManager {
	public class Compensator : ServicedComponent{

		#region Constructors

		[MonoTODO]
		public Compensator ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public Clerk Clerk {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual bool AbortRecord (LogRecord rec)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BeginAbort (bool fRecovery)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BeginCommit (bool fRecovery)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BeginPrepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool CommitRecord (LogRecord rec)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void EndAbort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void EndCommit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool EndPrepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool PrepareRecord (LogRecord rec)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
