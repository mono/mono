/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1997 by Silicon Graphics.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 * Original author: Bill Janssen
 * Heavily modified by Hans Boehm and others
 */

/*
 * This is incredibly OS specific code for tracking down data sections in
 * dynamic libraries.  There appears to be no way of doing this quickly
 * without groveling through undocumented data structures.  We would argue
 * that this is a bug in the design of the dlopen interface.  THIS CODE
 * MAY BREAK IN FUTURE OS RELEASES.  If this matters to you, don't hesitate
 * to let your vendor know ...
 *
 * None of this is safe with dlclose and incremental collection.
 * But then not much of anything is safe in the presence of dlclose.
 */
#if (defined(__linux__) || defined(__native_client__)) && !defined(_GNU_SOURCE)
    /* Can't test LINUX, since this must be define before other includes */
#   define _GNU_SOURCE
#endif
#if !defined(MACOS) && !defined(_WIN32_WCE)
#  include <sys/types.h>
#endif
#include "private/gc_priv.h"

/* BTL: avoid circular redefinition of dlopen if GC_SOLARIS_THREADS defined */
# if (defined(GC_PTHREADS) || defined(GC_SOLARIS_THREADS)) \
      && defined(dlopen) && !defined(GC_USE_LD_WRAP)
    /* To support threads in Solaris, gc.h interposes on dlopen by       */
    /* defining "dlopen" to be "GC_dlopen", which is implemented below.  */
    /* However, both GC_FirstDLOpenedLinkMap() and GC_dlopen() use the   */
    /* real system dlopen() in their implementation. We first remove     */
    /* gc.h's dlopen definition and restore it later, after GC_dlopen(). */
#   undef dlopen
#   define GC_must_restore_redefined_dlopen
# else
#   undef GC_must_restore_redefined_dlopen
# endif

#if (defined(DYNAMIC_LOADING) || defined(MSWIN32) || defined(MSWINCE)) \
    && !defined(PCR)
#if !defined(SUNOS4) && !defined(SUNOS5DL) && !defined(IRIX5) && \
    !defined(MSWIN32) && !defined(MSWINCE) && \
    !(defined(ALPHA) && defined(OSF1)) && \
    !defined(HPUX) && !((defined(LINUX) || defined(NACL)) && defined(__ELF__)) && \
    !defined(RS6000) && !defined(SCO_ELF) && !defined(DGUX) && \
    !(defined(FREEBSD) && defined(__ELF__)) && \
    !(defined(OPENBSD) && (defined(__ELF__) || defined(M68K))) && \
    !(defined(NETBSD) && defined(__ELF__)) && !defined(HURD) && \
    !defined(DARWIN)
 --> We only know how to find data segments of dynamic libraries for the
 --> above.  Additional SVR4 variants might not be too
 --> hard to add.
#endif

#include <stdio.h>
#ifdef SUNOS5DL
/* Avoid  #error "large files are not supported by libelf" errors */
#if defined(_ILP32) && (_FILE_OFFSET_BITS != 32)
#undef _FILE_OFFSET_BITS
#define _FILE_OFFSET_BITS 32
#endif
#   include <sys/elf.h>
#   include <dlfcn.h>
#   include <link.h>
#endif
#ifdef SUNOS4
#   include <dlfcn.h>
#   include <link.h>
#   include <a.out.h>
  /* struct link_map field overrides */
#   define l_next	lm_next
#   define l_addr	lm_addr
#   define l_name	lm_name
#endif

#if defined(NETBSD)
#   include <machine/elf_machdep.h>
#   define ELFSIZE ARCH_ELFSIZE
#endif

#if (defined(LINUX) || defined(NACL)) && defined(__ELF__) || defined(SCO_ELF) || \
    (defined(FREEBSD) && defined(__ELF__)) || defined(DGUX) || \
    (defined(OPENBSD) && defined(__ELF__)) || \
    (defined(NETBSD) && defined(__ELF__)) || defined(HURD)
#   include <stddef.h>
# if !defined(OPENBSD)
#   include <elf.h>
# endif
#   include <link.h>
#endif

/* Newer versions of GNU/Linux define this macro.  We
 * define it similarly for any ELF systems that don't.  */
#  ifndef ElfW
#    if defined(FREEBSD)
#      if __ELF_WORD_SIZE == 32
#        define ElfW(type) Elf32_##type
#      else
#        define ElfW(type) Elf64_##type
#      endif
#    else
#      ifdef NETBSD
#        if ELFSIZE == 32
#          define ElfW(type) Elf32_##type
#        else
#          define ElfW(type) Elf64_##type
#        endif
#      else
#        if !defined(ELF_CLASS) || ELF_CLASS == ELFCLASS32
#          define ElfW(type) Elf32_##type
#        else
#          define ElfW(type) Elf64_##type
#	 endif
#      endif
#    endif
#  endif 

#if defined(SUNOS5DL) && !defined(USE_PROC_FOR_LIBRARIES)

#ifdef LINT
    Elf32_Dyn _DYNAMIC;
#endif

static struct link_map *
GC_FirstDLOpenedLinkMap()
{
    extern ElfW(Dyn) _DYNAMIC;
    ElfW(Dyn) *dp;
    struct r_debug *r;
    static struct link_map * cachedResult = 0;
    static ElfW(Dyn) *dynStructureAddr = 0;
    			/* BTL: added to avoid Solaris 5.3 ld.so _DYNAMIC bug */

#   ifdef SUNOS53_SHARED_LIB
	/* BTL: Avoid the Solaris 5.3 bug that _DYNAMIC isn't being set	*/
	/* up properly in dynamically linked .so's. This means we have	*/
	/* to use its value in the set of original object files loaded	*/
	/* at program startup.						*/
	if( dynStructureAddr == 0 ) {
	  void* startupSyms = dlopen(0, RTLD_LAZY);
	  dynStructureAddr = (ElfW(Dyn)*)dlsym(startupSyms, "_DYNAMIC");
		}
#   else
	dynStructureAddr = &_DYNAMIC;
#   endif

    if( dynStructureAddr == 0) {
        return(0);
    }
    if( cachedResult == 0 ) {
        int tag;
        for( dp = ((ElfW(Dyn) *)(&_DYNAMIC)); (tag = dp->d_tag) != 0; dp++ ) {
            if( tag == DT_DEBUG ) {
                struct link_map *lm
                        = ((struct r_debug *)(dp->d_un.d_ptr))->r_map;
                if( lm != 0 ) cachedResult = lm->l_next; /* might be NIL */
                break;
            }
        }
    }
    return cachedResult;
}

