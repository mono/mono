#include <stdio.h>
typedef void (*fn_ptr) (void);

void
test_reverse_pinvoke (fn_ptr p);

void
test_reverse_pinvoke (fn_ptr p)
{
	printf ("testfunc called\n");
	p ();
}
