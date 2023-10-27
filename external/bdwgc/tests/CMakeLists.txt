#
# Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
# Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
# Copyright (c) 1998 by Fergus Henderson.  All rights reserved.
# Copyright (c) 2000-2010 by Hewlett-Packard Company.  All rights reserved.
##
# THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
# OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
##
# Permission is hereby granted to use or copy this program
# for any purpose,  provided the above notices are retained on all copies.
# Permission to modify the code and to distribute modified code is granted,
# provided the above notices are retained, and a notice that the code was
# modified is included with the above copyright notice.
##

ADD_DEFINITIONS(-DGC_NOT_DLL)

# Compile some tests as C++ to test extern "C" in header files.
SET_SOURCE_FILES_PROPERTIES(
    leak_test.c
    test.c
    PROPERTIES LANGUAGE CXX)

ADD_EXECUTABLE(gctest WIN32 test.c)
TARGET_LINK_LIBRARIES(gctest gc-lib)
ADD_TEST(NAME gctest COMMAND gctest)

ADD_EXECUTABLE(hugetest huge_test.c)
TARGET_LINK_LIBRARIES(hugetest gc-lib)
ADD_TEST(NAME hugetest COMMAND hugetest)

ADD_EXECUTABLE(leaktest leak_test.c)
TARGET_LINK_LIBRARIES(leaktest gc-lib)
ADD_TEST(NAME leaktest COMMAND leaktest)

ADD_EXECUTABLE(middletest middle.c)
TARGET_LINK_LIBRARIES(middletest gc-lib)
ADD_TEST(NAME middletest COMMAND middletest)

ADD_EXECUTABLE(realloc_test realloc_test.c)
TARGET_LINK_LIBRARIES(realloc_test gc-lib)
ADD_TEST(NAME realloc_test COMMAND realloc_test)

ADD_EXECUTABLE(smashtest smash_test.c)
TARGET_LINK_LIBRARIES(smashtest gc-lib)
ADD_TEST(NAME smashtest COMMAND smashtest)
