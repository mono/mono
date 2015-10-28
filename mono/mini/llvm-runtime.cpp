#include "llvm-runtime.h"

#include <glib.h>

extern "C" {

void
mono_llvm_cpp_throw_exception (void)
{
	gint32 *ex = NULL;

	/* The generated code catches an int32* */
	throw ex;
}

}
