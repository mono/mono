GitHubTarballPackage(
    'mono',
    'libgdiplus',
    '5.6',
    'f07965ec5fc2b22bfb300d5fef410d25ae472a34',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
