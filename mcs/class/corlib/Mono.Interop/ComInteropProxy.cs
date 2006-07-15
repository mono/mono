//
// Mono.Interop.ComInteropProxy
//
// Authors:
//   Jonathan Chambers <joncham@gmail.com>
//
// Copyright (C) 2006 Novell (http://www.novell.com)
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;


namespace Mono.Interop
{
	internal class ComInteropProxy : RealProxy, IRemotingTypeInfo
    {
        #region Sync with object-internals.h
		private __ComObject com_object;
        #endregion
        private string type_name;
		public ComInteropProxy (Type t)
			: base (t)
		{
			com_object = __ComObject.CreateRCW (t);
		}

        internal ComInteropProxy (IntPtr pUnk)
            : base (typeof (__ComObject))
        {
            com_object = new __ComObject(pUnk);
        }

		public override IMessage Invoke (IMessage msg)
		{
			Console.WriteLine ("Invoke");

			throw new Exception ("The method or operation is not implemented.");
		}

		public string TypeName
		{
			get { return type_name; }
			set { type_name = value; }
		}

		public bool CanCastTo (Type fromType, object o)
		{
            __ComObject co = o as __ComObject;
            if (co == null)
                throw new NotSupportedException ("Only RCWs are currently supported");

            if ((fromType.Attributes & TypeAttributes.Import) == 0)
                return false;

            if (co.GetInterface (fromType) == IntPtr.Zero)
                return false;
            
            return true;
		}
	}
}
