//
// MonoTests.Remoting.SyncCalls.cs
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
	public abstract class SyncCallTest : BaseCallTest
	{
		public override InstanceSurrogate GetInstanceSurrogate () { return new SyncInstanceSurrogate (); }
		public override AbstractSurrogate GetAbstractSurrogate () { return new SyncAbstractSurrogate (); }
		public override InterfaceSurrogate GetInterfaceSurrogate () { return new SyncInterfaceSurrogate (); }
	}

	public class SyncInstanceSurrogate : InstanceSurrogate
	{
		public override int Simple ()
		{
			return RemoteObject.Simple ();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return RemoteObject.PrimitiveParams (a, b, c, d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			return RemoteObject.PrimitiveParamsInOut (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			return RemoteObject.ComplexParams (a, b, c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			return RemoteObject.ComplexParamsInOut (ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			RemoteObject.ProcessContextData ();
		}
	}

	public class SyncAbstractSurrogate : AbstractSurrogate
	{
		public override int Simple ()
		{
			return RemoteObject.Simple ();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return RemoteObject.PrimitiveParams (a, b, c, d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			return RemoteObject.PrimitiveParamsInOut (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			return RemoteObject.ComplexParams (a, b, c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			return RemoteObject.ComplexParamsInOut (ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			RemoteObject.ProcessContextData ();
		}
	}

	public class SyncInterfaceSurrogate : InterfaceSurrogate
	{
		public override int Simple ()
		{
			return RemoteObject.Simple ();
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return RemoteObject.PrimitiveParams (a, b, c, d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			return RemoteObject.PrimitiveParamsInOut (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			return RemoteObject.ComplexParams (a, b, c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			return RemoteObject.ComplexParamsInOut (ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			RemoteObject.ProcessContextData ();
		}
	}
}
