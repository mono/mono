# Microsoft Developer Studio Project File - Name="libgc" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Static Library" 0x0104

CFG=libgc - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE
!MESSAGE NMAKE /f "libgc.mak".
!MESSAGE
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE
!MESSAGE NMAKE /f "libgc.mak" CFG="libgc - Win32 Debug"
!MESSAGE
!MESSAGE Possible choices for configuration are:
!MESSAGE
!MESSAGE "libgc - Win32 Release" (based on "Win32 (x86) Static Library")
!MESSAGE "libgc - Win32 Debug" (based on "Win32 (x86) Static Library")
!MESSAGE

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
RSC=rc.exe

!IF  "$(CFG)" == "libgc - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "..\..\..\lib"
# PROP Intermediate_Dir "..\..\..\obj\Release\libgc"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_MBCS" /D "_LIB" /YX /FD /c
# ADD CPP /nologo /W3 /GX /Zi /O2 /I "..\..\include" /FI"stdafx.h" /D "NDEBUG" /D "_LIB" /D "WIN32" /D "_MBCS" /Yu"stdafx.h" /Fd"..\..\..\lib\libgc.pdb" /FD /c
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LIB32=link.exe -lib
# ADD BASE LIB32 /nologo
# ADD LIB32 /nologo
# Begin Special Build Tool
OutDir=.\..\..\..\lib
TargetName=libgc
SOURCE="$(InputPath)"
PostBuild_Cmds=del $(OutDir)\$(TargetName).idb
# End Special Build Tool

!ELSEIF  "$(CFG)" == "libgc - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "..\..\..\lib"
# PROP Intermediate_Dir "..\..\..\obj\Debug\libgc"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_MBCS" /D "_LIB" /YX /FD /GZ /c
# ADD CPP /nologo /W3 /GX /Zi /Od /I "..\..\include" /FI"stdafx.h" /D "_DEBUG" /D "_LIB" /D "WIN32" /D "_MBCS" /Yu"stdafx.h" /Fd"..\..\..\lib\libgcd.pdb" /FD /GZ /c
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo /o"..\..\..\lib/libgcd.bsc"
LIB32=link.exe -lib
# ADD BASE LIB32 /nologo
# ADD LIB32 /nologo /out:"..\..\..\lib\libgcd.lib"
# Begin Special Build Tool
OutDir=.\..\..\..\lib
TargetName=libgcd
SOURCE="$(InputPath)"
PostBuild_Cmds=del $(OutDir)\$(TargetName).idb
# End Special Build Tool

!ENDIF

# Begin Target

# Name "libgc - Win32 Release"
# Name "libgc - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "c;cpp;cc;cxx;tcc;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=..\..\allchblk.c
# End Source File
# Begin Source File

SOURCE=..\..\alloc.c
# End Source File
# Begin Source File

SOURCE=..\..\backgraph.c
# End Source File
# Begin Source File

SOURCE=..\..\blacklst.c
# End Source File
# Begin Source File

SOURCE=..\..\dbg_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\gcj_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\fnlz_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\dyn_load.c
# End Source File
# Begin Source File

SOURCE=..\..\finalize.c
# End Source File
# Begin Source File

SOURCE=..\..\headers.c
# End Source File
# Begin Source File

SOURCE=..\..\mach_dep.c
# End Source File
# Begin Source File

SOURCE=..\..\malloc.c
# End Source File
# Begin Source File

SOURCE=..\..\mallocx.c
# End Source File
# Begin Source File

SOURCE=..\..\mark.c
# End Source File
# Begin Source File

SOURCE=..\..\mark_rts.c
# End Source File
# Begin Source File

SOURCE=..\..\misc.c
# End Source File
# Begin Source File

SOURCE=..\..\extra\msvc_dbg.c
# End Source File
# Begin Source File

SOURCE=..\..\new_hblk.c
# End Source File
# Begin Source File

SOURCE=..\..\obj_map.c
# End Source File
# Begin Source File

SOURCE=..\..\os_dep.c
# End Source File
# Begin Source File

SOURCE=..\..\ptr_chck.c
# End Source File
# Begin Source File

SOURCE=..\..\reclaim.c
# End Source File
# Begin Source File

SOURCE=..\stdafx.c
# ADD CPP /Yc"stdafx.h"
# End Source File
# Begin Source File

SOURCE=..\..\typd_mlc.c
# End Source File
# Begin Source File

SOURCE=..\..\win32_threads.c
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hh;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=..\..\include\gc_allocator.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_backptr.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_cpp.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_gcj.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_inl.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_inline.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_pthread_redirects.h
# End Source File
# Begin Source File

SOURCE=..\..\include\javaxfc.h
# End Source File
# Begin Source File

SOURCE=..\..\include\leak_detector.h
# End Source File
# Begin Source File

SOURCE=..\..\include\private\msvc_dbg.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_alloc_ptrs.h
# End Source File
# Begin Source File

SOURCE=..\..\include\new_gc_alloc.h
# End Source File
# Begin Source File

SOURCE=..\stdafx.h
# End Source File
# Begin Source File

SOURCE=..\..\include\gc_version.h
# End Source File
# Begin Source File

SOURCE=..\..\include\weakpointer.h
# End Source File
# End Group
# End Target
# End Project
