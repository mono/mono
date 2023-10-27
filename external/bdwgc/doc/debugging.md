# Debugging Garbage Collector Related Problems

This page contains some hints on debugging issues specific to the
Boehm-Demers-Weiser conservative garbage collector. It applies both
to debugging issues in client code that manifest themselves as collector
misbehavior, and to debugging the collector itself.

If you suspect a bug in the collector itself, it is strongly recommended that
you try the latest collector release before proceeding.

## Bus Errors and Segmentation Violations

If the fault occurred in `GC_find_limit`, or with incremental collection
enabled, this is probably normal. The collector installs handlers to take care
of these. You will not see these unless you are using a debugger. Your
debugger _should_ allow you to continue. It's often preferable to tell the
debugger to ignore SIGBUS and SIGSEGV ("handle SIGSEGV SIGBUS nostop noprint"
in gdb, "ignore SIGSEGV SIGBUS" in most versions of dbx) and set a breakpoint
in `abort`. The collector will call abort if the signal had another cause, and
there was not other handler previously installed.

We recommend debugging without incremental collection if possible. (This
applies directly to UNIX systems. Debugging with incremental collection under
win32 is worse. See README.win32.)

If the application generates an unhandled SIGSEGV or equivalent, it may often
be easiest to set the environment variable `GC_LOOP_ON_ABORT`. On many
platforms, this will cause the collector to loop in a handler when the SIGSEGV
is encountered (or when the collector aborts for some other reason), and
a debugger can then be attached to the looping process. This sidesteps common
operating system problems related to incomplete core files for multi-threaded
applications, etc.

## Other Signals

On most platforms, the multi-threaded version of the collector needs one or
two other signals for internal use by the collector in stopping threads. It is
normally wise to tell the debugger to ignore these. On Linux, the collector
currently uses SIGPWR and SIGXCPU by default.

## Warning Messages About Needing to Allocate Blacklisted Blocks

The garbage collector generates warning messages of the form:


    Needed to allocate blacklisted block at 0x...


or


    Repeated allocation of very large block ...


when it needs to allocate a block at a location that it knows to be referenced
by a false pointer. These false pointers can be either permanent (e.g.
a static integer variable that never changes) or temporary. In the latter
case, the warning is largely spurious, and the block will eventually
be reclaimed normally. In the former case, the program will still run
correctly, but the block will never be reclaimed. Unless the block is intended
to be permanent, the warning indicates a memory leak.

  1. Ignore these warnings while you are using GC_DEBUG. Some of the routines
  mentioned below don't have debugging equivalents. (Alternatively, write the
  missing routines and send them to me.)
  2. Replace allocator calls that request large blocks with calls to
  `GC_malloc_ignore_off_page` or `GC_malloc_atomic_ignore_off_page`. You may
  want to set a breakpoint in `GC_default_warn_proc` to help you identify such
  calls. Make sure that a pointer to somewhere near the beginning of the
  resulting block is maintained in a (preferably volatile) variable as long
  as the block is needed.
  3. If the large blocks are allocated with realloc, we suggest instead
  allocating them with something like the following. Note that the realloc
  size increment should be fairly large (e.g. a factor of 3/2) for this to
  exhibit reasonable performance. But we all know we should do that anyway.


        void * big_realloc(void *p, size_t new_size) {
            size_t old_size = GC_size(p);
            void * result;
            if (new_size <= 10000) return(GC_realloc(p, new_size));
            if (new_size <= old_size) return(p);
            result = GC_malloc_ignore_off_page(new_size);
            if (result == 0) return(0);
            memcpy(result,p,old_size);
            GC_free(p);
            return(result);
        }


  4. In the unlikely case that even relatively small object (<20KB)
  allocations are triggering these warnings, then your address space contains
  lots of "bogus pointers", i.e. values that appear to be pointers but aren't.
  Usually this can be solved by using `GC_malloc_atomic` or the routines
  in `gc_typed.h` to allocate large pointer-free regions of bitmaps, etc.
  Sometimes the problem can be solved with trivial changes of encoding
  in certain values. It is possible, to identify the source of the bogus
  pointers by building the collector with `-DPRINT_BLACK_LIST`, which will
  cause it to print the "bogus pointers", along with their location.
  5. If you get only a fixed number of these warnings, you are probably only
  introducing a bounded leak by ignoring them. If the data structures being
  allocated are intended to be permanent, then it is also safe to ignore them.
  The warnings can be turned off by calling `GC_set_warn_proc` with
  a procedure that ignores these warnings (e.g. by doing absolutely nothing).