#endif /* SUNOS5DL ... */

/* BTL: added to fix circular dlopen definition if GC_SOLARIS_THREADS defined */
# if defined(GC_must_restore_redefined_dlopen)
#   define dlopen GC_dlopen
# endif

#if defined(SUNOS4) && !defined(USE_PROC_FOR_LIBRARIES)

#ifdef LINT
    struct link_dynamic _DYNAMIC;
#endif

static struct link_map *
GC_FirstDLOpenedLinkMap()
{
    extern struct link_dynamic _DYNAMIC;

    if( &_DYNAMIC == 0) {
        return(0);
    }
    return(_DYNAMIC.ld_un.ld_1->ld_loaded);
}

/* Return the address of the ld.so allocated common symbol	*/
/* with the least address, or 0 if none.			*/
static ptr_t GC_first_common()
{
    ptr_t result = 0;
    extern struct link_dynamic _DYNAMIC;
    struct rtc_symb * curr_symbol;
    
    if( &_DYNAMIC == 0) {
        return(0);
    }
    curr_symbol = _DYNAMIC.ldd -> ldd_cp;
    for (; curr_symbol != 0; curr_symbol = curr_symbol -> rtc_next) {
        if (result == 0
            || (ptr_t)(curr_symbol -> rtc_sp -> n_value) < result) {
            result = (ptr_t)(curr_symbol -> rtc_sp -> n_value);
        }
    }
    return(result);
}

#endif  /* SUNOS4 ... */

# if defined(SUNOS4) || defined(SUNOS5DL)
/* Add dynamic library data sections to the root set.		*/
# if !defined(PCR) && !defined(GC_SOLARIS_THREADS) && defined(THREADS)
#   ifndef SRC_M3
	--> fix mutual exclusion with dlopen
#   endif  /* We assume M3 programs don't call dlopen for now */
# endif

# ifndef USE_PROC_FOR_LIBRARIES
void GC_register_dynamic_libraries()
{
  struct link_map *lm = GC_FirstDLOpenedLinkMap();
  

  for (lm = GC_FirstDLOpenedLinkMap();
       lm != (struct link_map *) 0;  lm = lm->l_next)
    {
#     ifdef SUNOS4
	struct exec *e;
	 
        e = (struct exec *) lm->lm_addr;
        GC_add_roots_inner(
      		    ((char *) (N_DATOFF(*e) + lm->lm_addr)),
		    ((char *) (N_BSSADDR(*e) + e->a_bss + lm->lm_addr)),
		    TRUE);
#     endif
#     ifdef SUNOS5DL
	ElfW(Ehdr) * e;
        ElfW(Phdr) * p;
        unsigned long offset;
        char * start;
        register int i;
        
        e = (ElfW(Ehdr) *) lm->l_addr;
        if (e == NULL)
          continue;

        p = ((ElfW(Phdr) *)(((char *)(e)) + e->e_phoff));
        offset = ((unsigned long)(lm->l_addr));
        for( i = 0; i < (int)(e->e_phnum); ((i++),(p++)) ) {
          switch( p->p_type ) {
            case PT_LOAD:
              {
                if( !(p->p_flags & PF_W) ) break;
                start = ((char *)(p->p_vaddr)) + offset;
                GC_add_roots_inner(
                  start,
                  start + p->p_memsz,
                  TRUE
                );
              }
              break;
            default:
              break;
          }
	}
#     endif
    }
#   ifdef SUNOS4
      {
      	static ptr_t common_start = 0;
      	ptr_t common_end;
      	extern ptr_t GC_find_limit();
      	
      	if (common_start == 0) common_start = GC_first_common();
      	if (common_start != 0) {
      	    common_end = GC_find_limit(common_start, TRUE);
      	    GC_add_roots_inner((char *)common_start, (char *)common_end, TRUE);
      	}
      }
#   endif
}

# endif /* !USE_PROC ... */
# endif /* SUNOS */

#if (defined(LINUX) || defined(NACL)) && defined(__ELF__) || defined(SCO_ELF) || \
    (defined(FREEBSD) && defined(__ELF__)) || defined(DGUX) || \
    (defined(OPENBSD) && defined(__ELF__)) || \
    (defined(NETBSD) && defined(__ELF__)) || defined(HURD)


#ifdef USE_PROC_FOR_LIBRARIES

#include <string.h>

#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>

#define MAPS_BUF_SIZE (32*1024)

extern ssize_t GC_repeat_read(int fd, char *buf, size_t count);
	/* Repeatedly read until buffer is filled, or EOF is encountered */
	/* Defined in os_dep.c.  					 */

char *GC_parse_map_entry(char *buf_ptr, word *start, word *end,
                         char *prot_buf, unsigned int *maj_dev);
word GC_apply_to_maps(word (*fn)(char *));
	/* From os_dep.c	*/

