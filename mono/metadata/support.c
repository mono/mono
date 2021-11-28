
#include "utils/mono-compiler.h"

#if defined(ENABLE_MONOTOUCH)
#include "../../support/zlib-helper.c"
#elif defined(ENABLE_MONODROID)
#include "../../support/nl.c"
#include "../../support/zlib-helper.c"
#else
MONO_EMPTY_SOURCE_FILE(empty);
#endif
