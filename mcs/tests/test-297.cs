using System;

[My((long)1)]
[My(TypeCode.Empty)]
[My(typeof(System.Enum))]
class T {
	public static int Main() {
		object[] a = Attribute.GetCustomAttributes (typeof (T), false);
		if (a.Length != 3)
			return 1;
		foreach (object o in a) {
			My attr = (My)o;
			if (attr.obj.GetType () == typeof (long)) {
				long val = (long) attr.obj;
				if (val != 1)
					return 2;
			} else if (attr.obj.GetType () == typeof (TypeCode)) {
				TypeCode val = (TypeCode) attr.obj;
				if (val != TypeCode.Empty)
					return 3;
			} else if (attr.obj.GetType ().IsSubclassOf (typeof (Type))) {
				Type val = (Type) attr.obj;
				if (val != typeof (System.Enum))
					return 4;
			} else
				return 5;
			
		}
                
		object[] ats = typeof(T).GetMethod("Login").GetCustomAttributes (typeof(My), true);
		My at = (My) ats[0];
                if (at.Val != AnEnum.a)
                    return 6;
                
		return 0;
	}
        
	[My(1, Val=AnEnum.a)]
	public void Login(string a)	{}        
}

[AttributeUsage(AttributeTargets.All,AllowMultiple=true)]
class My : Attribute {
	public object obj;
	public My (object o) {
		obj = o;
	}
        
	public AnEnum Val; 
}

public enum AnEnum
{
	a,b,c
}