word GC_register_map_entries(char *maps)
{
    char prot_buf[5];
    char *buf_ptr = maps;
    int count;
    word start, end;
    unsigned int maj_dev;
    word least_ha, greatest_ha;
    unsigned i;
    word datastart = (word)(DATASTART);

    /* Compute heap bounds. FIXME: Should be done by add_to_heap?	*/
	least_ha = (word)(-1);
	greatest_ha = 0;
	for (i = 0; i < GC_n_heap_sects; ++i) {
	    word sect_start = (word)GC_heap_sects[i].hs_start;
	    word sect_end = sect_start + GC_heap_sects[i].hs_bytes;
	    if (sect_start < least_ha) least_ha = sect_start;
	    if (sect_end > greatest_ha) greatest_ha = sect_end;
        }
    	if (greatest_ha < (word)GC_scratch_last_end_ptr)
	    greatest_ha = (word)GC_scratch_last_end_ptr; 

    for (;;) {
        buf_ptr = GC_parse_map_entry(buf_ptr, &start, &end, prot_buf, &maj_dev);
	if (buf_ptr == NULL) return 1;
	if (prot_buf[1] == 'w') {
	    /* This is a writable mapping.  Add it to		*/
	    /* the root set unless it is already otherwise	*/
	    /* accounted for.					*/
	    if (start <= (word)GC_stackbottom && end >= (word)GC_stackbottom) {
		/* Stack mapping; discard	*/
		continue;
	    }
#	    ifdef THREADS
	      if (GC_segment_is_thread_stack(start, end)) continue;
#	    endif
	    /* We no longer exclude the main data segment.		*/
	    if (start < least_ha && end > least_ha) {
		end = least_ha;
	    }
	    if (start < greatest_ha && end > greatest_ha) {
		start = greatest_ha;
	    }
	    if (start >= least_ha && end <= greatest_ha) continue;
	    GC_add_roots_inner((char *)start, (char *)end, TRUE);
	}
    }
    return 1;
}

void GC_register_dynamic_libraries()
{
   if (!GC_apply_to_maps(GC_register_map_entries))
       ABORT("Failed to read /proc for library registration.");
}

/* We now take care of the main data segment ourselves: */
GC_bool GC_register_main_static_data()
{
  return FALSE;
}
  
# define HAVE_REGISTER_MAIN_STATIC_DATA

#endif /* USE_PROC_FOR_LIBRARIES */

#if !defined(USE_PROC_FOR_LIBRARIES)
/* The following is the preferred way to walk dynamic libraries	*/
/* For glibc 2.2.4+.  Unfortunately, it doesn't work for older	*/
/* versions.  Thanks to Jakub Jelinek for most of the code.	*/

# if (defined(LINUX) || defined(NACL)) /* Are others OK here, too? */ \
     && (__GLIBC__ > 2 || (__GLIBC__ == 2 && __GLIBC_MINOR__ > 2) \
         || (__GLIBC__ == 2 && __GLIBC_MINOR__ == 2 && defined(DT_CONFIG))) 

/* We have the header files for a glibc that includes dl_iterate_phdr.	*/
/* It may still not be available in the library on the target system.   */
/* Thus we also treat it as a weak symbol.				*/
#define HAVE_DL_ITERATE_PHDR

static int GC_register_dynlib_callback(info, size, ptr)
     struct dl_phdr_info * info;
     size_t size;
     void * ptr;
{
  const ElfW(Phdr) * p;
  char * start;
  register int i;

  /* Make sure struct dl_phdr_info is at least as big as we need.  */
  if (size < offsetof (struct dl_phdr_info, dlpi_phnum)
      + sizeof (info->dlpi_phnum))
    return -1;

  p = info->dlpi_phdr;
  for( i = 0; i < (int)(info->dlpi_phnum); ((i++),(p++)) ) {
    switch( p->p_type ) {
      case PT_LOAD:
	{
	  if( !(p->p_flags & PF_W) ) break;
	  start = ((char *)(p->p_vaddr)) + info->dlpi_addr;
	  GC_add_roots_inner(start, start + p->p_memsz, TRUE);
	}
      break;
      default:
	break;
    }
  }

  * (int *)ptr = 1;	/* Signal that we were called */
  return 0;
}     

/* Return TRUE if we succeed, FALSE if dl_iterate_phdr wasn't there. */

#pragma weak dl_iterate_phdr

GC_bool GC_register_dynamic_libraries_dl_iterate_phdr()
{
  if (dl_iterate_phdr) {
    int did_something = 0;
    dl_iterate_phdr(GC_register_dynlib_callback, &did_something);
    if (!did_something) {
	/* dl_iterate_phdr may forget the static data segment in	*/
	/* statically linked executables.				*/
	GC_add_roots_inner(DATASTART, (char *)(DATAEND), TRUE);
#       if defined(DATASTART2)
          GC_add_roots_inner(DATASTART2, (char *)(DATAEND2), TRUE);
#       endif
    }

    return TRUE;
  } else {
    return FALSE;
  }
}

/* Do we need to separately register the main static data segment? */
GC_bool GC_register_main_static_data()
{
  return (dl_iterate_phdr == 0);
}

#define HAVE_REGISTER_MAIN_STATIC_DATA

# else /* !LINUX || version(glibc) < 2.2.4 */

/* Dynamic loading code for Linux running ELF. Somewhat tested on
 * Linux/x86, untested but hopefully should work on Linux/Alpha. 
 * This code was derived from the Solaris/ELF support. Thanks to
 * whatever kind soul wrote that.  - Patrick Bridges */

/* This doesn't necessarily work in all cases, e.g. with preloaded
 * dynamic libraries.						*/

#if defined(NETBSD) || defined(OPENBSD)
# if !defined(OPENBSD)
#  include <sys/exec_elf.h>
# endif
/* for compatibility with 1.4.x */
#  ifndef DT_DEBUG
#  define DT_DEBUG     21
#  endif
#  ifndef PT_LOAD
#  define PT_LOAD      1
#  endif
#  ifndef PF_W
#  define PF_W         2
#  endif
#else
#  include <elf.h>
#endif
#include <link.h>

# endif

#ifdef __GNUC__
# pragma weak _DYNAMIC
#endif
extern ElfW(Dyn) _DYNAMIC[];

