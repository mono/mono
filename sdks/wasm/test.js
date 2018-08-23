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
//end of all the nice shell glue code.


function inspect_object (o){
    var r="";
    for(var p in o) {
        var t = typeof o[p];
        r += "'" + p + "' => '" + t + "', ";
    }
    return r;
}


var Module = { 
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

var assemblies = [ "mscorlib.dll", "System.dll", "System.Core.dll", "Mono.Security.dll", "main.exe", "nunitlite.dll", "mini_tests.dll", "wasm_corlib_test.dll", "wasm_System_test.dll", "wasm_System.Core_test.dll", "binding_tests.dll" ];

load ("mono.js");
Module.finish_loading ();


Module.FS_createPath ("/", "managed", true, true);

//Load all assembly in @assemblies into the FS at /mananaged
assemblies.forEach (function(asm_name) {
	print ("LOADING " + asm_name)
	var asm = new Uint8Array (read ("managed/" + asm_name, 'binary'));
	Module.FS_createDataFile ("managed/" + asm_name, null, asm, true, true, true);	
});


var load_runtime = Module.cwrap ('mono_wasm_load_runtime', null, ['string', 'number'])
var assembly_load = Module.cwrap ('mono_wasm_assembly_load', 'number', ['string'])
var find_class = Module.cwrap ('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string'])
var find_method = Module.cwrap ('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number'])
const IGNORE_PARAM_COUNT = -1;

//test driver support code
var bad_send_msg_detected = false;
function mono_send_msg (key, val) {
	try {
		return Module.mono_method_invoke (send_message, null, "ss", [key, val]);
	} catch (e) {
		print ("BAD SEND MSG: " + e);
		bad_send_msg_detected = true;
		return null;
	}
}

load_runtime ("managed", 0);
var main_module = assembly_load ("main")
if (!main_module)
	throw 1;

var driver_class = find_class (main_module, "", "Driver")
if (!driver_class)
	throw 2;

var send_message = find_method (driver_class, "Send", IGNORE_PARAM_COUNT)
if (!send_message)
	throw 3;

//Ok, this is temporary
//this is a super big hack (move it to a decently named assembly, at the very least)
var binding_test_module = assembly_load ("binding_tests");
Module.mono_bindings_init("[binding_tests]WebAssembly.Runtime");

//binding test suite support code
var binding_test_class = find_class (binding_test_module, "", "TestClass");
if (!binding_test_class)
	throw 9;

function call_test_method(method_name, signature, args)
{
	var target_method = find_method (binding_test_class, method_name, IGNORE_PARAM_COUNT)
	if (!target_method)
		throw "Could not find " + method_name;

	return Module.mono_method_invoke (target_method, null, signature, args);
}

print ("-----LOADED ----");

for (var i = 0; i < arguments.length; ++i) {
	var res = mono_send_msg ("start-test", arguments [i])
	print ("-----STARTED " + arguments [i] + "---- " + res);

	if (res == "SUCCESS") {
		while (mono_send_msg ("pump-test", arguments [i]) != "DONE") {
			Module.pump_message ();
			print ("|");
		}
		print ("\nDONE")
	}
}

var status = mono_send_msg ("test-result", "");
print ("Test status " + status)
if (status != "PASS")
	fail_exec ("BAD TEST STATUS");

if (bad_send_msg_detected)
	fail_exec ("BAD MSG SEND DETECTED");
