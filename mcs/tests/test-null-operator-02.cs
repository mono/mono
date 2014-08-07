using System;

class CI
{
	public long Field;
	public sbyte? FieldNullable;
	public object FieldReference;

	public int Prop { get; set; }
	public byte? PropNullable { get; set; }
	public object PropReference { get; set; }

	public event Action ev1;
}

class C
{
	static int TestProperty ()
	{
		CI ci = null;

		var m1 = ci?.Prop;
		var m2 = ci?.PropNullable;
		var m3 = ci?.PropReference;

		var m4 = ci?.Prop.ToString () ?? "N";
		if (m4 != "N")
			return 1;

		var m5 = ci?.PropNullable.ToString () ?? "N";
		if (m5 != "N")
			return 2;

		var m6 = ci?.PropReference.ToString () ?? "N";
		if (m6 != "N")
			return 3; 

//        ci?.Prop = 6;

		ci = new CI ();
		m1 = ci?.Prop;
		m2 = ci?.PropNullable;
		m3 = ci?.PropReference;

//        ci?.Prop = 5;
//        if (ci.Prop != 5)
//            return 1;

// TODO: It's not allowed for now
//      ci?.Prop += 4;
//      var pp1 = ci?.Prop = 4;
//      var pp2 = ci?.Prop += 4;

		return 0;
	}

	static int TestField ()
	{
		CI ci = null;
		var m1 = ci?.Field;
		var m2 = ci?.FieldNullable;
		var m3 = ci?.FieldReference;
		var m4 = ci?.Field.ToString () ?? "N";
		if (m4 != "N")
			return 1;

		var m5 = ci?.FieldNullable.ToString () ?? "N";
		if (m5 != "N")
			return 2;

		var m6 = ci?.FieldReference.ToString () ?? "N";
		if (m6 != "N")
			return 3; 

//        ci?.Field = 6;

		ci = new CI ();
		m1 = ci?.Field;
		m2 = ci?.FieldNullable;
		m3 = ci?.FieldReference;

//        ci?.Field = 5;
//        if (ci.Field != 5)
//            return 1;

// TODO: It's not allowed for now
//      ci?.Field += 4;
//      var pp1 = ci?.Field = 4;
//      var pp2 = ci?.Field += 4;

		return 0;
	}
/*
	static int TestEvent ()
	{
		CI ci = null;
		ci?.ev1 += null;

		ci = new CI ();
		ci?.ev1 += null;

		return 0;
	}
*/
	static int Main ()
	{
		int res;

		res = TestProperty ();
		if (res != 0)
			return 10 + res;

		res = TestField ();
		if (res != 0)
			return 20 + res;

//		res = TestEvent ();
//		if (res != 0)
//			return 30 + res;            

		Console.WriteLine ("ok");
		return 0;
	}
}