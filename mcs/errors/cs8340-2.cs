// CS8340: `S.field': Instance fields in readonly structs must be readonly
// Line: 6
// Compiler options: -langversion:latest

readonly partial struct S
{

}

partial struct S
{
	int field;
}