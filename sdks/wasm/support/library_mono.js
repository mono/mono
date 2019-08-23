
var MonoSupportLib = {
	$MONO__postset: 'Module["pump_message"] = MONO.pump_message',
	$MONO: {
		pump_count: 0,
		timeout_queue: [],
		mono_wasm_runtime_is_ready : false,
		pump_message: function () {
			if (!this.mono_background_exec)
				this.mono_background_exec = Module.cwrap ("mono_background_exec", 'void', [ ]);
			while (MONO.timeout_queue.length > 0) {
				--MONO.pump_count;
				MONO.timeout_queue.shift()();
			}
			while (MONO.pump_count > 0) {
				--MONO.pump_count;
				this.mono_background_exec ();
			}
		},

		mono_wasm_get_call_stack: function() {
			if (!this.mono_wasm_current_bp_id)
				this.mono_wasm_current_bp_id = Module.cwrap ("mono_wasm_current_bp_id", 'number', [ ]);
			if (!this.mono_wasm_enum_frames)
				this.mono_wasm_enum_frames = Module.cwrap ("mono_wasm_enum_frames", 'void', [ ]);

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

		mono_wasm_get_variables: function(scope, var_list) {
			if (!this.mono_wasm_get_var_info)
				this.mono_wasm_get_var_info = Module.cwrap ("mono_wasm_get_var_info", 'void', [ 'number', 'number', 'number']);

			this.var_info = [];
			var numBytes = var_list.length * Int32Array.BYTES_PER_ELEMENT;
			var ptr = Module._malloc(numBytes);
			var heapBytes = new Int32Array(Module.HEAP32.buffer, ptr, numBytes);
			for (let i=0; i<var_list.length; i++) {
				heapBytes[i] = var_list[i]
			}
			this.mono_wasm_get_var_info (scope, heapBytes.byteOffset, var_list.length);
			Module._free(heapBytes.byteOffset);
			var res = this.var_info;
			this.var_info = []

			return res;
		},

		mono_wasm_get_object_properties: function(objId) {
			if (!this.mono_wasm_get_object_properties_info)
				this.mono_wasm_get_object_properties_info = Module.cwrap ("mono_wasm_get_object_properties", 'void', [ 'number' ]);

			this.var_info = [];
			console.log (">> mono_wasm_get_object_properties " + objId);
			this.mono_wasm_get_object_properties_info (objId);

			var res = this.var_info;
			this.var_info = []

			return res;
		},

		mono_wasm_get_array_values: function(objId) {
			if (!this.mono_wasm_get_array_values_info)
				this.mono_wasm_get_array_values_info = Module.cwrap ("mono_wasm_get_array_values", 'void', [ 'number' ]);

			this.var_info = [];
			console.log (">> mono_wasm_get_array_values " + objId);
			this.mono_wasm_get_array_values_info (objId);

			var res = this.var_info;
			this.var_info = []

			return res;
		},

		mono_wasm_start_single_stepping: function (kind) {
			console.log (">> mono_wasm_start_single_stepping " + kind);
			if (!this.mono_wasm_setup_single_step)
				this.mono_wasm_setup_single_step = Module.cwrap ("mono_wasm_setup_single_step", 'void', [ 'number']);

			this.mono_wasm_setup_single_step (kind);
		},

		mono_wasm_runtime_ready: function () {
			console.log (">>mono_wasm_runtime_ready");
			this.mono_wasm_runtime_is_ready = true;
			debugger;
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
				this.wasm_setenv = Module.cwrap ('mono_wasm_setenv', 'void', ['string', 'string']);
			this.wasm_setenv (name, value);
		},

		mono_wasm_set_runtime_options: function (options) {
			if (!this.wasm_parse_runtime_options)
				this.wasm_parse_runtime_options = Module.cwrap ('mono_wasm_parse_runtime_options', 'void', ['number', 'number']);
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
			Module.ccall ('mono_wasm_load_profiler_aot', 'void', ['string'], [arg]);
		},

		mono_load_runtime_and_bcl: function (vfs_prefix, deploy_prefix, enable_debugging, file_list, loaded_cb, fetch_file_cb) {
			var pending = file_list.length;
			var loaded_files = [];
			var mono_wasm_add_assembly = Module.cwrap ('mono_wasm_add_assembly', null, ['string', 'number', 'number']);

			if (!fetch_file_cb) {
				if (ENVIRONMENT_IS_NODE) {
					var fs = require('fs');
					fetch_file_cb = function (asset) {
						console.log("Loading... " + asset);
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
					if (!response.ok)
						throw "failed to load '" + file_name + "'";
					loaded_files.push (response.url);
					return response ['arrayBuffer'] ();
				}).then (function (blob) {
					var asm = new Uint8Array (blob);
					var memory = Module._malloc(asm.length);
					var heapBytes = new Uint8Array(Module.HEAPU8.buffer, memory, asm.length);
					heapBytes.set (asm);
					mono_wasm_add_assembly (file_name, memory, asm.length);

					console.log ("Loaded: " + file_name);
					--pending;
					if (pending == 0) {
						MONO.loaded_files = loaded_files;
						var load_runtime = Module.cwrap ('mono_wasm_load_runtime', null, ['string', 'number']);

						console.log ("initializing mono runtime");
						if (ENVIRONMENT_IS_SHELL) {
							try {
								load_runtime (vfs_prefix, enable_debugging);
							} catch (ex) {
								print ("load_runtime () failed: " + ex);
								var err = new Error();
								print ("Stacktrace: \n");
								print (err.stack);

								var wasm_exit = Module.cwrap ('mono_wasm_exit', 'void', ['number']);
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
			if (this.mono_clear_bps)
				this.mono_clear_bps = Module.cwrap ('mono_wasm_clear_all_breakpoints', 'void', [ ]);
			this.mono_clear_bps ();
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

	mono_wasm_add_properties_var: function(name) {
		MONO.var_info.push({
			name: Module.UTF8ToString (name),
		});
	},

	mono_wasm_add_array_item: function(position) {
		MONO.var_info.push({
			name: "[" + position + "]",
		});
	},

	mono_wasm_add_string_var: function(var_value) {
		if (var_value == 0) {
			MONO.var_info.push({
				value: {
					type: "object",
					subtype: "null"
				}
			});
		} else {
			MONO.var_info.push({
				value: {
					type: "string",
					value: Module.UTF8ToString (var_value),
				}
			});
		}
	},

	mono_wasm_add_obj_var: function(className, objectId) {
		if (objectId == 0) {
			MONO.var_info.push({
				value: {
					type: "object",
					className: Module.UTF8ToString (className),
					description: Module.UTF8ToString (className),
					subtype: "null"
				}
			});
		} else {
			MONO.var_info.push({
				value: {
					type: "object",
					className: Module.UTF8ToString (className),
					description: Module.UTF8ToString (className),
					objectId: "dotnet:object:"+ objectId,
				}
			});
		}
	},

	mono_wasm_add_array_var: function(className, objectId) {
		if (objectId == 0) {
			MONO.var_info.push({
				value: {
					type: "array",
					className: Module.UTF8ToString (className),
					description: Module.UTF8ToString (className),
					subtype: "null"
				}
			});
		} else {
			MONO.var_info.push({
				value: {
					type: "array",
					className: Module.UTF8ToString (className),
					description: Module.UTF8ToString (className),
					objectId: "dotnet:array:"+ objectId,
				}
			});
		}
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
		}
	},

	mono_set_timeout: function (timeout, id) {
		if (!this.mono_set_timeout_exec)
			this.mono_set_timeout_exec = Module.cwrap ("mono_set_timeout_exec", 'void', [ 'number' ]);
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (function () {
				this.mono_set_timeout_exec (id);
			}, timeout);
		} else if (ENVIRONMENT_IS_WORKER) {
			self.setTimeout (function () {
				this.mono_set_timeout_exec (id);
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

