// CS8341: Auto-implemented instance property `S.field' in readonly structs must be readonly
// Line: 6
// Compiler options: -langversion:latest

readonly struct S
{
	int field { get; set; }
}