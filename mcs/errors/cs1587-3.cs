// CS1587: XML comment is not placed on a valid language element
// Line: 7
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;
[Flags]
/// invalid comment between attributes and type declaration.
enum Foo {
}
