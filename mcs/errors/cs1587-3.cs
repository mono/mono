// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;
[Flags]
/// invalid comment between attributes and type declaration.
enum Foo {
}
