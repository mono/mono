//
// MonoTests.Remoting.AsyncCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using System.Text;
using System.Runtime.InteropServices;

namespace MonoTests.Remoting
{
	public abstract class AsyncCallTest : BaseCallTest
	{
		public override InstanceSurrogate GetInstanceSurrogate () { return new AsyncInstanceSurrogate (); }
		public override AbstractSurrogate GetAbstractSurrogate () { return new AsyncAbstractSurrogate (); }
		public override InterfaceSurrogate GetInterfaceSurrogate () { return new AsyncInterfaceSurrogate (); }

		public static void DoWork ()
		{
			for (int n=0; n<10; n++)
				Thread.Sleep (1);
		}
	}

	public delegate int DelegateSimple ();
	public delegate string DelegatePrimitiveParams (int a, uint b, char c, string d);
	public delegate string DelegatePrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2);
	public delegate Complex DelegateComplexParams (ArrayList a, Complex b, string c);
	public delegate Complex DelegateComplexParamsInOut (ref ArrayList a, out Complex b, byte[] bytes, StringBuilder sb, string c);
	public delegate void DelegateProcessContextData ();

	public class AsyncInstanceSurrogate : InstanceSurrogate
	{
		public override int Simple ()
		{
			DelegateSimple de = new DelegateSimple (RemoteObject.Simple);
			IAsyncResult ar = de.BeginInvoke (null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			DelegatePrimitiveParams de = new DelegatePrimitiveParams (RemoteObject.PrimitiveParams);
			IAsyncResult ar = de.BeginInvoke (a,b,c,d,null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			DelegatePrimitiveParamsInOut de = new DelegatePrimitiveParamsInOut (RemoteObject.PrimitiveParamsInOut);
			IAsyncResult ar = de.BeginInvoke (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2, null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ref a1, out a2, ref b1, out b2, ref c1, out c2, ref d1, out d2, ar);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			DelegateComplexParams de = new DelegateComplexParams (RemoteObject.ComplexParams);
			IAsyncResult ar = de.BeginInvoke (a,b,c,null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, StringBuilder sb, string c)
		{
			DelegateComplexParamsInOut de = new DelegateComplexParamsInOut (RemoteObject.ComplexParamsInOut);
			IAsyncResult ar = de.BeginInvoke (ref a, out b, bytes, sb, c, null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ref a, out b, ar);
		}

		public override void ProcessContextData ()
		{
			DelegateProcessContextData de = new DelegateProcessContextData (RemoteObject.ProcessContextData);
			IAsyncResult ar = de.BeginInvoke (null,null);
			AsyncCallTest.DoWork ();
			de.EndInvoke (ar);
		}
	}

	public class AsyncAbstractSurrogate : AbstractSurrogate
	{
		public override int Simple ()
		{
			DelegateSimple de = new DelegateSimple (RemoteObject.Simple);
			IAsyncResult ar = de.BeginInvoke (null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			DelegatePrimitiveParams de = new DelegatePrimitiveParams (RemoteObject.PrimitiveParams);
			IAsyncResult ar = de.BeginInvoke (a,b,c,d,null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			DelegatePrimitiveParamsInOut de = new DelegatePrimitiveParamsInOut (RemoteObject.PrimitiveParamsInOut);
			IAsyncResult ar = de.BeginInvoke (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2, null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ref a1, out a2, ref b1, out b2, ref c1, out c2, ref d1, out d2, ar);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			DelegateComplexParams de = new DelegateComplexParams (RemoteObject.ComplexParams);
			IAsyncResult ar = de.BeginInvoke (a,b,c,null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, StringBuilder sb, string c)
		{
			DelegateComplexParamsInOut de = new DelegateComplexParamsInOut (RemoteObject.ComplexParamsInOut);
			IAsyncResult ar = de.BeginInvoke (ref a, out b, bytes, sb, c, null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ref a, out b, ar);
		}

		public override void ProcessContextData ()
		{
			DelegateProcessContextData de = new DelegateProcessContextData (RemoteObject.ProcessContextData);
			IAsyncResult ar = de.BeginInvoke (null,null);
			AsyncCallTest.DoWork ();
			de.EndInvoke (ar);
		}
	}

	public class AsyncInterfaceSurrogate : InterfaceSurrogate
	{
		public override int Simple ()
		{
			DelegateSimple de = new DelegateSimple (RemoteObject.Simple);
			IAsyncResult ar = de.BeginInvoke (null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			DelegatePrimitiveParams de = new DelegatePrimitiveParams (RemoteObject.PrimitiveParams);
			IAsyncResult ar = de.BeginInvoke (a,b,c,d,null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			DelegatePrimitiveParamsInOut de = new DelegatePrimitiveParamsInOut (RemoteObject.PrimitiveParamsInOut);
			IAsyncResult ar = de.BeginInvoke (ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2, null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ref a1, out a2, ref b1, out b2, ref c1, out c2, ref d1, out d2, ar);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			DelegateComplexParams de = new DelegateComplexParams (RemoteObject.ComplexParams);
			IAsyncResult ar = de.BeginInvoke (a,b,c,null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ar);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, StringBuilder sb, string c)
		{
			DelegateComplexParamsInOut de = new DelegateComplexParamsInOut (RemoteObject.ComplexParamsInOut);
			IAsyncResult ar = de.BeginInvoke (ref a, out b, bytes, sb, c, null,null);
			AsyncCallTest.DoWork ();
			return de.EndInvoke (ref a, out b, ar);
		}

		public override void ProcessContextData ()
		{
			DelegateProcessContextData de = new DelegateProcessContextData (RemoteObject.ProcessContextData);
			IAsyncResult ar = de.BeginInvoke (null,null);
			AsyncCallTest.DoWork ();
			de.EndInvoke (ar);
		}
	}
}
