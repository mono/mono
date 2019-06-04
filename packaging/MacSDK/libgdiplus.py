GitHubTarballPackage(
    'mono',
    'libgdiplus',
    'mono-2018-12',
    'aea1a492713b0ab101876e4ee72dde0f1cb3e3b4',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
