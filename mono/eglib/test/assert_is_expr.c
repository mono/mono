/* This tests that g_assert should be an expression.
i.e. like ANSI C assert and others.

The test fails to compile with the old version and successfully
compiles with the new version.
*/

#include "../glib.h"

int main()
{
	1 || g_assert(1);

	return 0;
}
