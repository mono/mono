
var BindingSupportLib = {
	$BINDING__postset: 'Module["call_mono_method"] = BINDING.call_method.bind(BINDING)',
	$BINDING: {
		BINDING_ASM: "binding_tests",
		js_objects_table: [],		
		bindings_lazy_init: function () {
			if (this.init)
				return;
		
			this.assembly_load = Module.cwrap ('mono_wasm_assembly_load', 'number', ['string']);
			this.find_class = Module.cwrap ('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string']);
			this.find_method = Module.cwrap ('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number']);
			this.invoke_method = Module.cwrap ('mono_wasm_invoke_method', 'number', ['number', 'number', 'number']);
			this.mono_string_get_utf8 = Module.cwrap ('mono_wasm_string_get_utf8', 'number', ['number']);
			this.js_string_to_mono_string = Module.cwrap ('mono_wasm_string_from_js', 'number', ['string']);
			this.mono_get_obj_type = Module.cwrap ('mono_wasm_get_obj_type', 'number', ['number']);
			this.mono_unbox_int = Module.cwrap ('mono_unbox_int', 'number', ['number']);
			this.mono_unbox_float = Module.cwrap ('mono_wasm_unbox_float', 'number', ['number']);
			this.mono_array_length = Module.cwrap ('mono_wasm_array_length', 'number', ['number']);
			this.mono_array_get = Module.cwrap ('mono_wasm_array_get', 'number', ['number', 'number']);
			this.mono_obj_array_new = Module.cwrap ('mono_wasm_obj_array_new', 'number', ['number']);
			this.mono_obj_array_set = Module.cwrap ('mono_wasm_obj_array_set', 'void', ['number', 'number', 'number']);

			this.binding_module = this.assembly_load (this.BINDING_ASM);
			var wasm_runtime_class = this.find_class (this.binding_module, "WebAssembly", "Runtime")
			if (!wasm_runtime_class)
				throw "Can't find WebAssembly.Runtime class";

			var get_method = function(method_name) {
				var res = this.find_method (wasm_runtime_class, method_name, -1)
				if (!res)
					throw "Can't find method WebAssembly.Runtime:" + method_name;
				return res;
			}
			this.bind_js_obj = get_method ("BindJSObject");
			this.bind_existing_obj = get_method ("BindExistingObject");
			this.get_js_id = get_method ("GetJSObjectId");
			this.get_raw_mono_obj = get_method ("GetMonoObject");

			this.box_js_int = get_method ("BoxInt");
			this.box_js_double = get_method ("BoxDouble");
			this.box_js_bool = get_method ("BoxBool");
			this.setup_js_cont = get_method ("SetupJSContinuation");

			this.create_tcs = get_method ("CreateTaskSource");
			this.set_tcs_result = get_method ("SetTaskSourceResult");
			this.set_tcs_failure = get_method ("SetTaskSourceFailure");
			this.tcs_get_task_and_bind = get_method ("GetTaskAndBind");

			this.init = true;
		},		

		get_js_obj: function (js_handle) {
			if (js_handle > 0)
				return this.js_objects_table [js_handle - 1];
			return null;
		},
		
		//FIXME this is wastefull, we could remove the temp malloc by going the UTF16 route
		//FIXME this is unsafe, cuz raw objects could be GC'd.
		conv_string: function (mono_obj) {
			if (mono_obj == 0)
				return null;
			var raw = this.mono_string_get_utf8 (mono_obj);
			var res = Module.UTF8ToString (raw);
			Module._free (raw);

			return res;
		},
		
		mono_array_to_js_array: function (mono_array) {
			if (mono_array == 0)
				return null;

			var res = [];
			var len = this.mono_array_length (mono_array);
			for (var i = 0; i < len; ++i)
				res.push (this.unbox_mono_obj (this.mono_array_get (mono_array, i)));

			return res;
		},

		js_array_to_mono_array: function (js_array) {
			var mono_array = this.mono_obj_array_new (js_array.length);
			for (var i = 0; i < js_array.length; ++i) {
				this.mono_obj_array_set (mono_array, i, this.js_to_mono_obj (js_array [i]));
			}
			return mono_array;
		},

		unbox_mono_obj: function (mono_obj) {
			if (mono_obj == 0)
				return undefined;
			var type = this.mono_get_obj_type (mono_obj);
			//See MARSHAL_TYPE_ defines in driver.c
			switch (type) {
			case 1: // int
				return this.mono_unbox_int (mono_obj);
			case 2: // float
				return this.mono_unbox_float (mono_obj);
			case 3: //string
				return this.conv_string (mono_obj);
			case 4: //vts
				throw new Error ("no idea on how to unbox value types");
			case 5: { // delegate
				var obj = this.extract_js_obj (mono_obj);
				return function () {
					return BINDING.invoke_delegate (obj, arguments);
				};
			}
			case 6: {// Task
				var obj = this.extract_js_obj (mono_obj);
				var cont_obj = null;
				var promise = new Promise (function (resolve, reject) {
					cont_obj = {
						resolve: resolve,
						reject: reject
					};
				});

				this.call_method (this.setup_js_cont, null, "mo", [ mono_obj, cont_obj ]);
				return promise;
			}

			case 7: // ref type
				return this.extract_js_obj (mono_obj);
			default:
				throw new Error ("no idea on how to unbox object kind " + type);
			}
		},

		create_task_completion_source: function () {
			return this.call_method (this.create_tcs, null, "", []);
		},

		set_task_result: function (tcs, result) {
			this.call_method (this.set_tcs_result, null, "oo", [ tcs, result ]);
		},

		set_task_failure: function (tcs, reason) {
			this.call_method (this.set_tcs_failure, null, "os", [ tcs, reason.toString () ]);
		},

		js_to_mono_obj: function (js_obj) {
	  		this.bindings_lazy_init ();

			if (js_obj == null || js_obj == undefined)
				return 0;
			if (typeof js_obj == 'number') {
				if (parseInt(js_obj) == js_obj)
					return this.call_method (this.box_js_int, null, "im", [ js_obj ]);
				return this.call_method (this.box_js_double, null, "dm", [ js_obj ]);
			}
			if (typeof js_obj == 'string')
				return this.js_string_to_mono_string (js_obj);

			if (typeof js_obj == 'boolean')
				return this.call_method (this.box_js_bool, null, "im", [ js_obj ]);

			if (Promise.resolve(js_obj) === js_obj) {
				var the_task = this.try_extract_mono_obj (js_obj);
				if (the_task)
					return the_task;
				var tcs = this.create_task_completion_source ();
				//FIXME dispose the TCS once the promise completes
				js_obj.then (function (result) {
					BINDING.set_task_result (tcs, result);
				}, function (reason) {
					BINDING.set_task_failure (tcs, reason);
				})

				return this.get_task_and_bind (tcs, js_obj);
			}

			return this.extract_mono_obj (js_obj);
		},

		wasm_binding_obj_new: function (js_obj_id)
		{
			return this.call_method (this.bind_js_obj, null, "i", [js_obj_id]);
		},

		wasm_bind_existing: function (mono_obj, js_id)
		{
			return this.call_method (this.bind_existing_obj, null, "mi", [mono_obj, js_id]);
		},

		wasm_get_js_id: function (mono_obj)
		{
			return this.call_method (this.get_js_id, null, "m", [mono_obj]);
		},

		wasm_get_raw_obj: function (gchandle)
		{
			return this.call_method (this.get_raw_mono_obj, null, "im", [gchandle]);
		},

		try_extract_mono_obj:function (js_obj) {
			if (js_obj == null || js_obj == undefined || !js_obj.__mono_gchandle__)
				return 0;
			return this.wasm_get_raw_obj (js_obj.__mono_gchandle__);
		},

		get_task_and_bind: function (tcs, js_obj) {
			var task_gchandle = this.call_method (this.tcs_get_task_and_bind, null, "oi", [ tcs, this.js_objects_table.length + 1 ]);
			js_obj.__mono_gchandle__ = task_gchandle;
			this.js_objects_table.push (js_obj);
			return this.wasm_get_raw_obj (js_obj.__mono_gchandle__);
		},

		extract_mono_obj: function (js_obj) {
			//halp JS ppl, is this enough?
			if (js_obj == null || js_obj == undefined)
				return 0;

			if (!js_obj.__mono_gchandle__) {
				js_obj.__mono_gchandle__ = this.wasm_binding_obj_new(this.js_objects_table.length + 1);
				this.js_objects_table.push(js_obj);
			}

			return this.wasm_get_raw_obj (js_obj.__mono_gchandle__);
		},

		extract_js_obj: function (mono_obj) {
			if (mono_obj == 0)
				return null;

			var js_id = this.wasm_get_js_id (mono_obj);
			if (js_id > 0)
				return this.js_objects_table [js_id - 1];

			var js_obj = {
				__mono_gchandle__: this.wasm_bind_existing(mono_obj, this.js_objects_table.length + 1),
				is_mono_bridged_obj: true
			};

			this.js_objects_table.push(js_obj);

			return js_obj;
		},

		/*
		args_marshal is a string with one character per parameter that tells how to marshal it, here are the valid values:

		i: int32
		l: int64
		f: float
		d: double
		s: string
		o: js object will be converted to a C# object (this will box numbers/bool/promises)
		m: raw mono object. Don't use it unless you know what you're doing

		additionally you can append 'm' to args_marshal beyond `args.length` if you don't want the return value marshaled
		*/
		call_method: function (method, this_arg, args_marshal, args) {
			this.bindings_lazy_init ();

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
					Module.setValue (args_mem + i * 4, this.js_string_to_mono_string (args [i]), "i32");
				} else if (args_marshal[i] == 'm') {
					Module.setValue (args_mem + i * 4, args [i], "i32");
				} else if (args_marshal[i] == 'o') {
					Module.setValue (args_mem + i * 4, this.js_to_mono_obj (args [i]), "i32");
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

			var res = this.invoke_method (method, this_arg, args_mem, eh_throw);

			var eh_res = Module.getValue (eh_throw, "i32");

			if (extra_args_mem)
				Module._free (extra_args_mem);
			Module._free (args_mem);
			Module._free (eh_throw);

			if (eh_res != 0) {
				var msg = this.conv_string (res);
				throw new Error (msg); //the convention is that invoke_method ToString () any outgoing exception
			}

			if (args_marshal.length >= args.length && args_marshal [args.length] == 'm')
				return res;
			return this.unbox_mono_obj (res);
		},

		invoke_delegate: function (delegate_obj, js_args) {
			this.bindings_lazy_init ();

			if (!this.delegate_dynamic_invoke) {
				if (!this.corlib)
					this.corlib = this.assembly_load ("mscorlib");
				if (!this.delegate_class)
					this.delegate_class = this.find_class (this.corlib, "System", "Delegate");
				this.delegate_dynamic_invoke = this.find_method (this.delegate_class, "DynamicInvoke", -1);
			}
			var mono_args = this.js_array_to_mono_array (js_args);
			return this.call_method (this.delegate_dynamic_invoke, this.extract_mono_obj (delegate_obj), "m", [ mono_args ]);
		},
	},

	mono_wasm_invoke_js_with_args: function(js_handle, method_name, args, is_exception) {
		BINDING.bindings_lazy_init ();

		var obj = BINDING.get_js_obj (js_handle);
		if (!obj) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Invalid JS object handle '" + js_handle + "'");
		}

		var js_name = BINDING.conv_string (method_name);
		if (!js_name) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Invalid method name object '" + method_name + "'");
		}

		var js_args = BINDING.mono_array_to_js_array(args);

		var res;
		try {
			var m = obj [js_name];
			var res = m.apply (obj, js_args);
			return BINDING.js_to_mono_obj (res);
		} catch (e) {
			var res = e.toString ();
			setValue (is_exception, 1, "i32");
			if (res === null || res === undefined)
				res = "unknown exception";
			return BINDING.js_string_to_mono_string (res);
		}
	},
};

autoAddDeps(BindingSupportLib, '$BINDING')
mergeInto(LibraryManager.library, BindingSupportLib)

