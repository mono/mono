// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
// Line: 6
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

/// invalid comment on global attributes.
[assembly:System.CLSCompliant (true)]

enum Foo {
}
