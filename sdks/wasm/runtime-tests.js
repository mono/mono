
//glue code to deal with the differences between ch, d8, jsc and sm.
if (typeof print === "undefined")
	print = console.log;

// JavaScript core does not have a console defined
if (typeof console === "undefined") {
	var Console = function () {
		this.log = function(msg){ print(msg) };
	};
	console = new Console();
}

if (typeof console !== "undefined") {
	var has_console_warn = false;
	try {
		if (typeof console.warn !== "undefined")
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
	if (typeof scriptArgs !== "undefined")
		arguments = scriptArgs;
} catch(e) {}
//end of all the nice shell glue code.

// set up a global variable to be accessed in the App.init
var testArguments = arguments;

function inspect_object (o){
    var r="";
    for(var p in o) {
        var t = typeof o[p];
        r += "'" + p + "' => '" + t + "', ";
    }
    return r;
}

load ("config.js");

var Module = { 
	print: function(x) { print ("WASM: " + x) },
	printErr: function(x) { print ("WASM-ERR: " + x) },

	onRuntimeInitialized: function () {
		MONO.mono_load_runtime_and_bcl (
			config.vfs_prefix,
			config.deploy_prefix,
			config.enable_debugging,
			config.file_list,
			function () {
				config.add_bindings ();
				App.init ();
			},
			function (asset ) 
			{
				// The default mono_load_runtime_and_bcl defaults to using
				// fetch to load the assets.  It also provides a way to set a 
				// fetch promise callback.
				// Here we wrap the file read in a promise and fake a fetch response
				// structure.
				return new Promise((resolve, reject) => {
					 var response = { ok: true, url: asset, 
							arrayBuffer: function() {
								return new Promise((resolve2, reject2) => {
									resolve2(new Uint8Array (read (asset, 'binary')));
							}
						)}
					}
				   resolve(response)
				 })
			}
		);
	},
};

load ("mono.js");

var assembly_load = Module.cwrap ('mono_wasm_assembly_load', 'number', ['string'])
var find_class = Module.cwrap ('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string'])
var find_method = Module.cwrap ('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number'])
const IGNORE_PARAM_COUNT = -1;

var App = {
    init: function () {

		Module.print("Initializing.....");
		Module.print("Arguments: " + testArguments);

		if (testArguments[0] == "--regression") {
			var exec_regresion = Module.cwrap ('mono_wasm_exec_regression', 'number', ['number', 'string'])

			var res = 0;
				try {
					res = exec_regresion (10, testArguments[1]);
					Module.print ("REGRESSION RESULT: " + res);
				} catch (e) {
					Module.print ("ABORT: " + e);
					res = 1;
				}

			if (res)
				fail_exec ("REGRESSION TEST FAILED");

			return;
		}
		
		Module.print("Initializing Binding Test Suite support.....");

		//binding test suite support code
		binding_test_module = assembly_load ("binding_tests");
		if (!binding_test_module)
		{
			Module.printErr("Binding tests module 'binding_tests' not found.  Exiting Tests.")
			throw new Error("Binding tests module 'binding_tests' not found.  Exiting Tests.");
		}
		
		binding_test_class = find_class (binding_test_module, "", "TestClass");
		if (!binding_test_class)
		{
			Module.printErr("Binding tests class 'TestClass' not found.  Exiting Tests.")
			throw new Error("Binding tests class 'TestClass' not found.  Exiting Tests.");
		}		

		Module.print("Binding support complete.");

		
		Module.print("Checking for [main]Driver:Send ....");
		
		var send_message = undefined;
		
		try
		{
			send_message = BINDING.bind_static_method("[main]Driver:Send");
		}
		catch (e)
		{
			Module.printErr("[main]Driver:Send not found: " + e);
			throw e;
		
		}

		Module.print("Driver binding complete.");

		var bad_send_msg_detected = false;
		for (var i = 0; i < testArguments.length; ++i) {

			var res = "";
			try
			{
				res = send_message("start-test", testArguments [i])
			} catch (e) {
				printErr ("BAD SEND MSG: " + e);
				bad_send_msg_detected = true;
			}
			print ("-----STARTED " + testArguments [i] + "---- " + res);

			if (res == "SUCCESS") {
				while (send_message ("pump-test", testArguments [i]) != "DONE") 
				{
					Module.pump_message ();
					print ("|");
				}
				print ("\nDONE")
			}
		}

		var status = send_message ("test-result", "");
		print ("Test status " + status)
		if (status != "PASS")
			fail_exec ("BAD TEST STATUS");

		if (bad_send_msg_detected)
			fail_exec ("BAD MSG SEND DETECTED");
    },
};

//binding test suite support code
var binding_test_module = undefined;
var binding_test_class = undefined;

// This function is called from the binding test suite
function call_test_method(method_name, signature, args)
{
	var target_method = find_method (binding_test_class, method_name, IGNORE_PARAM_COUNT)
	if (!target_method)
		throw "Could not find " + method_name;

	return Module.mono_method_invoke (target_method, null, signature, args);
}
