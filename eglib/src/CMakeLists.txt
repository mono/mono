
list (APPEND SOURCES
	eglib-remap.h
	sort.frag.h
	glib.h
	garray.c
	gbytearray.c
	gerror.c
	vasprintf.h
	ghashtable.c
	giconv.c
	gmem.c
	gmodule.h
	goutput.c
	gqsort.c
	gstr.c
	gslist.c
	gstring.c
	gptrarray.c
	glist.c
	gqueue.c
	gpath.c
	gshell.c
	gspawn.c
	gfile.c
	gfile-posix.c
	gpattern.c
	gmarkup.c
	gutf8.c
	gunicode.c
	unicode-data.h
)

if (NEED_VASPRINTF)
	list (APPEND SOURCES vasprintf.c)
endif (NEED_VASPRINTF)

if (HOST_WIN32)
	list (APPEND SOURCES
		eglib-config.hw
		gdate-win32.c
		gdir-win32.c
		gfile-win32.c
		gmisc-win32.c
		gmodule-win32.c
		gtimer-win32.c
	)
else ()
	list (APPEND SOURCES
		gdate-unix.c
		gdir-unix.c
		gfile-unix.c
		gmisc-unix.c
		gmodule-unix.c
		gtimer-unix.c
	)
endif (HOST_WIN32)

add_library (eglib-obj OBJECT ${SOURCES})
set_target_properties (eglib-obj PROPERTIES COMPILE_FLAGS "-g -Wall -D_FORTIFY_SOURCE=2")

add_library (eglib SHARED $<TARGET_OBJECTS:eglib-obj>)
add_library (eglib-static STATIC $<TARGET_OBJECTS:eglib-obj>)

if (HOST_WIN32)
	set_target_properties (eglib PROPERTIES LINK_FLAGS "-lm -lpsapi ${LIBICONV}")
	set_target_properties (eglib-static PROPERTIES LINK_FLAGS "-lm -lpsapi ${LIBICONV}")
elseif (PLATFORM_ANDROID)
	set_target_properties (eglib PROPERTIES LINK_FLAGS "-llog")
	set_target_properties (eglib-static PROPERTIES LINK_FLAGS "-llog ${LIBICONV}")
else ()
	set_target_properties (eglib-static PROPERTIES LINK_FLAGS "${LIBICONV}")
endif (HOST_WIN32)
