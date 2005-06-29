// cs1580-2.cs: Invalid type for parameter `1' in XML comment cref attribute `Method(x,y)'
// Line: 7
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;
/// <see cref="Method(x,y)"/>
public class Test
{
}
