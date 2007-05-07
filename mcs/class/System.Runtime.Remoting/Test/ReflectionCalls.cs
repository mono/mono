//
// MonoTests.Remoting.ReflectionCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Reflection;
using System.Collections;
using NUnit.Framework;
using System.Text;
using System.Runtime.InteropServices;

namespace MonoTests.Remoting
{
	public abstract class ReflectionCallTest : BaseCallTest
	{
		public override InstanceSurrogate GetInstanceSurrogate () { return new ReflectionInstanceSurrogate (); }
		public override AbstractSurrogate GetAbstractSurrogate () { return new ReflectionAbstractSurrogate (); }
		public override InterfaceSurrogate GetInterfaceSurrogate () { return new ReflectionInterfaceSurrogate (); }

		public static int Simple (Type type, object target)
		{
			object[] parms = new object[0];
			MethodBase m = type.GetMethod ("Simple");
			return (int) m.Invoke (target, parms);
		}

		public static string PrimitiveParams (Type type, object target, int a, uint b, char c, string d)
		{
			object[] parms = new object[] {a,b,c,d};
			Type[] sig = new Type[] {typeof (int), typeof (uint), typeof (char), typeof (string)};
			MethodBase m = type.GetMethod ("PrimitiveParams", sig);
			return (string) m.Invoke (target, parms);
		}

		public static string PrimitiveParamsInOut (Type type, object target, ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			object[] parms = new object[] {a1,0,b1,0f,filler,c1,'\0',d1,null};
			MethodBase m = type.GetMethod ("PrimitiveParamsInOut");
			string res = (string) m.Invoke (target, parms);
			a1 = (int)parms[0];
			b1 = (float)parms[2];
			c1 = (char)parms[5];
			d1 = (string)parms[7];
			a2 = (int)parms[1];
			b2 = (float)parms[3];
			c2 = (char)parms[6];
			d2 = (string)parms[8];
			return res;
		}

		public static Complex ComplexParams (Type type, object target, ArrayList a, Complex b, string c)
		{
			object[] parms = new object[] {a,b,c};
			MethodBase m = type.GetMethod ("ComplexParams");
			return (Complex) m.Invoke (target, parms);
		}

		public static Complex ComplexParamsInOut (Type type, object target, ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			object[] parms = new object[] {a,null,bytes,sb,c};
			MethodBase m = type.GetMethod ("ComplexParamsInOut");
			Complex res = (Complex) m.Invoke (target, parms);
			a = (ArrayList) parms[0];
			b = (Complex) parms[1];
			return res;
		}

		public static void ProcessContextData (Type type, object target)
		{
			MethodBase m = type.GetMethod ("ProcessContextData");
			m.Invoke (target, null);
		}
	}

	public class ReflectionInstanceSurrogate : InstanceSurrogate
	{
		public override int Simple ()
		{
			return ReflectionCallTest.Simple (typeof (RemoteObject), RemoteObject);
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return ReflectionCallTest.PrimitiveParams (typeof (RemoteObject), RemoteObject, a, b, c, d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			return ReflectionCallTest.PrimitiveParamsInOut (typeof (RemoteObject), RemoteObject, ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			return ReflectionCallTest.ComplexParams (typeof (RemoteObject), RemoteObject, a, b, c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			return ReflectionCallTest.ComplexParamsInOut (typeof (RemoteObject), RemoteObject, ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			ReflectionCallTest.ProcessContextData (typeof (RemoteObject), RemoteObject);
		}
	}

	public class ReflectionAbstractSurrogate : AbstractSurrogate
	{
		public override int Simple ()
		{
			return ReflectionCallTest.Simple (typeof (AbstractRemoteObject), RemoteObject);
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return ReflectionCallTest.PrimitiveParams (typeof (AbstractRemoteObject), RemoteObject, a, b, c, d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			return ReflectionCallTest.PrimitiveParamsInOut (typeof (AbstractRemoteObject), RemoteObject, ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			return ReflectionCallTest.ComplexParams (typeof (AbstractRemoteObject), RemoteObject, a, b, c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			return ReflectionCallTest.ComplexParamsInOut (typeof (AbstractRemoteObject), RemoteObject, ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			ReflectionCallTest.ProcessContextData (typeof (AbstractRemoteObject), RemoteObject);
		}
	}

	public class ReflectionInterfaceSurrogate : InterfaceSurrogate
	{
		public override int Simple ()
		{
			return ReflectionCallTest.Simple (typeof (IRemoteObject), RemoteObject);
		}

		public override string PrimitiveParams (int a, uint b, char c, string d)
		{
			return ReflectionCallTest.PrimitiveParams (typeof (IRemoteObject), RemoteObject, a, b, c, d);
		}

		public override string PrimitiveParamsInOut (ref int a1, out int a2, ref float b1, out float b2, int filler, ref char c1, out char c2, ref string d1, out string d2)
		{
			return ReflectionCallTest.PrimitiveParamsInOut (typeof (IRemoteObject), RemoteObject, ref a1, out a2, ref b1, out b2, filler, ref c1, out c2, ref d1, out d2);
		}

		public override Complex ComplexParams (ArrayList a, Complex b, string c)
		{
			return ReflectionCallTest.ComplexParams (typeof (IRemoteObject), RemoteObject, a, b, c);
		}

		public override Complex ComplexParamsInOut (ref ArrayList a, out Complex b, [In,Out] byte[] bytes, [In,Out] StringBuilder sb, string c)
		{
			return ReflectionCallTest.ComplexParamsInOut (typeof (IRemoteObject), RemoteObject, ref a, out b, bytes, sb, c);
		}

		public override void ProcessContextData ()
		{
			ReflectionCallTest.ProcessContextData (typeof (IRemoteObject), RemoteObject);
		}
	}
}
