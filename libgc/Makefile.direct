# This is the original manually generated Makefile.  It may still be used
# to build the collector.
#
# Primary targets:
# gc.a - builds basic library
# c++ - adds C++ interface to library
# cords - adds cords (heavyweight strings) to library
# test - prints porting information, then builds basic version of gc.a,
#      	 and runs some tests of collector and cords.  Does not add cords or
#	 c++ interface to gc.a
# cord/de - builds dumb editor based on cords.
ABI_FLAG= 
# ABI_FLAG should be the cc flag that specifies the ABI.  On most
# platforms this will be the empty string.  Possible values:
# +DD64 for 64-bit executable on HP/UX.
# -n32, -n64, -o32 for SGI/MIPS ABIs.

AS_ABI_FLAG=$(ABI_FLAG)
# ABI flag for assembler.  On HP/UX this is +A64 for 64 bit
# executables.

CC=cc $(ABI_FLAG)
CXX=g++ $(ABI_FLAG)
AS=as $(AS_ABI_FLAG)
#  The above doesn't work with gas, which doesn't run cpp.
#  Define AS as `gcc -c -x assembler-with-cpp' instead.

# Redefining srcdir allows object code for the nonPCR version of the collector
# to be generated in different directories.
srcdir= .
VPATH= $(srcdir)

CFLAGS= -O -I$(srcdir)/include -DATOMIC_UNCOLLECTABLE -DNO_SIGNALS -DNO_EXECUTE_PERMISSION -DSILENT -DALL_INTERIOR_POINTERS

# To build the parallel collector on Linux, add to the above:
# -DGC_LINUX_THREADS -DPARALLEL_MARK -DTHREAD_LOCAL_ALLOC
# To build the parallel collector in a static library on HP/UX,
# add to the above:
# -DGC_HPUX_THREADS -DPARALLEL_MARK -DTHREAD_LOCAL_ALLOC -D_POSIX_C_SOURCE=199506L -mt
# To build the thread-safe collector on Tru64, add to the above:
# -pthread -DGC_OSF1_THREADS

# HOSTCC and HOSTCFLAGS are used to build executables that will be run as
# part of the build process, i.e. on the build machine.  These will usually
# be the same as CC and CFLAGS, except in a cross-compilation environment.
# Note that HOSTCFLAGS should include any -D flags that affect thread support.
HOSTCC=$(CC)
HOSTCFLAGS=$(CFLAGS)

# For dynamic library builds, it may be necessary to add flags to generate
# PIC code, e.g. -fPIC on Linux.

# Setjmp_test may yield overly optimistic results when compiled
# without optimization.

