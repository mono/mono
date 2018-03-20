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

var assemblies = [ "mscorlib.dll", "System.dll", "System.Core.dll", "main.exe", "nunitlite.dll", "mini_tests.dll", "wasm_corlib_test.dll", "wasm_System_test.dll", "wasm_System.Core_test.dll" ];

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

function call_method (method, this_arg, args) {
	var args_mem = Module._malloc (args.length * 4);
	var eh_throw = Module._malloc (4);
	for (var i = 0; i < args.length; ++i)
		Module.setValue (args_mem + i * 4, args [i], "i32");
	Module.setValue (eh_throw, 0, "i32");

	var res = invoke_method (method, this_arg, args_mem, eh_throw);

	var eh_res = Module.getValue (eh_throw, "i32");

	Module._free (args_mem);
	Module._free (eh_throw);

	if (eh_res != 0) {
		var msg = conv_string (res);
		throw new Error (msg); //the convention is that invoke_method ToString () any outgoing exception
	}

	return res;
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

function mono_send_msg (key, val) {
	try {
		return conv_string (call_method (send_message, null, [mono_string (key), mono_string (val)]));
	} catch (e) {
		print ("BAD SEND MSG: " + e);
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
	exit (1)
