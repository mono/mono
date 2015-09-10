SUBDIRS = assemblyresolve gc-descriptors

check-local: assemblyresolve/test/asm.dll testjit test-generic-sharing test-type-load test-cattr-type-load test-reflection-load-with-context test_platform test-process-exit test-messages test-unhandled-exception-2 rm-empty-logs
check-full: test-sgen check-local
check-parallel: compile-tests check-full

# for backwards compatibility on Wrench
test-wrench: check-parallel

aotcheck: testaot gshared-aot

TEST_PROG = ../interpreter/mint

JITTEST_PROG = $(if $(VALGRIND), valgrind $(VALGRIND_ARGS),) $(if $(SGEN),$(top_builddir)/mono/mini/mono-sgen,$(top_builddir)/mono/mini/mono)

TEST_PROG_RUN = MONO_CFG_DIR=$(mono_build_root)/runtime/etc $(LIBTOOL) --mode=execute $(TEST_PROG)
JITTEST_PROG_RUN = MONO_CFG_DIR=$(mono_build_root)/runtime/etc $(LIBTOOL) --mode=execute $(JITTEST_PROG)

RUNTIME_ARGS=--config tests-config --optimize=all --debug

CLASS=$(mcs_topdir)/class/lib/$(DEFAULT_PROFILE)

with_mono_path = MONO_PATH=$(CLASS)

RUNTIME = $(with_mono_path) $(top_builddir)/runtime/mono-wrapper

MKBUNDLE = \
	PKG_CONFIG_PATH=$(top_builddir):$(PKG_CONFIG_PATH) \
	$(RUNTIME) $(CLASS)/mkbundle.exe

MCS = $(RUNTIME) $(mcs_topdir)/class/lib/build/mcs.exe -unsafe -debug \
	-nowarn:0162 -nowarn:0168 -nowarn:0219 -nowarn:0414 -nowarn:0618 \
	-nowarn:0169 -nowarn:1690 -nowarn:0649 -nowarn:0612 -nowarn:3021 \
	-nowarn:0197
ILASM = $(RUNTIME) $(CLASS)/ilasm.exe

BENCHSRC=fib.cs random.cs nested-loops.cs ackermann.cs tight-loop.cs sieve.cs

STRESS_TESTS_SRC=	\
	abort-stress-1.cs	\
	abort-stress-2.cs	\
	abort-stress-3.cs	\
	domain-stress.cs	\
	gchandle-stress.cs	\
	monitor-stress.cs	\
	thread-stress.cs	\
	gc-stress.cs		\
	gc-copy-stress.cs	\
	gc-graystack-stress.cs		\
	exit-stress.cs		\
	process-stress.cs	\
	assembly-load-stress.cs

