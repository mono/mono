//
// from bug 80257
// 

public delegate object Get (Do d);



public class Do {
        public void Register (Get g)
        {
        }

        public void Register (object g)
        {
        }

        static object MyGet (Do d)
        {
                return null;
        }

        public void X ()
        {
                Register (Do.MyGet);
        }
}

public class User {
        public static void Main ()
        {
        }
}

