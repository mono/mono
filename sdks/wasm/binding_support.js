
var BindingSupportLib = {
	$BINDING__postset: 'BINDING.export_functions (Module);',
	$BINDING: {
		BINDING_ASM: "[binding_tests]WebAssembly.Runtime",
		mono_wasm_object_registry: [],
		mono_wasm_ref_counter: 0,
		mono_wasm_free_list: [],
		mono_wasm_marshal_enum_as_int: false,	
		mono_bindings_init: function (binding_asm) {
			this.BINDING_ASM = binding_asm;
		},

		export_functions: function (module) {
			module ["mono_bindings_init"] = BINDING.mono_bindings_init.bind(BINDING);
			module ["mono_method_invoke"] = BINDING.call_method.bind(BINDING);
			module ["mono_method_get_call_signature"] = BINDING.mono_method_get_call_signature.bind(BINDING);
			module ["mono_method_resolve"] = BINDING.resolve_method_fqn.bind(BINDING);
			module ["mono_bind_static_method"] = BINDING.bind_static_method.bind(BINDING);
			module ["mono_call_static_method"] = BINDING.call_static_method.bind(BINDING);
		},

		bindings_lazy_init: function () {
			if (this.init)
				return;
		
			this.assembly_load = Module.cwrap ('mono_wasm_assembly_load', 'number', ['string']);
			this.find_class = Module.cwrap ('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string']);
			this.find_method = Module.cwrap ('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number']);
			this.invoke_method = Module.cwrap ('mono_wasm_invoke_method', 'number', ['number', 'number', 'number', 'number']);
			this.mono_string_get_utf8 = Module.cwrap ('mono_wasm_string_get_utf8', 'number', ['number']);
			this.js_string_to_mono_string = Module.cwrap ('mono_wasm_string_from_js', 'number', ['string']);
			this.mono_get_obj_type = Module.cwrap ('mono_wasm_get_obj_type', 'number', ['number']);
			this.mono_unbox_int = Module.cwrap ('mono_unbox_int', 'number', ['number']);
			this.mono_unbox_float = Module.cwrap ('mono_wasm_unbox_float', 'number', ['number']);
			this.mono_array_length = Module.cwrap ('mono_wasm_array_length', 'number', ['number']);
			this.mono_array_get = Module.cwrap ('mono_wasm_array_get', 'number', ['number', 'number']);
			this.mono_obj_array_new = Module.cwrap ('mono_wasm_obj_array_new', 'number', ['number']);
			this.mono_obj_array_set = Module.cwrap ('mono_wasm_obj_array_set', 'void', ['number', 'number', 'number']);
			this.mono_unbox_enum = Module.cwrap ('mono_wasm_unbox_enum', 'number', ['number']);

			// receives a byteoffset into allocated Heap with a size.
			this.mono_typed_array_new = Module.cwrap ('mono_wasm_typed_array_new', 'number', ['number','number','number','number']);
			this.mono_array_to_heap = Module.cwrap ('mono_wasm_array_to_heap', 'void', ['number','number']);

			var binding_fqn_asm = this.BINDING_ASM.substring(this.BINDING_ASM.indexOf ("[") + 1, this.BINDING_ASM.indexOf ("]")).trim();
			var binding_fqn_class = this.BINDING_ASM.substring (this.BINDING_ASM.indexOf ("]") + 1).trim();
			
			this.binding_module = this.assembly_load (binding_fqn_asm);
			if (!this.binding_module)
				throw "Can't find bindings module assembly: " + binding_fqn_asm;

			if (binding_fqn_class !== null && typeof binding_fqn_class !== "undefined")
			{
				var namespace = "WebAssembly";
				var classname = binding_fqn_class.length > 0 ? binding_fqn_class : "Runtime";
				if (binding_fqn_class.indexOf(".") != -1) {
					var idx = binding_fqn_class.lastIndexOf(".");
					namespace = binding_fqn_class.substring (0, idx);
					classname = binding_fqn_class.substring (idx + 1);
				}
			}

			var wasm_runtime_class = this.find_class (this.binding_module, namespace, classname)
			if (!wasm_runtime_class)
				throw "Can't find " + binding_fqn_class + " class";

			var get_method = function(method_name) {
				var res = BINDING.find_method (wasm_runtime_class, method_name, -1)
				if (!res)
					throw "Can't find method " + namespace + "." + classname + ":" + method_name;
				return res;
			}
			this.bind_js_obj = get_method ("BindJSObject");
			this.bind_existing_obj = get_method ("BindExistingObject");
			this.unbind_js_obj = get_method ("UnBindJSObject");
			this.unbind_js_obj_and_free = get_method ("UnBindJSObjectAndFree");			
			this.unbind_raw_obj_and_free = get_method ("UnBindRawJSObjectAndFree");			
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
			this.get_call_sig = get_method ("GetCallSignature");

			this.object_to_string = get_method ("ObjectToString");

			this.object_to_enum = get_method ("ObjectToEnum");

			this.init = true;
		},		

		get_js_obj: function (js_handle) {
			if (js_handle > 0)
				return this.mono_wasm_require_handle(js_handle);
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

				if (typeof Promise === "undefined" || typeof Promise.resolve === "undefined")
					throw new Error ("Promises are not supported thus C# Tasks can not work in this context.");

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

			case 8: // bool
				return this.mono_unbox_int (mono_obj) != 0;

			case 9: // enum

				if(this.mono_wasm_marshal_enum_as_int)
				{
					return this.mono_unbox_enum (mono_obj);
				}
				else
				{
					enumValue = this.call_method(this.object_to_string, null, "m", [ mono_obj ]);
				}

				return enumValue;


			case 11: 
			case 12: 
			case 13: 
			case 14: 
			case 15: 
			case 16: 
			case 17: 
			case 18:
			{
				var res =  this.mono_array_to_js_typedarray(type, mono_obj); 
				return res;
			}			
	
			default:
				throw new Error ("no idea on how to unbox object kind " + type);
			}
		},

		create_task_completion_source: function () {
			return this.call_method (this.create_tcs, null, "i", [ -1 ]);
		},

		set_task_result: function (tcs, result) {
			tcs.is_mono_tcs_result_set = true;
			this.call_method (this.set_tcs_result, null, "oo", [ tcs, result ]);
			if (tcs.is_mono_tcs_task_bound)
				this.free_task_completion_source(tcs);
		},

		set_task_failure: function (tcs, reason) {
			tcs.is_mono_tcs_result_set = true;
			this.call_method (this.set_tcs_failure, null, "os", [ tcs, reason.toString () ]);
			if (tcs.is_mono_tcs_task_bound)
				this.free_task_completion_source(tcs);
		},

		// https://github.com/Planeshifter/emscripten-examples/blob/master/01_PassingArrays/sum_post.js
		js_typedarray_to_heap: function(typedArray){
			var numBytes = typedArray.length * typedArray.BYTES_PER_ELEMENT;
			var ptr = Module._malloc(numBytes);
			var heapBytes = new Uint8Array(Module.HEAPU8.buffer, ptr, numBytes);
			heapBytes.set(new Uint8Array(typedArray.buffer));
			return heapBytes;
		},
		mono_array_to_js_typedarray: function(type, mono_array){

			// length of our array
			var szLength = this.mono_array_length(mono_array);
				
			// The element size that will need to be allocated
			var bytes_per_element = 0;

			switch (type)
			{
				case 11: 
					bytes_per_element = Int8Array.BYTES_PER_ELEMENT; 
					break;
				case 12: 
					bytes_per_element = Uint8Array.BYTES_PER_ELEMENT; 
					break;
				case 13: 
					bytes_per_element = Int16Array.BYTES_PER_ELEMENT; 
					break;
				case 14: 
					bytes_per_element = Uint16Array.BYTES_PER_ELEMENT; 
					break;
				case 15: 
					bytes_per_element = Int32Array.BYTES_PER_ELEMENT; 
					break;
				case 16: 
					bytes_per_element = Uint32Array.BYTES_PER_ELEMENT; 
					break;
				case 17: 
					bytes_per_element = Float32Array.BYTES_PER_ELEMENT; 
					break;
				case 18:
					bytes_per_element = Float64Array.BYTES_PER_ELEMENT;
					break;
			}
			
			// Allocate bytes needed for the array of bytes
			var bufferSize = szLength * bytes_per_element;
			var bufferPtr = Module._malloc(bufferSize);

			// blit the mono array to the heap
			this.mono_array_to_heap(mono_array, bufferPtr);

			// result to be returned
			var res = null;

			// We now need to create a new typed array based off the heap view
			switch (type)
			{
				case 11: 
					res = Module.HEAP8.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 12: 
					res = Module.HEAPU8.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 13: 
					res = Module.HEAP16.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 14: 
					res = Module.HEAPU16.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 15: 
					res = Module.HEAP32.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 16: 
					res = Module.HEAPU32.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 17: 
					res = Module.HEAPF32.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
				case 18:
					res = Module.HEAPF64.slice(bufferPtr / bytes_per_element, bufferPtr / bytes_per_element + szLength);
					break;
			}

			// free the allocated memory
			Module._free(bufferPtr);
			// return new typed array
			return res;
			
		},
		js_to_mono_obj: function (js_obj) {
	  		this.bindings_lazy_init ();

			if (js_obj == null || js_obj == undefined)
				return 0;
			if (typeof js_obj === 'number') {
				if (parseInt(js_obj) == js_obj)
					return this.call_method (this.box_js_int, null, "im", [ js_obj ]);
				return this.call_method (this.box_js_double, null, "dm", [ js_obj ]);
			}
			if (typeof js_obj === 'string')
				return this.js_string_to_mono_string (js_obj);

			if (typeof js_obj === 'boolean')
				return this.call_method (this.box_js_bool, null, "im", [ js_obj ]);

			if (Promise.resolve(js_obj) === js_obj) {
				var the_task = this.try_extract_mono_obj (js_obj);
				if (the_task)
					return the_task;
				var tcs = this.create_task_completion_source ();

				js_obj.then (function (result) {
					BINDING.set_task_result (tcs, result);
				}, function (reason) {
					BINDING.set_task_failure (tcs, reason);
				})

				return this.get_task_and_bind (tcs, js_obj);
			}


			// JavaScript typed arrays are array-like objects and provide a mechanism for accessing 
			// raw binary data. (...) To achieve maximum flexibility and efficiency, JavaScript typed arrays 
			// split the implementation into buffers and views. A buffer (implemented by the ArrayBuffer object)
			//  is an object representing a chunk of data; it has no format to speak of, and offers no 
			// mechanism for accessing its contents. In order to access the memory contained in a buffer, 
			// you need to use a view. A view provides a context — that is, a data type, starting offset, 
			// and number of elements — that turns the data into an actual typed array.
			// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays
			if (!!(js_obj.buffer instanceof ArrayBuffer && js_obj.BYTES_PER_ELEMENT)) 
			{
				var arrayType = 0;	
				if (js_obj instanceof Int8Array)
					arrayType = 11;
				if (js_obj instanceof Uint8Array)
					arrayType = 12;
				if (js_obj instanceof Uint8ClampedArray)
					arrayType = 12;
				if (js_obj instanceof Int16Array)
					arrayType = 13;
				if (js_obj instanceof Uint16Array)
					arrayType = 14;
				if (js_obj instanceof Int32Array)
					arrayType = 15;
				if (js_obj instanceof Uint32Array)
					arrayType = 16;
				if (js_obj instanceof Float32Array)
					arrayType = 17;
				if (js_obj instanceof Float64Array)
					arrayType = 18;

				var heapBytes = this.js_typedarray_to_heap(js_obj);
				var bufferArray = this.mono_typed_array_new(heapBytes.byteOffset, js_obj.length, js_obj.BYTES_PER_ELEMENT, arrayType);
				Module._free(heapBytes.byteOffset);
				return bufferArray;
			}
			// The ArrayBuffer object is used to represent a generic, fixed-length raw binary data buffer. 
			// You cannot directly manipulate the contents of an ArrayBuffer; instead, you create one of the 
			// typed array objects or a DataView object which represents the buffer in a specific format, and 
			// use that to read and write the contents of the buffer.
			// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays#ArrayBuffer
			if (ArrayBuffer.isView(js_obj) || js_obj instanceof ArrayBuffer)
			{
				var byteView = new Uint8Array(js_obj);
				var heapBytes = this.js_typedarray_to_heap(byteView);
				byteView = null;
				var bufferArray = this.mono_typed_array_new(heapBytes.byteOffset, heapBytes.length, heapBytes.BYTES_PER_ELEMENT, 2);
				Module._free(heapBytes.byteOffset);
				return bufferArray;
			}

			return this.extract_mono_obj (js_obj);
		},
		js_to_mono_enum: function (method, parmIdx, js_obj) {
			this.bindings_lazy_init ();
    
			if (js_obj === null || typeof js_obj === "undefined")
				return 0;

			var monoObj = this.js_to_mono_obj(js_obj);
			// Check enum contract
			var monoEnum = this.call_method(this.object_to_enum, null, "iimm", [ method, parmIdx, monoObj ])
			// return the unboxed enum value.
			return this.mono_unbox_enum(monoEnum);
		},
		wasm_binding_obj_new: function (js_obj_id)
		{
			return this.call_method (this.bind_js_obj, null, "i", [js_obj_id]);
		},

		wasm_bind_existing: function (mono_obj, js_id)
		{
			return this.call_method (this.bind_existing_obj, null, "mi", [mono_obj, js_id]);
		},

		wasm_unbind_js_obj: function (js_obj_id)
		{
			return this.call_method (this.unbind_js_obj, null, "i", [js_obj_id]);
		},		

		wasm_unbind_js_obj_and_free: function (js_obj_id)
		{
			return this.call_method (this.unbind_js_obj_and_free, null, "i", [js_obj_id]);
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
			if (js_obj === null || typeof js_obj === "undefined" || typeof js_obj.__mono_gchandle__ === "undefined")
				return 0;
			return this.wasm_get_raw_obj (js_obj.__mono_gchandle__);
		},

		mono_method_get_call_signature: function(method) {
			this.bindings_lazy_init ();

			return this.call_method (this.get_call_sig, null, "i", [ method ]);
		},

		get_task_and_bind: function (tcs, js_obj) {
			var gc_handle = this.mono_wasm_free_list.length ? this.mono_wasm_free_list.pop() : this.mono_wasm_ref_counter++;
			var task_gchandle = this.call_method (this.tcs_get_task_and_bind, null, "oi", [ tcs, gc_handle + 1 ]);
			js_obj.__mono_gchandle__ = task_gchandle;
			this.mono_wasm_object_registry[gc_handle] = js_obj;
			this.free_task_completion_source(tcs);
			tcs.is_mono_tcs_task_bound = true;
			js_obj.__mono_bound_tcs__ = tcs.__mono_gchandle__;
			tcs.__mono_bound_task__ = js_obj.__mono_gchandle__;
			return this.wasm_get_raw_obj (js_obj.__mono_gchandle__);
		},

		free_task_completion_source: function (tcs) {
			if (tcs.is_mono_tcs_result_set)
			{
				this.call_method (this.unbind_raw_obj_and_free, null, "ii", [ tcs.__mono_gchandle__ ]);
			}
			if (tcs.__mono_bound_task__)
			{
				this.call_method (this.unbind_raw_obj_and_free, null, "ii", [ tcs.__mono_bound_task__ ]);
			}
		},

		extract_mono_obj: function (js_obj) {

			if (js_obj === null || typeof js_obj === "undefined")
				return 0;

			if (!js_obj.is_mono_bridged_obj) {
				var gc_handle = this.mono_wasm_register_obj(js_obj);
				return this.wasm_get_raw_obj (gc_handle);
			}


			return this.wasm_get_raw_obj (js_obj.__mono_gchandle__);
		},

		extract_js_obj: function (mono_obj) {
			if (mono_obj == 0)
				return null;

			var js_id = this.wasm_get_js_id (mono_obj);
			if (js_id > 0)
				return this.mono_wasm_require_handle(js_id);

			var gcHandle = this.mono_wasm_free_list.length ? this.mono_wasm_free_list.pop() : this.mono_wasm_ref_counter++;
			var js_obj = {
				__mono_gchandle__: this.wasm_bind_existing(mono_obj, gcHandle + 1),
				is_mono_bridged_obj: true
			};

			this.mono_wasm_object_registry[gcHandle] = js_obj;
			return js_obj;
		},

		/*
		args_marshal is a string with one character per parameter that tells how to marshal it, here are the valid values:

		i: int32
		j: int32 - Enum with underlying type of int32
		l: int64 
		k: int64 - Enum with underlying type of int64
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
				if (args_marshal[i] == 'i' || args_marshal[i] == 'f' || args_marshal[i] == 'l' || args_marshal[i] == 'd' || args_marshal[i] == 'j' || args_marshal[i] == 'k')
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
				} else if (args_marshal[i] == 'j'  || args_marshal[i] == 'k') {
					var enumVal = this.js_to_mono_enum(method, i, args[i]);
		
					var extra_cell = extra_args_mem + extra_arg_idx;
					extra_arg_idx += 8;

					if (args_marshal[i] == 'j')
						Module.setValue (extra_cell, enumVal, "i32");
					else if (args_marshal[i] == 'k')
						Module.setValue (extra_cell, enumVal, "i64");

					Module.setValue (args_mem + i * 4, extra_cell, "i32");
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
		
		resolve_method_fqn: function (fqn) {
			var assembly = fqn.substring(fqn.indexOf ("[") + 1, fqn.indexOf ("]")).trim();
			fqn = fqn.substring (fqn.indexOf ("]") + 1).trim();

			var methodname = fqn.substring(fqn.indexOf (":") + 1);
			fqn = fqn.substring (0, fqn.indexOf (":")).trim ();

			var namespace = "";
			var classname = fqn;
			if (fqn.indexOf(".") != -1) {
				var idx = fqn.lastIndexOf(".");
				namespace = fqn.substring (0, idx);
				classname = fqn.substring (idx + 1);
			}

			var asm = this.assembly_load (assembly);
			if (!asm)
				throw new Error ("Could not find assembly: " + assembly);

			var klass = this.find_class(asm, namespace, classname);
			if (!klass)
				throw new Error ("Could not find class: " + namespace + ":" +classname);

			var method = this.find_method (klass, methodname, -1);
			if (!method)
				throw new Error ("Could not find method: " + methodname);
			return method;
		},

		call_static_method: function (fqn, args, signature) {
			this.bindings_lazy_init ();

			var method = this.resolve_method_fqn (fqn);

			if (typeof signature === "undefined")
				signature = Module.mono_method_get_call_signature (method);

			return this.call_method (method, null, signature, args);
		},

		bind_static_method: function (fqn, signature) {
			this.bindings_lazy_init ();

			var method = this.resolve_method_fqn (fqn);

			if (typeof signature === "undefined")
				signature = Module.mono_method_get_call_signature (method);

			return function() {
				return BINDING.call_method (method, null, signature, arguments);
			};
		},
		// Object wrapping helper functions to handle reference handles that will
		// be used in managed code.
		mono_wasm_register_obj: function(obj) {

			var gc_handle = undefined;
			if (obj !== null && typeof obj !== "undefined") 
			{
				gc_handle = obj.__mono_gchandle__;

				if (typeof gc_handle === "undefined") {
					var handle = this.mono_wasm_free_list.length ?
								this.mono_wasm_free_list.pop() : this.mono_wasm_ref_counter++;
					obj.__mono_jshandle__ = handle;
					gc_handle = obj.__mono_gchandle__ = this.wasm_binding_obj_new(handle + 1);
					this.mono_wasm_object_registry[handle] = obj;
						
				}
			}
			return gc_handle;
		},
		mono_wasm_require_handle: function(handle) {
			if (handle > 0)
				return this.mono_wasm_object_registry[handle - 1];
			return null;
		},
		mono_wasm_unregister_obj: function(js_id) {
			var obj = this.mono_wasm_object_registry[js_id - 1];
			if (typeof obj  !== "undefined" && obj !== null) {
				var gc_handle = obj.__mono_gchandle__;
				if (typeof gc_handle  !== "undefined") {
					this.wasm_unbind_js_obj_and_free(js_id);
					delete obj.__mono_gchandle__;
					delete obj.__mono_jshandle__;
					this.mono_wasm_object_registry[js_id - 1] = undefined;
					this.mono_wasm_free_list.push(js_id - 1);
				}
			}
			return obj;
		},
		mono_wasm_free_handle: function(handle) {
			this.mono_wasm_unregister_obj(handle);
		},
		mono_wasm_get_global: function() {
			function testGlobal(obj) {
				obj['___mono_wasm_global___'] = obj;
				var success = typeof ___mono_wasm_global___ === 'object' && obj['___mono_wasm_global___'] === obj;
				if (!success) {
					delete obj['___mono_wasm_global___'];
				}
				return success;
			}
			if (typeof ___mono_wasm_global___ === 'object') {
				return ___mono_wasm_global___;
			}
			if (typeof global === 'object' && testGlobal(global)) {
				___mono_wasm_global___ = global;
			} else if (typeof window === 'object' && testGlobal(window)) {
				___mono_wasm_global___ = window;
			}
			if (typeof ___mono_wasm_global___ === 'object') {
				return ___mono_wasm_global___;
			}
			throw Error('unable to get mono wasm global object.');
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
	mono_wasm_get_object_property: function(js_handle, property_name, is_exception) {
		BINDING.bindings_lazy_init ();

		var obj = BINDING.mono_wasm_require_handle (js_handle);
		if (!obj) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Invalid JS object handle '" + js_handle + "'");
		}

		var js_name = BINDING.conv_string (property_name);
		if (!js_name) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Invalid property name object '" + js_name + "'");
		}

		var res;
		try {
			var m = obj [js_name];
			if (m === Object(m) && obj.__is_mono_proxied__)
				m.__is_mono_proxied__ = true;
				
			return BINDING.js_to_mono_obj (m);
		} catch (e) {
			var res = e.toString ();
			setValue (is_exception, 1, "i32");
			if (res === null || typeof res === "undefined")
				res = "unknown exception";
			return BINDING.js_string_to_mono_string (res);
		}
	},
    mono_wasm_set_object_property: function (js_handle, property_name, value, createIfNotExist, hasOwnProperty, is_exception) {

		BINDING.bindings_lazy_init ();

		var requireObject = BINDING.mono_wasm_require_handle (js_handle);
		if (!requireObject) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Invalid JS object handle '" + js_handle + "'");
		}

		var property = BINDING.conv_string (property_name);
		if (!property) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Invalid property name object '" + property_name + "'");
		}

        var result = false;

		var js_value = BINDING.unbox_mono_obj(value);

        if (createIfNotExist) {
            requireObject[property] = js_value;
            result = true;
        }
        else {
			result = false;
			if (!createIfNotExist)
			{
				if (!requireObject.hasOwnProperty(property))
					return false;
			}
            if (hasOwnProperty === true) {
                if (requireObject.hasOwnProperty(property)) {
                    requireObject[property] = js_value;
                    result = true;
                }
            }
            else {
                requireObject[property] = js_value;
                result = true;
            }
        
        }
        return BINDING.call_method (BINDING.box_js_bool, null, "im", [ result ]);
	},
	mono_wasm_get_global_object: function(global_name, is_exception) {
		BINDING.bindings_lazy_init ();

		var js_name = BINDING.conv_string (global_name);

		var globalObj = undefined;

		if (!js_name) {
			globalObj = BINDING.mono_wasm_get_global();
		}
		else {
			globalObj = BINDING.mono_wasm_get_global()[js_name];
		}

		if (globalObj === null || typeof globalObj === undefined) {
			setValue (is_exception, 1, "i32");
			return BINDING.js_string_to_mono_string ("Global object '" + js_name + "' not found.");
		}

		return BINDING.js_to_mono_obj (globalObj);
	},

};

autoAddDeps(BindingSupportLib, '$BINDING')
mergeInto(LibraryManager.library, BindingSupportLib)