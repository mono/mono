
var MonoSupportLib = {
	$MONO__postset: 'Module["pump_message"] = MONO.pump_message',
	$MONO: {
		pump_count: 0,
		pump_message: function () {
			while (MONO.pump_count > 0) {
				--MONO.pump_count;
				Module.ccall ("mono_gc_pump_callback");
			}
		}
	},

	request_gc_cycle: function () {
		++MONO.pump_count;
		if (ENVIRONMENT_IS_WEB) {
			window.setTimeout (MONO.pump_message, 0);
		}
	},
};

autoAddDeps(MonoSupportLib, '$MONO')
mergeInto(LibraryManager.library, MonoSupportLib)