# These define arguments influence the collector configuration:
# -DSILENT disables statistics printing, and improves performance.
# -DFIND_LEAK causes GC_find_leak to be initially set.
#   This causes the collector to assume that all inaccessible
#   objects should have been explicitly deallocated, and reports exceptions.
#   Finalization and the test program are not usable in this mode.
# -DGC_SOLARIS_THREADS enables support for Solaris (thr_) threads.
#   (Clients should also define GC_SOLARIS_THREADS and then include
#   gc.h before performing thr_ or dl* or GC_ operations.)
#   Must also define -D_REENTRANT.
# -DGC_SOLARIS_PTHREADS enables support for Solaris pthreads.
#   (Internally this define GC_SOLARIS_THREADS as well.)
# -DGC_IRIX_THREADS enables support for Irix pthreads.  See README.irix.
# -DGC_HPUX_THREADS enables support for HP/UX 11 pthreads.
#   Also requires -D_REENTRANT or -D_POSIX_C_SOURCE=199506L. See README.hp.
# -DGC_LINUX_THREADS enables support for Xavier Leroy's Linux threads.
#   see README.linux.  -D_REENTRANT may also be required.
# -DGC_OSF1_THREADS enables support for Tru64 pthreads.
# -DGC_FREEBSD_THREADS enables support for FreeBSD pthreads.
#   Appeared to run into some underlying thread problems.
# -DGC_DARWIN_THREADS enables support for Mac OS X pthreads.
# -DGC_AIX_THREADS enables support for IBM AIX threads.
# -DGC_DGUX386_THREADS enables support for DB/UX on I386 threads.
#   See README.DGUX386.
# -DGC_WIN32_THREADS enables support for win32 threads.  That makes sense
#   for this Makefile only under Cygwin.
# -DGC_THREADS should set the appropriate one of the above macros.
#   It assumes pthreads for Solaris.
# -DALL_INTERIOR_POINTERS allows all pointers to the interior
#   of objects to be recognized.  (See gc_priv.h for consequences.)
#   Alternatively, GC_all_interior_pointers can be set at process
#   initialization time.
# -DSMALL_CONFIG tries to tune the collector for small heap sizes,
#   usually causing it to use less space in such situations.
#   Incremental collection no longer works in this case.
# -DLARGE_CONFIG tunes the collector for unusually large heaps.
#   Necessary for heaps larger than about 500 MB on most machines.
#   Recommended for heaps larger than about 64 MB.
# -DDONT_ADD_BYTE_AT_END is meaningful only with -DALL_INTERIOR_POINTERS or
#   GC_all_interior_pointers = 1.  Normally -DALL_INTERIOR_POINTERS
#   causes all objects to be padded so that pointers just past the end of
#   an object can be recognized.  This can be expensive.  (The padding
#   is normally more than one byte due to alignment constraints.)
#   -DDONT_ADD_BYTE_AT_END disables the padding.
# -DNO_SIGNALS does not disable signals during critical parts of
#   the GC process.  This is no less correct than many malloc 
#   implementations, and it sometimes has a significant performance
#   impact.  However, it is dangerous for many not-quite-ANSI C
#   programs that call things like printf in asynchronous signal handlers.
#   This is on by default.  Turning it off has not been extensively tested with
#   compilers that reorder stores.  It should have been.
# -DNO_EXECUTE_PERMISSION may cause some or all of the heap to not
#   have execute permission, i.e. it may be impossible to execute
#   code from the heap.  Currently this only affects the incremental
#   collector on UNIX machines.  It may greatly improve its performance,
#   since this may avoid some expensive cache synchronization.
# -DGC_NO_OPERATOR_NEW_ARRAY declares that the C++ compiler does not support
#   the  new syntax "operator new[]" for allocating and deleting arrays.
#   See gc_cpp.h for details.  No effect on the C part of the collector.
#   This is defined implicitly in a few environments.  Must also be defined
#   by clients that use gc_cpp.h.
# -DREDIRECT_MALLOC=X causes malloc to be defined as alias for X.
#   Unless the following macros are defined, realloc is also redirected
#   to GC_realloc, and free is redirected to GC_free.
#   Calloc and strdup are redefined in terms of the new malloc.  X should
#   be either GC_malloc or GC_malloc_uncollectable, or
#   GC_debug_malloc_replacement.  (The latter invokes GC_debug_malloc
#   with dummy source location information, but still results in
#   properly remembered call stacks on Linux/X86 and Solaris/SPARC.
#   It requires that the following two macros also be used.)
#   The former is occasionally useful for working around leaks in code
#   you don't want to (or can't) look at.  It may not work for
#   existing code, but it often does.  Neither works on all platforms,
#   since some ports use malloc or calloc to obtain system memory.
#   (Probably works for UNIX, and win32.)  If you build with DBG_HDRS_ALL,
#   you should only use GC_debug_malloc_replacement as a malloc
#   replacement.
# -DREDIRECT_REALLOC=X causes GC_realloc to be redirected to X.
#   The canonical use is -DREDIRECT_REALLOC=GC_debug_realloc_replacement,
#   together with -DREDIRECT_MALLOC=GC_debug_malloc_replacement to
#   generate leak reports with call stacks for both malloc and realloc.
#   This also requires the following:
# -DREDIRECT_FREE=X causes free to be redirected to X.  The
#   canonical use is -DREDIRECT_FREE=GC_debug_free.
# -DIGNORE_FREE turns calls to free into a noop.  Only useful with
#   -DREDIRECT_MALLOC.
# -DNO_DEBUGGING removes GC_dump and the debugging routines it calls.
#   Reduces code size slightly at the expense of debuggability.
# -DJAVA_FINALIZATION makes it somewhat safer to finalize objects out of
#   order by specifying a nonstandard finalization mark procedure  (see
#   finalize.c).  Objects reachable from finalizable objects will be marked
#   in a sepearte postpass, and hence their memory won't be reclaimed.
#   Not recommended unless you are implementing a language that specifies
#   these semantics.  Since 5.0, determines only only the initial value
#   of GC_java_finalization variable.
# -DFINALIZE_ON_DEMAND causes finalizers to be run only in response
#   to explicit GC_invoke_finalizers() calls.
#   In 5.0 this became runtime adjustable, and this only determines the
#   initial value of GC_finalize_on_demand.
# -DATOMIC_UNCOLLECTABLE includes code for GC_malloc_atomic_uncollectable.
#   This is useful if either the vendor malloc implementation is poor,
#   or if REDIRECT_MALLOC is used.
# -DHBLKSIZE=ddd, where ddd is a power of 2 between 512 and 16384, explicitly
#   sets the heap block size.  Each heap block is devoted to a single size and
#   kind of object.  For the incremental collector it makes sense to match
#   the most likely page size.  Otherwise large values result in more
#   fragmentation, but generally better performance for large heaps.
# -DUSE_MMAP use MMAP instead of sbrk to get new memory.
#   Works for Solaris and Irix.
# -DUSE_MUNMAP causes memory to be returned to the OS under the right
#   circumstances.  This currently disables VM-based incremental collection.
#   This is currently experimental, and works only under some Unix,
#   Linux and Windows versions.
# -DMMAP_STACKS (for Solaris threads) Use mmap from /dev/zero rather than
#   GC_scratch_alloc() to get stack memory.
# -DPRINT_BLACK_LIST Whenever a black list entry is added, i.e. whenever
#   the garbage collector detects a value that looks almost, but not quite,
#   like a pointer, print both the address containing the value, and the
#   value of the near-bogus-pointer.  Can be used to identifiy regions of
#   memory that are likely to contribute misidentified pointers.
# -DKEEP_BACK_PTRS Add code to save back pointers in debugging headers
#   for objects allocated with the debugging allocator.  If all objects
#   through GC_MALLOC with GC_DEBUG defined, this allows the client
#   to determine how particular or randomly chosen objects are reachable
#   for debugging/profiling purposes.  The gc_backptr.h interface is
#   implemented only if this is defined.
# -DGC_ASSERTIONS Enable some internal GC assertion checking.  Currently
#   this facility is only used in a few places.  It is intended primarily
#   for debugging of the garbage collector itself, but could also
# -DDBG_HDRS_ALL Make sure that all objects have debug headers.  Increases
#   the reliability (from 99.9999% to 100% mod. bugs) of some of the debugging
#   code (especially KEEP_BACK_PTRS).  Makes -DSHORT_DBG_HDRS possible.
#   Assumes that all client allocation is done through debugging
#   allocators.
# -DSHORT_DBG_HDRS Assume that all objects have debug headers.  Shorten
#   the headers to minimize object size, at the expense of checking for
#   writes past the end of an object.  This is intended for environments
#   in which most client code is written in a "safe" language, such as
#   Scheme or Java.  Assumes that all client allocation is done using
#   the GC_debug_ functions, or through the macros that expand to these,
#   or by redirecting malloc to GC_debug_malloc_replacement.
#   (Also eliminates the field for the requested object size.)
#   occasionally be useful for debugging of client code.  Slows down the
#   collector somewhat, but not drastically.
# -DSAVE_CALL_COUNT=<n> Set the number of call frames saved with objects
#   allocated through the debugging interface.  Affects the amount of
#   information generated in leak reports.  Only matters on platforms
#   on which we can quickly generate call stacks, currently Linux/(X86 & SPARC)
#   and Solaris/SPARC and platforms that provide execinfo.h.
#   Default is zero.  On X86, client
#   code should NOT be compiled with -fomit-frame-pointer.
# -DSAVE_CALL_NARGS=<n> Set the number of functions arguments to be
#   saved with each call frame.  Default is zero.  Ignored if we
#   don't know how to retrieve arguments on the platform.
# -DCHECKSUMS reports on erroneously clear dirty bits, and unexpectedly
#   altered stubborn objects, at substantial performance cost.
#   Use only for debugging of the incremental collector.
# -DGC_GCJ_SUPPORT includes support for gcj (and possibly other systems
#   that include a pointer to a type descriptor in each allocated object).
#   Building this way requires an ANSI C compiler.
# -DUSE_I686_PREFETCH causes the collector to issue Pentium III style
#   prefetch instructions.  No effect except on X86 Linux platforms.
#   Assumes a very recent gcc-compatible compiler and assembler.
#   (Gas prefetcht0 support was added around May 1999.)
#   Empirically the code appears to still run correctly on Pentium II
#   processors, though with no performance benefit.  May not run on other
#   X86 processors?  In some cases this improves performance by
#   15% or so.
# -DUSE_3DNOW_PREFETCH causes the collector to issue AMD 3DNow style
#   prefetch instructions.  Same restrictions as USE_I686_PREFETCH.
#   Minimally tested.  Didn't appear to be an obvious win on a K6-2/500.
# -DUSE_PPC_PREFETCH causes the collector to issue PowerPC style
#   prefetch instructions.  No effect except on PowerPC OS X platforms.
#   Performance impact untested.
# -DGC_USE_LD_WRAP in combination with the old flags listed in README.linux
#   causes the collector some system and pthread calls in a more transparent
#   fashion than the usual macro-based approach.  Requires GNU ld, and
#   currently probably works only with Linux.
# -DTHREAD_LOCAL_ALLOC defines GC_local_malloc(), GC_local_malloc_atomic()
#   and GC_local_gcj_malloc().  Needed for gc_gcj.h interface.  These allocate
#   in a way that usually does not involve acquisition of a global lock.
#   Currently works only on platforms such as Linux which use pthread_support.c.
#   Recommended for multiprocessors.
# -DUSE_COMPILER_TLS causes thread local allocation to use compiler-supported
#   "__thread" thread-local variables.  This is the default in HP/UX.  It
#   may help performance on recent Linux installations.  (It failed for
#   me on RedHat 8, but appears to work on RedHat 9.)
# -DPARALLEL_MARK allows the marker to run in multiple threads.  Recommended
#   for multiprocessors.  Currently requires Linux on X86 or IA64, though
#   support for other Posix platforms should be fairly easy to add,
#   if the thread implementation is otherwise supported.
# -DNO_GETENV prevents the collector from looking at environment variables.
#   These may otherwise alter its configuration, or turn off GC altogether.
#   I don't know of a reason to disable this, except possibly if the
#   resulting process runs as a privileged user?
# -DUSE_GLOBAL_ALLOC.  Win32 only.  Use GlobalAlloc instead of
#   VirtualAlloc to allocate the heap.  May be needed to work around
#   a Windows NT/2000 issue.  Incompatible with USE_MUNMAP.
#   See README.win32 for details.
# -DMAKE_BACK_GRAPH. Enable GC_PRINT_BACK_HEIGHT environment variable.
#   See README.environment for details.  Experimental. Limited platform
#   support.  Implies DBG_HDRS_ALL.  All allocation should be done using
#   the debug interface.
# -DSTUBBORN_ALLOC allows allocation of "hard to change" objects, and thus
#   makes incremental collection easier.  Was enabled by default until 6.0.
#   Rarely used, to my knowledge.
# -DHANDLE_FORK attempts to make GC_malloc() work in a child process fork()ed
#   from a multithreaded parent.  Currently only supported by pthread_support.c.
#   (Similar code should work on Solaris or Irix, but it hasn't been tried.)
# -DTEST_WITH_SYSTEM_MALLOC causes gctest to allocate (and leak) large chunks
#   of memory with the standard system malloc.  This will cause the root
#   set and collected heap to grow significantly if malloced memory is
#   somehow getting traced by the collector.  This has no impact on the
#   generated library; it only affects the test.
# -DPOINTER_MASK=0x... causes candidate pointers to be ANDed with the
#   given mask before being considered.  If either this or the following
#   macro is defined, it will be assumed that all pointers stored in
#   the heap need to be processed this way.  Stack and register pointers
#   will be considered both with and without processing.
#   These macros are normally needed only to support systems that use
#   high-order pointer tags. EXPERIMENTAL.
# -DPOINTER_SHIFT=n causes the collector to left shift candidate pointers
#   by the indicated amount before trying to interpret them.  Applied
#   after POINTER_MASK. EXPERIMENTAL.  See also the preceding macro.
# -DDARWIN_DONT_PARSE_STACK Causes the Darwin port to discover thread
#   stack bounds in the same way as other pthread ports, without trying to
#   walk the frames onthe stack.  This is recommended only as a fallback
#   for applications that don't support proper stack unwinding.
#

