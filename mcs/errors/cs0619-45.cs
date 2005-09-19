// cs0619-45.cs: `A' is obsolete: `!!!'
// Line: 9

[System.Obsolete("!!!", true)]
class A: System.Attribute
{
}

[A]
class Obsolete {
}
