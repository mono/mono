// CS0249: Do not override `object.Finalize()'. Use destructor syntax instead
// Line: 5

class Sample {
        protected override void Finalize() {}
		static void Main () {}
}
