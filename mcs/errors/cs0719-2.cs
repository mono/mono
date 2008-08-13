// CS0719: Array elements cannot be of static type `StaticClass'
// Line: 12

using System;

static class StaticClass
{
}

class MainClass
{
	Type Type {
		get {
			return typeof (StaticClass []);
		}
	}
}
