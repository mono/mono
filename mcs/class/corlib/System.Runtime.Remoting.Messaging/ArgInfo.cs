//
// System.Runtime.Remoting.Messaging.ArgInfo.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// 2003 (C) Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Runtime.Remoting.Messaging
{
	internal enum ArgInfoType : byte { In, Out };

	internal class ArgInfo
	{
		int[] _paramMap;
		int _inoutArgCount;
		MethodBase _method;

		public ArgInfo(MethodBase method, ArgInfoType type)
		{
			_method = method;

			ParameterInfo[] parameters = _method.GetParameters();
			_paramMap = new int[parameters.Length];
			_inoutArgCount = 0;

			if (type == ArgInfoType.In) {
				for (int n=0; n<parameters.Length; n++)
					if (!parameters[n].ParameterType.IsByRef) { _paramMap [_inoutArgCount++] = n; }
			}
			else {
				for (int n=0; n<parameters.Length; n++)
					if (parameters[n].ParameterType.IsByRef || parameters[n].IsOut) 
					{ _paramMap [_inoutArgCount++] = n; }
			}
		}

		public int GetInOutArgIndex (int inoutArgNum)
		{
			return _paramMap[inoutArgNum];
		}

		public virtual string GetInOutArgName (int index)
		{
			return _method.GetParameters()[_paramMap[index]].Name;
		}

		public int GetInOutArgCount ()
		{
			return _inoutArgCount;
		}

		public object [] GetInOutArgs (object[] args)
		{
			object[] inoutArgs = new object[_inoutArgCount];
			for (int n=0; n<_inoutArgCount; n++)
				inoutArgs[n] = args[_paramMap[n]];
			return inoutArgs;
		}
	}
}
