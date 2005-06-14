// Compiler options: -doc:xml-033.xml

/// <summary>help text</summary>
public class MyClass
{
   /// <summary>help text</summary>
   public static void Main()
   {
   }

   /// <summary>help text</summary>
   public static explicit operator int(MyClass f)
   {
      return 0;
   }

   /// <summary>help text</summary>
   public static implicit operator char(MyClass f)
   {
      return ' ';
   }
}