# Disabled until ?mcs is fixed
#	bug-331958.cs
BASE_TEST_CS_SRC=		\
	bug-2907.cs		\
	array-init.cs		\
	arraylist.cs		\
	assemblyresolve_event.cs	\
	assemblyresolve_event3.cs	\
	assemblyresolve_event4.cs	\
	checked.cs		\
	char-isnumber.cs	\
	create-instance.cs	\
	field-layout.cs		\
	pack-layout.cs		\
	pack-bug.cs		\
	hash-table.cs		\
	test-ops.cs		\
	obj.cs			\
	test-dup-mp.cs		\
	string.cs		\
	stringbuilder.cs	\
	switch.cs		\
	outparm.cs		\
	delegate.cs		\
	bitconverter.cs		\
	exception.cs		\
	exception2.cs		\
	exception3.cs		\
	exception4.cs		\
	exception5.cs		\
	exception6.cs		\
	exception7.cs		\
	exception8.cs		\
	exception10.cs		\
	exception11.cs		\
	exception12.cs		\
	exception13.cs		\
	exception14.cs		\
	exception15.cs		\
	exception16.cs		\
	exception17.cs		\
	exception18.cs		\
	typeload-unaligned.cs	\
	struct.cs		\
	valuetype-gettype.cs	\
	typeof-ptr.cs		\
	static-constructor.cs	\
	pinvoke.cs		\
	pinvoke2.cs		\
	pinvoke3.cs		\
	pinvoke11.cs		\
	pinvoke13.cs		\
	pinvoke17.cs		\
	invoke.cs		\
	invoke2.cs		\
	runtime-invoke.cs		\
	invoke-string-ctors.cs		\
	reinit.cs		\
	box.cs			\
	array.cs		\
	enum.cs			\
	enum2.cs		\
	enum-intrins.cs		\
	property.cs		\
	enumcast.cs		\
	assignable-tests.cs	\
	array-cast.cs		\
	array-subtype-attr.cs	\
	cattr-compile.cs	\
	cattr-field.cs		\
	cattr-object.cs		\
	custom-attr.cs		\
	double-cast.cs		\
	newobj-valuetype.cs	\
	arraylist-clone.cs	\
	setenv.cs		\
	vtype.cs		\
	isvaluetype.cs		\
	iface6.cs		\
	iface7.cs		\
	ipaddress.cs		\
	array-vt.cs		\
	interface1.cs		\
	reflection-enum.cs	\
	reflection-prop.cs	\
	reflection4.cs		\
	reflection5.cs		\
	reflection-const-field.cs \
	many-locals.cs		\
	string-compare.cs	\
	test-prime.cs		\
	test-tls.cs		\
	params.cs		\
	reflection.cs		\
	interface.cs		\
	iface.cs		\
	iface2.cs		\
	iface3.cs		\
	iface4.cs		\
	iface-large.cs		\
	virtual-method.cs	\
	intptrcast.cs		\
	indexer.cs		\
	stream.cs		\
	console.cs		\
	shift.cs		\
	jit-int.cs		\
	jit-uint.cs		\
	jit-long.cs		\
	long.cs			\
	jit-ulong.cs		\
	jit-float.cs		\
	pop.cs			\
	time.cs			\
	appdomain.cs        \
	appdomain1.cs		\
	appdomain2.cs		\
	appdomain-client.cs	\
	appdomain-unload.cs \
	appdomain-async-invoke.cs	\
	loader.cs       \
	pointer.cs		\
	hashcode.cs		\
	delegate1.cs		\
	delegate2.cs		\
	delegate3.cs		\
	delegate5.cs		\
	delegate6.cs		\
	delegate7.cs		\
	delegate8.cs		\
	delegate9.cs		\
	delegate10.cs		\
	delegate11.cs		\
	delegate12.cs		\
	remoting1.cs		\
	remoting2.cs		\
	remoting3.cs		\
	remoting4.cs		\
	remoting5.cs		\
	largeexp.cs		\
	largeexp2.cs		\
	marshalbyref1.cs	\
	static-ctor.cs		\
	inctest.cs		\
	bound.cs		\
	array-invoke.cs		\
	test-arr.cs		\
	decimal.cs		\
	decimal-array.cs	\
	marshal.cs		\
	marshal1.cs		\
	marshal2.cs		\
	marshal3.cs		\
	marshal5.cs		\
	marshal6.cs		\
	marshal7.cs		\
	marshal8.cs		\
	marshal9.cs		\
	marshalbool.cs		\
	marshal-valuetypes.cs	\
	test-byval-in-struct.cs	\
	thread.cs		\
	thread5.cs		\
	thread6.cs		\
	thread-static.cs	\
	thread-static-init.cs	\
	context-static.cs	\
	float-pop.cs		\
	interfacecast.cs	\
	array3.cs		\
	classinit.cs		\
	classinit2.cs		\
	classinit3.cs		\
	synchronized.cs		\
	async_read.cs		\
	threadpool.cs		\
	threadpool1.cs		\
	threadpool-exceptions1.cs \
	threadpool-exceptions2.cs \
	threadpool-exceptions3.cs \
	threadpool-exceptions4.cs \
	threadpool-exceptions5.cs \
	threadpool-exceptions6.cs \
	threadpool-exceptions7.cs \
	base-definition.cs	\
	bug-27420.cs		\
	bug-47295.cs		\
	bug-46781.cs		\
	bug-48015.cs		\
	bug-42136.cs		\
	bug-59286.cs		\
	bug-70561.cs		\
	bug-78311.cs		\
	bug-78653.cs		\
	bug-78656.cs		\
	bug-77127.cs		\
	bug-323114.cs		\
	bug-Xamarin-5278.cs	\
	interlocked.cs		\
	cross-domain.cs		\
	appdomain-exit.cs	\
	delegate-async-exit.cs	\
	delegate-delegate-exit.cs	\
	delegate-exit.cs	\
	finalizer-abort.cs	\
	finalizer-exception.cs	\
	finalizer-exit.cs	\
	finalizer-thread.cs	\
	main-exit.cs	\
	main-returns-abort-resetabort.cs	\
	main-returns-background-abort-resetabort.cs	\
	main-returns-background-resetabort.cs	\
	main-returns-background.cs	\
	main-returns-background-change.cs	\
	main-returns.cs		\
	subthread-exit.cs	\
	desweak.cs		\
	cominterop.cs		\
	exists.cs		\
	handleref.cs	\
	transparentproxy.cs \
	dbnull-missing.cs	\
	test-type-ctor.cs 	\
	soft-float-tests.cs	\
	thread-exit.cs		\
	finalize-parent.cs	\
	assemblyresolve_event2.2.cs	\
	interlocked-2.2.cs	\
	pinvoke-2.2.cs 		\
	bug-78431.2.cs 		\
	bug-79684.2.cs 		\
	catch-generics.2.cs	\
	event-get.2.cs		\
	safehandle.2.cs		\
	stackframes-async.2.cs		\
	module-cctor-loader.2.cs	\
	generics-invoke-byref.2.cs	\
	generic-signature-compare.2.cs	\
	generics-sharing.2.cs	\
	shared-generic-methods.2.cs	\
	shared-generic-synchronized.2.cs	\
	generic-inlining.2.cs	\
	generic-initobj.2.cs	\
	generic-delegate.2.cs	\
	generic-sizeof.2.cs	\
	generic-virtual.2.cs	\
	generic-interface-methods.2.cs	\
	generic-array-type.2.cs	\
	generic-method-patching.2.cs	\
	generic-static-methods.2.cs	\
	generic-null-call.2.cs	\
	generic-special.2.cs	\
	generic-exceptions.2.cs	\
	generic-virtual2.2.cs	\
	generic-valuetype-interface.2.cs	\
	generic-getgenericarguments.2.cs	\
	generic-type-builder.2.cs	\
	generic-synchronized.2.cs	\
	generic-delegate-ctor.2.cs	\
	generic-array-iface-set.2.cs	\
	generic-typedef.2.cs	\
	generic-marshalbyref.2.cs	\
	generic-xdomain.2.cs	\
	dynamic-generic-size.cs	\
	bug-431413.2.cs	\
	bug-459285.2.cs	\
	generic-virtual-invoke.2.cs	\
	bug-461198.2.cs	\
	generic-sealed-virtual.2.cs	\
	generic-system-arrays.2.cs	\
	generic-stack-traces.2.cs	\
	generic-stack-traces2.2.cs	\
	bug-472600.2.cs	\
	recursive-generics.2.cs	\
	bug-473482.2.cs	\
	bug-473999.2.cs	\
	bug-479763.2.cs	\
	bug-616463.cs	\
	bug-80392.2.cs		\
	dynamic-method-access.2.cs	\
	dynamic-method-finalize.2.cs	\
	dynamic-method-stack-traces.cs	\
	bug-82194.2.cs	\
	anonarray.2.cs	\
	ienumerator-interfaces.2.cs	\
	array-enumerator-ifaces.2.cs	\
	generic_type_definition_encoding.2.cs \
	generic_type_definition.2.cs	\
	bug-333798.2.cs		\
	bug-333798-tb.2.cs		\
	bug-335131.2.cs		\
	bug-322722_patch_bx.2.cs		\
	bug-348522.2.cs		\
	bug-340662_bug.cs	\
	bug-322722_dyn_method_throw.2.cs	\
	bug-389886-2.cs	\
	bug-325283.2.cs	\
	thunks.cs \
	winx64structs.cs \
	bug-349190.2.cs	\
	nullable_boxing.2.cs	\
	valuetype-equals.cs	\
	custom-modifiers.2.cs	\
	bug-382986.cs	\
	test-inline-call-stack.cs	\
	bug-324535.cs	\
	modules.cs	\
	bug-81673.cs	\
	bug-81691.cs	\
	bug-80307.cs	\
	bug-415577.cs	\
	filter-stack.cs	\
	vararg2.cs	\
	bug-389886-sre-generic-interface-instances.cs	\
	bug-461867.cs	\
	bug-461941.cs	\
	bug-461261.cs	\
	bug-400716.cs	\
	bug-462592.cs	\
	bug-459094.cs	\
	generic-unloading.2.cs	\
	generic-unloading-sub.2.cs	\
	bug-467456.cs	\
	appdomain-unload-callback.cs	\
	bug-508538.cs	\
	bug-472692.2.cs		\
	gchandles.cs	\
	interlocked-3.cs	\
	interlocked-4.2.cs	\
	appdomain-thread-abort.cs \
	xdomain-threads.cs	\
	w32message.cs	\
	bug-544446.cs	\
	gc-altstack.cs	\
	large-gc-bitmap.cs	\
	bug-561239.cs	\
	bug-562150.cs	\
	bug-575941.cs	\
	bug-599469.cs	\
	bug-389886-3.cs \
	monitor.cs	\
	monitor-resurrection.cs	\
	dynamic-method-resurrection.cs	\
	bug-666008.cs	\
	bug-685908.cs	\
	sgen-long-vtype.cs	\
	delegate-invoke.cs	\
	bug-696593.cs	\
	bug-705140.cs	\
	bug-1147.cs	\
	mono-path.cs	\
	bug-bxc-795.cs	\
	bug-3903.cs	\
	async-with-cb-throws.cs	\
	appdomain-unload-doesnot-raise-pending-events.cs	\
	bug-6148.cs	\
	assembly_append_ordering.cs	\
	bug-10127.cs	\
	bug-18026.cs	\
	allow-synchronous-major.cs	\
	unload-appdomain-on-shutdown.cs	\
	block_guard_restore_aligment_on_exit.cs	\
	thread_static_gc_layout.cs \
	sleep.cs \
	bug-27147.cs	\
	bug-30085.cs	\
	bug-17537.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Add1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/addref.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/And1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/AndRef.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Args4.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Args5.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/AsgAdd1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/AsgAnd1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/AsgOr1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/AsgSub1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/AsgXor1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/BinaryRMW.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Call1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/CnsBool.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/CnsLng1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblAdd.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblAddConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblArea.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblArray.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblAvg2.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblAvg6.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblCall1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblCall2.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblDist.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblDiv.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblDivConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblFillArray.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblMul.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblMulConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblNeg.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblRem.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblRoots.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblSub.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblSubConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/DblVar.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/div1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/div2.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/divref.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Eq1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FactorialRec.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FibLoop.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FiboRec.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPAdd.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPAddConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPArea.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPArray.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPAvg2.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPAvg6.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPCall1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPCall2.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPConvDbl2Lng.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPConvF2F.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPConvF2I.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPConvF2Lng.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPConvI2F.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPDist.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPDiv.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPDivConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPError.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPFillArray.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPMath.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPMul.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPMulConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPNeg.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPRem.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPRoots.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPSmall.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPSub.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPSubConst.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/FPVar.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Gcd.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Ge1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Gt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Ind1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/InitObj.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/InstanceCalls.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/IntArraySum.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/IntConv.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Jmp1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrue1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueEqDbl.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueEqFP.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueEqInt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueGeDbl.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueGeFP.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueGeInt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueGtDbl.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueGtFP.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueGtInt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueLeDbl.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueLeFP.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueLeInt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueLtDbl.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueLtFP.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueLtInt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueNeDbl.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueNeFP.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/JTrueNeInt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Le1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/LeftShift.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/LngConv.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Localloc.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/LocallocLarge.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/LongArgsAndReturn.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Lt1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/mul1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/mul2.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/mul3.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/mul4.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Ne1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/NegRMW.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/NestedCall.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/NotAndNeg.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/NotRMW.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/ObjAlloc.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/OpMembersOfStructLocal.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Or1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/OrRef.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/rem1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/RightShiftRef.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/StaticCalls.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/StaticValueField.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/StructFldAddr.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/StructInstMethod.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Sub1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/SubRef.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Swap.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Switch.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Unbox.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/Xor1.cs	\
	ms-coreclr-tests/src/JIT/CodeGenBringUpTests/XorRef.cs	\
	ms-coreclr-tests/src/JIT/Directed/Arrays/complex1.cs	\
	ms-coreclr-tests/src/JIT/Directed/Arrays/complex2.cs	\
	ms-coreclr-tests/src/JIT/Directed/Arrays/simple1.cs	\
	ms-coreclr-tests/src/JIT/Directed/Arrays/simple2.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Base_1.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Base_2.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Base_3.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Base_4.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Base_6.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Peer_1.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Peer_2.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Peer_3.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Peer_4.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Generic_Test_CSharp_Peer_6.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Base_1.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Base_2.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Base_3.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Base_4.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Peer_1.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Peer_2.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Peer_3.cs	\
	ms-coreclr-tests/src/JIT/Directed/CheckedCtor/Test_CSharp_Peer_4.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Bool_And_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Bool_No_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Bool_Or_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Bool_Xor_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Double_And_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Double_No_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Double_Or_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Double_Xor_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Float_And_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Float_No_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Float_Or_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Float_Xor_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Int_And_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Int_No_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Int_Or_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/cmov/Int_Xor_Op.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/flowgraph/xaddmuly.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/33objref.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/cse1.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/cse2.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/lclfldadd.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/lclflddiv.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/lclfldmul.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/lclfldrem.cs	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/lclfldsub.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/interlocked/cmpxchg.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/interlocked/cse_cmpxchg.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/interlocked/IntrinsicTest_Overflow.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/interlocked/nullchecksuppress.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/interlocked/regalloc1.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/interlocked/regalloc2.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/pow/pow0.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/pow/pow1.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/pow/pow2.cs	\
	ms-coreclr-tests/src/JIT/Directed/intrinsic/pow/pow3.cs	\
	ms-coreclr-tests/src/JIT/Directed/lifetime/lifetime1.cs	\
	ms-coreclr-tests/src/JIT/Directed/lifetime/lifetime2.cs	\
	ms-coreclr-tests/src/JIT/Directed/localloc/localloc3.cs	\
	ms-coreclr-tests/src/JIT/Directed/newarr/newarr.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/int16.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/int32.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/int64.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/uint8.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/uint16.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/uint32.cs	\
	ms-coreclr-tests/src/JIT/Directed/shift/uint64.cs	\
	ms-coreclr-tests/src/JIT/Directed/StrAccess/straccess1.cs	\
	ms-coreclr-tests/src/JIT/Directed/StrAccess/straccess2.cs	\
	ms-coreclr-tests/src/JIT/Directed/StrAccess/straccess3.cs	\
	ms-coreclr-tests/src/JIT/Directed/StrAccess/straccess4.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP1.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP1a.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP1a2.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP1b.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP1c.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP1d.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP2.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP2a.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP2b.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SP2c.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SpAddr.cs	\
	ms-coreclr-tests/src/JIT/Directed/StructPromote/SpAddrAT.cs	\
	ms-coreclr-tests/src/JIT/Directed/TypedReference/TypedReference.cs	\
	ms-coreclr-tests/src/JIT/Directed/UnrollLoop/Dev10_846218.cs	\
	ms-coreclr-tests/src/JIT/Directed/UnrollLoop/loop1.cs	\
	ms-coreclr-tests/src/JIT/Directed/UnrollLoop/loop4.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class01_instance.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class01_static.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class04.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class05.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class06.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/class07.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struc01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct01_static.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct04.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct05.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct06.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/struct07.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/Struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/Jagged/Struct01_instance.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/MultiDim/class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/MultiDim/class01_Instance.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/MultiDim/class01_static.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/MultiDim/struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/MultiDim/struct01_Instance.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/ConstructedTypes/MultiDim/struct01_static.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/TypeParameters/Jagged/class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/TypeParameters/Jagged/struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/TypeParameters/MultiDim/class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Arrays/TypeParameters/MultiDim/struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/class1.cs	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/class2.cs	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt1.cs	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt2.cs	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt3.cs	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt4.cs	\
	ms-coreclr-tests/src/JIT/Generics/Constraints/Call_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Constraints/Call_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Constraints/Convert_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Constraints/Convert_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Constraints/Transitive_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Constraints/Transitive_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Boxing/box_unbox01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/GenToGen01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/GenToGen02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/GenToGen03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/GenToNonGen01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/GenToNonGen02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/GenToNonGen03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/NonGenToGen01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/NonGenToGen02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Conversions/Reference/NonGenToGen03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Coverage/chaos55915408cs.cs	\
	ms-coreclr-tests/src/JIT/Generics/Coverage/chaos56200037cs.cs	\
	ms-coreclr-tests/src/JIT/Generics/Coverage/chaos65204782cs.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/general_class_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/general_class_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/general_struct_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/general_struct_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_class_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_class_instance02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_class_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_class_static02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_struct_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_struct_instance02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_struct_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Exceptions/specific_struct_static02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/instance_assignment_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/instance_assignment_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/instance_equalnull_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/instance_equalnull_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/instance_passing_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/instance_passing_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/static_assignment_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/static_assignment_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/static_equalnull_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/static_equalnull_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/static_passing_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Fields/static_passing_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/Baseclass04.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/Baseclass05.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/BaseClass01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/BaseClass02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/BaseClass03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/class02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Classes/class03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Class02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Class03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Class04.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Class05.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Struct02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Struct03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Struct04.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Interfaces/Struct05.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Structs/struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Structs/struct02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Instantiation/Structs/struct03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/instance_assignment_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/instance_assignment_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/instance_equalnull_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/instance_equalnull_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/instance_passing_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/instance_passing_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/static_assignment_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/static_assignment_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/static_equalnull_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/static_equalnull_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/static_passing_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Locals/static_passing_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/class_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/class_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/interface_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/interface_class02.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/interface_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/interface_struct02.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/struct_instance01.cs	\
	ms-coreclr-tests/src/JIT/Generics/MemberAccess/struct_static01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/instance_assignment_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/instance_assignment_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/instance_equalnull_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/instance_equalnull_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/instance_passing_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/instance_passing_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/static_assignment_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/static_assignment_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/static_equalnull_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/static_equalnull_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/static_passing_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Parameters/static_passing_struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/class02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/class03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/dynamicTypes.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/objectBoxing.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/refTypesdynamic.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/struct01.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/struct03.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/Struct02.cs	\
	ms-coreclr-tests/src/JIT/Generics/Typeof/valueTypeBoxing.cs	\
	ms-coreclr-tests/src/JIT/Generics/TypeParameters/default_class01.cs	\
	ms-coreclr-tests/src/JIT/Generics/TypeParameters/default_struct01.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/9_and_alloca2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/9param.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/148343.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/eh1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/fgtest1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/fgtest2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/funclet.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/gcparaminreg.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/ret_struct_test1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/ret_struct_test4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/simple1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct1_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct1_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct1_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct2_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct2_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct2_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct2_5_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct3_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct3_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct3_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct4_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct4_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct4_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct5_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct5_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct5_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct6.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct6_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct6_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct6_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct7_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct8.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct9.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/struct9_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp1_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp1_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp1_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp1_5.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp1_6.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp2_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp2_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp2_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp2_4.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp3_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp4_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp5_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfp6_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfpseh5_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structfpseh6_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structref1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret1_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret1_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret2_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret2_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret2_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret3_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret3_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret3_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret4_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret4_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret4_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret5_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret5_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret5_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret6_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret6_2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structret6_3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/structva1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/test1.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/test2.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/test3.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/test_noalloca.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/misc/vbil.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/regress/vswhidbey/143837.cs	\
	ms-coreclr-tests/src/JIT/jit64/gc/regress/vswhidbey/339415.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/CGRecurse/CGRecurseAAA.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/CGRecurse/CGRecurseAAC.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/CGRecurse/CGRecurseACA.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/CGRecurse/CGRecurseACC.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/cgstress/CgStress1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/cgstress/CgStress2.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/cgstress/CgStress3.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/arrayexpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/arrayexpr2.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/fieldexpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/fieldexpr1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/fieldexpr2.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/fieldExprUnchecked1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/HugeArray.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/HugeArray1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/hugeexpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/HugeField1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/HugeField2.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/hugeSimpleExpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/mixedexpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/pointerexpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/pointerexpr1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/simpleexpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/simpleexpr1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/simpleexpr2.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/simpleexpr3.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/simpleexpr4.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/staticFieldExpr1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/staticFieldExpr1_1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/staticFieldExprUnchecked1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/volatilefield.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/cse/volatilestaticfield.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/inl/caninline.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/lim/lim_002.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/lur/lur_02.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/osr/osr001.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/ArrayBound.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/ArrayWith2Loops.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/ArrayWithFunc.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/BadMatrixMul.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/JaggedArray.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/MatrixMul.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/RngchkStress1.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/RngchkStress2.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/RngchkStress3.cs	\
	ms-coreclr-tests/src/JIT/jit64/opt/rngchk/SimpleArray_01.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/asurt/143616/foo.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/ddb/103087/103087.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/ddb/113574/113574.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/ddb/132977/132977.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/ndpw/160545/simple.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/ndpw/21015/interior_pointer.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/102964/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/329169/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/373472/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/471729/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/517867/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/524070/test1.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/524070/test2.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/528315/simple-repro.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/538615/test1.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/539509/test1.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/541067/test1.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/543229/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/549880/test.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/601425/stret.cs	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/610378/BigFrame.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcs.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcs2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcsbas.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcsbox.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcsmax.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcsmixed.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcsval.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcsvalbox.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/misc/arrres.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/misc/gcarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/misc/selfref.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/i4/i4.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/i4/i4flat.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/i8/i8.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/i8/i8flat.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/r4/r4.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/r4/r4flat.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/r8/r8.cs	\
	ms-coreclr-tests/src/JIT/Methodical/AsgOp/r8/r8flat.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/misc/enum.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/misc/nestval.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/misc/tailjump.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_call.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_ldarg.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_ldloc.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_newobj.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_call.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_ldarg.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_ldloc.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_newobj.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/iface/iface1.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/ilseq/commonBase.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/cast_throw.cs	\
	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/throw.cs	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/precise1.cs	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/precise1b.cs	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/precise2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/precise4.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Coverage/b433189.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/decimaldiv.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/i4div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/i8div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/negSignedMod.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/overlddiv.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/r4div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/r8div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/u4div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/div/u8div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/decimalrem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/i4rem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/i8rem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/overldrem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/r4rem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/r8rem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/u4rem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/divrem/rem/u8rem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/eh/interactions/ehSO.cs	\
	ms-coreclr-tests/src/JIT/Methodical/eh/interactions/volatileFromFinally.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_c.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_f4.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_f8.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_i1.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_i2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_i4.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_o.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_s.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit1.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit3.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit4.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit5.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit6.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit7.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/explicit8.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_double.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_float.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_objref.cs	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_valref.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug614098/intToByte.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug619534/ehCodeMotion.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug619534/finallyclone.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug619534/twoEndFinallys.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug621705/ptuple_lost.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug647189/ssa_tuIsAddr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug675304/arrayDim.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug675304/osrAddovershot.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679008/castClassEH.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679008/GCOverReporting.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679008/sealedCastVariance.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679008/singleRefField.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679008/zeroInitStackSlot.cs	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug723489/qMarkColon.cs	\
	ms-coreclr-tests/src/JIT/Methodical/FPtrunc/convr4a.cs	\
	ms-coreclr-tests/src/JIT/Methodical/FPtrunc/convr8a.cs	\
	ms-coreclr-tests/src/JIT/Methodical/inlining/bug505642/test.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/arrays/lcs_long.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/arrays/lcs_ulong.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/misc/binop.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/misc/box.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_addsub.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldc_div.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldc_mul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldc_mulovf.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldfld_mul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldfld_mulovf.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldsfld_mul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldsfld_mulovf.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_muldiv.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/superlong/superlong.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/addsub.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldc_mul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldc_mulovf.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldfld_mul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldfld_mulovf.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldsfld_mul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldsfld_mulovf.cs	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/muldiv.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param1a.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param2a.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param3a.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/callvirt/test1.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/ctor/val_ctor.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/deep/deep.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/recurse.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/obj.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfinally.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfinally_tail.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/basics/classarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/basics/doublearr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/basics/jaggedarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/basics/stringarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/basics/structarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/bool.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/byte.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/char.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/decimal.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/double.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/float.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/int.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/long.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/sbyte.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/short.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/uint.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/ulong.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/DataTypes/ushort.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/GaussJordan/classarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/GaussJordan/jaggedarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/GaussJordan/plainarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/GaussJordan/structarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/InnerProd/classarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/InnerProd/doublearr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/InnerProd/intarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/InnerProd/jaggedarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/InnerProd/stringarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/MDArray/InnerProd/structarr.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/arithm32.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/arithm64.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r4NaNadd.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r4NaNdiv.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r4NaNmul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r4NaNrem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r4NaNsub.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r8NaNadd.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r8NaNdiv.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r8NaNmul.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r8NaNrem.cs	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/r8NaNsub.cs	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/classic.cs	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/delegate.cs	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/generics.cs	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/generics2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/tailcall.cs	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/valuetype.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Overflow/FloatInfinitiesToInt.cs	\
	ms-coreclr-tests/src/JIT/Methodical/Overflow/FloatOvfToInt2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/array1.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/array2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/format.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/gcreport.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/lcs.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/native.cs	\
	ms-coreclr-tests/src/JIT/Methodical/refany/virtcall.cs	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall_v4/delegateParamCallTarget.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/call.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumper.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumps.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/vtret.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/ctor_recurse.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/gc_nested.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/han2.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/han3.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/han3_ctor.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/han3_ref.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/hanoi.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/knight.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/etc/nested.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/identity/accum.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/identity/vcall.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/port/lcs.cs	\
	ms-coreclr-tests/src/JIT/Methodical/VT/port/lcs_gcref.cs	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/refanyval.cs	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/unbox.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/args1.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/args2.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/args3.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/array.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/ArrayOfStructs.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/debug.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/deepcall.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/DelegInstanceFtn.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/DelegStaticFtn.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/GenericStructs.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/ifelse.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/indexer.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/inline.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_DelegateStruct.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_GenericMethods.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_Handler.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/inline_Many.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_MultipleReturn.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_NewObj.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_NormalizeStack.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/inline_Recursion.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_RecursiveMethod.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_RecursiveMethod21.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_SideAffects.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_STARG.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/InlineThrow.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Inline_Vars.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/interfaceCall.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/interfaceProperty.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/mathfunc.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/mthdimpl.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/property.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/ReturnStruct_Method.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/size.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/StructAsParam_Method.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/StructInClass.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/Struct_Opcodes.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/throwTest.cs	\
	ms-coreclr-tests/src/JIT/opt/Inline/trycatch.cs	\
	ms-coreclr-tests/src/JIT/Regression/clr-x64-JIT/v4.0/DevDiv34372/overRepLocalOpt.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M09.5-PDC/b12008/b12008.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M11-Beta1/b40089/b40089.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M11-Beta1/b40138/b40138.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M11-Beta1/b45679/b45679.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M12-Beta2/b46847/b46847.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/dev10/b392262/b392262.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/dev10/b400791/b400971.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/dev10/b402658/b402658.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/dev11/DevDiv_376412/DevDiv_376412.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.1-M1-Beta1/b119294/b119294.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.1-M1-Beta1/b130333/byteshift.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.1-M1-Beta1/b140711/b140711.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b102879/dblinf.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b124232/genisinst.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b169333/b169333.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b178119/hugemthfrm.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b178128/hugestruct.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b180381/constrained1.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b191926/ParamLimit.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b219940/struct01_gen.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b00735/b00735.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b02345/b02345.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b07211/byteshift.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b07900/rpPasses.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b08020/bbHndIndex.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b11762/b11762.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b16386/gtnop.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b16399/gtIsValid64RsltMul.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b19171/jmp2blk.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b30251/b578931.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b11490/b11490.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b12399/b12399.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b12624/b12624.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b13569/b13569.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14716/b14716.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b15728/b15728.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16238/b16238.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16328/b16328.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16335/b16335.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16423/b16423.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b20913/b20913.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b22290/b22290.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b24727/b24727.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b24728/b24728.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25647/b25647.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26558/b26558.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26560/b26560.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26732/b26732.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26863/b26863.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27811/b27811.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27819/b27819.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27824/b27824.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28037/b28037.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28042/b28042.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28776/b28776.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28787/b28787.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28790/b28790.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30126/b30126.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30128/b30128.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30630/b30630.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31732/b31732.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31748/b31748.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31749/b31749.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31763/b31763.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31912/b31912.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32303/b32303.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32345/b32345.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32560/b32560.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32801/b32801.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b13170/b13170.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b13178/b13178.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b13647/b13647.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14057/b14057.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14059/b14059.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14277/b14277.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14314/b14314.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14367/b14367.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14396/b14396.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14422/b14422.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14428/b14428.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14443/b14443.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14616/b14616.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14624/b14624.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14640/b14640.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b14673/b14673.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15155/b15155.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15468/b15468.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15526/b15526.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15783/b15783.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15786/b15786.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15797/b15797.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b15864/b15864.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09/b16294/b16294.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b02051/b02051.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b02076/b02076.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05477/b05477.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06464/b06464.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06680/b06680.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06859/b06859.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06924/b06924.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b08944/b08944a.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b08944/b08944b.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b09287/b09287.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b09452/b09452.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b13330/b13330.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b13466/b13466.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b30586/b30586.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b31878/b31878.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b34945/b34945.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b36274/b36274.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b36332/b36332.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b36470/b36470.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b36471/b36471.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b36472/b36472.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b37131/b37131.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b37598/b37598.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b37608/b37608.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b37636/b37636.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b38403/b38403.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b38556/b38556.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39217/b39217.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39224/b39224.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39951/b39951.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40141/b40141.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40216/b40216.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40221/b40221.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40496/b40496.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40521/b40521.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41063/b41063.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41234/b41234.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41391/b41391.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41470/b41470.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41488/b41488.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41495/b41495.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41621/b41621.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41918/b41918.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b42009/b42009.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b42013/b42013.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b42918/b42918.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b42929/b42929.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43010/b43010.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43313/b43313.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43719/b43719.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43958/b43958.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44193/b44193.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44297/b44297.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44410/b44410.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45015/b45015.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45259/b45259.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45270/b45270.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45458/b45458.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45535/b45535.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46170/b46170.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46629/b46629.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46641/b46641.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46649/b46649.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46867/b46867.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47047/b47047.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48248/b48248.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48797/b48797.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48805/b48805.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48864/b48864.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48872/b48872.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48990/b48990a.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48990/b48990b.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b49318/b49318.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b49322/b49322.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b49717/b49717.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31182/b31182.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31745/b31745.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31746/b31746.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31762/b31762.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31903/b31903.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31917/b31917.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37646/b37646.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b38269/b38269.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b41852/b41852.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b47975/b47975.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b48929/b48929.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b49809/b49809.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50042/b50042.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50145/b50145.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50145/b50145a.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50145/b50145b.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50145/b50145c.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50535/b50535.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51463/b51463.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51469/b51469.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51565/b51565.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51870/b51870.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b52572/b52572.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b52578/b52578.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b52746/b52746.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b52760/b52760.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b52838/b52838.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b52839/b52839.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b54667/b54667.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b55197/b55197.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56149/b56149.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56154/b56154.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56159/b56159.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59297/b59297.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b60723/b60723.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b61028/b61028.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b61515/b61515.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b61640/b61640.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b62498/b62498.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b62555/b62555.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b62892/b62892.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63183/b63183.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b68361/b68361.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b68634/b68634.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71099/b71099.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71120/b71120.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71135/b71135.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71155/b71155.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71231/b71231.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72136/b72136.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72996/b72996.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b73921/b73921.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b74182/b74182.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b75250/b75250.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b75509/b75509.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b76590/b76590.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b77707/b77707.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b77713/b77713.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b78392/b78392.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b79418/b79418.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b80045/b80045.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b82866/b82866.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b83690/b83690.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b84836/b84836.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b91377/b91377.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b87284/b87284.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b88712/b88712.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b88793/b88793.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b88797/b88797.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89277/b89277.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89279/b89279.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89506/b89506.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89600/b89600.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89946/b89946.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91189/b91189.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91230/b91230.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91248/b91248.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91855/b91855.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91859/b91859.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91867/b91867.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91917/b91917.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b92568/b92568.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b92614/b92614.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b92693/b92693.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b93027/b93027.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M14-SP1/b119538/b119538a.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M14-SP1/b119538/b119538b.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b148815/testclass.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b102533/DeadBlock.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b268908/vars2.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b320147/1086745236.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b321799/repro.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b338014/vsw338014.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b405223/b405223.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b416667/b416667.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b369916/b369916.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b106272/b106272.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b152292/b152292.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b565808/b565808.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b589202/b589202.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b598034/b598034.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b598649/b598649.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b602004/Inline_Vars2.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b608066/b608066.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b608198/b608198.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b609280/b609280.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b610562/b610562.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b610750/32vs64.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b610750/b610750.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b611219/b611219.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b72218/b72218.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b121938/ConstToString.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b151497/NullCheckBoxedValuetypeReturn.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b158861/b158861.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b163200/b163200.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b170362/b170362.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b33183/b33183.cs	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b49778/b49778.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/dev10_94677/loopvt.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/dev11_13912/dev11_13912.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/dev11_165544/seqpts.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_457559/Dev11_457559.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_5437/Dev11_5437.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_617302/Dev11_617302.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_646049/Dev11_646049.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/DevDiv2_10623/DevDiv2_10623.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/DevDiv2_8863/DevDiv2_8863.cs	\
	ms-coreclr-tests/src/JIT/Regression/Dev14/DevDiv_876169/DevDiv_876169.cs	\
	ms-coreclr-tests/src/JIT/Regression/JitBlue/DevDiv_794115/DevDiv_794115.cs	\
	ms-coreclr-tests/src/JIT/Regression/JitBlue/DevDiv_794631/DevDiv_794631.cs	\
	ms-coreclr-tests/src/JIT/Regression/JitBlue/DevDiv_815940/DevDiv_815940.cs	\
	ms-coreclr-tests/src/JIT/Regression/JitBlue/DevDiv_816617/DevDiv_816617.cs	\
	ms-coreclr-tests/src/JIT/Regression/JitBlue/GitHub_1296/GitHub_1296.cs	\
	ms-coreclr-tests/src/JIT/Regression/JitBlue/GitHub_1323/GitHub_1323.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b100336/emptytryfinally.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b103846/d.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b111130/makework.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b115253/hello2.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b119026/bug.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b119026/charbug.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b140298/test.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b141358/test.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b99219/b99219.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b102615/test1.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b102860/structret1_1.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b102887/struct5_2.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b124409/ericcprop3.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b91074/pack8.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b10789/switch.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b10790/locals10K.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b10827/MT_DEATH.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b19112/divbyte.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b19112/divshort.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b102729/b102729.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b102763/gcparamonstack.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b102886/ovf.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b10828/redundant.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b19394/stringArray114.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b21015/test.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b26496/_1d6bgof.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b27978/gcparaminreg.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b27980/struct1.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b28077/struct5_2.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b28141/structret1_1.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b29343/test.hfa12.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b184799/b184799.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b302558/_aopst1l.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b309539/_ba6c0ou.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b309548/_hngh669.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b311420/b311420.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b360587/repro.cs	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-RTM/b539509/b539509.cs	\
	ms-coreclr-tests/src/Loader/regressions/classloader/main.cs	\
	ms-coreclr-tests/src/Loader/regressions/classloader/vsw307137.cs	\
	ms-coreclr-tests/src/Loader/regressions/classloader-generics/repro237932.cs	\
	ms-coreclr-tests/src/Loader/regressions/classloader-generics/StaticsProblem5.cs	\
	ms-coreclr-tests/src/Loader/regressions/classloader-generics/vsw514968.cs	\
	ms-coreclr-tests/src/Loader/typeinitialization/circularcctorthreethreads03.cs	\
	ms-coreclr-tests/src/Regressions/common/ArrayCopy.cs	\
	ms-coreclr-tests/src/Regressions/common/avtest.cs	\
	ms-coreclr-tests/src/Regressions/common/interlock.cs	\
	ms-coreclr-tests/src/Regressions/common/test1307.cs	\
	ms-coreclr-tests/src/Regressions/common/testClass.cs	\
	ms-coreclr-tests/src/Regressions/common/testInterface.cs	\
	ms-coreclr-tests/src/Threading/ThreadStatics/ThreadStatic01.cs	\
	ms-coreclr-tests/src/Threading/ThreadStatics/ThreadStatic02.cs	\
	ms-coreclr-tests/src/Threading/ThreadStatics/ThreadStatic03.cs	\
	ms-coreclr-tests/src/Threading/ThreadStatics/ThreadStatic05.cs	\
	ms-coreclr-tests/src/Threading/ThreadStatics/ThreadStatic06.cs