static struct link_map *
GC_FirstDLOpenedLinkMap()
{
    ElfW(Dyn) *dp;
    static struct link_map *cachedResult = 0;

    if( _DYNAMIC == 0) {
        return(0);
    }
    if( cachedResult == 0 ) {
        int tag;
        for( dp = _DYNAMIC; (tag = dp->d_tag) != 0; dp++ ) {
	    /* FIXME: The DT_DEBUG header is not mandated by the	*/
	    /* ELF spec.  This code appears to be dependent on		*/
	    /* idiosynchracies of older GNU tool chains.  If this code	*/
	    /* fails for you, the real problem is probably that it is	*/
	    /* being used at all.  You should be getting the 		*/
	    /* dl_iterate_phdr version.					*/
            if( tag == DT_DEBUG ) {
                struct link_map *lm
                        = ((struct r_debug *)(dp->d_un.d_ptr))->r_map;
                if( lm != 0 ) cachedResult = lm->l_next; /* might be NIL */
                break;
            }
        }
    }
    return cachedResult;
}


void GC_register_dynamic_libraries()
{
  struct link_map *lm;
  

# ifdef HAVE_DL_ITERATE_PHDR
    if (GC_register_dynamic_libraries_dl_iterate_phdr()) {
	return;
    }
# endif
  lm = GC_FirstDLOpenedLinkMap();
  for (lm = GC_FirstDLOpenedLinkMap();
       lm != (struct link_map *) 0;  lm = lm->l_next)
    {
	ElfW(Ehdr) * e;
        ElfW(Phdr) * p;
        unsigned long offset;
        char * start;
        register int i;
        
        e = (ElfW(Ehdr) *) lm->l_addr;
        if (e == NULL)
          continue;

        p = ((ElfW(Phdr) *)(((char *)(e)) + e->e_phoff));
        offset = ((unsigned long)(lm->l_addr));
        for( i = 0; i < (int)(e->e_phnum); ((i++),(p++)) ) {
          switch( p->p_type ) {
            case PT_LOAD:
              {
                if( !(p->p_flags & PF_W) ) break;
                start = ((char *)(p->p_vaddr)) + offset;
                GC_add_roots_inner(start, start + p->p_memsz, TRUE);
              }
              break;
            default:
              break;
          }
	}
    }
}

#endif /* !USE_PROC_FOR_LIBRARIES */

#endif /* LINUX */

#if defined(IRIX5) || (defined(USE_PROC_FOR_LIBRARIES) && !defined(LINUX))

#include <sys/procfs.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <elf.h>
#include <errno.h>
#include <signal.h>  /* Only for the following test. */
#ifndef _sigargs
# define IRIX6
#endif

extern void * GC_roots_present();
	/* The type is a lie, since the real type doesn't make sense here, */
	/* and we only test for NULL.					   */


/* We use /proc to track down all parts of the address space that are	*/
/* mapped by the process, and throw out regions we know we shouldn't	*/
/* worry about.  This may also work under other SVR4 variants.		*/
void GC_register_dynamic_libraries()
{
    static int fd = -1;
    char buf[30];
    static prmap_t * addr_map = 0;
    static int current_sz = 0;	/* Number of records currently in addr_map */
    static int needed_sz;	/* Required size of addr_map		*/
    register int i;
    register long flags;
    register ptr_t start;
    register ptr_t limit;
    ptr_t heap_start = (ptr_t)HEAP_START;
    ptr_t heap_end = heap_start;

#   ifdef SUNOS5DL
#     define MA_PHYS 0
#   endif /* SUNOS5DL */

    if (fd < 0) {
      sprintf(buf, "/proc/%d", getpid());
	/* The above generates a lint complaint, since pid_t varies.	*/
	/* It's unclear how to improve this.				*/
      fd = open(buf, O_RDONLY);
      if (fd < 0) {
    	ABORT("/proc open failed");
      }
    }
    if (ioctl(fd, PIOCNMAP, &needed_sz) < 0) {
	GC_err_printf2("fd = %d, errno = %d\n", fd, errno);
    	ABORT("/proc PIOCNMAP ioctl failed");
    }
    if (needed_sz >= current_sz) {
        current_sz = needed_sz * 2 + 1;
        		/* Expansion, plus room for 0 record */
        addr_map = (prmap_t *)GC_scratch_alloc((word)
						(current_sz * sizeof(prmap_t)));
    }
    if (ioctl(fd, PIOCMAP, addr_map) < 0) {
        GC_err_printf4("fd = %d, errno = %d, needed_sz = %d, addr_map = 0x%X\n",
                        fd, errno, needed_sz, addr_map);
    	ABORT("/proc PIOCMAP ioctl failed");
    };
    if (GC_n_heap_sects > 0) {
    	heap_end = GC_heap_sects[GC_n_heap_sects-1].hs_start
    			+ GC_heap_sects[GC_n_heap_sects-1].hs_bytes;
    	if (heap_end < GC_scratch_last_end_ptr) heap_end = GC_scratch_last_end_ptr; 
    }
    for (i = 0; i < needed_sz; i++) {
        flags = addr_map[i].pr_mflags;
        if ((flags & (MA_BREAK | MA_STACK | MA_PHYS
		      | MA_FETCHOP | MA_NOTCACHED)) != 0) goto irrelevant;
        if ((flags & (MA_READ | MA_WRITE)) != (MA_READ | MA_WRITE))
            goto irrelevant;
          /* The latter test is empirically useless in very old Irix	*/
	  /* versions.  Other than the					*/
          /* main data and stack segments, everything appears to be	*/
          /* mapped readable, writable, executable, and shared(!!).	*/
          /* This makes no sense to me.	- HB				*/
        start = (ptr_t)(addr_map[i].pr_vaddr);
        if (GC_roots_present(start)) goto irrelevant;
        if (start < heap_end && start >= heap_start)
        	goto irrelevant;
#	ifdef MMAP_STACKS
	  if (GC_is_thread_stack(start)) goto irrelevant;
#	endif /* MMAP_STACKS */

        limit = start + addr_map[i].pr_size;
	/* The following seemed to be necessary for very old versions 	*/
	/* of Irix, but it has been reported to discard relevant	*/
	/* segments under Irix 6.5.  					*/
#	ifndef IRIX6
	  if (addr_map[i].pr_off == 0 && strncmp(start, ELFMAG, 4) == 0) {
	    /* Discard text segments, i.e. 0-offset mappings against	*/
	    /* executable files which appear to have ELF headers.	*/
	    caddr_t arg;
	    int obj;
#	    define MAP_IRR_SZ 10
	    static ptr_t map_irr[MAP_IRR_SZ];
	    				/* Known irrelevant map entries	*/
	    static int n_irr = 0;
	    struct stat buf;
	    register int i;
	    
	    for (i = 0; i < n_irr; i++) {
	        if (map_irr[i] == start) goto irrelevant;
	    }
	    arg = (caddr_t)start;
	    obj = ioctl(fd, PIOCOPENM, &arg);
	    if (obj >= 0) {
	        fstat(obj, &buf);
	        close(obj);
	        if ((buf.st_mode & 0111) != 0) {
	            if (n_irr < MAP_IRR_SZ) {
	                map_irr[n_irr++] = start;
	            }
	            goto irrelevant;
	        }
	    }
	  }
#	endif /* !IRIX6 */
        GC_add_roots_inner(start, limit, TRUE);
      irrelevant: ;
    }
    /* Dont keep cached descriptor, for now.  Some kernels don't like us */
    /* to keep a /proc file descriptor around during kill -9.		 */
    	if (close(fd) < 0) ABORT("Couldnt close /proc file");
	fd = -1;
}