CXXFLAGS= $(CFLAGS) 
AR= ar
RANLIB= ranlib


OBJS= alloc.o reclaim.o allchblk.o misc.o mach_dep.o os_dep.o mark_rts.o headers.o mark.o obj_map.o blacklst.o finalize.o new_hblk.o dbg_mlc.o malloc.o stubborn.o checksums.o solaris_threads.o pthread_support.o pthread_stop_world.o darwin_stop_world.o typd_mlc.o ptr_chck.o mallocx.o solaris_pthreads.o gcj_mlc.o specific.o gc_dlopen.o backgraph.o win32_threads.o

CSRCS= reclaim.c allchblk.c misc.c alloc.c mach_dep.c os_dep.c mark_rts.c headers.c mark.c obj_map.c pcr_interface.c blacklst.c finalize.c new_hblk.c real_malloc.c dyn_load.c dbg_mlc.c malloc.c stubborn.c checksums.c solaris_threads.c pthread_support.c pthread_stop_world.c darwin_stop_world.c typd_mlc.c ptr_chck.c mallocx.c solaris_pthreads.c gcj_mlc.c specific.c gc_dlopen.c backgraph.c win32_threads.c

CORD_SRCS=  cord/cordbscs.c cord/cordxtra.c cord/cordprnt.c cord/de.c cord/cordtest.c include/cord.h include/ec.h include/private/cord_pos.h cord/de_win.c cord/de_win.h cord/de_cmds.h cord/de_win.ICO cord/de_win.RC

