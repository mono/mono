#ifndef MONO_UTILS_WARD_H
#define MONO_UTILS_WARD_H

#ifdef __WARD__
#define MONO_PERMIT(...) __attribute__ ((permission (__VA_ARGS__)))
#else
#define MONO_PERMIT(...)
#endif

/* Add Ward permissions for external functions (e.g., libc, glib) here. */

#endif
