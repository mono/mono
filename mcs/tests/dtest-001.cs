//
// dynamic type attribute decoration
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

interface I<T>
{
}

class C
{
	public C (dynamic d)
	{
	}

	public dynamic a;
	public const dynamic c = default (dynamic);

	public dynamic Prop { set; get; }
	public dynamic Prop2 { set { } }

	public dynamic this[dynamic d] { set { } get { return 1; } }

	public dynamic Method (dynamic d)
	{
		return null;
	}

	// Transformation handling required
	public dynamic[] t;
	public dynamic[,] t2;
	public Func<dynamic, int, dynamic[]> v;
	public I<dynamic>[] iface;
	public Action<int[], object, dynamic> d2;
}

delegate dynamic Del (dynamic d);

class Test
{
	public static int Main ()
	{
		Type t = typeof (C);
		Type ca = typeof (System.Runtime.CompilerServices.DynamicAttribute);

		if (t.GetMember ("a")[0].GetCustomAttributes (ca, false).Length != 1)
			return 1;

		if (t.GetMember ("c")[0].GetCustomAttributes (ca, false).Length != 1)
			return 3;

		if (t.GetMember ("Prop")[0].GetCustomAttributes (ca, false).Length != 1)
			return 4;

		if (t.GetMember ("get_Prop")[0].GetCustomAttributes (ca, false).Length != 0)
			return 5;

		if (t.GetMethod ("get_Prop").ReturnParameter.GetCustomAttributes (ca, false).Length != 1)
			return 6;

		if (t.GetMember ("set_Prop")[0].GetCustomAttributes (ca, false).Length != 0)
			return 7;

		if (t.GetMethod ("set_Prop").ReturnParameter.GetCustomAttributes (ca, false).Length != 0)
			return 8;

		if (t.GetMethod ("set_Prop").GetParameters ()[0].GetCustomAttributes (ca, false).Length != 1)
			return 9;

		if (t.GetMember ("Prop2")[0].GetCustomAttributes (ca, false).Length != 1)
			return 10;

		if (t.GetMember ("set_Prop2")[0].GetCustomAttributes (ca, false).Length != 0)
			return 11;

		if (t.GetMember ("Item")[0].GetCustomAttributes (ca, false).Length != 1)
			return 12;

		if (t.GetMethod ("get_Item").ReturnParameter.GetCustomAttributes (ca, false).Length != 1)
			return 13;

		if (t.GetMethod ("get_Item").GetParameters ()[0].GetCustomAttributes (ca, false).Length != 1)
			return 14;

		if (t.GetMethod ("set_Item").ReturnParameter.GetCustomAttributes (ca, false).Length != 0)
			return 15;

		if (t.GetMethod ("set_Item").GetParameters ()[0].GetCustomAttributes (ca, false).Length != 1)
			return 16;

		if (t.GetMethod ("set_Item").GetParameters ()[1].GetCustomAttributes (ca, false).Length != 1)
			return 17;

		if (t.GetMember ("Method")[0].GetCustomAttributes (ca, false).Length != 0)
			return 18;

		if (t.GetMethod ("Method").GetParameters ()[0].GetCustomAttributes (ca, false).Length != 1)
			return 19;

		if (t.GetConstructors ()[0].GetParameters ()[0].GetCustomAttributes (ca, false).Length != 1)
			return 20;

		if (t.GetConstructors ()[0].GetCustomAttributes (ca, false).Length != 0)
			return 21;

		// Transformations
		DynamicAttribute da;
		da = t.GetMember ("t")[0].GetCustomAttributes (ca, false)[0] as DynamicAttribute;
		if (da == null)
			return 40;

		if (!da.TransformFlags.SequenceEqual (new bool[] { false, true }))
			return 41;

		da = t.GetMember ("t2")[0].GetCustomAttributes (ca, false)[0] as DynamicAttribute;
		if (da == null)
			return 42;

		if (!da.TransformFlags.SequenceEqual (new bool[] { false, true }))
			return 43;

		da = t.GetMember ("v")[0].GetCustomAttributes (ca, false)[0] as DynamicAttribute;
		if (da == null)
			return 44;
		if (!da.TransformFlags.SequenceEqual (new bool[] { false, true, false, false, true }))
			return 45;
		
		da = t.GetMember ("iface")[0].GetCustomAttributes (ca, false)[0] as DynamicAttribute;
		if (da == null)
			return 46;
		if (!da.TransformFlags.SequenceEqual (new bool[] { false, false, true }))
			return 47;

		da = t.GetMember ("d2")[0].GetCustomAttributes (ca, false)[0] as DynamicAttribute;
		if (da == null)
			return 48;
		if (!da.TransformFlags.SequenceEqual (new bool[] { false, false, false, false, true }))
			return 49;

		t = typeof (Del);

		if (t.GetMember ("Invoke")[0].GetCustomAttributes (ca, false).Length != 0)
			return 100;

		if (t.GetMethod ("Invoke").GetParameters ()[0].GetCustomAttributes (ca, false).Length != 1)
			return 101;

		if (t.GetMethod ("Invoke").ReturnParameter.GetCustomAttributes (ca, false).Length != 1)
			return 102;

		Console.WriteLine ("ok");
		return 0;
	}
}
