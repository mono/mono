function inspect_object (o){
    var r="";
    for(var p in o) {
        var t = typeof o[p];
        r += "'" + p + "' => '" + t + "', ";
    }
    return r;
}


var App = {
	init: function () {
		console.log ("ready to run!");
		var int_add = Module.mono_bind_static_method ("[debugger-test] Math:IntAdd");
		var res = int_add (20, 30);
		console.log("add is " +  res);
	}
};

var Module = require ("./mono.js");
var config = require ("./config.js")(Module);
require ("./runtime.js") (config, Module, App);
console.log ("done loading");
