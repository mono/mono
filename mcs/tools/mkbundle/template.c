void mono_mkbundle_init ()
{
	install_dll_config_files ();
	mono_register_bundled_assemblies(bundled);
}
