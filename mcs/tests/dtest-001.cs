//
// dynamic type attribute decoration
//

using System;

class C
{
	public C (dynamic d)
	{
	}
	
	public dynamic a;
	public dynamic[] a2;
	public const dynamic c = default (dynamic);
	
	public dynamic Prop { set; get; }
	public dynamic Prop2 { set {} }
	
	public dynamic this [dynamic d] { set {} get { return 1; } }
	
	public dynamic Method (dynamic d)
	{
		return null;
	}
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
		
		// TODO: Why is it needed
		//if (t.GetMember ("a2")[0].GetCustomAttributes (ca, false).Length != 1)
		//	return 2;
		
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
		
		if (t.GetMethod ("set_Prop").GetParameters()[0].GetCustomAttributes (ca, false).Length != 1)
			return 9;
		
		if (t.GetMember ("Prop2")[0].GetCustomAttributes (ca, false).Length != 1)
			return 10;

		if (t.GetMember ("set_Prop2")[0].GetCustomAttributes (ca, false).Length != 0)
			return 11;
		
		if (t.GetMember ("Item")[0].GetCustomAttributes (ca, false).Length != 1)
			return 12;
		
		if (t.GetMethod ("get_Item").ReturnParameter.GetCustomAttributes (ca, false).Length != 1)
			return 13;

		if (t.GetMethod ("get_Item").GetParameters()[0].GetCustomAttributes (ca, false).Length != 1)
			return 14;

		if (t.GetMethod ("set_Item").ReturnParameter.GetCustomAttributes (ca, false).Length != 0)
			return 15;

		if (t.GetMethod ("set_Item").GetParameters()[0].GetCustomAttributes (ca, false).Length != 1)
			return 16;

		if (t.GetMethod ("set_Item").GetParameters()[1].GetCustomAttributes (ca, false).Length != 1)
			return 17;

		if (t.GetMember ("Method")[0].GetCustomAttributes (ca, false).Length != 0)
			return 18;

		if (t.GetMethod ("Method").GetParameters()[0].GetCustomAttributes (ca, false).Length != 1)
			return 19;

		if (t.GetConstructors ()[0].GetParameters()[0].GetCustomAttributes (ca, false).Length != 1)
			return 20;

		if (t.GetConstructors ()[0].GetCustomAttributes (ca, false).Length != 0)
			return 21;
		
		
		t = typeof (Del);

		if (t.GetMember ("Invoke")[0].GetCustomAttributes (ca, false).Length != 0)
			return 100;

		if (t.GetMethod ("Invoke").GetParameters()[0].GetCustomAttributes (ca, false).Length != 1)
			return 101;

		if (t.GetMethod ("Invoke").ReturnParameter.GetCustomAttributes (ca, false).Length != 1)
			return 102;

		Console.WriteLine ("ok");
		return 0;
	}
}
