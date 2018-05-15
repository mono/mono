
var MonoSupportLib = {
	$MONO__postset: 'Module["pump_message"] = MONO.pump_message',
	$MONO: {
		pump_count: 0,
		timeout_queue: [],
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