# relies on TestLibrary:
#	ms-coreclr-tests/src/Regressions/common/AboveStackLimit.cs
#	ms-coreclr-tests/src/Regressions/common/date.cs
#	ms-coreclr-tests/src/Regressions/common/CompEx.cs
#	ms-coreclr-tests/src/Regressions/common/Marshal.cs
#	ms-coreclr-tests/src/Regressions/common/pow3.cs
#	ms-coreclr-tests/src/Regressions/common/DisableTransparencyEnforcement.cs
#	ms-coreclr-tests/src/Regressions/common/ThreadCulture.cs
#	ms-coreclr-tests/src/Regressions/common/ToLower.cs
#	ms-coreclr-tests/src/Regressions/common/unsafe.cs
#	ms-coreclr-tests/src/Regressions/expl_double/body_double.cs
#	ms-coreclr-tests/src/Regressions/expl_double/expl_double_1.cs

# needs parameters:
#	ms-coreclr-tests/src/Loader/regressions/polyrec/polyrec.cs

TEST_CS_SRC_DIST=	\
	$(BASE_TEST_CS_SRC)	\
	async-exc-compilation.cs \
	filter-stack.cs	\
	finally_guard.cs \
	finally_block_ending_in_dead_bb.cs

TEST_CS_SRC_GEN = \
	runtime-invoke.gen.cs \
	imt_big_iface_test.cs

if AMD64
TEST_CS_SRC = $(BASE_TEST_CS_SRC) $(TEST_CS_SRC_GEN) async-exc-compilation.cs finally_guard.cs finally_block_ending_in_dead_bb.cs
# #651684
PLATFORM_DISABLED_TESTS = finally_guard.exe
else
if X86
TEST_CS_SRC = $(BASE_TEST_CS_SRC) $(TEST_CS_SRC_GEN) async-exc-compilation.cs finally_guard.cs finally_block_ending_in_dead_bb.cs
else
TEST_CS_SRC = $(BASE_TEST_CS_SRC) $(TEST_CS_SRC_GEN)
endif
endif

if IA64
TEST_CS_SRC+=async-exc-compilation.cs filter-stack.cs
# bug #319249
PLATFORM_DISABLED_TESTS=exception17.exe
PLATFORM_DISABLED_TESTS+=winx64structs.exe
endif

if X86

if HOST_WIN32
PLATFORM_DISABLED_TESTS=async-exc-compilation.exe finally_guard.exe finally_block_ending_in_dead_bb.exe \
	bug-18026.exe monitor.exe threadpool-exceptions5.exe
endif

endif

if POWERPC
# bug #71274
PLATFORM_DISABLED_TESTS=finalizer-abort.exe finalizer-exception.exe finalizer-exit.exe
endif

if POWERPC64
# These tests hang
PLATFORM_DISABLED_TESTS=monitor.exe finalizer-abort.exe finalizer-exception.exe finalizer-exit.exe
endif

if ARM
PLATFORM_DISABLED_TESTS=filter-stack.exe
endif

if MIPS
# monitor.exe is racy
PLATFORM_DISABLED_TESTS=filter-stack.exe monitor.exe
endif

if S390X
PLATFORM_DISABLED_TESTS=dynamic-method-resurrection.exe
#PLATFORM_DISABLED_TESTS=dynamic-method-resurrection.exe exception17.exe
endif

if NACL_CODEGEN
# Tests that use Thread.Abort()
PLATFORM_DISABLED_TESTS= abort-stress-1.exe \
                         abort-stress-2.exe \
                         abort-stress-3.exe \
                         appdomain-thread-abort.exe \
                         async-exc-compilation.exe \
                         bug-561239.exe \
                         bug-70561.exe \
                         finalizer-abort.exe \
                         finally_guard.exe \
                         finally_block_ending_in_dead_bb.exe \
                         main-returns-abort-resetabort.exe \
                         main-returns-background-abort-resetabort.exe \
                         thread6.exe \
                         threadpool-exceptions5.exe \
                         threadpool-exceptions6.exe

# Tests that rely on AppDomain.Unload
PLATFORM_DISABLED_TESTS+= appdomain-async-invoke.exe \
                          appdomain-exit.exe \
                          appdomain-unload-callback.exe \
                          appdomain-unload.exe \
                          domain-stress.exe \
                          generic-unloading.2.exe \
                          monitor.exe \
                          remoting4.exe \
                          threadpool-exceptions7.exe \
                          xdomain-threads.exe

