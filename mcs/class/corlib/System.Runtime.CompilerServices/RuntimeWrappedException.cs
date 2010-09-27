//
// System.Runtime.CompilerServices.RuntimeWrappedException.cs
//
// Author: Zoltan Varga (vargaz@gmail.com)
//
// (C) Copyright, Ximian Inc.
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Runtime.CompilerServices {

	[Serializable]
	public sealed class RuntimeWrappedException : Exception
	{
#pragma warning disable 649
#region Synch with object-internals.h
		private object wrapped_exception;
#endregion
#pragma warning restore 649

		// Called by the runtime
		private RuntimeWrappedException () : base ("An object that does not derive from System.Exception has been wrapped in a RuntimeWrappedException.")
		{
		}

		public object WrappedException {
			get {
				return wrapped_exception;
			}
		}

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("WrappedException", wrapped_exception);
		}
	}
}
