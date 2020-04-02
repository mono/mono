
var MonoSupportLib = {
	$MONO__postset: 'MONO.export_functions (Module);',
	$MONO: {
		pump_count: 0,
		timeout_queue: [],
		_vt_stack: [],
		mono_wasm_runtime_is_ready : false,
		mono_wasm_ignore_pdb_load_errors: true,
		pump_message: function () {
			if (!this.mono_background_exec)
				this.mono_background_exec = Module.cwrap ("mono_background_exec", null);
			while (MONO.timeout_queue.length > 0) {
				--MONO.pump_count;
				MONO.timeout_queue.shift()();
			}
			while (MONO.pump_count > 0) {
				--MONO.pump_count;
				this.mono_background_exec ();
			}
		},

		export_functions: function (module) {
			module ["pump_message"] = MONO.pump_message;
			module ["mono_load_runtime_and_bcl"] = MONO.mono_load_runtime_and_bcl;
		},

		mono_text_decoder: undefined,
		string_decoder: {
			copy: function (mono_string) {
				if (mono_string == 0)
					return null;

				if (!this.mono_wasm_string_convert)
					this.mono_wasm_string_convert = Module.cwrap ("mono_wasm_string_convert", null, ['number']);

				this.mono_wasm_string_convert (mono_string);
				var result = this.result;
				this.result = undefined;
				return result;
			},
			decode: function (start, end, save) {
				if (!MONO.mono_text_decoder) {
					MONO.mono_text_decoder = typeof TextDecoder !== 'undefined' ? new TextDecoder('utf-16le') : undefined;
                }

				var str = "";
				if (MONO.mono_text_decoder) {
					// When threading is enabled, TextDecoder does not accept a view of a 
					// SharedArrayBuffer, we must make a copy of the array first.
					var subArray = Module.HEAPU8.buffer instanceof SharedArrayBuffer
						? Module.HEAPU8.slice(start, end)
						: Module.HEAPU8.subarray(start, end);

					str = MONO.mono_text_decoder.decode(subArray);
				} else {
					for (var i = 0; i < end - start; i+=2) {
						var char = Module.getValue (start + i, 'i16');
						str += String.fromCharCode (char);
					}
				}
				if (save)
					this.result = str;

				return str;
			},
		},

		mono_wasm_get_call_stack: function() {
			if (!this.mono_wasm_current_bp_id)
				this.mono_wasm_current_bp_id = Module.cwrap ("mono_wasm_current_bp_id", 'number');
			if (!this.mono_wasm_enum_frames)
				this.mono_wasm_enum_frames = Module.cwrap ("mono_wasm_enum_frames", null);

			var bp_id = this.mono_wasm_current_bp_id ();
			this.active_frames = [];
			this.mono_wasm_enum_frames ();

			var the_frames = this.active_frames;
			this.active_frames = [];
			return {
				"breakpoint_id": bp_id,
				"frames": the_frames,
			};
		},

		_fixup_name_value_objects: function (var_list) {
			var out_list = [];

			var i = 0;
			while (i < var_list.length) {
				var o = var_list [i];
				var name = o.name;
				if (name == null || name == undefined) {
					i ++;
					out_list.push (o);
					continue;
				}

				if (i + 1 < var_list.length) {
					var value = var_list [i+1].value;

					if (value != null && value != undefined) {
						var descr = value.description;
						if (descr == null || descr == undefined)
							value.description = '' + value.value;

						o.value = value;
					}
				}

				out_list.push (o);
				i += 2;
			}

			return out_list;
		},

		_filter_automatic_properties: function (props) {
			var names_found = {};
			var final_var_list = [];

			for (var i in props) {
				var p = props [i];
				if (p.name in names_found)
					continue;

				if (p.name.endsWith ("k__BackingField"))
					p.name = p.name.replace ("k__BackingField", "")
							.replace ('<', '')
							.replace ('>', '');

				names_found [p.name] = p.name;
				final_var_list.push (p);
			}

			return final_var_list;
		},

		mono_wasm_get_variables: function(scope, var_list) {
			if (!this.mono_wasm_get_var_info)
				this.mono_wasm_get_var_info = Module.cwrap ("mono_wasm_get_var_info", null, [ 'number', 'number', 'number']);

			this.var_info = [];
			var numBytes = var_list.length * Int32Array.BYTES_PER_ELEMENT;
			var ptr = Module._malloc(numBytes);
			var heapBytes = new Int32Array(Module.HEAP32.buffer, ptr, numBytes);
			for (let i=0; i<var_list.length; i++) {
				heapBytes[i] = var_list[i]
			}
			this.mono_wasm_get_var_info (scope, heapBytes.byteOffset, var_list.length);
			Module._free(heapBytes.byteOffset);
			var res = MONO._fixup_name_value_objects (this.var_info);

			//Async methods are special in the way that local variables can be lifted to generated class fields
			//value of "this" comes here either
			for (let i in res) {
				var name = res [i].name;
				if (name != undefined && name.indexOf ('>') > 0)
					res [i].name = name.substring (1, name.indexOf ('>'));
			}

			this.var_info = []

			return res;
		},

		mono_wasm_get_object_properties: function(objId, expandValueTypes) {
			if (!this.mono_wasm_get_object_properties_info)
				this.mono_wasm_get_object_properties_info = Module.cwrap ("mono_wasm_get_object_properties", null, [ 'number', 'bool' ]);

			this.var_info = [];
			this.mono_wasm_get_object_properties_info (objId, expandValueTypes);

			var res = MONO._filter_automatic_properties (MONO._fixup_name_value_objects (this.var_info));
			for (var i = 0; i < res.length; i++) {
				if (res [i].value.isValueType != undefined && res [i].value.isValueType)
					res [i].value.objectId = `dotnet:valuetype:${objId}:${res [i].fieldOffset}`;
			}

			this.var_info = [];

			return res;
		},

		mono_wasm_get_array_values: function(objId) {
			if (!this.mono_wasm_get_array_values_info)
				this.mono_wasm_get_array_values_info = Module.cwrap ("mono_wasm_get_array_values", null, [ 'number' ]);

			this.var_info = [];
			console.log (">> mono_wasm_get_array_values " + objId);
			this.mono_wasm_get_array_values_info (objId);

			var res = MONO._fixup_name_value_objects (this.var_info);
			this.var_info = [];

			return res;
		},

		mono_wasm_start_single_stepping: function (kind) {
			console.log (">> mono_wasm_start_single_stepping " + kind);
			if (!this.mono_wasm_setup_single_step)
				this.mono_wasm_setup_single_step = Module.cwrap ("mono_wasm_setup_single_step", 'number', [ 'number']);

			return this.mono_wasm_setup_single_step (kind);
		},

		mono_wasm_runtime_ready: function () {
			this.mono_wasm_runtime_is_ready = true;
			// DO NOT REMOVE - magic debugger init function
			console.debug ("mono_wasm_runtime_ready", "fe00e07a-5519-4dfe-b35a-f867dbaf2e28");
		},

		mono_wasm_set_breakpoint: function (assembly, method_token, il_offset) {
			if (!this.mono_wasm_set_bp)
				this.mono_wasm_set_bp = Module.cwrap ('mono_wasm_set_breakpoint', 'number', ['string', 'number', 'number']);

			return this.mono_wasm_set_bp (assembly, method_token, il_offset)
		},

		mono_wasm_remove_breakpoint: function (breakpoint_id) {
			if (!this.mono_wasm_del_bp)
				this.mono_wasm_del_bp = Module.cwrap ('mono_wasm_remove_breakpoint', 'number', ['number']);

			return this.mono_wasm_del_bp (breakpoint_id);
		},

		// Set environment variable NAME to VALUE
		// Should be called before mono_load_runtime_and_bcl () in most cases 
		mono_wasm_setenv: function (name, value) {
			if (!this.wasm_setenv)
				this.wasm_setenv = Module.cwrap ('mono_wasm_setenv', null, ['string', 'string']);
			this.wasm_setenv (name, value);
		},

		mono_wasm_set_runtime_options: function (options) {
			if (!this.wasm_parse_runtime_options)
				this.wasm_parse_runtime_options = Module.cwrap ('mono_wasm_parse_runtime_options', null, ['number', 'number']);
			var argv = Module._malloc (options.length * 4);
			var wasm_strdup = Module.cwrap ('mono_wasm_strdup', 'number', ['string']);
			aindex = 0;
			for (var i = 0; i < options.length; ++i) {
				Module.setValue (argv + (aindex * 4), wasm_strdup (options [i]), "i32");
				aindex += 1;
			}
			this.wasm_parse_runtime_options (options.length, argv);
		},

		//
		// Initialize the AOT profiler with OPTIONS.
		// Requires the AOT profiler to be linked into the app.
		// options = { write_at: "<METHODNAME>", send_to: "<METHODNAME>" }
		// <METHODNAME> should be in the format <CLASS>::<METHODNAME>.
		// write_at defaults to 'WebAssembly.Runtime::StopProfile'.
		// send_to defaults to 'WebAssembly.Runtime::DumpAotProfileData'.
		// DumpAotProfileData stores the data into Module.aot_profile_data.
		//
		mono_wasm_init_aot_profiler: function (options) {
			if (options == null)
				options = {}
			if (!('write_at' in options))
				options.write_at = 'WebAssembly.Runtime::StopProfile';
			if (!('send_to' in options))
				options.send_to = 'WebAssembly.Runtime::DumpAotProfileData';
			var arg = "aot:write-at-method=" + options.write_at + ",send-to-method=" + options.send_to;
			Module.ccall ('mono_wasm_load_profiler_aot', null, ['string'], [arg]);
		},

		// options = { write_at: "<METHODNAME>", send_to: "<METHODNAME>" }
		// <METHODNAME> should be in the format <CLASS>::<METHODNAME>.
		// write_at defaults to 'WebAssembly.Runtime::StopProfile'.
		// send_to defaults to 'WebAssembly.Runtime::DumpCoverageProfileData'.
		// DumpCoverageProfileData stores the data into Module.coverage_profile_data.
		mono_wasm_init_coverage_profiler: function (options) {
			if (options == null)
				options = {}
			if (!('write_at' in options))
				options.write_at = 'WebAssembly.Runtime::StopProfile';
			if (!('send_to' in options))
				options.send_to = 'WebAssembly.Runtime::DumpCoverageProfileData';
			var arg = "coverage:write-at-method=" + options.write_at + ",send-to-method=" + options.send_to;
			Module.ccall ('mono_wasm_load_profiler_coverage', null, ['string'], [arg]);
		},

		mono_load_runtime_and_bcl: function (vfs_prefix, deploy_prefix, enable_debugging, file_list, loaded_cb, fetch_file_cb) {
			var pending = file_list.length;
			var loaded_files = [];
			var mono_wasm_add_assembly = Module.cwrap ('mono_wasm_add_assembly', null, ['string', 'number', 'number']);

			if (!fetch_file_cb) {
				if (ENVIRONMENT_IS_NODE) {
					var fs = require('fs');
					fetch_file_cb = function (asset) {
						console.log("MONO_WASM: Loading... " + asset);
						var binary = fs.readFileSync (asset);
						var resolve_func2 = function(resolve, reject) {
							resolve(new Uint8Array (binary));
						};

						var resolve_func1 = function(resolve, reject) {
							var response = {
								ok: true,
								url: asset,
								arrayBuffer: function() {
									return new Promise(resolve_func2);
								}
							};
							resolve(response);
						};

						return new Promise(resolve_func1);
					};
				} else {
					fetch_file_cb = function (asset) {
						return fetch (asset, { credentials: 'same-origin' });
					}
				}
			}

			file_list.forEach (function(file_name) {
				
				var fetch_promise = fetch_file_cb (locateFile(deploy_prefix + "/" + file_name));

				fetch_promise.then (function (response) {
					if (!response.ok) {
						// If it's a 404 on a .pdb, we don't want to block the app from starting up.
						// We'll just skip that file and continue (though the 404 is logged in the console).
						if (response.status === 404 && file_name.match(/\.pdb$/) && MONO.mono_wasm_ignore_pdb_load_errors) {
							--pending;
							throw "MONO-WASM: Skipping failed load for .pdb file: '" + file_name + "'";
						}
						else {
							throw "MONO_WASM: Failed to load file: '" + file_name + "'";
						}
					}
					else {
						loaded_files.push (response.url);
						return response ['arrayBuffer'] ();
					}
				}).then (function (blob) {
					var asm = new Uint8Array (blob);
					var memory = Module._malloc(asm.length);
					var heapBytes = new Uint8Array(Module.HEAPU8.buffer, memory, asm.length);
					heapBytes.set (asm);
					mono_wasm_add_assembly (file_name, memory, asm.length);

					console.log ("MONO_WASM: Loaded: " + file_name);
					--pending;
					if (pending == 0) {
						MONO.loaded_files = loaded_files;
						var load_runtime = Module.cwrap ('mono_wasm_load_runtime', null, ['string', 'number']);

						console.log ("MONO_WASM: Initializing mono runtime");
						if (ENVIRONMENT_IS_SHELL || ENVIRONMENT_IS_NODE) {
							try {
								load_runtime (vfs_prefix, enable_debugging);
							} catch (ex) {
								print ("MONO_WASM: load_runtime () failed: " + ex);
								var err = new Error();
								print ("MONO_WASM: Stacktrace: \n");
								print (err.stack);

								var wasm_exit = Module.cwrap ('mono_wasm_exit', null, ['number']);
								wasm_exit (1);
							}
						} else {
							load_runtime (vfs_prefix, enable_debugging);
						}
						MONO.mono_wasm_runtime_ready ();
						loaded_cb ();
					}
				});
			});
		},

		mono_wasm_get_loaded_files: function() {
			console.log(">>>mono_wasm_get_loaded_files");
			return this.loaded_files;
		},
		
		mono_wasm_clear_all_breakpoints: function() {
			if (!this.mono_clear_bps)
				this.mono_clear_bps = Module.cwrap ('mono_wasm_clear_all_breakpoints', null);

			this.mono_clear_bps ();
		},
		
		mono_wasm_add_null_var: function(className)
		{
			fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
			MONO.var_info.push ({value: {
				type: "object",
				className: fixed_class_name,
				description: fixed_class_name,
				subtype: "null"
			}});
		},

		_mono_csharp_fixup_class_name: function(className)
		{
			// Fix up generic names like Foo`2<int, string> to Foo<int, string>
			// and nested class names like Foo/Bar to Foo.Bar
			return className.replace('/', '.').replace(/`\d+/, '');
		},
	},

	mono_wasm_add_bool_var: function(var_value) {
		MONO.var_info.push({
			value: {
				type: "boolean",
				value: var_value != 0,
			}
		});
	},

	mono_wasm_add_number_var: function(var_value) {
		MONO.var_info.push({
			value: {
				type: "number",
				value: var_value,
			}
		});
	},

	mono_wasm_add_properties_var: function(name, field_offset) {
		MONO.var_info.push({
			name: Module.UTF8ToString (name),
			fieldOffset: field_offset
		});
	},

	mono_wasm_begin_value_type_var: function(className) {
		fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
		var vt_obj = {
			value: {
				type: "object",
				className: fixed_class_name,
				description: fixed_class_name,
				// objectId will be generated by MonoProxy
				expanded: true,
				isValueType: true,
				members: []
			}
		};
		if (MONO._vt_stack.length == 0)
			MONO._old_var_info = MONO.var_info;

		MONO.var_info = vt_obj.value.members;
		MONO._vt_stack.push (vt_obj);
	},

	mono_wasm_end_value_type_var: function() {
		var top_vt_obj_popped = MONO._vt_stack.pop ();
		top_vt_obj_popped.value.members = MONO._filter_automatic_properties (
							MONO._fixup_name_value_objects (top_vt_obj_popped.value.members));

		if (MONO._vt_stack.length == 0) {
			MONO.var_info = MONO._old_var_info;
			MONO.var_info.push(top_vt_obj_popped);
		} else {
			var top_obj = MONO._vt_stack [MONO._vt_stack.length - 1];
			top_obj.value.members.push (top_vt_obj_popped);
			MONO.var_info = top_obj.value.members;
		}
	},

	mono_wasm_add_value_type_unexpanded_var: function (className) {
		fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
		MONO.var_info.push({
			value: {
				type: "object",
				className: fixed_class_name,
				description: fixed_class_name,
				// objectId added when enumerating object's properties
				expanded: false,
				isValueType: true
			}
		});
	},

	mono_wasm_add_enum_var: function(className, members, value) {
		// FIXME: flags
		//

		// group0: Monday:0
		// group1: Monday
		// group2: 0
		var re = new RegExp (`[,]?([^,:]+):(${value}(?=,)|${value}$)`, 'g')
		var members_str = Module.UTF8ToString (members);

		var match = re.exec(members_str);
		var member_name = match == null ? ('' + value) : match [1];

		fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
		MONO.var_info.push({
			value: {
				type: "object",
				className: fixed_class_name,
				description: member_name,
				isEnum: true
			}
		});
	},

	mono_wasm_add_array_item: function(position) {
		MONO.var_info.push({
			name: "[" + position + "]",
		});
	},

	mono_wasm_add_string_var: function(var_value) {
		if (var_value == 0) {
			MONO.mono_wasm_add_null_var ("string");
			return;
		} 

		MONO.var_info.push({
			value: {
				type: "string",
				value: Module.UTF8ToString (var_value),
			}
		});
	},

	mono_wasm_add_obj_var: function(className, objectId) {
		if (objectId == 0) {
			MONO.mono_wasm_add_null_var (className);
			return;
		}

		fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
		MONO.var_info.push({
			value: {
				type: "object",
				className: fixed_class_name,
				description: fixed_class_name,
				objectId: "dotnet:object:"+ objectId,
			}
		});
	},

	mono_wasm_add_array_var: function(className, objectId) {
		if (objectId == 0) {
			MONO.mono_wasm_add_null_var (className);
			return;
		}

		fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
		MONO.var_info.push({
			value: {
				type: "object",
				subtype: "array",
				className: fixed_class_name,
				description: fixed_class_name,
				objectId: "dotnet:array:"+ objectId,
			}
		});
	},

	mono_wasm_add_func_var: function(className, objectId) {
		if (objectId == 0) {
			MONO.mono_wasm_add_null_var (className);
			return;
		}

		fixed_class_name = MONO._mono_csharp_fixup_class_name(Module.UTF8ToString (className));
		MONO.var_info.push({
			value: {
				type: "function",
				description: fixed_class_name,
				objectId: "dotnet:object:"+ objectId,
			}
		});
	},

	mono_wasm_add_frame: function(il, method, name) {
		MONO.active_frames.push( {
			il_pos: il,
			method_token: method,
			assembly_name: Module.UTF8ToString (name)
		});
	},

	schedule_background_exec: function () {
		++MONO.pump_count;
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (MONO.pump_message, 0);
		} else if (ENVIRONMENT_IS_WORKER) {
			self.setTimeout (MONO.pump_message, 0);
		} else if (ENVIRONMENT_IS_NODE) {
			global.setTimeout (MONO.pump_message, 0);
		}
	},

	mono_set_timeout: function (timeout, id) {
		if (!this.mono_set_timeout_exec)
			this.mono_set_timeout_exec = Module.cwrap ("mono_set_timeout_exec", null, [ 'number' ]);
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (function () {
				this.mono_set_timeout_exec (id);
			}, timeout);
		} else if (ENVIRONMENT_IS_WORKER) {
			self.setTimeout (function () {
				this.mono_set_timeout_exec (id);
			}, timeout);
		} else if (ENVIRONMENT_IS_NODE) {
			global.setTimeout (function () {
				global.mono_set_timeout_exec (id);
			}, timeout);
		} else {
			++MONO.pump_count;
			MONO.timeout_queue.push(function() {
				this.mono_set_timeout_exec (id);
			})
		}
	},

	mono_wasm_fire_bp: function () {
		console.log ("mono_wasm_fire_bp");
		debugger;
	}
};

autoAddDeps(MonoSupportLib, '$MONO')
mergeInto(LibraryManager.library, MonoSupportLib)
