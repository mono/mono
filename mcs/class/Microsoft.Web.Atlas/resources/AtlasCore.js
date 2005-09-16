/* Portions of the code in this file is from: Prototype JavaScript
   framework, version 1.3.1, and is (c) 2005 Sam Stephenson
   <sam@conio.net> */

// stuff from prototype.js

var Prototype = {
  Version: '1.3.1',
  emptyFunction: function() {}
}

var Class = {
  create: function() {
    return function() { 
      this.initialize.apply(this, arguments);
    }
  }
}

Object.extend = function(destination, source) {
  for (property in source) {
    destination[property] = source[property];
  }
  return destination;
}

Object.prototype.extend = function(object) {
  return Object.extend.apply(this, [this, object]);
}

Function.prototype.bind = function(object) {
  var __method = this;
  return function() {
    __method.apply(object, arguments);
  }
}

var Try = {
  these: function() {
    var returnValue;

    for (var i = 0; i < arguments.length; i++) {
      var lambda = arguments[i];
      try {
        returnValue = lambda();
        break;
      } catch (e) {}
    }

    return returnValue;
  }
}

var Ajax = {
  getTransport: function() {
    return Try.these(
      function() {return new ActiveXObject('Msxml2.XMLHTTP')},
      function() {return new ActiveXObject('Microsoft.XMLHTTP')},
      function() {return new XMLHttpRequest()}
    ) || false;
  }
}

Ajax.Base = function() {};
Ajax.Base.prototype = {
  setOptions: function(options) {
    this.options = {
      method:       'post',
      asynchronous: true,
      parameters:   ''
    }.extend(options || {});
  },

  responseIsSuccess: function() {
    return this.transport.status == undefined
        || this.transport.status == 0 
        || (this.transport.status >= 200 && this.transport.status < 300);
  },

  responseIsFailure: function() {
    return !this.responseIsSuccess();
  }
}

Ajax.Request = Class.create();
Ajax.Request.Events = 
  ['Uninitialized', 'Loading', 'Loaded', 'Interactive', 'Complete'];

Ajax.Request.prototype = (new Ajax.Base()).extend({
  initialize: function(url, options) {
    this.transport = Ajax.getTransport();
    this.setOptions(options);
    this.request(url);
  },

  request: function(url) {
    var parameters = this.options.parameters || '';
    if (parameters.length > 0) parameters += '&_=';

    try {
      if (this.options.method == 'get')
        url += '?' + parameters;

      this.transport.open(this.options.method, url,
        this.options.asynchronous);

      if (this.options.asynchronous) {
        this.transport.onreadystatechange = this.onStateChange.bind(this);
        setTimeout((function() {this.respondToReadyState(1)}).bind(this), 10);
      }

      this.setRequestHeaders();

      var body = this.options.postBody ? this.options.postBody : parameters;
      this.transport.send(this.options.method == 'post' ? body : null);

    } catch (e) {
    }
  },

  setRequestHeaders: function() {
    var requestHeaders = 
      ['X-Requested-With', 'XMLHttpRequest',
       'X-Prototype-Version', Prototype.Version];

    if (this.options.method == 'post') {
      requestHeaders.push('Content-type', 
        'application/x-www-form-urlencoded');

      /* Force "Connection: close" for Mozilla browsers to work around
       * a bug where XMLHttpReqeuest sends an incorrect Content-length
       * header. See Mozilla Bugzilla #246651. 
       */
      if (this.transport.overrideMimeType)
        requestHeaders.push('Connection', 'close');
    }

    if (this.options.requestHeaders)
      requestHeaders.push.apply(requestHeaders, this.options.requestHeaders);

    for (var i = 0; i < requestHeaders.length; i += 2)
      this.transport.setRequestHeader(requestHeaders[i], requestHeaders[i+1]);
  },

  onStateChange: function() {
    var readyState = this.transport.readyState;
    if (readyState != 1)
      this.respondToReadyState(this.transport.readyState);
  },

  respondToReadyState: function(readyState) {
    var event = Ajax.Request.Events[readyState];

    if (event == 'Complete')
      (this.options['on' + this.transport.status]
       || this.options['on' + (this.responseIsSuccess() ? 'Success' : 'Failure')]
       || Prototype.emptyFunction)(this.transport);

    (this.options['on' + event] || Prototype.emptyFunction)(this.transport);

    /* Avoid memory leak in MSIE: clean up the oncomplete event handler */
    if (event == 'Complete')
      this.transport.onreadystatechange = Prototype.emptyFunction;
  }
});

// end stuff from prototype.js



function $ (a) {
	return document.getElementById(a);
}

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

// Web namespace
Type.registerNamespace ("Web");

Web.IDisposable = function () {
	this.dispose = Function.abstractMethod;
}
Type.registerInterface ("Web.IDisposable");


// Web.Net namespace
Type.registerNamespace ("Web.Net");

Web.Net.ServiceMethodRequest = {
	callMethod: function (path, service, args, completeHandler, timeoutHandler) {
		this.completeHandler = completeHandler;
		this.timeoutHandler = timeoutHandler;

		var params = '';
		for (var v in args) {
			if (v != "extend") {
				if (params != '')
					params += "&";
				params += v + "=" + args[v];
			}
		}

		var options = {
			parameters: params,
			onComplete: function (transport) { completeHandler (transport.responseText); }
		};

		var req = new Ajax.Request(path + "/" + service, options);
	}
}