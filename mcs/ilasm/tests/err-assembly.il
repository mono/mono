//Error: Multiple .assembly declarations
.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'test'
{
  .hash algorithm 0x00008004
}
.assembly 'one'
{
}
