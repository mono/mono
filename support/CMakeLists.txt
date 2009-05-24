
# Source code which helps implement the ANSI C standards, and thus *should* be
# portable to any platform having a C compiler.
set(MPH_C_SOURCE
	errno.c
	map.c
	map.h
	mph.h
	signal.c
	stdio.c
	string.c
	stdlib.c)

# Source code which helps implement POSIX and other related Unix standards,
# and *may* be portable between Unix platforms.
set(MPH_UNIX_SOURCE
	dirent.c
	fcntl.c
	fstab.c
	grp.c
	macros.c
	old-map.c
	old-map.h
	pwd.c
	serial.c
	sys-mman.c
	sys-sendfile.c
	sys-stat.c
	sys-statvfs.c
	sys-time.c
	sys-utsname.c
	sys-wait.c
	sys-xattr.c
	syslog.c
	time.c
	unistd.c
	utime.c
	x-struct-str.c)

if(PLATFORM_WIN32)
set(MPH_SOURCE ${MPH_C_SOURCE})
set(MPH_LIBS)
else()
set(MPH_SOURCE ${MPH_C_SOURCE} ${MPH_UNIX_SOURCE})
set(MPH_LIBS ${GLIB_LIBS})
endif()

set(MINIZIP_SOURCE 
	minizip/crypt.h
	minizip/ioapi.c
	minizip/ioapi.h
	minizip/unzip.c
	minizip/unzip.h
	minizip/zip.c
	minizip/zip.h)

set(ZLIB_SOURCES 
	adler32.c
	compress.c
	crc32.c	
	gzio.c	
	uncompr.c
	deflate.c
	trees.c	
	zutil.c	
	inflate.c
	infback.c
	inftrees.c
	inffast.c
	crc32.h	
	deflate.h  
	inffast.h  
	inffixed.h  
	inflate.h  
	inftrees.h  
	trees.h  
	zconf.h  
	zlib.h  
	zutil.h)

if(HAVE_ZLIB)
set(Z_SOURCE  zlib_macros.c)
set(Z_LIBS  -lz)
else()
set(Z_SOURCE  zlib_macros.c ${ZLIB_SOURCES})
set(Z_LIBS)
endif()

set(libMonoPosixHelper_la_SOURCES
	${MPH_SOURCE}
	${Z_SOURCE}
	${MINIZIP_SOURCE})

set(libMonoPosixHelper_la_LIBADD
	${MPH_LIBS}
	${Z_LIBS}
	${XATTR_LIB})

# set(libMonoPosixHelper_la_LDFLAGS  -no-undefined -version-info 1:0:1
###set(libMonoPosixHelper_la_LDFLAGS  -no-undefined -avoid-version)
###set(libMonoSupportW_la_LDFLAGS  -no-undefined -avoid-version)

set(libMonoSupportW_la_SOURCES
		supportw.c
		support-heap.c
		supportw.h)

set(libMonoSupportW_la_LIBADD
		${GLIB_LIBS})

include_directories(${top_srcdir})
include_directories(${GLIB2_INCLUDE_DIRS})
set(CMAKE_C_FLAGS "${CFLAGS} ${CPPFLAGS}")
link_directories(../mini)

add_library(MonoPosixHelper SHARED ${libMonoPosixHelper_la_SOURCES})
target_link_libraries(MonoPosixHelper ${libMonoPosixHelper_la_LIBADD})

if(PLATFORM_WIN32)
else()
###set(lib_LTLIBRARIES
###	libMonoPosixHelper.la
###	${SUPPORT})
endif()

# 
# Use this target to refresh the values in map.[ch]
#
add_custom_target(refresh
  COMMAND cp `pkg-config --variable=Programs create-native-map` .
  COMMAND ${top_builddir}/runtime/mono-wrapper create-native-map.exe 
	--autoconf-member=d_off                               
	--autoconf-member=d_reclen                            
	--autoconf-member=d_type                              
	--exclude-native-symbol=Mono_Posix_Stdlib_snprintf    
	--impl-macro=_GNU_SOURCE --impl-macro=_XOPEN_SOURCE   
	--impl-header="<sys/types.h>"                         
	--impl-header="<sys/stat.h>"                          
	--autoconf-header="<sys/time.h>"                      
	--autoconf-header="<sys/poll.h>"                      
	--autoconf-header="<sys/wait.h>"                      
	--autoconf-header="<sys/statvfs.h>"                   
	--autoconf-header="<sys/xattr.h>"                     
	--autoconf-header="<sys/mman.h>"                      
	--autoconf-header="<unistd.h>"                        
	--impl-header="<fcntl.h>"                             
	--impl-header="<signal.h>"                            
	--autoconf-header="<poll.h>"                          
	--autoconf-header="<grp.h>"                           
	--impl-header="<errno.h>"                             
	--autoconf-header="<syslog.h>"                        
	--autoconf-header="<dirent.h>"                        
	--autoconf-header="<utime.h>"                         
	--impl-header="<time.h>"                              
	--impl-header=""mph.h""                             
	--rename-member=st_atime=st_atime_                    
	--rename-member=st_ctime=st_ctime_                    
	--rename-member=st_mtime=st_mtime_                    
	--rename-namespace=Mono.Unix.Native=Mono.Posix        
	--library=MonoPosixHelper                             
	${mcs_topdir}/class/lib/net_2_0/Mono.Posix.dll map
)