// CS3019: CLS compliance checking will not be performed on `T' because it is not visible from outside this assembly
// Line: 8
// Compiler options: -warnaserror -warn:2

using System;
[assembly:CLSCompliant(true)]

public class CLSClass<[CLSCompliant (false)] T>
{
}