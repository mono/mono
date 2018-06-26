
ifeq ($(UNAME),Linux)
LINUX_FLAVOR=$(shell ./determine-linux-flavor.sh)
endif

ifeq ($(LINUX_FLAVOR),Ubuntu)
MXE_PREFIX=/usr

.PHONY: provision-mxe
provision-mxe:
	@echo $(LINUX_FLAVOR) Linux does not require mxe provisioning. mingw from packages is used instead
else
MXE_SRC?=$(TOP)/sdks/builds/toolchains/mxe
MXE_PREFIX_DIR?=$(TOP)/sdks/out

# This is not overridable
MXE_PREFIX:=$(MXE_PREFIX_DIR)/mxe-$(shell echo $(MXE_HASH) | head -c 7)

$(MXE_PREFIX):
	mkdir -p $@

ifeq ($(UNAME),Darwin)
.PHONY: download-mxe
download-mxe: | $(MXE_PREFIX)
	wget --no-verbose -O - https://xamjenkinsartifact.blob.core.windows.net/build-package-osx-mxe/mxe-osx-$(MXE_HASH).tar.gz | tar -C $(MXE_PREFIX) -xf - --strip 2 tmp/mxe-$(MXE_HASH)
else
.PHONY: download-mxe
download-mxe:
	$(error We prebuild MXE only on macOS)
endif

$(MXE_SRC)/Makefile:
	git clone -b xamarin https://github.com/xamarin/mxe.git $(dir $@)
	cd $(dir $@) && git checkout $(MXE_HASH)

.PHONY: build-mxe
build-mxe: | $(MXE_SRC)/Makefile
	$(MAKE) -C $(MXE_SRC) gcc cmake zlib pthreads dlfcn-win32 mman-win32 \
		PREFIX="$(MXE_PREFIX)" MXE_TARGETS="i686-w64-mingw32.static x86_64-w64-mingw32.static" \
			OS_SHORT_NAME="disable-native-plugins" PATH="$$PATH:$(MXE_PREFIX)/bin:$(dir $(shell brew list gettext | grep bin/autopoint$))"

$(MXE_PREFIX)/.stamp:
	rm -rf $(MXE_PREFIX)
	$(MAKE) download-mxe || $(MAKE) build-mxe
	touch $@

.PHONY: provision-mxe
provision-mxe: $(MXE_PREFIX)/.stamp
endif
