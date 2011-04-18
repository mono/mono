// CS0577: Conditional not valid on `MainClass.implicit operator MainClass(C)' because it is a constructor, destructor, operator or explicit interface implementation
// Line: 8

class C {}

class MainClass {
        [System.Diagnostics.Conditional("DEBUG")]
        public static implicit operator MainClass (C m)
        {
            return null;
        }

		public static void Main () {}
}

