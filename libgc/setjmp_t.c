/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */
 
/* Check whether setjmp actually saves registers in jmp_buf. */
/* If it doesn't, the generic mark_regs code won't work.     */
/* Compilers vary as to whether they will put x in a 	     */
/* (callee-save) register without -O.  The code is	     */
/* contrived such that any decent compiler should put x in   */
/* a callee-save register with -O.  Thus it is is 	     */
/* recommended that this be run optimized.  (If the machine  */
/* has no callee-save registers, then the generic code is    */
/* safe, but this will not be noticed by this piece of       */
/* code.)  This test appears to be far from perfect.	     */
#include <stdio.h>
#include <setjmp.h>
#include <string.h>
#include "private/gcconfig.h"

#ifdef OS2
/* GETPAGESIZE() is set to getpagesize() by default, but that	*/
/* doesn't really exist, and the collector doesn't need it.	*/
#define INCL_DOSFILEMGR
#define INCL_DOSMISC
#define INCL_DOSERRORS
#include <os2.h>

int
getpagesize()
{
    ULONG result[1];
    
    if (DosQuerySysInfo(QSV_PAGE_SIZE, QSV_PAGE_SIZE,
    		        (void *)result, sizeof(ULONG)) != NO_ERROR) {
    	fprintf(stderr, "DosQuerySysInfo failed\n");
    	result[0] = 4096;
    }
    return((int)(result[0]));
}
#endif

struct {char a_a; char * a_b;} a;

int * nested_sp()
{
    int dummy;
    
    return(&dummy);
}

main()
{
	int dummy;
	long ps = GETPAGESIZE();
	jmp_buf b;
	register int x = (int)strlen("a");  /* 1, slightly disguised */
	static int y = 0;

	printf("This appears to be a %s running %s\n", MACH_TYPE, OS_TYPE);
	if (nested_sp() < &dummy) {
	  printf("Stack appears to grow down, which is the default.\n");
	  printf("A good guess for STACKBOTTOM on this machine is 0x%lx.\n",
	         ((unsigned long)(&dummy) + ps) & ~(ps-1));
	} else {
	  printf("Stack appears to grow up.\n");
	  printf("Define STACK_GROWS_UP in gc_private.h\n");
	  printf("A good guess for STACKBOTTOM on this machine is 0x%lx.\n",
	         ((unsigned long)(&dummy) + ps) & ~(ps-1));
	}
	printf("Note that this may vary between machines of ostensibly\n");
	printf("the same architecture (e.g. Sun 3/50s and 3/80s).\n");
	printf("On many machines the value is not fixed.\n");
	printf("A good guess for ALIGNMENT on this machine is %ld.\n",
	       (unsigned long)(&(a.a_b))-(unsigned long)(&a));
	
	/* Encourage the compiler to keep x in a callee-save register */
	x = 2*x-1;
	printf("");
	x = 2*x-1;
	setjmp(b);
	if (y == 1) {
	    if (x == 2) {
		printf("Generic mark_regs code probably wont work\n");
#		if defined(SPARC) || defined(RS6000) || defined(VAX) || defined(MIPS) || defined(M68K) || defined(I386) || defined(NS32K) || defined(RT)
		    printf("Assembly code supplied\n");
#		else
		    printf("Need assembly code\n");
#		endif
	    } else if (x == 1) {
		printf("Generic mark_regs code may work\n");
	    } else {
		printf("Very strange setjmp implementation\n");
	    }
	}
	y++;
	x = 2;
	if (y == 1) longjmp(b,1);
	return(0);
}

int g(x)
int x;
{
	return(x);
}
