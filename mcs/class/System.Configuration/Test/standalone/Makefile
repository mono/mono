thisdir = class/System.Configuration/Test/standalone
SUBDIRS = 
include ../../../../build/rules.make

TESTS = t1.exe t2.exe t3.exe t4.exe t5.exe t6.exe t7.exe t8.exe t9.exe t10.exe t11.exe t12.exe t15.exe t16.exe t17.exe t18.exe t19.exe t20.exe t21.exe t22.exe t23.exe t24.exe t25.exe t28.exe t29.exe t30.exe t31.exe t32.exe t33.exe t34.exe t35.exe t36.exe t37.exe t38.exe t39.exe t40.exe t41.exe t42.exe t43.exe t44.exe t45.exe t46.exe t47.exe t48.exe
# t13.exe t14.exe t26.exe t27.exe

check:	local compare

local: $(TESTS)
	@for i in $(TESTS); do \
		echo running test $$i; \
		MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $$i > $$i.result; \
	done

compare:
	@ECODE=0; \
	for i in $(TESTS); do \
		printf "$$i: "; \
		if diff --strip-trailing-cr $$i.expected $$i.result >/dev/null ; then echo "  OK"; else echo "  FAILED"; ECODE=1; fi; \
	done; \
	exit $$ECODE

clean:
	rm -f *.exe *.mdb *.result

t36.exe : t36.cs t36-lib.cs
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/System.Configuration.dll -t:library t36-lib.cs
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/System.Configuration.dll -r:t36-lib.dll t36.cs

t46.exe : t46.cs t46-lib.cs
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/System.Configuration.dll -t:library t46-lib.cs
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/System.Configuration.dll -r:t46-lib.dll /out:$@ Assert.cs t46.cs

%.exe: %.cs
	$(CSCOMPILE) /out:$@ Assert.cs $< -r:$(topdir)/class/lib/$(PROFILE)/System.Configuration.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Web.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Data.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Xml.dll

