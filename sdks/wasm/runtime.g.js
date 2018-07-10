
var Module = { 
	onRuntimeInitialized: function () {
		MONO.mono_load_runtime_and_bcl (
			"@VFS_PREFIX@",
			"@DEPLOY_PREFIX@",
			@ENABLE_DEBUGGING@,
			[ @FILE_LIST@ ],
			function () {
				@BINDINGS_LOADING@
				App.init ();
			});
	},
};
