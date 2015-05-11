if SUPPORT_SGEN
sgen_dirs = sgen
endif

if CROSS_COMPILING
SUBDIRS = arch utils io-layer cil metadata $(sgen_dirs) mini dis profiler
else
if INSTALL_MONOTOUCH
SUBDIRS = utils io-layer metadata arch $(sgen_dirs) mini profiler

monotouch-do-build:
	@list='$(SUBDIRS)'; for subdir in $$list; do \
	  case "x$$subdir" in \
		xmetadata ) target="monotouch-do-build" ;; \
		xmini ) target="monotouch-do-build" ;; \
		* ) target="all" ;; \
	  esac; \
	  echo "Making $$target in $$subdir"; \
	  (cd $$subdir && $(MAKE) $(AM_MAKEFLAGS) $$target); \
    done;

monotouch-do-clean:
	@list='$(SUBDIRS)'; for subdir in $$list; do \
	  case "x$$subdir" in \
		xmetadata ) target="monotouch-do-clean" ;; \
		xmini ) target="monotouch-do-clean" ;; \
		* ) target="clean" ;; \
	  esac; \
	  echo "Making $$target in $$subdir"; \
	  (cd $$subdir && $(MAKE) $(AM_MAKEFLAGS) $$target); \
    done;
else

mono_subdirs = arch utils io-layer cil metadata $(sgen_dirs) mini dis tests unit-tests benchmark profiler

SUBDIR_PREFIX = subdir

define SUBDIR_RULE
.PHONY: $(SUBDIR_PREFIX)-$2-$1

$(SUBDIR_PREFIX)-$2-$1:
	@echo "Making $2 in $1"; \
	(cd $1 && $(MAKE) $(AM_MAKEFLAGS) $2)
endef

define S
$(addprefix $(SUBDIR_PREFIX)-all-,$1)
endef

all-local: $(call S,$(mono_subdirs))

$(foreach SUBDIR,$(mono_subdirs),$(eval $(call SUBDIR_RULE,$(SUBDIR),all)))

$(call S,mini): $(call S,metadata utils)
$(call S,mini): $(call S,arch dis io-layer metadata sgen utils)
$(call S,profiler): $(call S,mini)
$(call S,tests): $(call S,io-layer metadata sgen utils)
$(call S,unit-tests): $(call S,io-layer metadata sgen utils)
$(call S,dis): $(call S,io-layer metadata sgen utils)

clean-local: $(addprefix $(SUBDIR_PREFIX)-clean-,$(mono_subdirs))

$(foreach SUBDIR,$(mono_subdirs),$(eval $(call SUBDIR_RULE,$(SUBDIR),clean)))

endif
endif
DIST_SUBDIRS = arch utils io-layer cil metadata $(sgen_dirs) mini dis tests unit-tests benchmark profiler
