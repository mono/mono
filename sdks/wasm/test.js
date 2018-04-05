//glue code to deal with the differences between ch, d8, jsc and sm.
if (print == undefined)
	print = console.log;

if (console.warn === undefined)
	console.warn = console.log;

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

var assemblies = [ "mscorlib.dll", "System.dll", "System.Core.dll", "Mono.Security.dll", "main.exe", "nunitlite.dll", "mini_tests.dll", "wasm_corlib_test.dll", "wasm_System_test.dll", "wasm_System.Core_test.dll" ];

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
var invoke_method = Module.cwrap ('mono_wasm_invoke_method', 'number', ['number', 'number', 'number'])
var mono_string_get_utf8 = Module.cwrap ('mono_wasm_string_get_utf8', 'number', ['number'])
var mono_string = Module.cwrap ('mono_wasm_string_from_js', 'number', ['string'])

var mono_get_obj_type = Module.cwrap ('mono_wasm_get_obj_type', 'number', ['number'])
var mono_unbox_int = Module.cwrap ('mono_unbox_int', 'number', ['number'])
var mono_unbox_float = Module.cwrap ('mono_wasm_unbox_float', 'number', ['number'])

function call_method (method, this_arg, args_marshal, args) {
	var extra_args_mem = 0;
	for (var i = 0; i < args.length; ++i) {
		//long/double memory must be 8 bytes aligned and I'm being lazy here
		if (args_marshal[i] == 'i' || args_marshal[i] == 'f' || args_marshal[i] == 'l' || args_marshal[i] == 'd')
			extra_args_mem += 8;
	}

	var extra_args_mem = extra_args_mem ? Module._malloc (extra_args_mem) : 0;
	var extra_arg_idx = 0;
	var args_mem = Module._malloc (args.length * 4);
	var eh_throw = Module._malloc (4);
	for (var i = 0; i < args.length; ++i) {
		if (args_marshal[i] == 's') {
			Module.setValue (args_mem + i * 4, mono_string (args [i]), "i32");
		} else if (args_marshal[i] == 'i' || args_marshal[i] == 'f' || args_marshal[i] == 'l' || args_marshal[i] == 'd') {
			var extra_cell = extra_args_mem + extra_arg_idx;
			extra_arg_idx += 8;

			if (args_marshal[i] == 'i')
				Module.setValue (extra_cell, args [i], "i32");
			else if (args_marshal[i] == 'l')
				Module.setValue (extra_cell, args [i], "i64");
			else if (args_marshal[i] == 'f')
				Module.setValue (extra_cell, args [i], "float");
			else
				Module.setValue (extra_cell, args [i], "double");

			Module.setValue (args_mem + i * 4, extra_cell, "i32");
		}
	}
	Module.setValue (eh_throw, 0, "i32");

	var res = invoke_method (method, this_arg, args_mem, eh_throw);

	var eh_res = Module.getValue (eh_throw, "i32");

	if (extra_args_mem)
		Module._free (extra_args_mem);
	Module._free (args_mem);
	Module._free (eh_throw);

	if (eh_res != 0) {
		var msg = conv_string (res);
		throw new Error (msg); //the convention is that invoke_method ToString () any outgoing exception
	}

	return unbox_mono_obj (res);
}

//FIXME this is wastefull, we could remove the temp malloc by going the UTF16 route
//FIXME this is unsafe, cuz raw objects could be GC'd.
function conv_string (mono_obj) {
	if (mono_obj == 0)
		return null;
	var raw = mono_string_get_utf8 (mono_obj);
	var res = Module.UTF8ToString (raw);
	Module._free (raw);

	return res;
}

function unbox_mono_obj(mono_obj) {
	if (mono_obj == 0)
		return undefined;
	var type = mono_get_obj_type (mono_obj);
	switch (type) {
	case 1: // int
		return mono_unbox_int (mono_obj);
	case 2: // float
		return mono_unbox_float (mono_obj);
	case 3: //string
		return conv_string (mono_obj);
	case 4: // ref type
		throw new Error ("no idea on how to unbox objects yet");
	case 5: //vts
		throw new Error ("no idea on how to unbox value types yet");
	default:
		throw new Error ("no idea on how to unbox object kind " + type);
	}
}

var bad_semd_msg_detected = false;
function mono_send_msg (key, val) {
	try {
		return call_method (send_message, null, "ss", [key, val]);
	} catch (e) {
		print ("BAD SEND MSG: " + e);
		bad_semd_msg_detected = true;
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

var send_message = find_method (driver_class, "Send", -1)
if (!send_message)
	throw 3;

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

if (bad_semd_msg_detected)
	fail_exec ("BAD MSG SEND DETECTED");
