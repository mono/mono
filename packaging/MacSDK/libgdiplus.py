GitHubTarballPackage(
    'mono',
    'libgdiplus',
    '6.0.4',
    'c5b9035d14146e054f2636f2e70dd3577bdaa397',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
