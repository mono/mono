
.stamp-bcl-toolchain:
	touch $@

.stamp-bcl-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/bcl
	cd $(TOP)/sdks/builds/bcl && $(TOP)/configure --with-monodroid=yes --with-monotouch=yes --with-profile4_x=no --with-mcs-docs=no --disable-btls --disable-nls --disable-boehm
	touch $@

$(TOP)/sdks/out/bcl/monodroid $(TOP)/sdks/out/bcl/monotouch:
	mkdir -p $@

.PHONY: package-bcl
package-bcl: $(TOP)/sdks/out/bcl/monodroid $(TOP)/sdks/out/bcl/monotouch
	cp -R $(TOP)/mcs/class/lib/monodroid/* $(TOP)/sdks/out/bcl/monodroid
	cp -R $(TOP)/mcs/class/lib/monotouch/* $(TOP)/sdks/out/bcl/monotouch

.PHONY: clean-bcl
clean-bcl::
	rm -rf .stamp-bcl-toolchain .stamp-bcl-configure $(TOP)/sdks/builds/bcl

TARGETS += bcl
