
// provide a way for classes/interfaces to specify abstract methods
Function.prototype.abstractMethod = function () {
	throw "Abstract method should be implemented";
}

// get a reference to the global object so we can register namespaces off it.
var __global = this;

var types = new Array();
var Type = {
	registerNamespace: function (name) {
		var segments = name.split ('.');

		var parent = __global;
		for (i = 0; i < segments.length; i ++) {
			var new_parent = parent[segments[i]];
			if (new_parent == null) {
				new_parent = new Object();
				parent[segments[i]] = new_parent;
			}
			parent = new_parent;
		}
	},

	registerClass: function (name, parent, interfaces) {
		var new_type = eval (name); /* XXX ew... */

		new_type.initializeBase = function (o, a) {
			parent.apply (o, a);
		}
		new_type.callBaseMethod = function (o, m, a) {
			var fun = parent.prototype[m];
			return fun.apply (o, a);
		}

		types[name] = new_type;
	},

	registerAbstractClass: function (name, parent, interfaces) {
		/* not really sure what to do about this one... */
		this.registerClass (name, parent, interfaces);
	},

	registerInterface: function (name) {
		/* not really sure what to do about this one... */
		var new_type = eval (name); /* XXX ew... */

		types[name] = new_type;
	}
}

// not sure where this should go, but the Demo script uses it

Type.registerNamespace ("Web");

Web.IDisposable = function () {
	this.dispose = Function.abstractMethod;
}
Type.registerInterface ("Web.IDisposable");



function $ (a) {
	return document.getElementById(a);
}