# endif /* USE_PROC || IRIX5 */

# if defined(MSWIN32) || defined(MSWINCE)

# define WIN32_LEAN_AND_MEAN
# define NOSERVICE
# include <windows.h>
# include <stdlib.h>

  /* We traverse the entire address space and register all segments 	*/
  /* that could possibly have been written to.				*/
  
  extern GC_bool GC_is_heap_base (ptr_t p);

# ifdef GC_WIN32_THREADS
    extern void GC_get_next_stack(char *start, char **lo, char **hi);
    void GC_cond_add_roots(char *base, char * limit)
    {
      char * curr_base = base;
      char * next_stack_lo;
      char * next_stack_hi;
   
      if (base == limit) return;
      for(;;) {
	  GC_get_next_stack(curr_base, &next_stack_lo, &next_stack_hi);
	  if (next_stack_lo >= limit) break;
	  GC_add_roots_inner(curr_base, next_stack_lo, TRUE);
	  curr_base = next_stack_hi;
      }
      if (curr_base < limit) GC_add_roots_inner(curr_base, limit, TRUE);
    }
# else
    void GC_cond_add_roots(char *base, char * limit)
    {
      char dummy;
      char * stack_top
	 = (char *) ((word)(&dummy) & ~(GC_sysinfo.dwAllocationGranularity-1));
      if (base == limit) return;
      if (limit > stack_top && base < GC_stackbottom) {
    	  /* Part of the stack; ignore it. */
    	  return;
      }
      GC_add_roots_inner(base, limit, TRUE);
    }
# endif

# ifdef MSWINCE
  /* Do we need to separately register the main static data segment? */
  GC_bool GC_register_main_static_data()
  {
    return FALSE;
  }
# else /* win32 */
  extern GC_bool GC_no_win32_dlls;

  GC_bool GC_register_main_static_data()
  {
    return GC_no_win32_dlls;
  }
# endif /* win32 */
  
# define HAVE_REGISTER_MAIN_STATIC_DATA

  /* The frame buffer testing code is dead in this version.	*/
  /* We leave it here temporarily in case the switch to just 	*/
  /* testing for MEM_IMAGE sections causes un expected 		*/
  /* problems.							*/
  GC_bool GC_warn_fb = TRUE;	/* Warn about traced likely 	*/
  				/* graphics memory.		*/
  GC_bool GC_disallow_ignore_fb = FALSE;
  int GC_ignore_fb_mb;	/* Ignore mappings bigger than the 	*/
  			/* specified number of MB.		*/
  GC_bool GC_ignore_fb = FALSE; /* Enable frame buffer 	*/
  				/* checking.		*/
  
  /* Issue warning if tracing apparent framebuffer. 		*/
  /* This limits us to one warning, and it's a back door to	*/
  /* disable that.						*/
 
  /* Should [start, start+len) be treated as a frame buffer	*/
  /* and ignored?						*/
  /* Unfortunately, we currently are not quite sure how to tell	*/
  /* this automatically, and rely largely on user input.	*/
  /* We expect that any mapping with type MEM_MAPPED (which 	*/
  /* apparently excludes library data sections) can be safely	*/
  /* ignored.  But we're too chicken to do that in this 	*/
  /* version.							*/
  /* Based on a very limited sample, it appears that:		*/
  /* 	- Frame buffer mappings appear as mappings of large	*/
  /*	  length, usually a bit less than a power of two.	*/
  /*	- The definition of "a bit less" in the above cannot	*/
  /*	  be made more precise.					*/
  /*	- Have a starting address at best 64K aligned.		*/
  /*	- Have type == MEM_MAPPED.				*/
  static GC_bool is_frame_buffer(ptr_t start, size_t len, DWORD tp)
  {
    static GC_bool initialized = FALSE;
#   define MB (1024*1024)
#   define DEFAULT_FB_MB 15
#   define MIN_FB_MB 3

    if (GC_disallow_ignore_fb || tp != MEM_MAPPED) return FALSE;
    if (!initialized) {
      char * ignore_fb_string =  GETENV("GC_IGNORE_FB");

      if (0 != ignore_fb_string) {
	while (*ignore_fb_string == ' ' || *ignore_fb_string == '\t')
	  ++ignore_fb_string;
	if (*ignore_fb_string == '\0') {
	  GC_ignore_fb_mb = DEFAULT_FB_MB;
	} else {
	  GC_ignore_fb_mb = atoi(ignore_fb_string);
	  if (GC_ignore_fb_mb < MIN_FB_MB) {
	    WARN("Bad GC_IGNORE_FB value.  Using %ld\n", DEFAULT_FB_MB);
	    GC_ignore_fb_mb = DEFAULT_FB_MB;
	  }
	}
	GC_ignore_fb = TRUE;
      } else {
	GC_ignore_fb_mb = DEFAULT_FB_MB;  /* For warning */
      }
      initialized = TRUE;
    }
    if (len >= ((size_t)GC_ignore_fb_mb << 20)) {
      if (GC_ignore_fb) {
	return TRUE;
      } else {
	if (GC_warn_fb) {
	  WARN("Possible frame buffer mapping at 0x%lx: \n"
	       "\tConsider setting GC_IGNORE_FB to improve performance.\n",
	       start);
	  GC_warn_fb = FALSE;
	}
	return FALSE;
      }
    } else {
      return FALSE;
    }
  }