## The Collector References a Bad Address in GC_malloc

This typically happens while the collector is trying to remove an entry from
its free list, and the free list pointer is bad because the free list link
in the last allocated object was bad.

With >99% probability, you wrote past the end of an allocated object. Try
setting `GC_DEBUG` before including `gc.h` and allocating with `GC_MALLOC`.
This will try to detect such overwrite errors.

## Unexpectedly Large Heap

Unexpected heap growth can be due to one of the following:

  1. Data structures that are being unintentionally retained. This is commonly
  caused by data structures that are no longer being used, but were not
  cleared, or by caches growing without bounds.
  2. Pointer misidentification. The garbage collector is interpreting integers
  or other data as pointers and retaining the "referenced" objects. A common
  symptom is that GC_dump() shows much of the heap as black-listed.
  3. Heap fragmentation. This should never result in unbounded growth, but
  it may account for larger heaps. This is most commonly caused by allocation
  of large objects.
  4. Per object overhead. This is usually a relatively minor effect, but
  it may be worth considering. If the collector recognizes interior pointers,
  object sizes are increased, so that one-past-the-end pointers are correctly
  recognized. The collector can be configured not to do this
  (`-DDONT_ADD_BYTE_AT_END`).

The collector rounds up object sizes so the result fits well into the chunk
size (`HBLKSIZE`, normally 4K on 32 bit machines, 8K on 64 bit machines) used
by the collector. Thus it may be worth avoiding objects of size 2K + 1 (or 2K
if a byte is being added at the end.)  The last two cases can often
be identified by looking at the output of a call to `GC_dump`. Among other
things, it will print the list of free heap blocks, and a very brief
description of all chunks in the heap, the object sizes they correspond to,
and how many live objects were found in the chunk at the last collection.

Growing data structures can usually be identified by:

  1. Building the collector with `-DKEEP_BACK_PTRS`,
  2. Preferably using debugging allocation (defining `GC_DEBUG` before
  including `gc.h` and allocating with `GC_MALLOC`), so that objects will
  be identified by their allocation site,
  3. Running the application long enough so that most of the heap is composed
  of "leaked" memory, and
  4. Then calling `GC_generate_random_backtrace` from gc_backptr.h a few times
  to determine why some randomly sampled objects in the heap are being
  retained.

The same technique can often be used to identify problems with false pointers,
by noting whether the reference chains printed
by `GC_generate_random_backtrace` involve any misidentified pointers.
An alternate technique is to build the collector with `-DPRINT_BLACK_LIST`
which will cause it to report values that are almost, but not quite, look like
heap pointers. It is very likely that actual false pointers will come from
similar sources.

In the unlikely case that false pointers are an issue, it can usually
be resolved using one or more of the following techniques:

  1. Use `GC_malloc_atomic` for objects containing no pointers. This is
  especially important for large arrays containing compressed data,
  pseudo-random numbers, and the like. It is also likely to improve GC
  performance, perhaps drastically so if the application is paging.
  2. If you allocate large objects containing only one or two pointers at the
  beginning, either try the typed allocation primitives is`gc_typed.h`,
  or separate out the pointer-free component.
  3. Consider using `GC_malloc_ignore_off_page` to allocate large objects.
  (See `gc.h` and above for details. Large means >100K in most environments.)
  4. If your heap size is larger than 100MB or so, build the collector with
  `-DLARGE_CONFIG`. This allows the collector to keep more precise black-list
  information.
  5. If you are using heaps close to, or larger than, a gigabyte on a 32-bit
  machine, you may want to consider moving to a platform with 64-bit pointers.
  This is very likely to resolve any false pointer issues.

## Prematurely Reclaimed Objects

The usual symptom of this is a segmentation fault, or an obviously overwritten
value in a heap object. This should, of course, be impossible. In practice,
it may happen for reasons like the following:

  1. The collector did not intercept the creation of threads correctly
  in a multi-threaded application, e.g. because the client called
  `pthread_create` without including `gc.h`, which redefines it.
  2. The last pointer to an object in the garbage collected heap was stored
  somewhere were the collector could not see it, e.g. in an object allocated
  with system `malloc`, in certain types of `mmap`ed files, or in some data
  structure visible only to the OS. (On some platforms, thread-local storage
  is one of these.)
  3. The last pointer to an object was somehow disguised, e.g. by XORing
  it with another pointer.
  4. Incorrect use of `GC_malloc_atomic` or typed allocation.
  5. An incorrect `GC_free` call.
  6. The client program overwrote an internal garbage collector data
  structure.
  7. A garbage collector bug.
  8. (Empirically less likely than any of the above.) A compiler optimization
  that disguised the last pointer.

