class T {
        static object get_obj() {
                return new object ();
        }
        static int Main() {
                object o = get_obj ();
                if (o == "string")
                        return 1;
                return 0;
        }
}
