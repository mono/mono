using System; 
using System.Reflection; 
 
[AttributeUsage(AttributeTargets.Field)]
public class AccessibleAttribute:Attribute {} 
 
public class MyClass 
{ 
        [Accessible]
        public const int MyConst = 1; 
} 
 
 
public class Test 
{ 
    public static int Main() 
    { 
        FieldInfo fieldInfo = typeof(MyClass).GetField("MyConst", 
            BindingFlags.Static | BindingFlags.Public); 
 
        AccessibleAttribute[] attributes = 
          fieldInfo.GetCustomAttributes( 
            typeof(AccessibleAttribute), true) as AccessibleAttribute[]; 
        
        if (attributes != null)
        {
                Console.WriteLine ("Succeeded");
                return 0;
        }
        return 1;        
    } 
}