# pinvoke2 attaches a thread to the runtime, but
# doesn't 'unattach' it and it hangs in GC on exit
PLATFORM_DISABLED_TESTS+= pinvoke2.exe

# Tests that currently hang waiting for non-main threads
# to exit in NaCl, need to investigate.  Most are AppDomain
# creation and Delegate tests.
PLATFORM_DISABLED_TESTS+= appdomain1.exe \
                          delegate9.exe \
                          marshal-valuetypes.exe \
                          cross-domain.exe \
                          stackframes-async.2.exe \
                          generic-marshalbyref.2.exe \
                          generic-xdomain.2.exe \
                          bug-415577.exe

# Tests that fail trying to write files (appdomain create mostly)
PLATFORM_DISABLED_TESTS+= bug-335131.2.exe \
                          bug-349190.2.exe \
                          bug-80307.exe \
                          bug-462592.exe

# FIXME: don't know why delegate2.exe fails, it shouldn't
PLATFORM_DISABLED_TESTS+= delegate2.exe

# These tests newly fail with the latest revision. pinvoke3 fails because
# of a thread attach, the others have not been investigated.  TODO revisit.
PLATFORM_DISABLED_TESTS+= pinvoke3.exe \
                          async_read.exe \
                          async-with-cb-throws.exe \
                          appdomain-unload-doesnot-raise-pending-events.exe \
                          gsharing-valuetype-layout.exe

if X86
# FIXME: There are problems with async callbacks and results on NaCl 32-bit
PLATFORM_DISABLED_TESTS+= delegate1.exe \
                          delegate3.exe \
                          delegate5.exe \
                          delegate8.exe \
                          threadpool.exe \
                          threadpool1.exe \
                          threadpool-exceptions3.exe \
                          bug-323114.exe \
                          delegate-exit.exe \
                          bug-80392.2.exe

# FIXME: These tests hang/fail for unknown reasons, deal with exiting
PLATFORM_DISABLED_TESTS+= main-returns-background-resetabort.exe \
                          main-returns-background.exe \
                          main-returns-background-change.exe
endif

endif

# The two finalizer tests only work under sgen
# gc-altstack.exe fails under boehm because it has no support for altstack
# bug-459094.exe creates an extremely deep directory tree
# delegate-invoke.exe depends on 929c6bc9b6d76a273f251e6f5dfacac36e9c38bd which was
# reverted.
# bug-Xamarin-5278.exe got broken by 5d26590e79da139a284459299aee95c25f4cd835
DISABLED_TESTS=			\
	delegate-async-exception.exe	\
	bug-348522.2.exe	\
	bug-459094.exe \
	delegate-invoke.exe \
	bug-Xamarin-5278.exe \
	$(PLATFORM_DISABLED_TESTS) \
	$(EXTRA_DISABLED_TESTS)

DISABLED_TESTS_WRENCH=	\
	$(DISABLED_TESTS)	\
	$(PLATFORM_DISABLED_TESTS_WRENCH)	\
	main-returns-background-resetabort.exe \
	main-returns-background-abort-resetabort.exe	\
	thread6.exe	\
	assemblyresolve_event3.exe \
	delegate2.exe	\
	finally_guard.exe \
	gc-altstack.exe \
	generic-xdomain.2.exe

AOT_DISABLED_TESTS=constraints-load.exe

# These only compile with MS CSC
TEST_CSC_SRC=			\
	vararg.cs

