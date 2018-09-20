MONO_OPENSSL_SOURCES_FILES = \
	openssl-bio.c \
	openssl-bio.h \
	openssl-error.c \
	openssl-error.h \
	openssl-key.c \
	openssl-key.h \
	openssl-pkcs12.c \
	openssl-pkcs12.h \
	openssl-rsa.c \
	openssl-rsa.h \
	openssl-ssl.c \
	openssl-ssl-ctx.c \
	openssl-ssl-ctx.h \
	openssl-ssl.h \
	openssl-time64.c \
	openssl-util.c \
	openssl-util.h \
	openssl-x509.c \
	openssl-x509-chain.c \
	openssl-x509-chain.h \
	openssl-x509-crl.c \
	openssl-x509-crl.h \
	openssl-x509.h \
	openssl-x509-lookup.c \
	openssl-x509-lookup.h \
	openssl-x509-lookup-mono.c \
	openssl-x509-lookup-mono.h \
	openssl-x509-name.c \
	openssl-x509-name.h \
	openssl-x509-revoked.c \
	openssl-x509-revoked.h \
	openssl-x509-store.c \
	openssl-x509-store-ctx.c \
	openssl-x509-store-ctx.h \
	openssl-x509-store.h \
	openssl-x509-verify-param.c \
	openssl-x509-verify-param.h \
	CMakeLists.txt

EXTRA_DIST = $(MONO_OPENSSL_SOURCES_FILES)

CMAKE_VERBOSE=$(if $(V),VERBOSE=1,)
NINJA_VERBOSE=$(if ($V),-v,)

if NINJA
NINJA_ARGS = -G Ninja
BUILDFILE = build.ninja
CMAKE_MAKE_PROGRAM = $(shell which ninja-build || which ninja)
else
NINJA_ARGS =
BUILDFILE = Makefile
CMAKE_MAKE_PROGRAM = $(shell which gmake || which gnumake || which make)
endif

CMAKE_ARGS = -D CMAKE_MAKE_PROGRAM=$(CMAKE_MAKE_PROGRAM) -D CMAKE_INSTALL_PREFIX:PATH=$(prefix) -D OPENSSL_ROOT:PATH=$(OPENSSL_ROOT) \
	-D SRC_DIR:PATH=$(abs_top_srcdir)/mono/openssl -D OPENSSL_CFLAGS:STRING="$(OPENSSL_CFLAGS)" $(NINJA_ARGS)

all-local: build-shared/libmono-openssl-shared$(libsuffix)

build-shared/$(BUILDFILE):
	-mkdir -p build-shared
	(cd build-shared && CC="$(CC)" CXX="$(CXX)" CFLAGS="$(CFLAGS) $(OPENSSL_CFLAGS)" LDFLAGS="$(LDFLAGS) $(OPENSSL_LDFLAGS)" $(CMAKE) $(CMAKE_ARGS) $(OPENSSL_CMAKE_ARGS) -DBUILD_SHARED_LIBS=1 $(abs_top_srcdir)/mono/openssl)

if NINJA
build-shared/libmono-openssl-shared$(libsuffix): build-shared/$(BUILDFILE) $(MONO_OPENSSL_SOURCES_FILES)
	ninja -C build-shared $(NINJA_VERBOSE)
else
build-shared/libmono-openssl-shared$(libsuffix): build-shared/$(BUILDFILE) $(MONO_OPENSSL_SOURCES_FILES)
	$(MAKE) -C build-shared $(CMAKE_VERBOSE)
endif

clean-local:
	-rm -rf build-shared

install-exec-local:
	$(mkinstalldirs) "$(DESTDIR)$(libdir)"
	$(install_sh) build-shared/libmono-openssl-shared.* "$(DESTDIR)$(libdir)"
