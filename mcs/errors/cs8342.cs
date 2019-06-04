// CS8342: `S.e': Field-like instance events are not allowed in readonly structs
// Line: 6
// Compiler options: -langversion:latest

using System;

readonly struct S
{
	event Action e;
}