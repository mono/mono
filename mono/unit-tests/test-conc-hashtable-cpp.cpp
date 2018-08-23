// Currently we cannot compile the file containing main as C++,
// using the -xc++ switch. The compiler applies the -xc++ switch
// to .o/.a files and fails. But we can if we rename to change
// the extension to .cpp. This is like a rename.
#include "test-conc-hashtable.c"