TEST_IL_SRC=			\
	field-access.il		\
	method-access.il	\
	ldftn-access.il		\
	cpblkTest.il		\
	vbinterface.il		\
	calliTest.il		\
	ckfiniteTest.il		\
	fault-handler.il		\
	locallocTest.il		\
	initblkTest.il		\
	qt-instance.il		\
	vararg.il		\
	bug-29859.il		\
	bug-78549.il		\
	static-fields-nonconst.il	\
	reload-at-bb-end.il	\
	test-enum-indstoreil.il	\
	filter-bug.il		\
	even-odd.il		\
	bug-82022.il	\
	vt-sync-method.il	\
	enum_types.il	\
	invalid-token.il	\
	call_missing_method.il	\
	call_missing_class.il	\
	ldfld_missing_field.il	\
	ldfld_missing_class.il	\
	find-method.2.il	\
	bug-79215.2.il	\
	bug-79956.2.il	\
	bug-327438.2.il \
	bug-387274.2.il	\
	bug-426309.2.il	\
	ldtoken_with_byref_typespec.2.il \
	resolve_method_bug.2.il \
	resolve_field_bug.2.il \
	resolve_type_bug.2.il	\
	generics-sharing-other-exc.2.il	\
	generic-ldobj.2.il	\
	generic-mkrefany.2.il	\
	generic-refanyval.2.il	\
	generic-ldtoken.2.il	\
	generic-ldtoken-method.2.il	\
	generic-ldtoken-field.2.il	\
	generic-tailcall.2.il	\
	generic-tailcall2.2.il	\
	generic-array-exc.2.il	\
	generic-valuetype-newobj2.2.il	\
	generic-valuetype-newobj.2.il	\
	generic-constrained.2.il	\
	generic-type-load-exception.2.il	\
	bug-81466.il	\
	bug457574.il	\
	bug445361.il	\
	bug-463303.il	\
	bug469742.2.il	\
	bug-528055.il	\
	constraints-load.il	\
	array_load_exception.il \
	bug-481403.il	\
	interface-with-static-method.il	\
	bug-515884.il	\
	bug-633291.il	\
	delegate-with-null-target.il	\
	bug-318677.il	\
	gsharing-valuetype-layout.il	\
	invalid_generic_instantiation.il	\
	ms-coreclr-tests/src/JIT/BBT/Scenario4/Not-Int32.il	\
	ms-coreclr-tests/src/JIT/Directed/Convert/implicitConv.il	\
	ms-coreclr-tests/src/JIT/Directed/Convert/minopts_convu1.il	\
	ms-coreclr-tests/src/JIT/Directed/ExcepFilters/excepobj/excepobj.il	\
	ms-coreclr-tests/src/JIT/Directed/ExcepFilters/fault/fault.il	\
	ms-coreclr-tests/src/JIT/Directed/ExcepFilters/mixed/mixed.il	\
	ms-coreclr-tests/src/JIT/Directed/FaultHandlers/CallOrder/CallOrder.il	\
	ms-coreclr-tests/src/JIT/Directed/FaultHandlers/Nesting/Nesting.il	\
	ms-coreclr-tests/src/JIT/Directed/FaultHandlers/Simple/simple.il	\
	ms-coreclr-tests/src/JIT/Directed/IL/Tailcall/JitTailcall1.il	\
	ms-coreclr-tests/src/JIT/Directed/IL/Tailcall/Jittailcall2.il	\
	ms-coreclr-tests/src/JIT/Directed/IL/leave/leave2.il	\
	ms-coreclr-tests/src/JIT/Directed/IL/rethrow/rethrow1.il	\
	ms-coreclr-tests/src/JIT/Directed/IL/rethrow/rethrow2.il	\
	ms-coreclr-tests/src/JIT/Directed/Misc/SIDEEFFECTS/BadRegArgs.il	\
	ms-coreclr-tests/src/JIT/Directed/Misc/SIDEEFFECTS/SideEffects.il	\
	ms-coreclr-tests/src/JIT/Directed/Misc/function_pointer/funcptrtest.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/Box_Unbox.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/Desktop/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/arglist64.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/array_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/cpblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/field_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/fielda_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/initblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/initobj.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/ldind_stind.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/1/ldloca.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/Box_Unbox.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/arglist64.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/array_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/cpblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/field_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/fielda_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/initblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/initobj.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/ldind_stind.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/2/ldloca.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/Box_Unbox.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/Desktop/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/arglist64.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/array_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/cpblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/field_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/fielda_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/initblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/initobj.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/ldind_stind.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/unaligned/4/ldloca.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/Box_Unbox.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/Desktop/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/add.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/arglist64.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/array_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/cpblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/field_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/fielda_tests.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/initblk.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/initobj.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/ldind_stind.il	\
	ms-coreclr-tests/src/JIT/Directed/PREFIX/volatile/1/ldloca.il	\
	ms-coreclr-tests/src/JIT/Directed/RVAInit/oddsize.il	\
	ms-coreclr-tests/src/JIT/Directed/UnrollLoop/loop3.il	\
	ms-coreclr-tests/src/JIT/Directed/array-il/simple3.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/badendfinally.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/badtailcall.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/byrefsubbyref1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/calli2.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/ceeillegal.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/ldelemnullarr2.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/ldfldstatic1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/ldfldunboxedvt.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/ldvirtftnsideeffect.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/nullsdarr.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/refanytype1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/stfldstatic1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/subovfun1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/volatilldind.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/importer/volatilstind.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/arrgetlen.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/lcliimpl.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/ldsshrstsfld.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/ldvirtftncalli.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/ovfldiv2.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/ovflrem2.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/stfldstatic1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/stfldstatic2.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/subbyref.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/switchdefaultonly1.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/switchdefaultonly2.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/switchdefaultonly3.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/trashreg.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/volatilecpobj.il	\
	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/zeroinit.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/catch1.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/catch2.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/catch3.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/filter1.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/filter2.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/filter3.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/try1.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/try2.il	\
	ms-coreclr-tests/src/JIT/Directed/leave/try3.il	\
	ms-coreclr-tests/src/JIT/Directed/refbyref/byrefconvert.il	\
	ms-coreclr-tests/src/JIT/Directed/shift/int8.il	\
	ms-coreclr-tests/src/JIT/Directed/shift/nativeint.il	\
	ms-coreclr-tests/src/JIT/Directed/shift/nativeuint.il	\
	ms-coreclr-tests/src/JIT/Directed/throwbox/fault.il	\
	ms-coreclr-tests/src/JIT/Directed/throwbox/filter.il	\
	ms-coreclr-tests/src/JIT/Directed/throwbox/finally.il	\
	ms-coreclr-tests/src/JIT/Directed/throwbox/rethrow.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/Dev10_863995.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/init_byte.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/init_int32.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/init_int64.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/init_struct.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/init_uint32.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/init_uint64.il	\
	ms-coreclr-tests/src/JIT/Directed/zeroinit/tail.il	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt1.il	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt2.il	\
	ms-coreclr-tests/src/JIT/Generics/ConstrainedCall/vt3.il	\
	ms-coreclr-tests/src/JIT/Generics/regression/DDB148379/ddb148379.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Base/add.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/conv_ovf_i1_un.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_i1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_i2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_u1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_u2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_u4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_add_ovf_u8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_c_cpblk.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_c_initblk.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_c_nop.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_c_ret.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_c_switch.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ckfinite_r4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ckfinite_r8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_i4_i1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_i4_i2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_i4_u4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_i8_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_i8_u8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_r8_i.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_r8_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_r8_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_u4_i.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_u4_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_u4_u1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_u4_u2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_u8_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_conv_ovf_u8_u4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_i1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_i2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_u1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_u2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_u4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_mul_ovf_u8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_neg_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_neg_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ret_i.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ret_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ret_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ret_r4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ret_r8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_ret_ref.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_i.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_i1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_i2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_i4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_i8.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_u1.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_u2.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_u4.il	\
	ms-coreclr-tests/src/JIT/IL_Conformance/Old/Conformance_Base/ldc_sub_ovf_u8.il	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/huge_struct.il	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/lcs/lcs_ldlen.il	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/misc/lengthm2.il	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/misc/selfref.il	\
	ms-coreclr-tests/src/JIT/Methodical/Arrays/range/negIndexRngChkElim.il	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/boxunbox/array.il	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/boxunbox/chain.il	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/boxunbox/local.il	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/misc/enum.il	\
	ms-coreclr-tests/src/JIT/Methodical/Boxing/misc/tailjump.il	\
	ms-coreclr-tests/src/JIT/Methodical/Coverage/b39946.il	\
	ms-coreclr-tests/src/JIT/Methodical/Coverage/b518440.il	\
	ms-coreclr-tests/src/JIT/Methodical/Coverage/hole.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/conv_i8_i.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/conv_i8_u.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/convovf_i8_i.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/convovf_i8_u-ia64.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_conv.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_fld.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_flood.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_flow.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_flow_merge.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_prop.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_qsort1.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_qsort2.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_ref.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_seq.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/i_vfld.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/ptr.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/qperm.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/sizeof-ia64.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_conv.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_fld.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_flood.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_flow.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_flow_merge.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_prop.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_qsort1.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_qsort2.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_ref.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_seq.il	\
	ms-coreclr-tests/src/JIT/Methodical/ELEMENT_TYPE_IU/u_vfld.il	\
	ms-coreclr-tests/src/JIT/Methodical/FPtrunc/convr4d.il	\
	ms-coreclr-tests/src/JIT/Methodical/FPtrunc/convr8d.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param1b.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param1c.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param3b.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25param3c.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/25params/25paramMixed.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfault.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfault_jmp.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfault_tail.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfinally_ind.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfinally_jmp.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfinally_jmpind.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/SEH/catchfinally_tail.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/callvirt/test1.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/callvirt/test2.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/callvirt/test3.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/ctor/val_cctor.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/ctor/val_ctor_newobj.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/deep/deep1.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/deep/deep2.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/ftn_t.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/instftn.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/instftn_t.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/recurse_calli.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/recurse_jmp.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/recurse_jmpi.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/recurse_tail_call.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/recurse_tail_calli.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/valftn.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/valftn_t.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/virtftn.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/fptr/virtftn_t.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/fr4.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/fr8.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/i4u2.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/i4u4.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/i8u8.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/ii1.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/ii2.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/ii4.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/iu2.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/iu4.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/implicit/objref.il	\
	ms-coreclr-tests/src/JIT/Methodical/Invoke/thiscall/thisnull.il	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/comp32.il	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/comp64.il	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/cond32.il	\
	ms-coreclr-tests/src/JIT/Methodical/NaN/cond64.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/aa.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/calli.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/dd.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/ee.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumper1.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumper2.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumper3.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumper4.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/jumper5.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/vtret.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/callconv/vtret2.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/identity/accum.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/identity/livecall.il	\
	ms-coreclr-tests/src/JIT/Methodical/VT/identity/volatile.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/castclass_catch.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/castclass_catch_neg.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/filter.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/isinst_catch.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/array/arrays.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/array/castclass_ldlen.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/array/isinst_ldlen.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_call.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_calli.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_ldarg.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/castclass_ldloc.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_call.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_calli.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_ldarg.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/isinst_ldloc.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/coverage/ldnull.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/iface/iface2.il	\
	ms-coreclr-tests/src/JIT/Methodical/casts/ilseq/typeEqualOp.il	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/misc/deadlock.il	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/Desktop/prefldinit3.il	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/prefldinit1.il	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/prefldinit2.il	\
	ms-coreclr-tests/src/JIT/Methodical/cctor/simple/prefldinit4.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_c.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_f4.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_f8.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_i1.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_i2.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_i4.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_o.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refarg_s.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_c.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_i1.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_i2.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_i4.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_o.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_o2.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_r4.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_r8.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/basic/refloc_u2.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/funcptr/expl_funcptr_gc.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/funcptr/expl_funcptr_val.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/funcptr/seq_funcptr_gc.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/funcptr/seq_funcptr_val.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/misc/refarg_box_val.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_double.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_float.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_objref.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotarg_valref.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotate_i4.il	\
	ms-coreclr-tests/src/JIT/Methodical/explicit/rotate/rotate_u2.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug619534/ILStackAllocRepro.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/bug619534/twoEndFinallys.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug642944/GCMaskForGSCookie.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679053/cpblkInt32.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679053/flowgraph.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679955/indexMinusOne.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679955/volatileLocal1.il	\
	ms-coreclr-tests/src/JIT/Methodical/flowgraph/dev10_bug679955/volatileLocal2.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/arrays/hugedim.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/arrays/lcs_long.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/arrays/lcs_ulong.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/misc/binop.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/misc/box.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_addsub.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldc_div.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldc_mul.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldc_mulovf.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldfld_mul.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldfld_mulovf.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldsfld_mul.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_ldsfld_mulovf.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/signed/s_muldiv.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/superlong/superlong.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/addsub.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldc_mul.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldc_mulovf.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldfld_mul.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldfld_mulovf.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldsfld_mul.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/ldsfld_mulovf.il	\
	ms-coreclr-tests/src/JIT/Methodical/int64/unsigned/muldiv.il	\
	ms-coreclr-tests/src/JIT/Methodical/ldtoken/ldtoken.il	\
	ms-coreclr-tests/src/JIT/Methodical/ldtoken/ldtokena.il	\
	ms-coreclr-tests/src/JIT/Methodical/ldtoken/ptr_types.il	\
	ms-coreclr-tests/src/JIT/Methodical/localloc/verify/verify01_dynamic.il	\
	ms-coreclr-tests/src/JIT/Methodical/localloc/verify/verify01_large.il	\
	ms-coreclr-tests/src/JIT/Methodical/localloc/verify/verify01_small.il	\
	ms-coreclr-tests/src/JIT/Methodical/localloc/zeroinit/zeroinit01_large.il	\
	ms-coreclr-tests/src/JIT/Methodical/localloc/zeroinit/zeroinit01_small.il	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/generics2.il	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/tailcall.il	\
	ms-coreclr-tests/src/JIT/Methodical/nonvirtualcall/valuetype.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/array1.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/array2.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/array3.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/native.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/seq.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/stress2.il	\
	ms-coreclr-tests/src/JIT/Methodical/refany/u_native.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_enum.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_i2_bool.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_i4_i1.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_i4_u.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_i_u2.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_obj.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/compat_v.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/deep_array.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/deep_array_nz.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/deep_gc.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/deep_inst.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/deep_value.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/deep_virt.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/gcval.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/gcval_nested.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/gcval_sideeffect.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/recurse_ep.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/reference_i.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_2a.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_2b.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_2c.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_3b.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_mutual_rec.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_switch.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall/test_virt.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall_v4/smallFrame.il	\
	ms-coreclr-tests/src/JIT/Methodical/tailcall_v4/tailcall_AV.il	\
	ms-coreclr-tests/src/JIT/Methodical/varargs/callconv/gc_ctor.il	\
	ms-coreclr-tests/src/JIT/Methodical/varargs/callconv/val_ctor.il	\
	ms-coreclr-tests/src/JIT/Methodical/varargs/misc/Dev10_615402.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxblk/cpblk3.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxblk/initblk3.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/ldobj/ldobj_I.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/ldobj/ldobj_I8.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/ldobj/ldobj_R4.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/ldobj/ldobj_R8.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/ldobj/ldobj_U2.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/ldobj/ldobj_V.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/const.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/ldelema.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/mdarray.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/refanyval.il	\
	ms-coreclr-tests/src/JIT/Methodical/xxobj/operand/unbox.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M09.5-PDC/b16935/b16935.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M11-Beta1/b44018/b44018.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M11-Beta1/b45046/b45046.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M12-Beta2/b26323/b26323.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-EJIT/V1-M12-Beta2/b35455/b35455.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b06440/b06440a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b06440/b06440b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b06440/b06440c.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b07341/b07341.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b09495/b09495.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10665/b10665.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10666/b10666.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10894/b10894.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10897/b10897.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10939/b10939.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10940/b10940a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b10940/b10940b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b11021/b11021.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b11413/b11413.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b11949/b11949.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b12274/b12274.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b12487/b12487.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b13509/b13509.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b13522/b13522.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b13586/b13586.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b13738/b13738.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14067/b14067a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14067/b14067b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14068/b14068.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14070/b14070.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14135/b14135.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14197/b14197.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14199/b14199.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14202/b14202.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14264/b14264.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14294/b14294.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14325/b14325.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14326/b14326.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14329/b14329.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14350/b14350.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14431/b14431.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14591/b14591.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14769/b14769.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14770/b14770.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14777/b14777.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14927/b14927.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b14928/b14928.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b15222/b15222.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b15244/b15244.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b15299/b15299.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16039/b16039.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16049/b16049.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16071/b16071.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16241/b16241.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16295/b16295.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16498/b16498.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16499/b16499a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16499/b16499b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16500/b16500.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16503/b16503.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16554/b16554.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16881/b16881a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16881/b16881b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16886/b16886.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16895/b16895.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16896/b16896.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16922/b16922.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b16928/b16928.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b18852/b18852.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b20079/b20079.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b20217/b20217.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b20249/b20249.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25458/b25458.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25459/b25459.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25463/b25463.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25468/b25468-ia64.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25474/b25474.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25491/b25491.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25507/b25507.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25701/b25701-ia64.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25739/b25739.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25813/b25813.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25815/b25815.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25833/b25833.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b25835/b25835.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26020/b26020.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26153/b26153.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26155/b26155.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26332/b26332.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26512/b26512.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26748/b26748.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26888/b26888.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b26957/b26957.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27535/b27535.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27538/b27538.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27564/b27564.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27657/b27657.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27658/b27658.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27880/b27880.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27883/b27883.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b27917/b27917.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28080/b28080.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28522/b28522.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28594/b28594.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28595/b28595.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28596/b28596.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28597/b28597.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28805/b28805.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28806/b28806.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28927/b28927.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b29583/b29583.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30125/b30125.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30792/b30792.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30799/b30799.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30838/b30838.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30862/b30862.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30864/b30864.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30869/b30869.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b30892/b30892.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31102/b31102.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31273/b31273.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31321/b31321.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31343/b31343.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b31448/b31448.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32093/b32093.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32374/b32374.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32551/b32551a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b32551/b32551b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b34423/b34423.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b02043/b02043.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b02062/b02062.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b03995/b03995.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04083/b04083.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04250/b04250.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04257/b04257.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04306/b04306.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04345/b04345.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04384/b04384.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04538/b04538.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04574/b04574.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04583/b04583.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04612/b04612.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04639/b04639.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b04726/b04726.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05214/b05214.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05617/b05617.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05619/b05619.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05621/b05621.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05622/b05622.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05637/b05637.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05639/b05639.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05737/b05737.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05740/b05740.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05773/b05773.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05780/b05780.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05784/b05784.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b05933/b05933.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06435/b06435.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06436/b06436.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06595/b06595.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06730/b06730.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b06754/b06754.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b07082/b07082.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b07411/b07411.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b07458/b07458.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b07704/b07704.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b08107/b08107.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b08109/b08109.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b08672/b08672.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b08797/b08797.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M10/b09254/b09254.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b27873/b27873.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b29351/b29351.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b33759/b33759.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b33792/b33792.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b33888/b33888.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b33922/b33922.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b33928/b33928.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b35784/b35784.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b36030/b36030.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39381/b39381.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39397/b39397.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39417/b39417.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39455/b39455.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b39946/b39946.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40174/b40174.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40269/b40269.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40347/b40347.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40380/b40380.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40721/b40721.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40725/b40725.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41126/b41126.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41129/b41129.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41262/b41262.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41278/b41278.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41627/b41627.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b42387/b42387.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b42732/b42732.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43040/b43040.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43069/b43069.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43115/b43115.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43121/b43121.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43160/b43160.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43378/b43378.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43714/b43714.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43963/b43963.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43994/b43994.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44020/b44020.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44204/b44204.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44224/b44224.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44723/b44723.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44724/b44724.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44861/b44861.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44879/b44879.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44946/b44946.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44983/b44983.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44984/b44984.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44985/b44985.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45439/b45439.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45541/b45541.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45956/b45956.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45984/b45984.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b45985/b45985.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46292/b46292.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46569/b46569.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46576/b46576.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46583/b46583.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b46897/b46897.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47022/b47022.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47080/b47080-ia64.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47093/b47093.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47610/b47610.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47885/b47885.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b47906/b47906.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48350/b48350.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48554/b48554a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48554/b48554b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b48614/b48614.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b49101/b49101.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b49984/b49984.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b16122/b16122.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b18857/b18857.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b28598/b28598.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b28949/b28949a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b30868/b30868.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31283/b31283.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31289/b31289.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31292/b31292.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31423/b31423.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31452/b31452.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31493/b31493.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31547/b31547.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31780/b31780.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b31784/b31784.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b32613/b32613.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b32614/b32614.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33125/b33125.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33131/b33131.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33135/b33135.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33335/b33335.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33361/b33361.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33362/b33362.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33388/b33388.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33585/b33585.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b33586/b33586.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b34951/b34951.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b34952/b34952.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b34953/b34953.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35315/b35315.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35344/b35344.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35348/b35348.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35351/b35351.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35354/b35354.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35366/b35366.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35486/b35486.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35635/b35635.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b35779/b35779.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b36301/b36301.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b36302/b36302.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b36304/b36304.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b36975/b36975.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37214/b37214.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37215/b37215.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37238/b37238.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37256/b37256.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37578/b37578.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b37830/b37830.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b40006/b40006.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b40347/b40347.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b46566/b46566.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b47471/b47471.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b47886/b47886.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b49104/b49104.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b49142/b49142.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b49435/b49435-ia64.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50026/b50026.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50027/b50027.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b50033/b50033.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51420/b51420.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51515/b51515.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b51575/b51575.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53650/b53650.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53662/b53662.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53878/b53878.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53884/b53884.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53942/b53942a.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53942/b53942b.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53958/b53958.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53977/b53977.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53980/b53980.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53994/b53994.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b53995/b53995.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b54565/b54565.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b54566/b54566.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b54611/b54611.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b54971/b54971.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b55216/b55216.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b55875/b55875.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b55923/b55923.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56066/b56066.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56068/b56068.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56174/b56174.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56349/b56349.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b56780/b56780.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b57492/b57492.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b57493/b57493.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b57516/b57516.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b57518/b57518.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b57952/b57952.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b58358/b58358.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b58360/b58360.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b58689/b58689.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b58690/b58690.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b58866/b58866.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59319/b59319.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59320/b59320.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59337/b59337.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59477/b59477.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59478/b59478.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59508/b59508.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59554/b59554.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59647/b59647.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59782/b59782.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59822/b59822.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59857/b59857.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59858/b59858.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59947/b59947.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59948/b59948.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59949/b59949.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59952/b59952.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b59953/b59953.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b60127/b60127.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b60142/b60142.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b60194/b60194.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b61025/b61025.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b61185/b61185.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b61215/b61215.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b62145/b62145.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63552/b63552.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63725/b63725.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63726/b63726.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63730/b63730.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63732/b63732.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b63823/b63823.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b64026/b64026.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b64560/b64560.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b65087/b65087.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b65176/b65176.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b65407/b65407.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b66226/b66226.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b66425/b66425.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b66583/b66583.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b66620/b66620.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b66679/b66679.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b67351/b67351.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b67744/b67744.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b67819/b67819.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b67987/b67987.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b68028/b68028.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b68045/b68045.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b68872/b68872.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b69225/b69225.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b69227/b69227.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b69512/b69512.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b69528/b69528.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b69848/b69848.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70267/b70267.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70289/b70289.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70335/b70335.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70354/b70354.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70808/b70808.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70909/b70909.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70964/b70964.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70967/b70967.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70992/b70992.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b70994/b70994.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71003/b71003.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71179/b71179.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71318/b71318.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71722/b71722.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71831/b71831.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71869/b71869.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b71999/b71999.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72160/b72160.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72161/b72161.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72518/b72518.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72522/b72522.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72699/b72699.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b72828/b72828.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b73079/b73079.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b73207/b73207.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b73283/b73283.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b73786/b73786.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b74608/b74608.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b74937/b74937.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b74939/b74939.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b74976/b74976.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b75888/b75888.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b75890/b75890.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b75944/b75944.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b75945/b75945.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b77304/b77304.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b77806/b77806.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b77950/b77950.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b79250/b79250.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b80043/b80043.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b80824/b80824.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b81938/b81938.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b82160/b82160.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b82247/b82247.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b82249/b82249.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b82715/b82715.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b84909/b84909.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b84916/b84916.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b84971/b84971.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b85316/b85316.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b101147/b101147.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b85477/b85477.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b86139/b86139.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89409/b89409.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b89546/b89546.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b90129/b90129.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91021/b91021.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91203/b91203.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91223/b91223.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b91359/b91359.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b92736/b92736.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b94306/b94306.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M13-RTM/b99667/b99667.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b147814/rembug.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b147816/loop.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/params-mixed.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/params-none.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/params-object.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/params-value.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/static-mixed.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/static-object.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.1-M1-Beta1/b139895/b139895.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.1-M1-Beta1/b140118/b140118.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b147147/samabo.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b147924/bigdat.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b180381/constrained2.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b220968/1.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b223924/bug2.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b223932/bug3.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-Beta1/b223936/bug4.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b00722/b00722.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b04319/b04319.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b05623/b05623.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b06020/b06020.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b07369/b07369.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b07383/b07383.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b07947/b07947.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b13452/b13452.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b14617/b14617.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b15617/arrayDim.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M01/b18049/loop2.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b102962/t.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b102962/t2.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b102962/vc178279.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b115932/t.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b115932/t1.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b118260/b118260.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1.2-M02/b138117/ldsfldainitonlyvt.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b091942/nullref.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b309555/x86-test-1084346006.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b323557/1087985874.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b353858/b353858.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b374944/helper.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b409617/b409617.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b415164/b415164.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b423755/Desktop/b423755.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b471305/b471305.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b475589/b475589.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b489329/b489329.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b518440/b518440.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b598031/test.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-RTM/b598031/test2.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/dev10/b464149/b464149.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b170362/b170362.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b174294/spill.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/b176032/b176032.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b173313/b173313.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b193264/b193264.il	\
	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/b48850/b48850.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_468598/Test_HndIndex_10_Reordered.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_b473131/b473131.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_b473131/b473131_byte.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_b473131/b473131_fld.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_b473131/b473131_intptr.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_b473131/b473131_struct.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/DevDiv2_11321/DevDiv2_11321.il	\
	ms-coreclr-tests/src/JIT/Regression/Dev11/External/Dev11_90434/UseUnalignedDouble.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b102759/b102759.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b102870/b102870.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b103838/c.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b106158/branchoutoftry.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b108366/bug.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b108908/b108908.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b109721/bug.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b109878/rem_r4.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b112982/exceptobj.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b113286/array.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b114628/array.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b115103/b115103.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b141062/ericswitch.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b79852/b79852.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b79858/b79858.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b80365/b80365-ia64.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b80373/b80373.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b80737/fbug5.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b80738/fbug6.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b81763/b81763.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b81764/b81764.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b81766/b81766.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84129/b84129.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84131/b84131.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84136/b84136.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84586/b84586.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84590/b84590.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84592/b84592.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84957/b84957.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84958/b84958.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84961/b84961.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b84962/b84962.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85314/b85314.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85315/b85315.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85316/f2.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85317/b85317.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85564/b85564.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85565/b85565.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b85566/b85566.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b92713/b92713.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/M00/b92726/b92726.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b223862/bug1.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-Beta1/b91953/2.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b10802/repro.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b10841/repro_good.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b11131/bug2.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b11878/test_07.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b12022/test_10.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b12263/nullref.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b12343/test_04a.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b12390/test_04a.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b12425/test_17.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M01/b13691/init_byte.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b102518/hole.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b102844/tailcallcalli.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b14366/bug2.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b14368/bug4.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b14369/bug5.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b16198/decm010.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b16224/test_25.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b17023/test1a.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b17751/bug4.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b17904/test.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b22521/branch_06.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b22680/1.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b28158/test.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b28158/test64.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V1.2-M02/b29727/2.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b309576/bug2.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b333008/b333008.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b356258/b356258.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b431098/b431098.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-Beta2/b450688/b450688.il	\
	ms-coreclr-tests/src/JIT/Regression/VS-ia64-JIT/V2.0-RTM/b460385/b460385.il	\
	ms-coreclr-tests/src/JIT/Regression/clr-x64-JIT/v4.0/b602182/b602182.il	\
	ms-coreclr-tests/src/JIT/Regression/v4/dev10_804810/dev10_804810.il	\
	ms-coreclr-tests/src/JIT/jit64/localloc/zeroinit/zeroinit01_large.il	\
	ms-coreclr-tests/src/JIT/jit64/localloc/zeroinit/zeroinit01_small.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/il/jmp.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/cg/il/ldftn.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/cprop/Dev10_884217_IL.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/cprop/cprop002.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/inl/inl001.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/lim/lim_001.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/lur/lur_01.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/osr/osr015.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/223862/bne.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/223862/conv.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/223862/div.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/223862/mul1.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/223862/mul_exception.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/223862/rem.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/228572/conv.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/481244/foo.il	\
	ms-coreclr-tests/src/JIT/jit64/opt/regress/vswhidbey/481244/foo2.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/ddb/118414/118414.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/ddb/127931/127931.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/phoenix/62322/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/102974/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/153682/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/266693/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/333007/test1.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/336666/test1.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/373472/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/404708/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/460412/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/534486/exchange.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/575343/test.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/575343/test2.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/606733/Bug606733.il	\
	ms-coreclr-tests/src/JIT/jit64/regress/vsw/607586/607586.il	\
	ms-coreclr-tests/src/JIT/jit64/verif/sniff/fg/ver_fg_13.il	\
	ms-coreclr-tests/src/JIT/opt/AssertionPropagation/CPropOverflow.il	\
	ms-coreclr-tests/src/JIT/opt/DumpDisasm/JitMinOpts/BBCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/DumpDisasm/JitMinOpts/CodeSize1.il	\
	ms-coreclr-tests/src/JIT/opt/DumpDisasm/JitMinOpts/InstrCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/DumpDisasm/JitMinOpts/LVNumCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/DumpDisasm/JitMinOpts/LVRefCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/ETW/TailCallCases.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/BBCnt0.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/BBCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/CodeSize0.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/CodeSize1.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/InstrCnt0.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/InstrCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/LVNumCnt0.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/LVNumCnt1.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/LVRefCnt0.il	\
	ms-coreclr-tests/src/JIT/opt/JitMinOpts/Perf/LVRefCnt1.il

