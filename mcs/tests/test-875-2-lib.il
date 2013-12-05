.assembly extern mscorlib
{
}

.assembly extern 'test-875-lib-missing'
{
}

.assembly 'test-875-2-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'test-875-2-lib'

.class extern forwarder N.Lib
{
  .assembly extern 'test-875-lib-missing'
}

