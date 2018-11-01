
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
				this.mono_wasm_get_var_info = Module.cwrap ("mono_wasm_get_var_info", 'void', [ 'number', 'number']);

			//FIXME it would be more efficient to do a single call passing an array with var_list as argument instead
			this.var_info = [];
			for (var i = 0; i <  var_list.length; ++i)
				this.mono_wasm_get_var_info (scope, var_list [i]);

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

	mono_wasm_add_int_var: function(var_value) {
		MONO.var_info.push({
			value: {
				type: "number",
				value: var_value,
			}
		});
	},

	mono_wasm_add_long_var: function(var_value) {
		MONO.var_info.push({
			value: {
				type: "number",
				value: var_value,
			}
		});
	},

	mono_wasm_add_float_var: function(var_value) {
		MONO.var_info.push({
			value: {
				type: "number",
				value: var_value,
			}
		});
	},

	mono_wasm_add_double_var: function(var_value) {
		MONO.var_info.push({
			value: {
				type: "number",
				value: var_value,
			}
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
		}
	},

	mono_set_timeout: function (timeout, id) {
		if (!this.mono_set_timeout_exec)
			this.mono_set_timeout_exec = Module.cwrap ("mono_set_timeout_exec", 'void', [ 'number' ]);
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (function () {
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

