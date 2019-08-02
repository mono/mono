GitHubTarballPackage(
    'mono',
    'libgdiplus',
    'release/6.0',
    '8ad7eafc0cbe891e784188681fc6fb6373ff394e',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
