check-microbench: DebianShootoutMono.stamp
	@$(MAKE) test-run-microbench

DebianShootoutMono.stamp: Makefile microbench.mk
	@$(MAKE) validate-DebianShootoutMono RESET_VERSIONS=1
	@$(MAKE) prepare-dlls
	@touch $@

abs_top_srcdir = $(abspath $(top_srcdir))
NET_4_X_RUNTIME = MONO_PATH=$(abs_top_srcdir)/mcs/class/lib/net_4_x $(abs_top_srcdir)/runtime/mono-wrapper
FULL_AOT_RUNTIME = MONO_PATH=$(abs_top_srcdir)/mcs/class/lib/testing_aot_full $(abs_top_srcdir)/runtime/mono-wrapper

define BenchmarkDotNetTemplate
run-bench-$(1):: DebianShootoutMono.stamp
	MONO_BENCH_AOT_RUN="$(AOT_RUN_FLAGS)" MONO_BENCH_AOT_BUILD="$(AOT_BUILD_FLAGS)" MONO_BENCH_EXECUTABLE="$(abs_top_srcdir)/runtime/mono-wrapper" MONO_BENCH_PATH="$(abs_top_srcdir)/mcs/class/lib/$(DEFAULT_PROFILE)" $(NET_4_X_RUNTIME) DebianShootoutMono/release/DebianShootoutMono.exe

test-run-microbench:: run-microbench-$(1)
endef

.PHONY: prepare-dlls

ifdef AOT_BUILD_FLAGS
prepare-dlls: 
	$(FULL_AOT_RUNTIME) $(AOT_BUILD_FLAGS) DebianShootoutMono/release/*.{dll,exe}

else

prepare-dlls:

endif

$(eval $(call BenchmarkDotNetTemplate,BinaryTrees))
$(eval $(call BenchmarkDotNetTemplate,NBody))
$(eval $(call BenchmarkDotNetTemplate,Mandelbrot))
$(eval $(call BenchmarkDotNetTemplate,RegexRedux))
$(eval $(call BenchmarkDotNetTemplate,SpectralNorm))
$(eval $(call BenchmarkDotNetTemplate,Fannkuchredux))
$(eval $(call BenchmarkDotNetTemplate,Fasta))
$(eval $(call BenchmarkDotNetTemplate,KNucleotide))
$(eval $(call BenchmarkDotNetTemplate,RevComp))