CORD_OBJS=  cord/cordbscs.o cord/cordxtra.o cord/cordprnt.o

SRCS= $(CSRCS) mips_sgi_mach_dep.s rs6000_mach_dep.s alpha_mach_dep.S \
    sparc_mach_dep.S include/gc.h include/gc_typed.h \
    include/private/gc_hdrs.h include/private/gc_priv.h \
    include/private/gcconfig.h include/private/gc_pmark.h \
    include/gc_inl.h include/gc_inline.h include/gc_mark.h \
    threadlibs.c if_mach.c if_not_there.c gc_cpp.cc include/gc_cpp.h \
    gcname.c include/weakpointer.h include/private/gc_locks.h \
    gcc_support.c mips_ultrix_mach_dep.s include/gc_alloc.h \
    include/new_gc_alloc.h include/gc_allocator.h \
    include/javaxfc.h sparc_sunos4_mach_dep.s sparc_netbsd_mach_dep.s \
    include/private/solaris_threads.h include/gc_backptr.h \
    hpux_test_and_clear.s include/gc_gcj.h \
    include/gc_local_alloc.h include/private/dbg_mlc.h \
    include/private/specific.h powerpc_darwin_mach_dep.s \
    include/leak_detector.h include/gc_amiga_redirects.h \
    include/gc_pthread_redirects.h ia64_save_regs_in_stack.s \
    include/gc_config_macros.h include/private/pthread_support.h \
    include/private/pthread_stop_world.h include/private/darwin_semaphore.h \
    include/private/darwin_stop_world.h $(CORD_SRCS)

DOC_FILES= README.QUICK doc/README.Mac doc/README.MacOSX doc/README.OS2 \
	doc/README.amiga doc/README.cords doc/debugging.html \
	doc/README.dj doc/README.hp doc/README.linux doc/README.rs6000 \
	doc/README.sgi doc/README.solaris2 doc/README.uts \
	doc/README.win32 doc/barrett_diagram doc/README \
        doc/README.contributors doc/README.changes doc/gc.man \
	doc/README.environment doc/tree.html doc/gcdescr.html \
	doc/README.autoconf doc/README.macros doc/README.ews4800 \
	doc/README.DGUX386 doc/README.arm.cross doc/leak.html \
	doc/scale.html doc/gcinterface.html doc/README.darwin \
	doc/simple_example.html

