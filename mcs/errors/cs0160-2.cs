// CS0160: A previous catch clause already catches all exceptions of this or a super type `System.Exception'
// Line: 8

class ClassMain {
        public static void Main() {
                try { }
                catch (System.Exception) { }
                catch (System.Exception) { }
        }
}
