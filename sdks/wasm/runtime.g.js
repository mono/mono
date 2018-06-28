//FIXME move this to library_mono.js
var mono_wasm_runtime_is_ready = false;
function mono_wasm_runtime_ready () {
	console.log (">>mono_wasm_runtime_ready");
	mono_wasm_runtime_is_ready = true;
	debugger;
}



var Module = { 
	onRuntimeInitialized: function () {
		var assemblies = [ @FILE_LIST@ ];
		Module.FS_createPath ("/", "@VFS_PREFIX@", true, true);

		var pending = 0;
		var loaded_files = [];
		assemblies.forEach (function(file_name) {
			++pending;
			fetch ("@DEPLOY_PREFIX@" + file_name, { credentials: 'same-origin' }).then (function (response) {
				if (!response.ok)
					throw "failed to load '" + file_name + "'";
				loaded_files.push (response.url);
				return response['arrayBuffer']();
			}).then (function (blob) {
				var asm = new Uint8Array (blob);
				Module.FS_createDataFile ("@VFS_PREFIX@/" + file_name, null, asm, true, true, true);
				console.log ("Loaded: " + file_name);
				--pending;
				if (pending == 0)
					Module.bclLoadingDone (loaded_files);
			});
		});
	},
	
	bclLoadingDone: function (loaded_files) {
		MonoRuntime.init (loaded_files);
	}
};

var MonoRuntime = {
	init: function (loaded_files) {
		this.load_runtime = Module.cwrap ('mono_wasm_load_runtime', null, ['string', 'number']);

		//FIXME move this two to library_mono.js (better yet - library_mono_debugger.js)
		this.mono_wasm_set_breakpoint = Module.cwrap ('mono_wasm_set_breakpoint', 'number', ['string', 'number', 'number']);
		this.mono_wasm_clear_all_breakpoints = Module.cwrap ('mono_wasm_clear_all_breakpoints', 'void', [ ]);
		this.get_loaded_files = function() { return loaded_files; };

		this.load_runtime ("@VFS_PREFIX@", @ENABLE_DEBUGGING@);
		mono_wasm_runtime_ready ();
		@BINDINGS_LOADING@
		App.init ();
	},
};
