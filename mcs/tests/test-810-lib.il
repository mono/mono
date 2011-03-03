.assembly extern mscorlib
{
}

.assembly 'test-810-lib'
{
}

.module 'test-810-lib.dll'

.class interface public abstract auto ansi IA
{
}

.class interface public abstract auto ansi IB
       implements IA
{
}

.class interface public abstract auto ansi IC
       implements IB
{
}
