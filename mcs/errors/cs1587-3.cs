// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
// Line: 7
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;
[Flags]
/// invalid comment between attributes and type declaration.
enum Foo {
}