# throws syntax error when processing with Mono ilasm:
#	ms-coreclr-tests/src/JIT/jit64/opt/cprop/Dev10_844071.il
#	ms-coreclr-tests/src/JIT/Methodical/Arrays/misc/initializearray_enum.il
#	ms-coreclr-tests/src/JIT/Methodical/casts/SEH/isinst_catch_neg.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40199/b40199.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b40411/b40411.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41002/b41002.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41149/b41149.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b41164/b41164.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b43033/b43033.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M11-Beta1/b44657/b44657.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b49335/b49335.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M12-Beta2/b54006/b54006.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V2.0-Beta2/b409748/b409748.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.2/ddb/b429039/b429039.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/v2.1/DDB/B168384/LdfldaHack.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-QFE/b151440/params-varargs.il
#	ms-coreclr-tests/src/JIT/Regression/Dev11/Dev11_468598/Test_HndIndex_10_Plain.il
#	ms-coreclr-tests/src/JIT/Regression/Dev11/External/Dev11_14131/VectorForwarder.il

# pinvokes to win32 api, will need to move to HOST_WIN32 specific section:
#	ms-coreclr-tests/src/JIT/Directed/pinvoke/jump.il
#	ms-coreclr-tests/src/JIT/Directed/pinvoke/sysinfo.il
#	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/Desktop/callipinvoke.il
#	ms-coreclr-tests/src/JIT/Directed/coverage/oldtests/callipinvoke.il
#	ms-coreclr-tests/src/JIT/Directed/IL/PInvokeTail/tailwinapi.il
#	ms-coreclr-tests/src/JIT/Regression/CLR-x86-JIT/V1-M09.5-PDC/b28901/b28901.il
#	ms-coreclr-tests/src/JIT/Regression/clr-x64-JIT/v2.1/b173569/b173569.il



# pre-requisite test sources: files that are not test themselves
# but that need to be compiled
PREREQ_IL_SRC=event-il.il module-cctor.il
PREREQ_CS_SRC=
PREREQ_IL_DLL_SRC=event-il.il module-cctor.il
PREREQ_CS_DLL_SRC=

PREREQSI_IL=$(PREREQ_IL_SRC:.il=.exe)
PREREQSI_CS=$(PREREQ_CS_SRC:.cs=.exe)
TESTSI_CS=$(TEST_CS_SRC:.cs=.exe)
TESTSI_IL=$(TEST_IL_SRC:.il=.exe)
TESTBS=$(BENCHSRC:.cs=.exe)
STRESS_TESTS=$(STRESS_TESTS_SRC:.cs=.exe)

EXTRA_DIST=test-driver test-runner.cs $(TEST_CS_SRC_DIST) $(TEST_IL_SRC) \
	$(BENCHSRC) $(STRESS_TESTS_SRC) stress-runner.pl $(PREREQ_IL_SRC) $(PREREQ_CS_SRC)

%.exe: %.il
	$(ILASM) -out:$@ $<

%.exe: %.cs TestDriver.dll
	$(MCS) -r:TestDriver.dll -out:$@ $<

# mkbundle works on ppc, but the pkg-config POC doesn't when run with make test
if POWERPC
test_platform:
else
# Can't use mkbundle on win32 since there is no static build there
# Can't run test-unhandled-exception on Windows because of all the debug popups...
if HOST_WIN32
test_platform:
else
# mkbundle uses the installed mono-2.pc so it won't work if there is no system mono
#test_platform:	testbundle test-iomap-regression
test_platform:	test-iomap-regression
endif
endif

if X86
if HOST_WIN32
else
test_platform: test-eglib-remap
endif
endif

# Target to precompile the test executables
tests: $(TESTSI_CS) $(TESTSI_IL) $(TESTBS) libtest.la $(PREREQSI_IL) $(PREREQSI_CS) $(GSHARED_TESTS)

#
# Test that no symbols are missed in eglib-remap.h
#
OK_G_SYMBOLS='g_list\|g_slist\|g_concat_dir_and_file\|g_Ctoc\'
if NACL_CODEGEN
test-eglib-remap:
else
test-eglib-remap:
	@echo "Testing eglib remap..."
	@if which nm > /dev/null; then if nm $(top_builddir)/mono/mini/mono | grep -v $(OK_G_SYMBOLS) | grep 't g_'; then exit 1; else exit 0; fi; fi
endif

#
# Tests that the internals in mono/io-layer/messages.c are ok by triggering the 
# code that checks that the table is properly sorted
#
if NACL_CODEGEN
test-messages:
else
if HOST_WIN32
test-messages:
else
test-messages: w32message.exe
	> test_messages.zero
	$(with_mono_path) $(JITTEST_PROG_RUN) w32message.exe > w32message.allout 2>&1 && cmp test_messages.zero w32message.allout
endif
endif

if AMD64
test-sgen : sgen-tests
else
if X86
if HOST_WIN32
test-sgen :
else
test-sgen : sgen-tests
endif
else
if ARM
test-sgen : sgen-tests
else
if S390X
test-sgen : sgen-regular-tests
endif
endif
endif
endif

# Precompile the test assemblies in parallel
compile-tests:
	$(MAKE) -j4 $(TESTSI_CS) $(TESTSI_IL) $(TESTBS) libtest.la $(PREREQSI_IL) $(PREREQSI_CS)

# Remove empty .stdout and .stderr files for wrench
rm-empty-logs:
	@echo "Removing empty logs..."
	@find . '(' -name "*.stdout" -o -name "*.stderr" ')' -empty -exec rm {} \;

assemblyresolve/test/asm.dll:
	$(MAKE) -C assemblyresolve prereq

TestDriver.dll:
	$(MCS) -target:library -out:$@ $(srcdir)/../mini/TestDriver.cs

test_cs: $(TEST_PROG) $(TESTSI_CS) libtest.la
	@failed=0; \
	passed=0; \
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	if [ "x$$CI" = "x1" ]; then disabled_tests="$(DISABLED_TESTS_WRENCH)"; else disabled_tests="$(DISABLED_TESTS)"; fi; \
	for i in $(TESTSI_CS); do	\
		if $(srcdir)/test-driver '$(with_mono_path) $(TEST_PROG_RUN)' $$i "$${disabled_tests}" "$${dump_action}" $(RUNTIME_ARGS); \
		then \
			passed=`expr $${passed} + 1`; \
		else \
			if [ $$? = 2 ]; then break; fi; \
			failed=`expr $${failed} + 1`; \
		fi \
	done; \
	echo; echo ".cs: $${passed} test(s) passed. $${failed} test(s) did not pass."; echo

test_il: $(TEST_PROG) $(TESTSI_IL) libtest.la
	@failed=0; \
	passed=0; \
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	if [ "x$$CI" = "x1" ]; then disabled_tests="$(DISABLED_TESTS_WRENCH)"; else disabled_tests="$(DISABLED_TESTS)"; fi; \
	for i in $(TESTSI_IL); do	\
		if $(srcdir)/test-driver '$(with_mono_path) $(TEST_PROG_RUN)' $$i "$${disabled_tests}" "$${dump_action}" $(RUNTIME_ARGS); \
		then \
			passed=`expr $${passed} + 1`; \
		else \
			if [ $$? = 2 ]; then break; fi; \
			failed=`expr $${failed} + 1`; \
		fi \
	done; \
	echo; echo ".il: $${passed} test(s) passed. $${failed} test(s) did not pass."; echo

testb: $(TEST_PROG) $(TESTBS)
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	if [ "x$$CI" = "x1" ]; then disabled_tests="$(DISABLED_TESTS_WRENCH)"; else disabled_tests="$(DISABLED_TESTS)"; fi; \
	for i in $(TESTBS); do	\
		$(srcdir)/test-driver '$(with_mono_path) $(TEST_PROG_RUN)' $$i "$${disabled_tests}" "$${dump_action}" $(RUNTIME_ARGS);	\
	done

runtest: $(TESTSI_CS) $(TESTSI_IL) $(TESTBS) libtest.la $(PREREQSI_IL) $(PREREQSI_CS)
	@failed=0; \
	passed=0; \
	failed_tests="";\
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	if [ "x$$CI" = "x1" ]; then disabled_tests="$(DISABLED_TESTS_WRENCH)"; else disabled_tests="$(DISABLED_TESTS)"; fi; \
	rm -f testlist testlist.sorted; \
	for i in $(TESTSI_CS) $(TESTBS) $(TESTSI_IL); do echo $${i} >> testlist; sort testlist > testlist.sorted; done; \
	for i in `cat testlist.sorted`; do \
		rm -f $${i}.so; \
		if [ x$(AOT) = x1 ]; then if echo $(AOT_DISABLED_TESTS) | grep -v -q $${i}; then $(with_mono_path) $(JITTEST_PROG_RUN) --aot --debug $${i} > $${i}.aotlog 2>&1 || exit 1; fi; fi; \
		if $(srcdir)/test-driver '$(with_mono_path) $(JITTEST_PROG_RUN)' $$i "$${disabled_tests}" "$${dump_action}" $(RUNTIME_ARGS); \
		then \
			passed=`expr $${passed} + 1`; \
		else \
			if [ $$? = 2 ]; then break; fi; \
			failed=`expr $${failed} + 1`; \
			failed_tests="$${failed_tests} $$i"; \
		fi \
	done; \
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	rm -f testlist testlist.sorted; \
	if [ $${failed} != 0 ]; then \
	  echo -e "\nFailed tests:\n"; \
	  for i in $${failed_tests}; do \
	    echo $${i}; \
	    if [ "x$$V" = "x1" ]; then \
	      cat $${i}.stdout; \
	      cat $${i}.stderr; \
	    fi; \
	  done; \
	  exit 1; \
	fi

runtest-managed: test-runner.exe $(TESTSI_CS) $(TESTSI_IL) $(TESTBS) libtest.la $(PREREQSI_IL) $(PREREQSI_CS)
	@if [ "x$$CI" = "x1" ]; then disabled_tests="$(DISABLED_TESTS_WRENCH)"; else disabled_tests="$(DISABLED_TESTS)"; fi; \
	$(RUNTIME) ./test-runner.exe -j a --disabled "$${disabled_tests}" $(TESTSI_CS) $(TESTBS) $(TESTSI_IL)

testjit:
	@if test x$(M) != x0; then $(MAKE) runtest-managed; else $(MAKE) runtest; fi

testaot:
	@$(MAKE) AOT=1 runtest

testtrace:
	@$(MAKE) RUNTIME_ARGS="$${RUNTIME_ARGS} --trace" runtest

testinterp:
	@$(MAKE) JITTEST_PROG_RUN="$(TEST_PROG_RUN)" runtest

testjitspeed: $(JITTEST_PROG) $(TESTBS)
	for i in $(TESTBS); do	\
		echo $$i;	\
		time $(JITTEST_PROG) $$i;	\
	done

test-iomap-regression: exists.cs
	@$(MCS) $(srcdir)/exists.cs -out:exists.exe
	@echo "Testing exists.exe..."
	@MONO_IOMAP=all $(RUNTIME) exists.exe

stresstest: $(STRESS_TESTS)
	@failed=0; \
	passed=0; \
	failed_tests="";\
	for i in $(STRESS_TESTS); do	\
		if $(srcdir)/stress-runner.pl $$i ../mini/mono $(RUNTIME_ARGS); \
		then \
			passed=`expr $${passed} + 1`; \
		else \
			if [ $$? = 2 ]; then break; fi; \
			failed=`expr $${failed} + 1`; \
			failed_tests="$${failed_tests} $$i"; \
		fi \
	done; \
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	if [ $${failed} != 0 ]; then echo -e "\nFailed tests:\n"; \
	  for i in $${failed_tests}; do echo $${i}; done; exit 1; fi

testbundle: console.exe
	@echo "Testing mkbundle..."
	@$(MKBUNDLE) --static console.exe > mkbundle.stdout
	@$(with_mono_path) MONO_CFG_DIR=$(mono_cfg_dir) ./a.out >> mkbundle.stdout
	@- rm -rf a.out

