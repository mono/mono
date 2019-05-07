GitHubTarballPackage(
    'mono',
    'libgdiplus',
    '5.6.1',
    '3506915c865dec746565bbc4a5683d4e53e842b4',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