TESTS= tests/test.c tests/test_cpp.cc tests/trace_test.c \
	tests/leak_test.c tests/thread_leak_test.c tests/middle.c

GNU_BUILD_FILES= configure.in Makefile.am configure acinclude.m4 \
		 libtool.m4 install-sh configure.host Makefile.in \
		 aclocal.m4 config.sub config.guess \
		 include/Makefile.am include/Makefile.in \
		 doc/Makefile.am doc/Makefile.in \
		 ltmain.sh mkinstalldirs depcomp missing

OTHER_MAKEFILES= OS2_MAKEFILE NT_MAKEFILE NT_THREADS_MAKEFILE gc.mak \
		 BCC_MAKEFILE EMX_MAKEFILE WCC_MAKEFILE Makefile.dj \
		 PCR-Makefile SMakefile.amiga Makefile.DLLs \
		 digimars.mak Makefile.direct NT_STATIC_THREADS_MAKEFILE
#	Makefile and Makefile.direct are copies of each other.

OTHER_FILES= Makefile setjmp_t.c callprocs pc_excludes \
           MacProjects.sit.hqx MacOS.c \
           Mac_files/datastart.c Mac_files/dataend.c \
           Mac_files/MacOS_config.h Mac_files/MacOS_Test_config.h \
           add_gc_prefix.c gc_cpp.cpp \
	   version.h AmigaOS.c \
	   $(TESTS) $(GNU_BUILD_FILES) $(OTHER_MAKEFILES)

CORD_INCLUDE_FILES= $(srcdir)/include/gc.h $(srcdir)/include/cord.h \
	$(srcdir)/include/ec.h $(srcdir)/include/private/cord_pos.h

UTILS= if_mach if_not_there threadlibs

# Libraries needed for curses applications.  Only needed for de.
CURSES= -lcurses -ltermlib

# The following is irrelevant on most systems.  But a few
# versions of make otherwise fork the shell specified in
# the SHELL environment variable.
SHELL= /bin/sh

SPECIALCFLAGS = -I$(srcdir)/include
# Alternative flags to the C compiler for mach_dep.c.
# Mach_dep.c often doesn't like optimization, and it's
# not time-critical anyway.
# Set SPECIALCFLAGS to -q nodirect_code on Encore.

all: gc.a gctest

LEAKFLAGS=$(CFLAGS) -DFIND_LEAK

BSD-pkg-all: bsd-libgc.a bsd-libleak.a

bsd-libgc.a:
	$(MAKE) CFLAGS="$(CFLAGS)" clean c++-t
	mv gc.a bsd-libgc.a

bsd-libleak.a:
	$(MAKE) -f Makefile.direct CFLAGS="$(LEAKFLAGS)" clean c++-nt
	mv gc.a bsd-libleak.a

BSD-pkg-install: BSD-pkg-all
	${CP} bsd-libgc.a libgc.a
	${INSTALL_DATA} libgc.a ${PREFIX}/lib
	${INSTALL_DATA} gc.h gc_cpp.h ${PREFIX}/include
	${INSTALL_MAN} doc/gc.man ${PREFIX}/man/man3/gc.3

pcr: PCR-Makefile include/private/gc_private.h include/private/gc_hdrs.h \
include/private/gc_locks.h include/gc.h include/private/gcconfig.h \
mach_dep.o $(SRCS)
	$(MAKE) -f PCR-Makefile depend
	$(MAKE) -f PCR-Makefile

$(OBJS) tests/test.o dyn_load.o dyn_load_sunos53.o: \
    $(srcdir)/include/private/gc_priv.h \
    $(srcdir)/include/private/gc_hdrs.h $(srcdir)/include/private/gc_locks.h \
    $(srcdir)/include/gc.h $(srcdir)/include/gc_pthread_redirects.h \
    $(srcdir)/include/private/gcconfig.h $(srcdir)/include/gc_typed.h \
    $(srcdir)/include/gc_config_macros.h Makefile
# The dependency on Makefile is needed.  Changing
# options such as -DSILENT affects the size of GC_arrays,
# invalidating all .o files that rely on gc_priv.h

mark.o typd_mlc.o finalize.o ptr_chck.o: $(srcdir)/include/gc_mark.h $(srcdir)/include/private/gc_pmark.h

specific.o pthread_support.o: $(srcdir)/include/private/specific.h

solaris_threads.o solaris_pthreads.o: $(srcdir)/include/private/solaris_threads.h

dbg_mlc.o gcj_mlc.o: $(srcdir)/include/private/dbg_mlc.h

tests/test.o: tests $(srcdir)/tests/test.c
	$(CC) $(CFLAGS) -c $(srcdir)/tests/test.c
	mv test.o tests/test.o

tests:
	mkdir tests

base_lib gc.a: $(OBJS) dyn_load.o $(UTILS)
	echo > base_lib
	rm -f dont_ar_1
	./if_mach SPARC SUNOS5 touch dont_ar_1
	./if_mach SPARC SUNOS5 $(AR) rus gc.a $(OBJS) dyn_load.o
	./if_mach M68K AMIGA touch dont_ar_1
	./if_mach M68K AMIGA $(AR) -vrus gc.a $(OBJS) dyn_load.o
	./if_not_there dont_ar_1 $(AR) ru gc.a $(OBJS) dyn_load.o
	./if_not_there dont_ar_1 $(RANLIB) gc.a || cat /dev/null
