
var MonoSupportLib = {
	$MONO__postset: 'Module["pump_message"] = MONO.pump_message',
	$MONO: {
		pump_count: 0,
		timeout_queue: [],
		pump_message: function () {
			while (MONO.timeout_queue.length > 0) {
				--MONO.pump_count;
				MONO.timeout_queue.shift()();
			}
			while (MONO.pump_count > 0) {
				--MONO.pump_count;
				Module.ccall ("mono_background_exec");
			}
		}
	},

	schedule_background_exec: function () {
		++MONO.pump_count;
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (MONO.pump_message, 0);
		}
	},


	mono_set_timeout: function (timeout, id) {
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (function () {
				Module.ccall ("mono_set_timeout_exec", 'void', [ 'number' ], [ id ]);
			}, timeout);
		} else {
			++MONO.pump_count;
			MONO.timeout_queue.push(function() {
				Module.ccall ("mono_set_timeout_exec", 'void', [ 'number' ], [ id ]);
			})
		}
		
	}
};

autoAddDeps(MonoSupportLib, '$MONO')
mergeInto(LibraryManager.library, MonoSupportLib)

