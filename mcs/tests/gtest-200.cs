class Test
{

 public static T QueryInterface<T>(object val)
   where T : class
 {
     if (val == null)
         return null;

     // First, see if the given object can be directly cast
     // to the requested type.  This will be a common case,
     // especially when checking for standard behavior interface
     // implementations (like IXrcDataElement).
     T tval = val as T;
     if (tval != null)
         return tval;

     // ... rest of method unimportant and omitted ...
     return null;
 }
}

class Driver
{
 public static void Main () {}
}

