// cs1584.cs: XML comment on 'Test' has syntactically incorrect attribute '.'
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;

/// <see cref="." />
public class Test
{
}