#	ignore ranlib failure; that usually means it doesn't exist, and isn't needed

cords: $(CORD_OBJS) cord/cordtest $(UTILS)
	rm -f dont_ar_3
	./if_mach SPARC SUNOS5 touch dont_ar_3
	./if_mach SPARC SUNOS5 $(AR) rus gc.a $(CORD_OBJS)
	./if_mach M68K AMIGA touch dont_ar_3
	./if_mach M68K AMIGA $(AR) -vrus gc.a $(CORD_OBJS)
	./if_not_there dont_ar_3 $(AR) ru gc.a $(CORD_OBJS)
	./if_not_there dont_ar_3 $(RANLIB) gc.a || cat /dev/null

gc_cpp.o: $(srcdir)/gc_cpp.cc $(srcdir)/include/gc_cpp.h $(srcdir)/include/gc.h Makefile
	$(CXX) -c $(CXXFLAGS) $(srcdir)/gc_cpp.cc

test_cpp: $(srcdir)/tests/test_cpp.cc $(srcdir)/include/gc_cpp.h gc_cpp.o $(srcdir)/include/gc.h \
base_lib $(UTILS)
	rm -f test_cpp
	./if_mach HP_PA HPUX $(CXX) $(CXXFLAGS) -o test_cpp $(srcdir)/tests/test_cpp.cc gc_cpp.o gc.a -ldld `./threadlibs`
	./if_not_there test_cpp $(CXX) $(CXXFLAGS) -o test_cpp $(srcdir)/tests/test_cpp.cc gc_cpp.o gc.a `./threadlibs`

c++-t: c++
	./test_cpp 1

c++-nt: c++
	@echo "Use ./test_cpp 1 to test the leak library"

c++: gc_cpp.o $(srcdir)/include/gc_cpp.h test_cpp
	rm -f dont_ar_4
	./if_mach SPARC SUNOS5 touch dont_ar_4
	./if_mach SPARC SUNOS5 $(AR) rus gc.a gc_cpp.o
	./if_mach M68K AMIGA touch dont_ar_4
	./if_mach M68K AMIGA $(AR) -vrus gc.a gc_cpp.o
	./if_not_there dont_ar_4 $(AR) ru gc.a gc_cpp.o
	./if_not_there dont_ar_4 $(RANLIB) gc.a || cat /dev/null
	./test_cpp 1
	echo > c++

dyn_load_sunos53.o: dyn_load.c
	$(CC) $(CFLAGS) -DSUNOS53_SHARED_LIB -c $(srcdir)/dyn_load.c -o $@

# SunOS5 shared library version of the collector
sunos5gc.so: $(OBJS) dyn_load_sunos53.o
	$(CC) -G -o sunos5gc.so $(OBJS) dyn_load_sunos53.o -ldl
	ln sunos5gc.so libgc.so

# Alpha/OSF shared library version of the collector
libalphagc.so: $(OBJS)
	ld -shared -o libalphagc.so $(OBJS) dyn_load.o -lc
	ln libalphagc.so libgc.so

# IRIX shared library version of the collector
libirixgc.so: $(OBJS) dyn_load.o
	ld -shared $(ABI_FLAG) -o libirixgc.so $(OBJS) dyn_load.o -lc
	ln libirixgc.so libgc.so

# Linux shared library version of the collector
liblinuxgc.so: $(OBJS) dyn_load.o
	gcc -shared -o liblinuxgc.so $(OBJS) dyn_load.o
	ln liblinuxgc.so libgc.so

# Alternative Linux rule.  This is preferable, but is likely to break the
# Makefile for some non-linux platforms.
# LIBOBJS= $(patsubst %.o, %.lo, $(OBJS))
#
#.SUFFIXES: .lo $(SUFFIXES)
#
#.c.lo:
#	$(CC) $(CFLAGS) $(CPPFLAGS) -fPIC -c $< -o $@
#
# liblinuxgc.so: $(LIBOBJS) dyn_load.lo
# 	gcc -shared -Wl,-soname=libgc.so.0 -o libgc.so.0 $(LIBOBJS) dyn_load.lo
#	touch liblinuxgc.so

