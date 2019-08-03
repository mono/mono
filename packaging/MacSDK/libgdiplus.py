GitHubTarballPackage(
    'mono',
    'libgdiplus',
    '6.0.1',
    '5bc2f73373cb879e1c78c91fe4bf63f3893f8714',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
