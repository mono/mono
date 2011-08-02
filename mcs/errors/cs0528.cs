// CS0528:  `A' is already listed in interface list
// Line: 6
interface A {
        void stuff ();
}
class C: A, A {
        public void stuff () {}
        static void Main() {}
}

