// cs8214.cs: Generic type can not derive from Attribute class
// Line: 5
using System;

class X<T> : Attribute {
}

class D { static void Main () {}}
