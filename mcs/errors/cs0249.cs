// cs0249.cs: Do not override object.Finalize. Instead, provide a destructor
// Line: 5

class Sample {
        protected override void Finalize() {}
		static void Main () {}
}

