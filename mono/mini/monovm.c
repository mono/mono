#include <config.h>
#include <mono/utils/mono-compiler.h>
#include "monovm.h"


int
monovm_initialize (int propertyCount, const char **propertyKeys, const char **propertyValues)
{
	return -1;
}

int
monovm_execute_assembly (int argc, const char **argv, const char *managedAssemblyPath, unsigned int *exitCode)
{
	return -1;
}

int
monovm_shutdown (int *latchedExitCode)
{
	return -1;
}

