include $(top_srcdir)/mk/common.mk

DEBIANSHOOTOUTMONO_PATH=DebianShootoutMono

SUBMODULES_CONFIG_FILE = $(top_srcdir)/mono/tests/microbenchmarks/SUBMODULES.json
include $(top_srcdir)/scripts/submodules/versions.mk

$(eval $(call ValidateVersionTemplate,DebianShootoutMono,DEBIANSHOOTOUTMONO))

# Bump the given submodule to the revision given by the REV make variable
# If COMMIT is 1, commit the change
bump-DebianShootoutMono: __bump-version-DebianShootoutMono

# Bump the given submodule to the branch given by the BRANCH/REMOTE_BRANCH make variables
# If COMMIT is 1, commit the change
bump-branch-DebianShootoutMono: __bump-branch-DebianShootoutMono

# Bump the given submodule to its current GIT version
# If COMMIT is 1, commit the change
bump-current-DebianShootoutMono: __bump-current-version-DebianShootoutMono

clean-local:
	$(RM) -r $(BENCH_PATH)

EXTRA_DIST=SUBMODULES.json

DebianShootoutMono.stamp: Makefile
	$(MAKE) reset-DebianShootoutMono
	$(MAKE) prepare-dlls
	touch $@

all-local: test

check-local: test

abs_top_srcdir = $(abspath $(top_srcdir))
NET_4_X_RUNTIME = MONO_PATH=$(abs_top_srcdir)/mcs/class/lib/net_4_x $(abs_top_srcdir)/runtime/mono-wrapper
FULL_AOT_RUNTIME = MONO_PATH=$(abs_top_srcdir)/mcs/class/lib/testing_aot_full $(abs_top_srcdir)/runtime/mono-wrapper

define BenchmarkDotNetTemplate
test-$(1):: DebianShootoutMono.stamp
	MONO_BENCH_AOT_RUN="$(AOT_RUN_FLAGS)" MONO_BENCH_AOT_BUILD="$(AOT_BUILD_FLAGS)" MONO_BENCH_EXECUTABLE="$(abs_top_srcdir)/runtime/mono-wrapper" MONO_BENCH_PATH="$(abs_top_srcdir)/mcs/class/lib/$(DEFAULT_PROFILE)" $(NET_4_X_RUNTIME) DebianShootoutMono/release/DebianShootoutMono.exe

test:: test-$(1)
endef

if FULL_AOT_TESTS
prepare-dlls: DebianShootoutMono.stamp
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

#$(eval $(call BenchmarkDotNetTemplate,Template))
