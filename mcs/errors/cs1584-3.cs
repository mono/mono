// cs1584-3.cs: XML comment on `Test' has syntactically incorrect cref attribute `.'
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;

/// <see cref="." />
public class Test
{
}
