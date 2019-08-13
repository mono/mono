GitHubTarballPackage(
    'mono',
    'libgdiplus',
    '6.0.2',
    '35ae5341c2326c44e26058ca90b12303bb9b1da0',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