mach_dep.o: $(srcdir)/mach_dep.c $(srcdir)/mips_sgi_mach_dep.s \
	    $(srcdir)/mips_ultrix_mach_dep.s \
            $(srcdir)/rs6000_mach_dep.s $(srcdir)/powerpc_darwin_mach_dep.s \
	    $(srcdir)/sparc_mach_dep.S $(srcdir)/sparc_sunos4_mach_dep.s \
	    $(srcdir)/ia64_save_regs_in_stack.s \
	    $(srcdir)/sparc_netbsd_mach_dep.s $(UTILS)
	rm -f mach_dep.o
	./if_mach MIPS IRIX5 $(CC) -c -o mach_dep.o $(srcdir)/mips_sgi_mach_dep.s
	./if_mach MIPS RISCOS $(AS) -o mach_dep.o $(srcdir)/mips_ultrix_mach_dep.s
	./if_mach MIPS ULTRIX $(AS) -o mach_dep.o $(srcdir)/mips_ultrix_mach_dep.s
	./if_mach POWERPC DARWIN $(AS) -o mach_dep.o $(srcdir)/powerpc_darwin_mach_dep.s
	./if_mach ALPHA LINUX $(CC) -c -o mach_dep.o $(srcdir)/alpha_mach_dep.S
	./if_mach SPARC SUNOS5 $(CC) -c -o mach_dep.o $(srcdir)/sparc_mach_dep.S
	./if_mach SPARC SUNOS4 $(AS) -o mach_dep.o $(srcdir)/sparc_sunos4_mach_dep.s
	./if_mach SPARC OPENBSD $(AS) -o mach_dep.o $(srcdir)/sparc_sunos4_mach_dep.s
	./if_mach SPARC NETBSD $(AS) -o mach_dep.o $(srcdir)/sparc_netbsd_mach_dep.s
	./if_mach IA64 "" as $(AS_ABI_FLAG) -o ia64_save_regs_in_stack.o $(srcdir)/ia64_save_regs_in_stack.s
	./if_mach IA64 "" $(CC) -c -o mach_dep1.o $(SPECIALCFLAGS) $(srcdir)/mach_dep.c
	./if_mach IA64 "" ld -r -o mach_dep.o mach_dep1.o ia64_save_regs_in_stack.o
	./if_not_there mach_dep.o $(CC) -c $(SPECIALCFLAGS) $(srcdir)/mach_dep.c

mark_rts.o: $(srcdir)/mark_rts.c $(UTILS)
	rm -f mark_rts.o
	-./if_mach ALPHA OSF1 $(CC) -c $(CFLAGS) -Wo,-notail $(srcdir)/mark_rts.c
	./if_not_there mark_rts.o $(CC) -c $(CFLAGS) $(srcdir)/mark_rts.c
#	Work-around for DEC optimizer tail recursion elimination bug.
#  The ALPHA-specific line should be removed if gcc is used.

alloc.o: version.h

cord:
	mkdir cord

cord/cordbscs.o: cord $(srcdir)/cord/cordbscs.c $(CORD_INCLUDE_FILES)
	$(CC) $(CFLAGS) -c -I$(srcdir) $(srcdir)/cord/cordbscs.c
	mv cordbscs.o cord/cordbscs.o
#  not all compilers understand -o filename

cord/cordxtra.o: cord $(srcdir)/cord/cordxtra.c $(CORD_INCLUDE_FILES)
	$(CC) $(CFLAGS) -c -I$(srcdir) $(srcdir)/cord/cordxtra.c
	mv cordxtra.o cord/cordxtra.o

cord/cordprnt.o: cord $(srcdir)/cord/cordprnt.c $(CORD_INCLUDE_FILES)
	$(CC) $(CFLAGS) -c -I$(srcdir) $(srcdir)/cord/cordprnt.c
	mv cordprnt.o cord/cordprnt.o

cord/cordtest: $(srcdir)/cord/cordtest.c $(CORD_OBJS) gc.a $(UTILS)
	rm -f cord/cordtest
	./if_mach SPARC DRSNX $(CC) $(CFLAGS) -o cord/cordtest $(srcdir)/cord/cordtest.c $(CORD_OBJS) gc.a -lucb
	./if_mach HP_PA HPUX $(CC) $(CFLAGS) -o cord/cordtest $(srcdir)/cord/cordtest.c $(CORD_OBJS) gc.a -ldld `./threadlibs`
	./if_mach M68K AMIGA $(CC) $(CFLAGS) -UGC_AMIGA_MAKINGLIB -o cord/cordtest $(srcdir)/cord/cordtest.c $(CORD_OBJS) gc.a `./threadlibs`
	./if_not_there cord/cordtest $(CC) $(CFLAGS) -o cord/cordtest $(srcdir)/cord/cordtest.c $(CORD_OBJS) gc.a `./threadlibs`

cord/de: $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a $(UTILS)
	rm -f cord/de
	./if_mach SPARC DRSNX $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a $(CURSES) -lucb `./threadlibs`
	./if_mach HP_PA HPUX $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a $(CURSES) -ldld `./threadlibs`
	./if_mach RS6000 "" $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a -lcurses
	./if_mach POWERPC DARWIN $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a
	./if_mach I386 LINUX $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a -lcurses `./threadlibs`
	./if_mach ALPHA LINUX $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a -lcurses `./threadlibs`
	./if_mach IA64 LINUX $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a -lcurses `./threadlibs`
	./if_mach M68K AMIGA $(CC) $(CFLAGS) -UGC_AMIGA_MAKINGLIB -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a -lcurses
	./if_not_there cord/de $(CC) $(CFLAGS) -o cord/de $(srcdir)/cord/de.c cord/cordbscs.o cord/cordxtra.o gc.a $(CURSES) `./threadlibs`

if_mach: $(srcdir)/if_mach.c $(srcdir)/include/private/gcconfig.h
	$(HOSTCC) $(HOSTCFLAGS) -o if_mach $(srcdir)/if_mach.c

threadlibs: $(srcdir)/threadlibs.c $(srcdir)/include/private/gcconfig.h Makefile
	$(HOSTCC) $(HOSTCFLAGS) -o threadlibs $(srcdir)/threadlibs.c

if_not_there: $(srcdir)/if_not_there.c
	$(HOSTCC) $(HOSTCFLAGS) -o if_not_there $(srcdir)/if_not_there.c

