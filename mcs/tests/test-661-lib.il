.assembly extern mscorlib
{
}

.assembly 'test-661-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'test-661-lib.dll'

.class interface public abstract auto ansi SummaryInfo
{
	.method public hidebysig newslot specialname abstract virtual instance object get_Property(int32 Pid) cil managed
	{
	}

	.method public hidebysig newslot specialname abstract virtual instance void  set_Property(int32 Pid, object 'value') cil managed
	{
	}

	.property instance object Property(int32)
	{
		.get instance object SummaryInfo::get_Property(int32)
		.set instance void SummaryInfo::set_Property(int32,object)
	}
}
