VERSION=0.13.99

DIRS=jay nant mcs class nunit nunit20 monoresgen tools mbas
INSTALL= /usr/bin/install

all: 
	if test x$(OS) = xWindows_NT; then make linux; else make -f makefile.gnu; fi

install:
	if test x$(OS) = xWindows_NT; then make windowsinstall; else make -f makefile.gnu install; fi

windows:
	for i in $(DIRS); do 			\
		(cd $$i; make linux) || exit 1;	\
	done

linux:
	for i in $(DIRS); do 			\
		(cd $$i; make linux) || exit 1;	\
	done

test:
	if test x$(OS) = xWindows_NT; then make testwindows; else make -f makefile.gnu test; fi

testcorlib:
	if test x$(OS) = xWindows_NT; then make testcorlibwindows; else make -f makefile.gnu testcorlib; fi

testwindows:
	$(MAKE) -C nunit
	$(MAKE) -C nunit20
	$(MAKE) -C class test

testcorlibwindows:
	$(MAKE) -C class testcorlib

clean:
	if test x$(OS) = xWindows_NT; then make cleanwindows; else make cleanlinux; fi

cleanwindows:
	for i in $(DIRS); do 			\
		(cd $$i; make clean)		\
	done

cleanlinux:
	for i in $(DIRS); do 			\
		(cd $$i; make -f makefile.gnu clean)		\
	done

dist: 
	(c=`pwd`; d=`basename $$c`; cd ..; cp -a $$d mcs-$(VERSION); cd mcs-$(VERSION); make clean; cd ..; \
	tar czvf $$d/mcs-$(VERSION).tar.gz --exclude=CVS --exclude='.#*' --exclude=core --exclude='*~' --exclude='*.exe' mcs-$(VERSION); \
	rm -rf mcs-$(VERSION))

windowsinstall:
	if test x$$prefix = x; then		\
		echo Usage is: make install prefix=X:/cygwin/home/MyHome/mono/install;  exit 1;	\
	fi
	mkdir -p $(prefix)/bin/
	for iexe in $(MONO_WIN_INSTALL_BIN) ; do					\
		echo Installing exe $$iexe;	\
		($(INSTALL) -m 755 $$iexe $(prefix)/bin/) || exit 1;	\
		sed -e 's^\@bindir\@^$(prefix)^g' -e "s^\\@thewindowexe\\@^`basename \"$$iexe\"`^g" < ./winexe.in > ./winexe.tmp;	\
		mv ./winexe.tmp ./$$iexe.sh;	\
		($(INSTALL) -m 755 $$iexe.sh $(prefix)/bin/) || exit 1;		\
	done
	mkdir -p $(prefix)/lib/
	for idll in $(MONO_WIN_INSTALL_LIB) ; do				\
		echo Installing dll $$idll;	\
		($(INSTALL) -m 755 $$idll $(prefix)/lib/) || exit 1;	\
	done

MONO_WIN_INSTALL_LIB=	\
	class/lib/I18N.CJK.dll	\
	class/lib/I18N.MidEast.dll	\
	class/lib/I18N.Other.dll	\
	class/lib/I18N.Rare.dll	\
	class/lib/I18N.West.dll	\
	class/lib/I18N.dll	\
	class/lib/Microsoft.VisualBasic.dll	\
	class/lib/Microsoft.VisualC.dll	\
	class/lib/Mono.Data.MySql.dll	\
	class/lib/Mono.Data.PostgreSqlClient.dll	\
	class/lib/Mono.Data.SqliteClient.dll	\
	class/lib/Mono.Data.SybaseClient.dll	\
	class/lib/Mono.Data.Tds.dll	\
	class/lib/Mono.Data.TdsClient.dll	\
	class/lib/Mono.PEToolkit.dll	\
	class/lib/System.Configuration.Install.dll	\
	class/lib/System.Data.dll	\
	class/lib/System.Drawing.dll	\
	class/lib/System.EnterpriseServices.dll	\
	class/lib/System.Runtime.Remoting.dll	\
	class/lib/System.Runtime.Serialization.Formatters.Soap.dll	\
	class/lib/System.Web.Services.dll	\
	class/lib/System.Web.dll	\
	class/lib/System.Xml.dll	\
	class/lib/System.dll	\
	class/lib/corlib.dll	\
	class/lib/corlib_cmp.dll	\
	nunit/NUnitBase.dll	\
	nunit/NUnitCore.dll	\
	nunit/NUnitCore_mono.dll

MONO_WIN_INSTALL_BIN=	\
	mcs/mcs.exe	\
	mbas/mbas.exe	\
	nant/NAnt.exe	\
	nunit/NUnitConsole.exe	\
	nunit/NUnitConsole_mono.exe	\
	ilasm/ilasm.exe	\
	monoresgen/monoresgen.exe	\
	tools/EnumCheck.exe	\
	tools/IFaceDisco.exe	\
	tools/verifier.exe	\
	tools/GenerateDelegate.exe	\
	tools/monostyle.exe	\
	tools/SqlSharp/sqlsharp.exe	\
	tools/corcompare/CorCompare.exe
