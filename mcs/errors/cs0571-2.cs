// cs0571.cs: 'System.Reflection.MemberInfo.Name.get' : cannot explicitly call operator or accessor// Line: 9
using System;

public class EntryPoint {
        public static void Main () {
                Type type = typeof(string);
                Console.WriteLine (type.get_Name());
        }
}