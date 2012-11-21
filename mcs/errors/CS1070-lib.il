.assembly extern mscorlib
{
}

.assembly extern 'CS1070-lib-missing'
{
}

.assembly 'CS1070-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS1070-lib.dll'

.class extern forwarder E
{
  .assembly extern 'CS1070-lib-missing'
}

.class extern forwarder C
{
  .assembly extern 'CS1070-lib-missing'
}

