public class BaseList<SubType>
{
        // without this field instantiation everything works fine.
        object obj = new object ();
}

public class XXX : BaseList<object>
{
}

public class Crash
{
        public static void Main()
        {
                // After creating an array, instantiation got broken.
                XXX [] arr = new XXX [0];
                // this array creation is OK
                // BaseList<object> [] arr = new BaseList<object> [0];

                new BaseList<object> (); // even this causes SISSEGV
        }
}
