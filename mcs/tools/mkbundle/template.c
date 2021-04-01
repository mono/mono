void mono_mkbundle_init ()
{
	init_default_mono_api_struct ();
	install_dll_config_files ();
	mono_api.mono_register_bundled_assemblies(bundled);

	install_aot_modules ();
}
