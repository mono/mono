//Multiple .assembly declarations but with same name (valid)
.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'test'
{
  .hash algorithm 0x00008004
}
.assembly 'test'
{
}

