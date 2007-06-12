// CS0179: `Bar.Foo.set' cannot declare a body because it is marked extern
// Line: 5

class Bar {
	extern int Foo {
		set { }
		get { }
	}
}

