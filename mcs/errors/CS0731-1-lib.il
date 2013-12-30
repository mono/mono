.assembly extern mscorlib
{
}

.assembly extern 'CS0731-2-lib'
{
  .ver 0:0:0:0
}

.assembly 'CS0731-1-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0731-1-lib.dll'

.class extern forwarder A
{
  .assembly extern 'CS0731-2-lib'
}
