TESTS = t1.exe t2.exe t3.exe t4.exe t5.exe t6.exe t7.exe t8.exe t9.exe t10.exe t11.exe t12.exe t15.exe t16.exe t17.exe t18.exe t19.exe t20.exe t21.exe t22.exe t23.exe t24.exe t25.exe t26.exe t27.exe t28.exe t29.exe t30.exe t31.exe t32.exe t33.exe t34.exe t35.exe t36.exe t37.exe t38.exe t39.exe t40.exe t41.exe t42.exe t43.exe t44.exe t45.exe t46.exe t47.exe
# t13.exe t14.exe

check:	local compare

local: $(TESTS)
	@for i in $(TESTS); do \
		echo running test $$i; \
		MONO_PATH=../../../lib/net_2_0 mono --debug $$i > $$i.result; \
	done

compare:
	@for i in $(TESTS); do \
		echo -n "$$i: "; \
		if diff --strip-trailing-cr $$i.expected $$i.result >/dev/null ; then echo "  OK"; else echo "  FAILED"; fi; \
	done

clean:
	rm -f *.exe *.mdb *.result

t36.exe : t36.cs t36-lib.cs
	gmcs /debug -r:System.Configuration.dll -t:library t36-lib.cs
	gmcs /debug -r:System.Configuration.dll -r:t36-lib.dll t36.cs

t46.exe : t46.cs t46-lib.cs
	gmcs /debug -r:System.Configuration.dll -t:library t46-lib.cs
	gmcs /debug -r:System.Configuration.dll -r:t46-lib.dll /out:$@ Assert.cs t46.cs

%.exe: %.cs
	gmcs /debug /out:$@ Assert.cs $< -r:System.Configuration.dll -r:System.Web.dll -r:System.Data.dll