EXTRA_DIST += load-missing.il t-missing.cs load-exceptions.cs
test-type-load: TestDriver.dll
	@$(ILASM) /dll /output:load-missing.dll $(srcdir)/load-missing.il > /dev/null
	@$(MCS) -t:library -out:t.dll -d:FOUND $(srcdir)/t-missing.cs
	@$(MCS) -r:TestDriver.dll -r:load-missing.dll -r:t.dll -out:load-exceptions.exe $(srcdir)/load-exceptions.cs
	@$(MCS) -t:library -out:t.dll $(srcdir)/t-missing.cs
	@echo "Testing load-exception.exe..."
	@$(RUNTIME) load-exceptions.exe > load-exceptions.exe.stdout 2> load-exceptions.exe.stderr

EXTRA_DIST += custom-attr-errors.cs custom-attr-errors-lib.cs
test-cattr-type-load: TestDriver.dll custom-attr-errors.cs custom-attr-errors-lib.cs
	$(MCS) -D:WITH_MEMBERS /t:library $(srcdir)/custom-attr-errors-lib.cs
	$(MCS) -r:TestDriver.dll -r:custom-attr-errors-lib.dll  $(srcdir)/custom-attr-errors.cs
	$(MCS) /t:library $(srcdir)/custom-attr-errors-lib.cs
	@echo "Testing custom-attribute-load-exceptions.exe..."
	@$(RUNTIME) custom-attr-errors.exe > custom-attr-errors.exe.stdout 2> custom-attr-errors.exe.stderr

EXTRA_DIST += reflection-load-with-context-lib.cs reflection-load-with-context-second-lib.cs reflection-load-with-context.cs
test-reflection-load-with-context: reflection-load-with-context-lib.cs reflection-load-with-context-second-lib.cs reflection-load-with-context.exe
	rm -rf reflection-load-dir
	mkdir reflection-load-dir
	$(MCS) /t:library $(srcdir)/reflection-load-with-context-second-lib.cs -out:reflection-load-dir/reflection-load-with-context-second-lib.dll
	$(MCS) /t:library -r:reflection-load-dir/reflection-load-with-context-second-lib.dll $(srcdir)/reflection-load-with-context-lib.cs -out:reflection-load-dir/reflection-load-with-context-lib.dll
	@echo "Testing reflection-load-with-context.cs.exe..."
	@$(RUNTIME) reflection-load-with-context.exe > reflection-load-with-context.exe.stdout 2> reflection-load-with-context.exe.stderr


EXTRA_DIST += debug-casts.cs
# This depends on TLS, so its not ran by default
debug-casts:
	@$(MCS) -r:TestDriver.dll $(srcdir)/debug-casts.cs
	@$(RUNTIME) --debug=casts debug-casts.exe

EXTRA_DIST += sgen-bridge.cs sgen-descriptors.cs sgen-gshared-vtype.cs sgen-bridge-major-fragmentation.cs sgen-domain-unload.cs sgen-weakref-stress.cs sgen-cementing-stress.cs sgen-case-23400.cs 	finalizer-wait.cs critical-finalizers.cs sgen-domain-unload-2.cs sgen-suspend.cs sgen-new-threads-dont-join-stw.cs sgen-bridge-xref.cs bug-17590.cs sgen-toggleref.cs


#those are actually configurations, eg plain_sgen-descriptors.exe
#DISABLE_SGEN_TESTS =

SGEN_TESTS =	\
	finalizer-wait.exe	\
	critical-finalizers.exe	\
	sgen-descriptors.exe	\
	sgen-gshared-vtype.exe	\
	sgen-domain-unload.exe	\
	sgen-domain-unload-2.exe	\
	sgen-weakref-stress.exe	\
	sgen-cementing-stress.exe	\
	sgen-case-23400.exe	\
	sgen-new-threads-dont-join-stw.exe	\
	gc-graystack-stress.exe	\
	bug-17590.exe

SGEN_CONFIGURATIONS =	\
	"|plain"	\
	"major=marksweep-conc|ms-conc"	\
	"major=marksweep-conc,minor=split|ms-conc-split"	\
	"minor=split|ms-split"	\
	"minor=split,alloc-ratio=95|ms-split-95"	\
	"|plain-clear-at-gc|clear-at-gc"	\
	"major=marksweep-conc|ms-conc-clear-at-gc|clear-at-gc"	\
	"minor=split|ms-split-clear-at-gc|clear-at-gc"


#FIXME We should move to use SGEN_CONFIGURATIONS once sgen supports trailling commas or its argument list.
SGEN_BRIDGE_CONFIGURATIONS =	\
	"|plain"	\
	",major=marksweep-conc|ms-conc"	\
	"minor=split|ms-split"	\
	"bridge-implementation=new|plain-new-bridge"	\
	"bridge-implementation=new,major=marksweep-conc|ms-conc-new-bridge"	\
	"bridge-implementation=new,minor=split|ms-split-new-bridge"	\
	"bridge-implementation=tarjan|plain-tarjan-bridge"	\
	"bridge-implementation=tarjan,minor=split|ms-split-tarjan-bridge"

sgen-regular-tests: $(SGEN_TESTS)
	@failed=0; \
	passed=0; \
	failed_tests="";\
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	for test in $+; do	\
		echo "...$$test";	\
		for conf in $(SGEN_CONFIGURATIONS); do 	\
			name=`echo $$conf | cut -d\| -f 2`;	\
			params=`echo $$conf | cut -d\| -f 1`;	\
			debug_opt=`echo $$conf | cut -d\| -f 3`;	\
			test_name="$${test}|$${name}";	\
			if MONO_GC_PARAMS="$$params" MONO_ENV_OPTIONS="--gc=sgen" MONO_GC_DEBUG="$$debug_opt" $(srcdir)/test-driver '$(with_mono_path) $(JITTEST_PROG_RUN)' $$test_name "$(DISABLED_TESTS_SGEN)" "$${dump_action}" $(RUNTIME_ARGS);	\
			then \
				passed=`expr $${passed} + 1`; \
			else \
				if [ $$? = 2 ]; then break; fi; \
				failed=`expr $${failed} + 1`; \
				failed_tests="$${failed_tests} $$test_name"; \
			fi \
		done	\
	done;	\
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	if [ $${failed} != 0 ]; then echo -e "\nFailed tests:\n"; \
	  for i in $${failed_tests}; do echo $${i}; done; exit 1; fi

sgen-bridge-tests1: sgen-bridge.exe sgen-bridge-major-fragmentation.exe
	@failed=0; \
	passed=0; \
	failed_tests="";\
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	for test in $+; do	\
		echo "...$$test";	\
		for conf in $(SGEN_BRIDGE_CONFIGURATIONS); do 	\
			name=`echo $$conf | cut -d\| -f 2`;	\
			params=`echo $$conf | cut -d\| -f 1`;	\
			test_name="$${test}|$${name}";	\
			if MONO_GC_DEBUG="bridge=Bridge" MONO_GC_PARAMS="$${params}" MONO_ENV_OPTIONS="--gc=sgen" $(srcdir)/test-driver '$(with_mono_path) $(JITTEST_PROG_RUN)' $$test_name "$(DISABLED_TESTS_SGEN)" "$${dump_action}" $(RUNTIME_ARGS);	\
			then \
				passed=`expr $${passed} + 1`; \
			else \
				if [ $$? = 2 ]; then break; fi; \
				failed=`expr $${failed} + 1`; \
				failed_tests="$${failed_tests} $$test_name"; \
			fi \
		done	\
	done;	\
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	if [ $${failed} != 0 ]; then echo -e "\nFailed tests:\n"; \
	  for i in $${failed_tests}; do echo $${i}; done; exit 1; fi

sgen-bridge-tests2: sgen-bridge-xref.exe
	@failed=0; \
	passed=0; \
	failed_tests="";\
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	for test in $+; do	\
		echo "...$$test";	\
		for conf in $(SGEN_BRIDGE_CONFIGURATIONS); do 	\
			name=`echo $$conf | cut -d\| -f 2`;	\
			params=`echo $$conf | cut -d\| -f 1`;	\
			test_name="$${test}|$${name}";	\
			if MONO_GC_DEBUG="bridge=2Bridge" MONO_GC_PARAMS="$${params}" MONO_ENV_OPTIONS="--gc=sgen" $(srcdir)/test-driver '$(with_mono_path) $(JITTEST_PROG_RUN)' $$test_name "$(DISABLED_TESTS_SGEN)" "$${dump_action}" $(RUNTIME_ARGS);	\
			then \
				passed=`expr $${passed} + 1`; \
			else \
				if [ $$? = 2 ]; then break; fi; \
				failed=`expr $${failed} + 1`; \
				failed_tests="$${failed_tests} $$test_name"; \
			fi \
		done	\
	done;	\
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	if [ $${failed} != 0 ]; then echo -e "\nFailed tests:\n"; \
	  for i in $${failed_tests}; do echo $${i}; done; exit 1; fi

sgen-toggleref-tests: sgen-toggleref.exe
	@failed=0; \
	passed=0; \
	failed_tests="";\
	if [ "x$$V" = "x1" ]; then dump_action="dump-output"; else dump_action="no-dump"; fi; \
	for test in $+; do	\
		echo "...$$test";	\
		for conf in $(SGEN_CONFIGURATIONS); do 	\
			name=`echo $$conf | cut -d\| -f 2`;	\
			params=`echo $$conf | cut -d\| -f 1`;	\
			test_name="$${test}|$${name}";	\
			if MONO_GC_PARAMS="toggleref-test,$$params" MONO_ENV_OPTIONS="--gc=sgen" $(srcdir)/test-driver '$(with_mono_path) $(JITTEST_PROG_RUN)' $$test_name "$(DISABLED_TESTS_SGEN)" "$${dump_action}" $(RUNTIME_ARGS);	\
			then \
				passed=`expr $${passed} + 1`; \
			else \
				if [ $$? = 2 ]; then break; fi; \
				failed=`expr $${failed} + 1`; \
				failed_tests="$${failed_tests} $$test_name"; \
			fi \
		done	\
	done;	\
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	if [ $${failed} != 0 ]; then echo -e "\nFailed tests:\n"; \
	  for i in $${failed_tests}; do echo $${i}; done; exit 1; fi

sgen-bridge-tests: sgen-bridge-tests1 sgen-bridge-tests2

sgen-tests: sgen-regular-tests sgen-bridge-tests sgen-toggleref-tests

AOT_CONFIGURATIONS=	\
	"|regular"	\
	"--gc=boehm|boehm"

#LLVM crashes, got to fix it first.
#	"--llvm|llvm"	\
#	"--llvm --gc=boehm|llvm+boehm"