clean: 
	rm -f gc.a *.o *.exe tests/*.o gctest gctest_dyn_link test_cpp \
	      setjmp_test  mon.out gmon.out a.out core if_not_there if_mach \
	      threadlibs $(CORD_OBJS) cord/cordtest cord/de 
	-rm -f *~

gctest: tests/test.o gc.a $(UTILS)
	rm -f gctest
	./if_mach SPARC DRSNX $(CC) $(CFLAGS) -o gctest  tests/test.o gc.a -lucb
	./if_mach HP_PA HPUX $(CC) $(CFLAGS) -o gctest  tests/test.o gc.a -ldld `./threadlibs`
	./if_mach M68K AMIGA $(CC) $(CFLAGS) -UGC_AMIGA_MAKINGLIB -o gctest  tests/test.o gc.a `./threadlibs`
	./if_not_there gctest $(CC) $(CFLAGS) -o gctest tests/test.o gc.a `./threadlibs`

# If an optimized setjmp_test generates a segmentation fault,
# odds are your compiler is broken.  Gctest may still work.
# Try compiling setjmp_t.c unoptimized.
setjmp_test: $(srcdir)/setjmp_t.c $(srcdir)/include/gc.h $(UTILS)
	$(CC) $(CFLAGS) -o setjmp_test $(srcdir)/setjmp_t.c

test:  KandRtest cord/cordtest
	cord/cordtest

# Those tests that work even with a K&R C compiler:
KandRtest: setjmp_test gctest
	./setjmp_test
	./gctest

add_gc_prefix: $(srcdir)/add_gc_prefix.c $(srcdir)/version.h
	$(CC) -o add_gc_prefix $(srcdir)/add_gc_prefix.c

gcname: $(srcdir)/gcname.c $(srcdir)/version.h
	$(CC) -o gcname $(srcdir)/gcname.c

gc.tar: $(SRCS) $(DOC_FILES) $(OTHER_FILES) add_gc_prefix gcname
	cp Makefile Makefile.old
	cp Makefile.direct Makefile
	rm -f `./gcname`
	ln -s . `./gcname`
	./add_gc_prefix $(SRCS) $(DOC_FILES) $(OTHER_FILES) > /tmp/gc.tar-files
	tar cvfh gc.tar `cat /tmp/gc.tar-files`
	cp gc.tar `./gcname`.tar
	gzip `./gcname`.tar
	rm `./gcname`

pc_gc.tar: $(SRCS) $(OTHER_FILES)
	tar cvfX pc_gc.tar pc_excludes $(SRCS) $(OTHER_FILES)

floppy: pc_gc.tar
	-mmd a:/cord
	-mmd a:/cord/private
	-mmd a:/include
	-mmd a:/include/private
	mkdir /tmp/pc_gc
	cat pc_gc.tar | (cd /tmp/pc_gc; tar xvf -)
	-mcopy -tmn /tmp/pc_gc/* a:
	-mcopy -tmn /tmp/pc_gc/cord/* a:/cord
	-mcopy -mn /tmp/pc_gc/cord/de_win.ICO a:/cord
	-mcopy -tmn /tmp/pc_gc/cord/private/* a:/cord/private
	-mcopy -tmn /tmp/pc_gc/include/* a:/include
	-mcopy -tmn /tmp/pc_gc/include/private/* a:/include/private
	rm -r /tmp/pc_gc

gc.tar.Z: gc.tar
	compress gc.tar

gc.tar.gz: gc.tar
	gzip gc.tar

lint: $(CSRCS) tests/test.c
	lint -DLINT $(CSRCS) tests/test.c | egrep -v "possible pointer alignment problem|abort|exit|sbrk|mprotect|syscall|change in ANSI|improper alignment"

# BTL: added to test shared library version of collector.
# Currently works only under SunOS5.  Requires GC_INIT call from statically
# loaded client code.
ABSDIR = `pwd`
gctest_dyn_link: tests/test.o libgc.so
	$(CC) -L$(ABSDIR) -R$(ABSDIR) -o gctest_dyn_link tests/test.o -lgc -ldl -lthread

gctest_irix_dyn_link: tests/test.o libirixgc.so
	$(CC) -L$(ABSDIR) -o gctest_irix_dyn_link tests/test.o -lirixgc

# The following appear to be dead, especially since libgc_globals.h
# is apparently lost.
test_dll.o: tests/test.c libgc_globals.h
	$(CC) $(CFLAGS) -DGC_USE_DLL -c tests/test.c -o test_dll.o

test_dll: test_dll.o libgc_dll.a libgc.dll
	$(CC) test_dll.o -L$(ABSDIR) -lgc_dll -o test_dll

SYM_PREFIX-libgc=GC

# Uncomment the following line to build a GNU win32 DLL
# include Makefile.DLLs

reserved_namespace: $(SRCS)
	for file in $(SRCS) tests/test.c tests/test_cpp.cc; do \
		sed s/GC_/_GC_/g < $$file > tmp; \
		cp tmp $$file; \
		done

user_namespace: $(SRCS)
	for file in $(SRCS) tests/test.c tests/test_cpp.cc; do \
		sed s/_GC_/GC_/g < $$file > tmp; \
		cp tmp $$file; \
		done
