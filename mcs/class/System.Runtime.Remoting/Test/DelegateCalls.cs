//
// MonoTests.Remoting.DelegateCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using NUnit.Framework;
using System.Text;
using System.Runtime.InteropServices;

namespace MonoTests.Remoting
{
	public abstract class DelegateCallTest : BaseCallTest
	{
		public override InstanceSurrogate GetInstanceSurrogate () { return new DelegateInstanceSurrogate (); }
		public override AbstractSurrogate GetAbstractSurrogate () { return new DelegateAbstractSurrogate (); }
		public override InterfaceSurrogate GetInterfaceSurrogate () { return new DelegateInterfaceSurrogate (); }
	}

	public class DelegateInstanceSurrogate : InstanceSurrogate
	{
		public override int Simple ()
		{
			DelegateSimple de = new DelegateSimple (RemoteObject.Simple);
			return de ();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			DelegatePrimitiveParams de = new DelegatePrimitiveParams (RemoteObject.PrimitiveParams);
			return de (a,b,c,d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			DelegatePrimitiveParamsInOut de = new DelegatePrimitiveParamsInOut (RemoteObject.PrimitiveParamsInOut);
			return de (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			DelegateComplexParams de = new DelegateComplexParams (RemoteObject.ComplexParams);
			return de (a,b,c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			DelegateComplexParamsInOut de = new DelegateComplexParamsInOut (RemoteObject.ComplexParamsInOut);
			return de (ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			DelegateProcessContextData de = new DelegateProcessContextData (RemoteObject.ProcessContextData);
			de ();
		}
	}

	public class DelegateAbstractSurrogate : AbstractSurrogate
	{
		public override int Simple ()
		{
			DelegateSimple de = new DelegateSimple (RemoteObject.Simple);
			return de ();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			DelegatePrimitiveParams de = new DelegatePrimitiveParams (RemoteObject.PrimitiveParams);
			return de (a,b,c,d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			DelegatePrimitiveParamsInOut de = new DelegatePrimitiveParamsInOut (RemoteObject.PrimitiveParamsInOut);
			return de (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			DelegateComplexParams de = new DelegateComplexParams (RemoteObject.ComplexParams);
			return de (a,b,c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			DelegateComplexParamsInOut de = new DelegateComplexParamsInOut (RemoteObject.ComplexParamsInOut);
			return de (ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			DelegateProcessContextData de = new DelegateProcessContextData (RemoteObject.ProcessContextData);
			de ();
		}
	}

	public class DelegateInterfaceSurrogate : InterfaceSurrogate
	{
		public override int Simple ()
		{
			DelegateSimple de = new DelegateSimple (RemoteObject.Simple);
			return de ();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			DelegatePrimitiveParams de = new DelegatePrimitiveParams (RemoteObject.PrimitiveParams);
			return de (a,b,c,d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			DelegatePrimitiveParamsInOut de = new DelegatePrimitiveParamsInOut (RemoteObject.PrimitiveParamsInOut);
			return de (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			DelegateComplexParams de = new DelegateComplexParams (RemoteObject.ComplexParams);
			return de (a,b,c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			DelegateComplexParamsInOut de = new DelegateComplexParamsInOut (RemoteObject.ComplexParamsInOut);
			return de (ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			DelegateProcessContextData de = new DelegateProcessContextData (RemoteObject.ProcessContextData);
			de ();
		}
	}
}
