MAKEFLAGS := $(MAKEFLAGS) --no-builtin-rules

if SUPPORT_SGEN
sgen_dirs = sgen
endif

if BTLS
btls_dirs = btls
endif

if MONO_NATIVE
native_dirs = native
endif

managed_unit_test_dirs = tests
native_unit_test_dirs = unit-tests
culture_dirs = culture

if CROSS_COMPILING
SUBDIRS = $(btls_dirs) $(culture_dirs) eglib arch utils cil zlib $(sgen_dirs) metadata mini dis profiler $(native_dirs)
else
if INSTALL_MONOTOUCH
SUBDIRS = $(btls_dirs) $(culture_dirs) eglib arch utils zlib $(sgen_dirs) metadata mini profiler $(native_dirs)
else
SUBDIRS = $(btls_dirs) $(culture_dirs) eglib arch utils cil zlib $(sgen_dirs) metadata mini dis $(managed_unit_test_dirs) $(native_unit_test_dirs) benchmark profiler $(native_dirs)
endif
endif

DIST_SUBDIRS = btls $(culture_dirs) native eglib arch utils cil zlib $(sgen_dirs) metadata mini dis $(managed_unit_test_dirs) $(native_unit_test_dirs) benchmark profiler
