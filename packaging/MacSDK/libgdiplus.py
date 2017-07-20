GitHubTarballPackage(
    'mono',
    'libgdiplus',
    '5.4',
    '350eb49a45ca5a7383c01d49df72438347a5dbc9',
    configure='CFLAGS="%{gcc_flags} %{local_gcc_flags} -I/opt/X11/include" ./autogen.sh --prefix="%{package_prefix}"',
    override_properties={
        'make': 'C_INCLUDE_PATH="" make'})
