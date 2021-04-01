
var Module = { 
	onRuntimeInitialized: function () {
	  MONO.mono_wasm_init_coverage_profiler (null);
		MONO.mono_load_runtime_and_bcl (
			config.vfs_prefix,
			config.deploy_prefix,
			config.enable_debugging,
			config.file_list,
			function () {
			  try {
				App.init ();
			  } catch (ex) {
				console.out (ex);
			  }
			}
		)
	},
};

print = console.log;

function test_exit (exit_code) {
	if (is_browser) {
		// Notify the puppeteer script
		Module.exit_code = exit_code;
		console.log ("WASM EXIT " + exit_code);
	} else {
		Module.wasm_exit (exit_code);
	}
}

function fail_exec (reason) {
	console.log (reason);
	test_exit (1);
}

var App = {
    init: function () {
	  console.log ("HELLO!");

	  var assembly_load = Module.cwrap ('mono_wasm_assembly_load', 'number', ['string'])
	  var find_class = Module.cwrap ('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string'])
	  var find_method = Module.cwrap ('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number'])
	  var runtime_invoke = Module.cwrap ('mono_wasm_invoke_method', 'number', ['number', 'number', 'number', 'number']);
	  var string_from_js = Module.cwrap ('mono_wasm_string_from_js', 'number', ['string']);
	  var assembly_get_entry_point = Module.cwrap ('mono_wasm_assembly_get_entry_point', 'number', ['number']);
	  var string_get_utf8 = Module.cwrap ('mono_wasm_string_get_utf8', 'string', ['number']);
	  var string_array_new = Module.cwrap ('mono_wasm_string_array_new', 'number', ['number']);
	  var obj_array_set = Module.cwrap ('mono_wasm_obj_array_set', 'void', ['number', 'number', 'number']);
	  var exit = Module.cwrap ('mono_wasm_exit', 'void', ['number']);
	  var wasm_setenv = Module.cwrap ('mono_wasm_setenv', 'void', ['string', 'string']);
	  var wasm_set_main_args = Module.cwrap ('mono_wasm_set_main_args', 'void', ['number', 'number']);
	  var wasm_strdup = Module.cwrap ('mono_wasm_strdup', 'number', ['string'])

	  main_assembly = assembly_load ("main.exe");
	  if (main_assembly == 0)
		fail_exec ("Error: Unable to load main executable.'");
	  main_method = assembly_get_entry_point (main_assembly);
	  if (main_method == 0)
		fail_exec ("Error: Main (string[]) method not found.");
	  var app_args = string_array_new (0);
	  var main_argc = 1;
	  var main_argv = Module._malloc (main_argc * 4);
	  aindex = 0;
	  Module.setValue (main_argv + (aindex * 4), wasm_strdup ("main.exe"), "i32")
	  wasm_set_main_args (main_argc, main_argv);

	  try {
		var invoke_args = Module._malloc (4);
		Module.setValue (invoke_args, app_args, "i32");
		var eh_throw = Module._malloc (4);
		Module.setValue (eh_throw, 0, "i32");
		var res = runtime_invoke (main_method, 0, invoke_args, eh_throw);
		var eh_res = Module.getValue (eh_throw, "i32");
		if (eh_res == 1) {
		  print ("Exception:" + string_get_utf8 (res));
		  test_exit (1);
		}
	  } catch (ex) {
		print ("JS exception: " + ex);
		print (ex.stack);
		test_exit (1);
	  }
	}
};
