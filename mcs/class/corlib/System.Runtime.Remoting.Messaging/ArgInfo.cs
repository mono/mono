//
// System.Runtime.Remoting.Messaging.ArgInfo.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// 2003 (C) Lluis Sanchez Gual
//

using System;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging
{
	public enum ArgInfoType : byte { In, Out };

	public class ArgInfo
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
					if(!parameters[n].ParameterType.IsByRef) { _paramMap[_inoutArgCount++] = n; }
			}
			else {
				for (int n=0; n<parameters.Length; n++)
					if(parameters[n].ParameterType.IsByRef) { _paramMap[_inoutArgCount++] = n; }
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