test-aot:
	@failed=0; \
	passed=0; \
	failed_tests="";\
	profile=$(DEFAULT_PROFILE);	\
	tmpfile=`mktemp -t mono_aot_output` || exit 1;	\
	rm -f test-aot-$${name}.stdout test-aot-$${name}.stderr; \
	for assembly in $(mcs_topdir)/class/lib/$$profile/*.dll	; do	\
		asm_name=`basename $$assembly`;	\
		echo "... $$asm_name";	\
		for conf in $(AOT_CONFIGURATIONS); do	\
			name=`echo $$conf | cut -d\| -f 2`;	\
			params=`echo $$conf | cut -d\| -f 1`;	\
			test_name="$${asm_name}|$${name}";	\
			echo "  $$test_name";	\
			if MONO_PATH=$(mcs_topdir)/class/lib/$$profile $(JITTEST_PROG_RUN) $$params --aot=outfile=$$tmpfile $$assembly >> test-aot-$${name}.stdout 2>> test-aot-$${name}.stderr;	\
			then \
				passed=`expr $${passed} + 1`; \
			else \
				failed=`expr $${failed} + 1`; \
				failed_tests="$${failed_tests} $$test_name"; \
			fi \
		done;	\
	done;	\
	rm	$$tmpfile;	\
	echo "$${passed} test(s) passed. $${failed} test(s) did not pass."; \
	if [ $${failed} != 0 ]; then echo -e "\nFailed tests:\n"; \
	for i in $${failed_tests}; do echo $${i}; done; exit 1; fi


# Generated tests for runtime invoke
EXTRA_DIST += gen-runtime-invoke.cs
runtime-invoke.gen.exe: TestDriver.dll gen-runtime-invoke.exe
	$(RUNTIME) gen-runtime-invoke.exe > runtime-invoke.gen.cs
	$(MCS) -out:runtime-invoke.gen.exe -r:TestDriver.dll runtime-invoke.gen.cs

EXTRA_DIST += make-imt-test.cs
imt_big_iface_test.exe: TestDriver.dll make-imt-test.exe
	$(RUNTIME) make-imt-test.exe > imt_big_iface_test.cs
	$(MCS) -out:imt_big_iface_test.exe -r:TestDriver.dll imt_big_iface_test.cs

EXTRA_DIST += test-inline-call-stack-library.cs test-inline-call-stack.cs
test-inline-call-stack-library.dll: TestDriver.dll $(srcdir)/test-inline-call-stack-library.cs
	$(MCS) -t:library -out:test-inline-call-stack-library.dll $(srcdir)/test-inline-call-stack-library.cs
test-inline-call-stack.exe: TestDriver.dll test-inline-call-stack-library.dll $(srcdir)/test-inline-call-stack.cs
	$(MCS) -r:TestDriver.dll -r:test-inline-call-stack-library.dll -out:test-inline-call-stack.exe $(srcdir)/test-inline-call-stack.cs

EXTRA_DIST += unhandled-exception-base-configuration.config
EXTRA_DIST += unhandled-exception-legacy-configuration.config
EXTRA_DIST += appdomain-unload.exe.config
EXTRA_DIST += unhandled-exception-test-case.2.cs
EXTRA_DIST += unhandled-exception-test-runner.2.cs
unhandled-exception-test-case.1.cs: unhandled-exception-test-case.2.cs
	cp unhandled-exception-test-case.2.cs unhandled-exception-test-case.1.cs
unhandled-exception-test-case-legacy.1.cs: unhandled-exception-test-case.1.cs
	cp unhandled-exception-test-case.1.cs unhandled-exception-test-case-legacy.1.cs
unhandled-exception-test-case-legacy.2.cs: unhandled-exception-test-case.2.cs
	cp unhandled-exception-test-case.2.cs unhandled-exception-test-case-legacy.2.cs
unhandled-exception-config_files: unhandled-exception-base-configuration.config unhandled-exception-legacy-configuration.config
	cp unhandled-exception-base-configuration.config unhandled-exception-test-case.1.exe.config
	cp unhandled-exception-base-configuration.config unhandled-exception-test-case.2.exe.config
	cp unhandled-exception-legacy-configuration.config unhandled-exception-test-case-legacy.1.exe.config
	cp unhandled-exception-legacy-configuration.config unhandled-exception-test-case-legacy.2.exe.config
unhandled-exception-test-case.1.exe: unhandled-exception-test-case.1.cs
	$(MCS) /debug unhandled-exception-test-case.1.cs
unhandled-exception-test-case-legacy.1.exe: unhandled-exception-test-case-legacy.1.cs
	$(MCS) /debug unhandled-exception-test-case-legacy.1.cs
unhandled-exception-test-case.2.exe: unhandled-exception-test-case.2.cs
	$(MCS) /debug unhandled-exception-test-case.2.cs
unhandled-exception-test-case-legacy.2.exe: unhandled-exception-test-case-legacy.2.cs
	$(MCS) /debug unhandled-exception-test-case-legacy.2.cs
unhandled-exception-test-runner.2.exe: unhandled-exception-test-runner.2.cs
	$(MCS) /debug unhandled-exception-test-runner.2.cs

test-unhandled-exception: unhandled-exception-test-runner.2.exe unhandled-exception-config_files unhandled-exception-test-case.1.exe unhandled-exception-test-case-legacy.1.exe  unhandled-exception-test-case.2.exe unhandled-exception-test-case-legacy.2.exe
	$(RUNTIME) unhandled-exception-test-runner.2.exe RUNTIME:../mini/mono,GTC:F

# We use 'test-support-files' to handle an ordering issue between the 'mono/' and 'runtime/' directories
bug-80307.exe: $(srcdir)/bug-80307.cs
	$(MCS) -r:System.Web -out:$@ $(srcdir)/bug-80307.cs
	cd $(top_builddir)/runtime && $(MAKE) test-support-files

EXTRA_DIST += bug-81673-interface.cs
bug-81673.exe bug-81673-interface.dll: $(srcdir)/bug-81673.cs $(srcdir)/bug-81673-interface.cs
	$(MCS) -target:library -out:bug-81673-interface.dll $(srcdir)/bug-81673-interface.cs
	$(MCS) -out:bug-81673.exe -r:bug-81673-interface.dll $(srcdir)/bug-81673.cs
	$(MCS) -define:WITH_STOP -target:library -out:bug-81673-interface.dll $(srcdir)/bug-81673-interface.cs

EXTRA_DIST += bug-81691-a.cs bug-81691-b.cs
bug-81691.exe bug-81691-b.dll: $(srcdir)/bug-81691.cs $(srcdir)/bug-81691-a.cs $(srcdir)/bug-81691-b.cs
	$(MCS) -target:library -out:bug-81691-a.dll $(srcdir)/bug-81691-a.cs
	$(MCS) -target:library -out:bug-81691-b.dll -r:bug-81691-a.dll $(srcdir)/bug-81691-b.cs
	$(MCS) -r:bug-81691-b.dll -out:bug-81691.exe $(srcdir)/bug-81691.cs
	rm -f bug-81691-a.dll
bug-81691.exe: bug-81691-b.dll

EXTRA_DIST += bug-81466-lib.il
bug-81466-lib.dll: bug-81466-lib.il
	$(ILASM) /dll /output:bug-81466-lib.dll $(srcdir)/bug-81466-lib.il
bug-81466.exe: bug-81466.il bug-81466-lib.dll
	$(ILASM) /exe /output:bug-81466.exe $(srcdir)/bug-81466.il

EXTRA_DIST += bug-324535-il.il
bug-324535-il.dll : bug-324535-il.il
	$(ILASM) /dll /output:bug-324535-il.dll $(srcdir)/bug-324535-il.il
bug-324535.exe : bug-324535.cs bug-324535-il.dll
	$(MCS) -r:bug-324535-il.dll -out:bug-324535.exe $(srcdir)/bug-324535.cs

EXTRA_DIST += custom-modifiers.2.cs custom-modifiers-lib.il
custom-modifiers-lib.dll: custom-modifiers-lib.il
	$(ILASM) /dll /output:custom-modifiers-lib.dll $(srcdir)/custom-modifiers-lib.il
custom-modifiers.2.exe: custom-modifiers.2.cs custom-modifiers-lib.dll
	$(MCS) -r:custom-modifiers-lib.dll -out:custom-modifiers.2.exe $(srcdir)/custom-modifiers.2.cs

EXTRA_DIST += bug-382986-lib.cs
bug-382986-lib.dll: bug-382986-lib.cs
	$(MCS) -target:library -out:$@ $(srcdir)/bug-382986-lib.cs
bug-382986.exe: bug-382986.cs bug-382986-lib.dll
	$(MCS) -out:$@ -r:bug-382986-lib.dll $(srcdir)/bug-382986.cs

EXTRA_DIST += bug-17537-helper.cs
bug-17537-helper.exe: bug-17537-helper.cs
	$(MCS) -out:$@ $(srcdir)/bug-17537-helper.cs
	chmod -x $@

bug-17537.exe: bug-17537.cs bug-17537-helper.exe
	$(MCS) -out:$@ $(srcdir)/bug-17537.cs

EXTRA_DIST += coreclr-security.cs

coreclr-security.exe : coreclr-security.cs
	$(SMCS) -out:coreclr-security.exe $(srcdir)/coreclr-security.cs

test-coreclr-security : coreclr-security.exe
	@echo "Testing coreclr-security.exe..."
	@$(RUNTIME_MOONLIGHT) --security=core-clr-test coreclr-security.exe

EXTRA_DIST += generic-unboxing.2.il
generic-unboxing.2.dll : generic-unboxing.2.il
	$(ILASM) /dll /output:generic-unboxing.2.dll $(srcdir)/generic-unboxing.2.il

EXTRA_DIST += generic-boxing.2.il
generic-boxing.2.dll : generic-boxing.2.il generic-unboxing.2.dll
	$(ILASM) /dll /output:generic-boxing.2.dll $(srcdir)/generic-boxing.2.il

EXTRA_DIST += generic-unbox.2.cs
generic-unbox.2.exe : generic-unbox.2.cs generic-unboxing.2.dll
	$(MCS) -r:generic-unboxing.2.dll -out:$@ $(srcdir)/generic-unbox.2.cs

EXTRA_DIST += generic-box.2.cs
generic-box.2.exe : generic-box.2.cs generic-unboxing.2.dll generic-boxing.2.dll
	$(MCS) -r:generic-unboxing.2.dll,generic-boxing.2.dll -out:$@ $(srcdir)/generic-box.2.cs

EXTRA_DIST += generic-delegate2.2.cs generic-delegate2-lib.2.il
generic-delegate2-lib.2.dll : generic-delegate2-lib.2.il
	$(ILASM) /dll /output:$@ $(srcdir)/generic-delegate2-lib.2.il
generic-delegate2.2.exe : generic-delegate2.2.cs generic-delegate2-lib.2.dll
	$(MCS) -r:generic-delegate2-lib.2.dll -out:$@ $(srcdir)/generic-delegate2.2.cs

bug-3903.exe: bug-3903.cs
	$(MCS) -sdk:2 $(srcdir)/bug-3903.cs -out:$@

gshared: test-generic-sharing

gshared-aot:
	@$(MAKE) AOT=1 gshared

GSHARED_TESTS = \
		generics-sharing.2.exe shared-generic-methods.2.exe	\
		shared-generic-synchronized.2.exe generic-initobj.2.exe		\
		generics-sharing-other-exc.2.exe generic-box.2.exe		\
		generic-unbox.2.exe generic-delegate.2.exe generic-sizeof.2.exe	\
		generic-ldobj.2.exe generic-mkrefany.2.exe			\
		generic-refanyval.2.exe generic-ldtoken.2.exe			\
		generic-ldtoken-method.2.exe  generic-ldtoken-field.2.exe	\
		generic-virtual.2.exe generic-tailcall.2.exe			\
		generic-interface-methods.2.exe generic-array-type.2.exe	\
		generic-method-patching.2.exe generic-static-methods.2.exe	\
		generic-null-call.2.exe generic-tailcall2.2.exe			\
		generic-array-exc.2.exe generic-special.2.exe			\
		generic-exceptions.2.exe generic-delegate2.2.exe		\
		generic-virtual2.2.exe generic-valuetype-interface.2.exe	\
		generic-valuetype-newobj.2.exe generic-valuetype-newobj2.2.exe	\
		generic-getgenericarguments.2.exe generic-type-builder.2.exe	\
		generic-synchronized.2.exe generic-delegate-ctor.2.exe		\
		generic-constrained.2.exe bug-431413.2.exe			\
		generic-virtual-invoke.2.exe generic-typedef.2.exe		\
		generic-marshalbyref.2.exe bug-459285.2.exe bug-461198.2.exe	\
		generic-sealed-virtual.2.exe generic-system-arrays.2.exe	\
		generic-stack-traces.2.exe generic-stack-traces2.2.exe		\
		bug-472600.2.exe bug-473482.2.exe bug-473999.2.exe		\
		bug-479763.2.exe				\
		generic-type-load-exception.2.exe bug-616463.exe	\
		bug-1147.exe

test-generic-sharing-normal: $(GSHARED_TESTS)
	@for fn in $+ ; do	\
		echo "Testing $$fn ...";	\
		$(RUNTIME) -O=gshared                $$fn > $$fn.stdout || exit 1;	\
		$(RUNTIME) -O=gshared,shared         $$fn > $$fn.stdout || exit 1;	\
		$(RUNTIME) -O=gshared,-inline        $$fn > $$fn.stdout || exit 1;	\
		$(RUNTIME) -O=gshared,-inline,shared $$fn > $fn.stdout || exit 1;	\
		if [ x$(AOT) = x1 ]; then $(with_mono_path) $(JITTEST_PROG_RUN) --aot --debug $$fn > /dev/null || exit 1; $(RUNTIME) $$fn > $$fn.stdout || exit 1; fi; \
	done

test-generic-sharing-managed: test-runner.exe $(GSHARED_TESTS)
	@$(RUNTIME) ./test-runner.exe -j a --opt-sets "gshared gshared,shared gshared,-inline gshared,-inline,shared" $(GSHARED_TESTS)

if NACL_CODEGEN
test-generic-sharing:
else
test-generic-sharing:
	@if test x$(M) != x0; then $(MAKE) test-generic-sharing-managed; else $(MAKE) test-generic-sharing-normal; fi
endif

EXTRA_DIST += async-exceptions.cs
async-exceptions.exe : async-exceptions.cs
	$(MCS) -out:async-exceptions.exe $(srcdir)/async-exceptions.cs
test-async-exceptions : async-exceptions.exe
	for i in `echo 0 1 2 3 4 5 6 7 8 9 10`; do $(RUNTIME) --inject-async-exc Tests:foo $$i async-exceptions.exe || exit 1; done
	for i in `echo 0 1 2 3 4 5 6 7 8 9 10`; do $(RUNTIME) --inject-async-exc Tests:bar $$i async-exceptions.exe || exit 1; done

EXTRA_DIST += modules.cs modules-m1.cs
modules-m1.netmodule: modules-m1.cs
	$(MCS) -out:$@ /target:module $(srcdir)/modules-m1.cs
modules.exe: modules.cs modules-m1.netmodule TestDriver.dll
	$(MCS) -out:$@ /addmodule:modules-m1.netmodule -r:TestDriver.dll $(srcdir)/modules.cs

# Useful if mono is compiled with --enable-shared=no
patch-libtool:
	cp "../../libtool" .
	sed -e 's,build_libtool_libs=no,build_libtool_libs=yes,g' libtool > 2; mv 2 libtool
	sed -e 's,LIBTOOL =,LIBTOOL2 =,g' Makefile > 2 && echo "LIBTOOL = bash ./libtool" > 1 && cat 1 2 > Makefile
	touch libtest.c


if NACL_CODEGEN
test-process-exit:
else
EXTRA_DIST += threadpool-in-processexit.cs threadpool-in-processexit.exe.stdout.expected
test-process-exit:
	@$(MCS) $(srcdir)/threadpool-in-processexit.cs -out:threadpool-in-processexit.exe
	@echo "Testing threadpool-in-processexit.exe..."
	@$(RUNTIME) threadpool-in-processexit.exe > threadpool-in-processexit.exe.stdout
	@diff -w threadpool-in-processexit.exe.stdout $(srcdir)/threadpool-in-processexit.exe.stdout.expected
endif

OOM_TESTS =	\
	gc-oom-handling.exe	\
	gc-oom-handling2.exe


test-oom: $(OOM_TESTS)
	@for fn in $+ ; do	\
		echo "Testing $$fn ...";	\
		MONO_GC_PARAMS=max-heap-size=16m MONO_ENV_OPTIONS="--gc=sgen"                     $(RUNTIME) $$fn > $$fn.stdout || exit 1;	\
		MONO_GC_PARAMS=max-heap-size=16m                                                  $(RUNTIME) $$fn > $$fn.stdout || exit 1;	\
	done

if HOST_WIN32
test-unhandled-exception-2:
else
test-unhandled-exception-2: unhandled-exception.exe
	@echo "Testing unhandled-exception_1 ..."; $(RUNTIME) $+ 1 1> unhandled-exception_1.exe.stdout 2> unhandled-exception_1.exe.stderr; if test "x$$?" != "x1";   then exit 1; fi;	\
	 echo "Testing unhandled-exception_2 ..."; $(RUNTIME) $+ 2 1> unhandled-exception_2.exe.stdout 2> unhandled-exception_2.exe.stderr; if test "x$$?" != "x0";   then exit 2; fi;	\
	 echo "Testing unhandled-exception_3 ..."; $(RUNTIME) $+ 3 1> unhandled-exception_3.exe.stdout 2> unhandled-exception_3.exe.stderr; if test "x$$?" != "x0";   then exit 3; fi;	\
	 echo "Testing unhandled-exception_4 ..."; $(RUNTIME) $+ 4 1> unhandled-exception_4.exe.stdout 2> unhandled-exception_4.exe.stderr; if test "x$$?" != "x0";   then exit 4; fi;	\
	 echo "Testing unhandled-exception_5 ..."; $(RUNTIME) $+ 5 1> unhandled-exception_5.exe.stdout 2> unhandled-exception_5.exe.stderr; if test "x$$?" != "x255"; then exit 5; fi;	\
	 echo "Testing unhandled-exception_6 ..."; $(RUNTIME) $+ 6 1> unhandled-exception_6.exe.stdout 2> unhandled-exception_6.exe.stderr; if test "x$$?" != "x0";   then exit 6; fi;	\
	 echo "Testing unhandled-exception_7 ..."; $(RUNTIME) $+ 7 1> unhandled-exception_7.exe.stdout 2> unhandled-exception_7.exe.stderr; if test "x$$?" != "x0";   then exit 7; fi;	\
	 echo "Testing unhandled-exception_8 ..."; $(RUNTIME) $+ 8 1> unhandled-exception_8.exe.stdout 2> unhandled-exception_8.exe.stderr; if test "x$$?" != "x3";   then exit 8; fi
endif

noinst_LTLIBRARIES = libtest.la

AM_CPPFLAGS = $(GLIB_CFLAGS)

if HOST_WIN32
# gcc-3.4.4 emits incorrect code when making indirect calls to stdcall functions using a tail call
# This shows up when compiling mono_test_marshal_delegate ()
libtest_la_CFLAGS=-fno-optimize-sibling-calls
# the exported names created by gcc for stdcall functions are missing the leading _, so MS.NET
# can't find them. So we use --kill-at to remove the @ suffix as well.
libtest_la_LDFLAGS=-no-undefined -rpath `pwd` -Wl,--kill-at
else
libtest_la_LDFLAGS = -rpath `pwd`
endif
libtest_la_SOURCES = libtest.c
libtest_la_LIBADD = $(GLIB_LIBS) $(LIBICONV)

CLEANFILES = $(TESTSI_CS) $(TESTSI_IL) $(STRESS_TESTS) *.dll *.stdout *.aotlog *.exe stest.dat
