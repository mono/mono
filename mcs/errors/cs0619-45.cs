// CS0619-45: `A' is obsolete: `!!!'
// Line: 9

[System.Obsolete("!!!", true)]
class A: System.Attribute
{
}

[A]
class Obsolete {
}
