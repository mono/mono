
list (APPEND SOURCES
	test.c
	test.h
	tests.h
	driver.c
	hashtable.c
	string-util.c
	string.c
	slist.c
	sizes.c
	ptrarray.c
	list.c
	array.c
	fake.c
	path.c
	queue.c
	shell.c
	spawn.c
	timer.c
	file.c
	pattern.c
	dir.c
	markup.c
	unicode.c
	utf8.c
	endian.c
	module.c
	memory.c
)

add_executable (test-eglib EXCLUDE_FROM_ALL ${SOURCES})
set_target_properties (test-eglib PROPERTIES COMPILE_FLAGS "-Wall -DEGLIB_TESTS=1 -D_FORTIFY_SOURCE=2 -DDRIVER_NAME=\\\"EGlib\\\"")
target_link_libraries (test-eglib eglib-static)
