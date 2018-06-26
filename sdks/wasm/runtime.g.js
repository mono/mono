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
		assemblies.forEach (function(file_name) {
			++pending;
			fetch ("@DEPLOY_PREFIX@" + file_name, { credentials: 'same-origin' }).then (function (response) {
				if (!response.ok)
					throw "failed to load '" + file_name + "'";
				return response['arrayBuffer']();
			}).then (function (blob) {
				var asm = new Uint8Array (blob);
				Module.FS_createDataFile ("@DEPLOY_PREFIX@" + file_name, null, asm, true, true, true);
				console.log ("Loaded: " + file_name);
				--pending;
				if (pending == 0)
					Module.bclLoadingDone ();
			});
		});
	},
	
	bclLoadingDone: function () {
		MonoRuntime.init ();
	}
};

var MonoRuntime = {
	init: function () {
		this.load_runtime = Module.cwrap ('mono_wasm_load_runtime', null, ['string', 'number']);

		//FIXME move this two to library_mono.js (better yet - library_mono_debugger.js)
		this.mono_wasm_set_breakpoint = Module.cwrap ('mono_wasm_set_breakpoint', 'number', ['string', 'number', 'number']);
		this.mono_wasm_clear_all_breakpoints = Module.cwrap ('mono_wasm_clear_all_breakpoints', 'void', [ ]);

		this.load_runtime ("@VFS_PREFIX@", @ENABLE_DEBUGGING@);
		mono_wasm_runtime_ready ();
		@BINDINGS_LOADING@
		App.init ();
	},
};