# ifdef DEBUG_VIRTUALQUERY
  void GC_dump_meminfo(MEMORY_BASIC_INFORMATION *buf)
  {
    GC_printf4("BaseAddress = %lx, AllocationBase = %lx, RegionSize = %lx(%lu)\n",
	       buf -> BaseAddress, buf -> AllocationBase, buf -> RegionSize,
	       buf -> RegionSize);
    GC_printf4("\tAllocationProtect = %lx, State = %lx, Protect = %lx, "
	       "Type = %lx\n",
	       buf -> AllocationProtect, buf -> State, buf -> Protect,
	       buf -> Type);
  }
# endif /* DEBUG_VIRTUALQUERY */

  void GC_register_dynamic_libraries()
  {
    MEMORY_BASIC_INFORMATION buf;
    DWORD result;
    DWORD protect;
    LPVOID p;
    char * base;
    char * limit, * new_limit;

#   ifdef MSWIN32
      if (GC_no_win32_dlls) return;
#   endif
    base = limit = p = GC_sysinfo.lpMinimumApplicationAddress;
#   if defined(MSWINCE) && !defined(_WIN32_WCE_EMULATION)
    /* Only the first 32 MB of address space belongs to the current process */
    while (p < (LPVOID)0x02000000) {
        result = VirtualQuery(p, &buf, sizeof(buf));
	if (result == 0) {
	    /* Page is free; advance to the next possible allocation base */
	    new_limit = (char *)
		(((DWORD) p + GC_sysinfo.dwAllocationGranularity)
		 & ~(GC_sysinfo.dwAllocationGranularity-1));
	} else
#   else
    while (p < GC_sysinfo.lpMaximumApplicationAddress) {
        result = VirtualQuery(p, &buf, sizeof(buf));
#   endif
	{
	    if (result != sizeof(buf)) {
		ABORT("Weird VirtualQuery result");
	    }
	    new_limit = (char *)p + buf.RegionSize;
	    protect = buf.Protect;
	    if (buf.State == MEM_COMMIT
		&& (protect == PAGE_EXECUTE_READWRITE
		    || protect == PAGE_READWRITE)
		&& !GC_is_heap_base(buf.AllocationBase)
		/* This used to check for
		 * !is_frame_buffer(p, buf.RegionSize, buf.Type)
		 * instead of just checking for MEM_IMAGE.
		 * If something breaks, change it back. */
		&& buf.Type == MEM_IMAGE) {  
#	        ifdef DEBUG_VIRTUALQUERY
	          GC_dump_meminfo(&buf);
#	        endif
		if ((char *)p != limit) {
		    GC_cond_add_roots(base, limit);
		    base = p;
		}
		limit = new_limit;
	    }
	}
        if (p > (LPVOID)new_limit /* overflow */) break;
        p = (LPVOID)new_limit;
    }
    GC_cond_add_roots(base, limit);
  }

#endif /* MSWIN32 || MSWINCE */
  
#if defined(ALPHA) && defined(OSF1)

#include <loader.h>

void GC_register_dynamic_libraries()
{
  int status;
  ldr_process_t mypid;

  /* module */
    ldr_module_t moduleid = LDR_NULL_MODULE;
    ldr_module_info_t moduleinfo;
    size_t moduleinfosize = sizeof(moduleinfo);
    size_t modulereturnsize;    

  /* region */
    ldr_region_t region; 
    ldr_region_info_t regioninfo;
    size_t regioninfosize = sizeof(regioninfo);
    size_t regionreturnsize;

  /* Obtain id of this process */
    mypid = ldr_my_process();
  
  /* For each module */
    while (TRUE) {

      /* Get the next (first) module */
        status = ldr_next_module(mypid, &moduleid);

      /* Any more modules? */
        if (moduleid == LDR_NULL_MODULE)
            break;    /* No more modules */

      /* Check status AFTER checking moduleid because */
      /* of a bug in the non-shared ldr_next_module stub */
        if (status != 0 ) {
            GC_printf1("dynamic_load: status = %ld\n", (long)status);
            {
                extern char *sys_errlist[];
                extern int sys_nerr;
                extern int errno;
                if (errno <= sys_nerr) {
                    GC_printf1("dynamic_load: %s\n", (long)sys_errlist[errno]);
               } else {
                    GC_printf1("dynamic_load: %d\n", (long)errno);
                }
        }
            ABORT("ldr_next_module failed");
         }

      /* Get the module information */
        status = ldr_inq_module(mypid, moduleid, &moduleinfo,
                                moduleinfosize, &modulereturnsize); 
        if (status != 0 )
            ABORT("ldr_inq_module failed");

      /* is module for the main program (i.e. nonshared portion)? */
          if (moduleinfo.lmi_flags & LDR_MAIN)
              continue;    /* skip the main module */

#     ifdef VERBOSE
          GC_printf("---Module---\n");
          GC_printf("Module ID            = %16ld\n", moduleinfo.lmi_modid);
          GC_printf("Count of regions     = %16d\n", moduleinfo.lmi_nregion);
          GC_printf("flags for module     = %16lx\n", moduleinfo.lmi_flags); 
          GC_printf("pathname of module   = \"%s\"\n", moduleinfo.lmi_name);
#     endif

      /* For each region in this module */
        for (region = 0; region < moduleinfo.lmi_nregion; region++) {

          /* Get the region information */
            status = ldr_inq_region(mypid, moduleid, region, &regioninfo,
                                    regioninfosize, &regionreturnsize);
            if (status != 0 )
                ABORT("ldr_inq_region failed");

          /* only process writable (data) regions */
            if (! (regioninfo.lri_prot & LDR_W))
                continue;

#         ifdef VERBOSE
              GC_printf("--- Region ---\n");
              GC_printf("Region number    = %16ld\n",
              	        regioninfo.lri_region_no);
              GC_printf("Protection flags = %016x\n",  regioninfo.lri_prot);
              GC_printf("Virtual address  = %16p\n",   regioninfo.lri_vaddr);
              GC_printf("Mapped address   = %16p\n",   regioninfo.lri_mapaddr);
              GC_printf("Region size      = %16ld\n",  regioninfo.lri_size);
              GC_printf("Region name      = \"%s\"\n", regioninfo.lri_name);
#         endif

          /* register region as a garbage collection root */
            GC_add_roots_inner (
                (char *)regioninfo.lri_mapaddr,
                (char *)regioninfo.lri_mapaddr + regioninfo.lri_size,
                TRUE);

        }
    }
}
#endif

#if defined(HPUX)

#include <errno.h>
#include <dl.h>

extern int errno;
extern char *sys_errlist[];
extern int sys_nerr;

void GC_register_dynamic_libraries()
{
  int status;
  int index = 1; /* Ordinal position in shared library search list */
  struct shl_descriptor *shl_desc; /* Shared library info, see dl.h */

  /* For each dynamic library loaded */
    while (TRUE) {

      /* Get info about next shared library */
        status = shl_get(index, &shl_desc);

      /* Check if this is the end of the list or if some error occured */
        if (status != 0) {
#	 ifdef GC_HPUX_THREADS
	   /* I've seen errno values of 0.  The man page is not clear	*/
	   /* as to whether errno should get set on a -1 return.	*/
	   break;
#	 else
          if (errno == EINVAL) {
              break; /* Moved past end of shared library list --> finished */
          } else {
              if (errno <= sys_nerr) {
                    GC_printf1("dynamic_load: %s\n", (long) sys_errlist[errno]);
              } else {
                    GC_printf1("dynamic_load: %d\n", (long) errno);
	      }
              ABORT("shl_get failed");
          }
#	 endif
        }

#     ifdef VERBOSE
          GC_printf0("---Shared library---\n");
          GC_printf1("\tfilename        = \"%s\"\n", shl_desc->filename);
          GC_printf1("\tindex           = %d\n", index);
          GC_printf1("\thandle          = %08x\n",
					(unsigned long) shl_desc->handle);
          GC_printf1("\ttext seg. start = %08x\n", shl_desc->tstart);
          GC_printf1("\ttext seg. end   = %08x\n", shl_desc->tend);
          GC_printf1("\tdata seg. start = %08x\n", shl_desc->dstart);
          GC_printf1("\tdata seg. end   = %08x\n", shl_desc->dend);
          GC_printf1("\tref. count      = %lu\n", shl_desc->ref_count);
#     endif

      /* register shared library's data segment as a garbage collection root */
        GC_add_roots_inner((char *) shl_desc->dstart,
			   (char *) shl_desc->dend, TRUE);

        index++;
    }
}
#endif /* HPUX */

#ifdef RS6000
#pragma alloca
#include <sys/ldr.h>
#include <sys/errno.h>
void GC_register_dynamic_libraries()
{
	int len;
	char *ldibuf;
	int ldibuflen;
	struct ld_info *ldi;

	ldibuf = alloca(ldibuflen = 8192);

	while ( (len = loadquery(L_GETINFO,ldibuf,ldibuflen)) < 0) {
		if (errno != ENOMEM) {
			ABORT("loadquery failed");
		}
		ldibuf = alloca(ldibuflen *= 2);
	}

	ldi = (struct ld_info *)ldibuf;
	while (ldi) {
		len = ldi->ldinfo_next;
		GC_add_roots_inner(
				ldi->ldinfo_dataorg,
				(ptr_t)(unsigned long)ldi->ldinfo_dataorg
			        + ldi->ldinfo_datasize,
				TRUE);
		ldi = len ? (struct ld_info *)((char *)ldi + len) : 0;
	}
}
#endif /* RS6000 */

#ifdef DARWIN

/* __private_extern__ hack required for pre-3.4 gcc versions.	*/
#ifndef __private_extern__
# define __private_extern__ extern
# include <mach-o/dyld.h>
# undef __private_extern__
#else
# include <mach-o/dyld.h>
#endif
#include <mach-o/getsect.h>

/*#define DARWIN_DEBUG*/

const static struct { 
        const char *seg;
        const char *sect;
} GC_dyld_sections[] = {
        { SEG_DATA, SECT_DATA },
        { SEG_DATA, SECT_BSS },
        { SEG_DATA, SECT_COMMON }
};
    
#ifdef DARWIN_DEBUG
static const char *GC_dyld_name_for_hdr(struct mach_header *hdr) {
    unsigned long i,c;
    c = _dyld_image_count();
    for(i=0;i<c;i++) if(_dyld_get_image_header(i) == hdr)
        return _dyld_get_image_name(i);
    return NULL;
}
#endif
        
/* This should never be called by a thread holding the lock */
static void GC_dyld_image_add(struct mach_header* hdr, unsigned long slide) {
    unsigned long start,end,i;
    const struct section *sec;
    if (GC_no_dls) return;
    for(i=0;i<sizeof(GC_dyld_sections)/sizeof(GC_dyld_sections[0]);i++) {
        sec = getsectbynamefromheader(
            hdr,GC_dyld_sections[i].seg,GC_dyld_sections[i].sect);
        if(sec == NULL || sec->size == 0) continue;
        start = slide + sec->addr;
        end = start + sec->size;
#	ifdef DARWIN_DEBUG
            GC_printf4("Adding section at %p-%p (%lu bytes) from image %s\n",
                start,end,sec->size,GC_dyld_name_for_hdr(hdr));
#       endif
        GC_add_roots((char*)start,(char*)end);
    }
#   ifdef DARWIN_DEBUG
        GC_print_static_roots();
#   endif
}

/* This should never be called by a thread holding the lock */
static void GC_dyld_image_remove(struct mach_header* hdr, unsigned long slide) {
    unsigned long start,end,i;
    const struct section *sec;
    for(i=0;i<sizeof(GC_dyld_sections)/sizeof(GC_dyld_sections[0]);i++) {
        sec = getsectbynamefromheader(
            hdr,GC_dyld_sections[i].seg,GC_dyld_sections[i].sect);
        if(sec == NULL || sec->size == 0) continue;
        start = slide + sec->addr;
        end = start + sec->size;
#	ifdef DARWIN_DEBUG
            GC_printf4("Removing section at %p-%p (%lu bytes) from image %s\n",
                start,end,sec->size,GC_dyld_name_for_hdr(hdr));
#		endif
        GC_remove_roots((char*)start,(char*)end);
    }
#   ifdef DARWIN_DEBUG
        GC_print_static_roots();
#   endif
}

void GC_register_dynamic_libraries() {
    /* Currently does nothing. The callbacks are setup by GC_init_dyld() 
    The dyld library takes it from there. */
}

/* The _dyld_* functions have an internal lock so no _dyld functions
   can be called while the world is stopped without the risk of a deadlock.
   Because of this we MUST setup callbacks BEFORE we ever stop the world.
   This should be called BEFORE any thread in created and WITHOUT the
   allocation lock held. */
   
void GC_init_dyld() {
  static GC_bool initialized = FALSE;
  char *bind_fully_env = NULL;
  
  if(initialized) return;
  
#   ifdef DARWIN_DEBUG
  GC_printf0("Registering dyld callbacks...\n");
#   endif
  
  /* Apple's Documentation:
     When you call _dyld_register_func_for_add_image, the dynamic linker runtime
     calls the specified callback (func) once for each of the images that is
     currently loaded into the program. When a new image is added to the program,
     your callback is called again with the mach_header for the new image, and the 	
     virtual memory slide amount of the new image. 
     
     This WILL properly register already linked libraries and libraries 
     linked in the future
  */
  
    _dyld_register_func_for_add_image(GC_dyld_image_add);
    _dyld_register_func_for_remove_image(GC_dyld_image_remove);

    /* Set this early to avoid reentrancy issues. */
    initialized = TRUE;

    bind_fully_env = getenv("DYLD_BIND_AT_LAUNCH");
    
    if (bind_fully_env == NULL) {
#   ifdef DARWIN_DEBUG
      GC_printf0("Forcing full bind of GC code...\n");
#   endif
      
      if(!_dyld_bind_fully_image_containing_address((unsigned long*)GC_malloc))
        GC_abort("_dyld_bind_fully_image_containing_address failed");
    }

}

#define HAVE_REGISTER_MAIN_STATIC_DATA
GC_bool GC_register_main_static_data()
{
  /* Already done through dyld callbacks */
  return FALSE;
}

#endif /* DARWIN */

#else /* !DYNAMIC_LOADING */

#ifdef PCR

#   include "il/PCR_IL.h"
#   include "th/PCR_ThCtl.h"
#   include "mm/PCR_MM.h"

void GC_register_dynamic_libraries()
{
    /* Add new static data areas of dynamically loaded modules.	*/
        {
          PCR_IL_LoadedFile * p = PCR_IL_GetLastLoadedFile();
          PCR_IL_LoadedSegment * q;
          
          /* Skip uncommited files */
          while (p != NIL && !(p -> lf_commitPoint)) {
              /* The loading of this file has not yet been committed	*/
              /* Hence its description could be inconsistent.  		*/
              /* Furthermore, it hasn't yet been run.  Hence its data	*/
              /* segments can't possibly reference heap allocated	*/
              /* objects.						*/
              p = p -> lf_prev;
          }
          for (; p != NIL; p = p -> lf_prev) {
            for (q = p -> lf_ls; q != NIL; q = q -> ls_next) {
              if ((q -> ls_flags & PCR_IL_SegFlags_Traced_MASK)
                  == PCR_IL_SegFlags_Traced_on) {
                GC_add_roots_inner
                	((char *)(q -> ls_addr), 
                	 (char *)(q -> ls_addr) + q -> ls_bytes,
                	 TRUE);
              }
            }
          }
        }
}


#else /* !PCR */

void GC_register_dynamic_libraries(){}

int GC_no_dynamic_loading;

#endif /* !PCR */

#endif /* !DYNAMIC_LOADING */

#ifndef HAVE_REGISTER_MAIN_STATIC_DATA

/* Do we need to separately register the main static data segment? */
GC_bool GC_register_main_static_data()
{
  return TRUE;
}
#endif /* HAVE_REGISTER_MAIN_STATIC_DATA */

