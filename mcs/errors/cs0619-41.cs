// CS0619: `Y' is obsolete: `ooo'
// Line: 6

using System;

class X : I<Y>
{
}

interface I<T>
{

}

[Obsolete("ooo", true)]
class Y
{
}