The following relatively simple techniques should be tried first to narrow
down the problem:

  1. If you are using the incremental collector try turning it off for
  debugging.
  2. If you are using shared libraries, try linking statically. If that works,
  ensure that DYNAMIC_LOADING is defined on your platform.
  3. Try to reproduce the problem with fully debuggable unoptimized code. This
  will eliminate the last possibility, as well as making debugging easier.
  4. Try replacing any suspect typed allocation and `GC_malloc_atomic` calls
  with calls to `GC_malloc`.
  5. Try removing any `GC_free` calls (e.g. with a suitable `#define`).
  6. Rebuild the collector with `-DGC_ASSERTIONS`.
  7. If the following works on your platform (i.e. if gctest still works if
  you do this), try building the collector with
  `-DREDIRECT_MALLOC=GC_malloc_uncollectable`. This will cause the collector
  to scan memory allocated with malloc.

If all else fails, you will have to attack this with a debugger. The suggested
steps are:

  1. Call `GC_dump` from the debugger around the time of the failure. Verify
  that the collectors idea of the root set (i.e. static data regions which
  it should scan for pointers) looks plausible. If not, i.e. if it does not
  include some static variables, report this as a collector bug. Be sure
  to describe your platform precisely, since this sort of problem is nearly
  always very platform dependent.
  2. Especially if the failure is not deterministic, try to isolate
  it to a relatively small test case.
  3. Set a break point in `GC_finish_collection`. This is a good point
  to examine what has been marked, i.e. found reachable, by the collector.
  4. If the failure is deterministic, run the process up to the last
  collection before the failure. Note that the variable `GC_gc_no` counts
  collections and can be used to set a conditional breakpoint in the right
  one. It is incremented just before the call to `GC_finish_collection`.
  If object `p` was prematurely recycled, it may be helpful to look
  at `*GC_find_header(p)` at the failure point. The `hb_last_reclaimed` field
  will identify the collection number during which its block was last swept.
  5. Verify that the offending object still has its correct contents at this
  point. Then call `GC_is_marked(p)` from the debugger to verify that the
  object has not been marked, and is about to be reclaimed. Note that
  `GC_is_marked(p)` expects the real address of an object (the address of the
  debug header if there is one), and thus it may be more appropriate to call
  `GC_is_marked(GC_base(p))` instead.
  6. Determine a path from a root, i.e. static variable, stack, or register
  variable, to the reclaimed object. Call `GC_is_marked(q)` for each object
  `q` along the path, trying to locate the first unmarked object, say `r`.
  7. If `r` is pointed to by a static root, verify that the location pointing
  to it is part of the root set printed by `GC_dump`. If it is on the stack
  in the main (or only) thread, verify that `GC_stackbottom` is set correctly
  to the base of the stack. If it is in another thread stack, check the
  collector's thread data structure (`GC_thread[]` on several platforms)
  to make sure that stack bounds are set correctly.
  8. If `r` is pointed to by heap object `s`, check that the collector's
  layout description for `s` is such that the pointer field will be scanned.
  Call `*GC_find_header(s)` to look at the descriptor for the heap chunk.
  The `hb_descr` field specifies the layout of objects in that chunk.
  See `gc_mark.h` for the meaning of the descriptor. (If its low order 2 bits
  are zero, then it is just the length of the object prefix to be scanned.
  This form is always used for objects allocated with `GC_malloc` or
  `GC_malloc_atomic`.)
  9. If the failure is not deterministic, you may still be able to apply some
  of the above technique at the point of failure. But remember that objects
  allocated since the last collection will not have been marked, even if the
  collector is functioning properly. On some platforms, the collector can
  be configured to save call chains in objects for debugging. Enabling this
  feature will also cause it to save the call stack at the point of the last
  GC in `GC_arrays._last_stack`.
  10. When looking at GC internal data structures remember that a number
  of `GC_xxx` variables are really macro defined to `GC_arrays._xxx`, so that
  the collector can avoid scanning them.
