TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config


all: build

build::
	$(MAKE) -C builds all


package::
	$(MAKE) -C builds package

clean::
	$(MAKE) -C builds clean

ifndef DISABLE_IOS
build::
	$(MAKE) -C ios build

package::
	$(MAKE) -C ios package

clean::
	$(MAKE) -C ios clean
endif

ifndef DISABLE_ANDROID
build::
	$(MAKE) -C android build

package::
	$(MAKE) -C android package

clean::
	$(MAKE) -C android clean
endif

ifndef DISABLE_WASM
build::
	$(MAKE) -C wasm build

package::
	$(MAKE) -C wasm package

clean::
	$(MAKE) -C wasm clean
endif
