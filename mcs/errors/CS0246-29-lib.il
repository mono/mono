.assembly extern mscorlib
{
}

.assembly extern 'missing'
{
  .ver 0:0:0:0
}

.assembly 'CS0246-29-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0246-29-lib.dll'

.class extern forwarder A
{
  .assembly extern 'missing'
}
.class extern B
{
  .class extern A
}