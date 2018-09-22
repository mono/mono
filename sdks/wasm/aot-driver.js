//glue code to deal with the differences between ch, d8, jsc and sm.
if (print == undefined)
	print = console.log;


if (console != undefined) {
	var has_console_warn = false;
	try {
		if (console.warn != undefined)
			has_console_warn = true;
	} catch(e) {}

	if (!has_console_warn)
		console.warn = console.log;
}

fail_exec = function(reason) {
	print (reason);
	throw "FAIL";
}

try {
	arguments = WScript.Arguments;
	load = WScript.LoadScriptFile;
	read = WScript.LoadBinaryFile;
	fail_exec = function(reason) {
		print (reason);
		WScript.Quit(1);
	}
} catch(e) {}

try {
	if (scriptArgs !== undefined)
		arguments = scriptArgs;
} catch(e) {}


// load("runtime.js")

var App = {
	init: function () { print ("runtime init finished"); }
};

function ResolveEagerPromise(value) {
	this.then = function(cb) {
		var res = cb (value);
		return new ResolveEagerPromise(res);
	}
}

var Module = { 
	onRuntimeInitialized: function () {
		print("initing the runtime");
		MONO.mono_load_runtime_and_bcl (
			"managed",
			"managed",
			0,
			[ "mscorlib.dll", "mini_tests_basic.dll",  ],
			function () { App.init (); },
			function (file) { 
				// print("loading " + file)
				return new ResolveEagerPromise ({
					ok: true,
					url: file,
					arrayBuffer: function() { return read (file, 'binary') }
				})
			});
	},

	print: function(x) { print ("WASM: " + x) },
	printErr: function(x) { print ("WASM-ERR: " + x) },

    totalDependencies: 0,
    monitorRunDependencies: function(left) {
      this.totalDependencies = Math.max(this.totalDependencies, left);
      print("STATUS: "+ (left ? 'Preparing... (' + (this.totalDependencies-left) + '/' + this.totalDependencies + ')' : 'All downloads complete.'));
    },

	instantiateWasm: function (env, receiveInstance) {
		//merge Module's env with emcc's env
		env.env = Object.assign({}, env.env, this.env);
		var module = new WebAssembly.Module (read ('mono.wasm', 'binary'))
		this.wasm_instance = new WebAssembly.Instance (module, env);
		this.em_cb = receiveInstance;
		return this
	},

	finish_loading: function () {
		this.em_cb (this.wasm_instance);
	},

	env: {
	},

};


load("mono.js")
Module.finish_loading ();

function not_null(value) {
	if (!value)
		throw "error";
	return value;
}

var assembly_load = Module.cwrap ('mono_wasm_assembly_load', 'number', ['string'])
var find_class = Module.cwrap ('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string'])
var find_method = Module.cwrap ('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number'])
var mono_runtime_invoke = Module.cwrap ('mono_wasm_invoke_method', 'number', ['number', 'number', 'number', 'number']);
var mono_unbox_int = Module.cwrap ('mono_unbox_int', 'number', ['number']);
const IGNORE_PARAM_COUNT = -1;

var test_suite = not_null (assembly_load("mini_tests_basic"))
var basic_tests = not_null (find_class (test_suite, "", "BasicTests"))
var test_0_return = not_null (find_method (basic_tests, "test_0_return", IGNORE_PARAM_COUNT))

var eh_throw = Module._malloc (4);
Module.setValue (eh_throw, 0, "i32");
var res = mono_unbox_int (mono_runtime_invoke (test_0_return, 0, 0, eh_throw));
var eh_res = Module.getValue (eh_throw, "i32");
print ("res is " + res + " eh is " + eh_res);




