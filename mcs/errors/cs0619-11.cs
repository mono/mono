// CS0619: `I' is obsolete: `Do not use it.'
// Line: 15

using System;

[Obsolete("Do not use it.", true)]
interface I
{
}

class A
{
}

class C: A, I
{
}