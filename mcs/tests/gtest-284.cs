using System;
using System.Collections;
using System.Collections.Generic;

public interface Y
{ }

public class X : Y
{
}

public struct Foo : Y
{ }

public static class CollectionTester
{
	static int Test<T> (IList<T> list)
	{
		if (list.Count != 1)
			return 1;

		ICollection<T> collection = list;
		if (collection.Count != 1)
			return 2;

		IEnumerable<T> enumerable = list;
		IEnumerator<T> enumerator = enumerable.GetEnumerator ();
		if (!enumerator.MoveNext ())
			return 3;
		if (enumerator.MoveNext ())
			return 4;

		return 0;
	}

	public static int Test ()
	{
#region X
		X[] xarray = new X [] { new X () };

		int result;
		result = Test<X> (xarray);
		if (result != 0)
			return result;

		result = Test<object> (xarray);
		if (result != 0)
			return 10 + result;

		result = Test<Y> (xarray);
		if (result != 0)
			return 20 + result;
#endregion

#region int
		int[] iarray = new int [] { 5 };
		result = Test<int> (iarray);
		if (result != 0)
			return 30 + result;

		result = Test<uint> ((IList<uint>) (object) iarray);
		if (result != 0)
			return 40 + result;

		uint[] uiarray = new uint [] { 5 };
		result = Test<int> ((IList<int>) (object) uiarray);
		if (result != 0)
			return 50 + result;

		result = Test<uint> (uiarray);
		if (result != 0)
			return 60 + result;
#endregion

#region long
		long[] larray = new long [] { 5 };
		result = Test<long> (larray);
		if (result != 0)
			return 70 + result;

		result = Test<ulong> ((IList<ulong>) (object) larray);
		if (result != 0)
			return 80 + result;

		ulong[] ularray = new ulong [] { 5 };
		result = Test<long> ((IList<long>) (object) ularray);
		if (result != 0)
			return 90 + result;

		result = Test<ulong> (ularray);
		if (result != 0)
			return 100 + result;
#endregion

#region short
		short[] sarray = new short [] { 5 };
		result = Test<short> (sarray);
		if (result != 0)
			return 110 + result;

		result = Test<ushort> ((IList<ushort>) (object) sarray);
		if (result != 0)
			return 120 + result;

		ushort[] usarray = new ushort [] { 5 };
		result = Test<short> ((IList<short>) (object) usarray);
		if (result != 0)
			return 130 + result;

		result = Test<ushort> (usarray);
		if (result != 0)
			return 140 + result;
#endregion

#region byte
		byte[] barray = new byte [] { 5 };
		result = Test<byte> (barray);
		if (result != 0)
			return 150 + result;

		result = Test<sbyte> ((IList<sbyte>) (object) barray);
		if (result != 0)
			return 160 + result;

		sbyte[] sbarray = new sbyte [] { 5 };
		result = Test<byte> ((IList<byte>) (object) sbarray);
		if (result != 0)
			return 170 + result;

		result = Test<sbyte> (sbarray);
		if (result != 0)
			return 180 + result;
#endregion

		return 0;
	}
}

public static class InterfaceTester
{
	public const bool Debug = false;

	static readonly Type ilist_type;
	static readonly Type icollection_type;
	static readonly Type ienumerable_type;
	static readonly Type generic_ilist_type;
	static readonly Type generic_icollection_type;
	static readonly Type generic_ienumerable_type;
	static readonly Type icloneable_type;

	static InterfaceTester ()
	{
		ilist_type = typeof (IList);
		icollection_type = typeof (ICollection);
		ienumerable_type = typeof (IEnumerable);
		generic_ilist_type = typeof (IList<>);
		generic_icollection_type = typeof (ICollection<>);
		generic_ienumerable_type = typeof (IEnumerable<>);
		icloneable_type = typeof (ICloneable);
	}

	enum State {
		Missing,
		Found,
		Extra
	}

	static int Test (Type t, params Type[] iface_types)
	{
		Hashtable ifaces = new Hashtable ();
		ifaces.Add (ilist_type, State.Missing);
		ifaces.Add (icollection_type, State.Missing);
		ifaces.Add (ienumerable_type, State.Missing);
		ifaces.Add (icloneable_type, State.Missing);
#if NET_4_0
		ifaces.Add (typeof (IStructuralEquatable), State.Missing);
		ifaces.Add (typeof (IStructuralComparable), State.Missing);
#endif
		Type array_type = t.MakeArrayType ();

		if (Debug) {
			Console.WriteLine ("Checking {0}", t);
			foreach (Type iface in t.GetInterfaces ())
				Console.WriteLine ("  {0}", iface);
		}

		foreach (Type iface in iface_types) {
			Type[] gargs = new Type[] { iface };
			ifaces.Add (generic_ilist_type.MakeGenericType (gargs), State.Missing);
			ifaces.Add (generic_icollection_type.MakeGenericType (gargs), State.Missing);
			ifaces.Add (generic_ienumerable_type.MakeGenericType (gargs), State.Missing);

#if NET_4_5
			ifaces.Add (typeof (IReadOnlyCollection<>).MakeGenericType (gargs), State.Missing);
			ifaces.Add (typeof (IReadOnlyList<>).MakeGenericType (gargs), State.Missing);
#endif
		}

		foreach (Type iface in array_type.GetInterfaces ()) {
			if (ifaces.Contains (iface))
				ifaces [iface] = State.Found;
			else
				ifaces.Add (iface, State.Extra);
		}

		int errors = 0;

		foreach (Type iface in ifaces.Keys) {
			State state = (State) ifaces [iface];
			if (state == State.Found) {
				if (Debug)
					Console.WriteLine ("Found {0}", iface);
				continue;
			} else {
				if (Debug)
					Console.WriteLine ("ERROR: {0} {1}", iface, state);
				errors++;
			}
		}

		if (Debug)
			Console.WriteLine ();

		return errors;
	}

	public static int Test ()
	{
		int result = Test (typeof (X), typeof (X));
		if (result != 0)
			return result;

		result = Test (typeof (Y), typeof (Y));
		if (result != 0)
			return 100 + result;

		result = Test (typeof (DateTime), typeof (DateTime));
		if (result != 0)
			return 200 + result;

		result = Test (typeof (float), typeof (float));
		if (result != 0)
			return 300 + result;

		result = Test (typeof (int), typeof (int));
		if (result != 0)
			return 400 + result;

		result = Test (typeof (uint), typeof (uint));
		if (result != 0)
			return 500 + result;

		result = Test (typeof (long), typeof (long));
		if (result != 0)
			return 600 + result;

		result = Test (typeof (ulong), typeof (ulong));
		if (result != 0)
			return 700 + result;

		result = Test (typeof (short), typeof (short));
		if (result != 0)
			return 800 + result;

		result = Test (typeof (ushort), typeof (ushort));
		if (result != 0)
			return 900 + result;

		result = Test (typeof (Foo), typeof (Foo));
		if (result != 0)
			return 1000 + result;

		return 0;
	}
}

class Z
{
	static int Test ()
	{
		int result;
		result = CollectionTester.Test ();
		if (result != 0)
			return result;
		result = InterfaceTester.Test ();
		if (result != 0)
			return 10000 + result;
		return 0;
	}

	public static int Main ()
	{
		int result = Test ();
		if (result == 0)
			Console.WriteLine ("OK");
		else
			Console.WriteLine ("ERROR: {0}", result);
		return result;
	}
}
