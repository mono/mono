// CS8036: Only one part of a partial type can declare primary constructor parameters
// Line: 8
// Compiler options: -langversion:experimental

partial class Part(int arg)
{
}

partial class Part(int arg)
{
}