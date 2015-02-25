.assembly extern mscorlib
{
}

.assembly extern 'test-911-lib-missing'
{
}

.assembly 'test-911-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'test-911-lib'

.class extern forwarder N
{
  .assembly extern 'test-911-lib-missing'
}

