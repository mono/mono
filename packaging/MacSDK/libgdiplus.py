GitHubTarballPackage(
    'mono',
    'libgdiplus',
    'release/6.0',
    '93f43204a94ac75f468c58334ad07726958f4f26',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
