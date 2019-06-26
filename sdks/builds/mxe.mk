
ifeq ($(UNAME),Linux)
LINUX_FLAVOR=$(shell ./determine-linux-flavor.sh)
endif

LINUX_WITH_MINGW=:Ubuntu:,:Debian:,:Debian GNU/Linux:
LINUX_HAS_MINGW=$(if $(findstring :$(LINUX_FLAVOR):,$(LINUX_WITH_MINGW)),yes)

ifeq ($(UNAME),Windows)
.PHONY: provision-mxe
provision-mxe:
	@echo "Won't provision MXE on Windows. Please install mingw packages, instead."

else ifeq ($(LINUX_HAS_MINGW),yes)
MXE_PREFIX=/usr

.PHONY: provision-mxe
provision-mxe:
	@echo $(LINUX_FLAVOR) Linux does not require mxe provisioning. mingw from packages is used instead
else
MXE_PREFIX:=$(shell brew --prefix)

.PHONY: provision-mxe
provision-mxe:
	brew tap xamarin/xamarin-android-windeps
	brew install mingw-w64 xamarin/xamarin-android-windeps/mingw-zlib
endif

.PHONY: provision
provision: provision-mxe
