//-----------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------
// MicrosoftAjax.js
// Microsoft AJAX Framework.


Function.__typeName = 'Function';
Function.__class = true;

Function.createCallback = function Function$createCallback(method, context) {
    /// <param name="method" type="Function"></param>
    /// <param name="context" mayBeNull="true"></param>
    /// <returns type="Function"></returns>
    var e = Function._validateParams(arguments, [
        {name: "method", type: Function},
        {name: "context", mayBeNull: true}
    ]);
    if (e) throw e;


        
    return function() {
        var l = arguments.length;
        if (l > 0) {
                        var args = [];
            for (var i = 0; i < l; i++) {
                args[i] = arguments[i];
            }
            args[l] = context;
            return method.apply(this, args);
        }
        return method.call(this, context);
    }
}

Function.createDelegate = function Function$createDelegate(instance, method) {
    /// <param name="instance" mayBeNull="true"></param>
    /// <param name="method" type="Function"></param>
    /// <returns type="Function"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", mayBeNull: true},
        {name: "method", type: Function}
    ]);
    if (e) throw e;


        
    return function() {
        return method.apply(instance, arguments);
    }
}

Function.emptyFunction = Function.emptyMethod = function Function$emptyMethod() {
    if (arguments.length !== 0) throw Error.parameterCount();
}

Function._validateParams = function Function$_validateParams(params, expectedParams) {
                                                                                                                                    
    var e;

    e = Function._validateParameterCount(params, expectedParams);
    if (e) {
        e.popStackFrame();
        return e;
    }

    for (var i=0; i < params.length; i++) {
                                var expectedParam = expectedParams[Math.min(i, expectedParams.length - 1)];

        var paramName = expectedParam.name;
        if (expectedParam.parameterArray) {
                        paramName += "[" + (i - expectedParams.length + 1) + "]";
        }

        e = Function._validateParameter(params[i], expectedParam, paramName);
        if (e) {
            e.popStackFrame();
            return e;
        }
    }


    return null;
}

Function._validateParameterCount = function Function$_validateParameterCount(params, expectedParams) {
    var maxParams = expectedParams.length;
    var minParams = 0;
    for (var i=0; i < expectedParams.length; i++) {
        if (expectedParams[i].parameterArray) {
            maxParams = Number.MAX_VALUE;
        }
        else if (!expectedParams[i].optional) {
            minParams++;
        }
    }

    if (params.length < minParams || params.length > maxParams) {
        var e = Error.parameterCount();
        e.popStackFrame();
        return e;
    }

    return null;
}

Function._validateParameter = function Function$_validateParameter(param, expectedParam, paramName) {
    var e;

    var expectedType = expectedParam.type;
    var expectedInteger = !!expectedParam.integer;
    var expectedDomElement = !!expectedParam.domElement;
    var mayBeNull = !!expectedParam.mayBeNull;

    e = Function._validateParameterType(param, expectedType, expectedInteger, expectedDomElement, mayBeNull, paramName);
    if (e) {
        e.popStackFrame();
        return e;
    }

        var expectedElementType = expectedParam.elementType;
    var elementMayBeNull = !!expectedParam.elementMayBeNull;
    if (expectedType === Array && typeof(param) !== "undefined" && param !== null &&
        (expectedElementType || !elementMayBeNull)) {
        var expectedElementInteger = !!expectedParam.elementInteger;
        var expectedElementDomElement = !!expectedParam.elementDomElement;
        for (var i=0; i < param.length; i++) {
            var elem = param[i];
            e = Function._validateParameterType(elem, expectedElementType,
                expectedElementInteger, expectedElementDomElement, elementMayBeNull,
                paramName + "[" + i + "]");
            if (e) {
                e.popStackFrame();
                return e;
            }
        }
    }

    return null;
}

Function._validateParameterType = function Function$_validateParameterType(param, expectedType, expectedInteger, expectedDomElement, mayBeNull, paramName) {
    var e;

    if (typeof(param) === "undefined") {
        if (mayBeNull) {
            return null;
        }
        else {
            e = Error.argumentUndefined(paramName);
            e.popStackFrame();
            return e;
        }
    }

    if (param === null) {
        if (mayBeNull) {
            return null;
        }
        else {
            e = Error.argumentNull(paramName);
            e.popStackFrame();
            return e;
        }
    }

    if (expectedType && expectedType.__enum) {
        if (typeof(param) !== 'number') {
            e = Error.argumentType(paramName, Object.getType(param), expectedType);
            e.popStackFrame();
            return e;
        }
        if ((param % 1) === 0) {
            var values = expectedType.prototype;
            if (!expectedType.__flags || (param === 0)) {
                for (var i in values) {
                    if (values[i] === param) return null;
                }
            }
            else {
                var v = param;
                for (var i in values) {
                    var vali = values[i];
                    if (vali === 0) continue;
                    if ((vali & param) === vali) {
                        v -= vali;
                    }
                    if (v === 0) return null;
                }
            }
        }
        e = Error.argumentOutOfRange(paramName, param, String.format(Sys.Res.enumInvalidValue, param, expectedType.getName()));
        e.popStackFrame();
        return e;
    }

                    if (expectedDomElement && (param !== window) && (param !== document) &&
        !(window.HTMLElement && (param instanceof HTMLElement)) &&
        (typeof(param.nodeName) !== 'string')) {

        e = Error.argument(paramName, Sys.Res.argumentDomElement);
        e.popStackFrame();
        return e;
    }

        if (expectedType && !expectedType.isInstanceOfType(param)) {
        e = Error.argumentType(paramName, Object.getType(param), expectedType);
        e.popStackFrame();
        return e;
    }

    if (expectedType === Number && expectedInteger) {
                        if ((param % 1) !== 0) {
            e = Error.argumentOutOfRange(paramName, param, Sys.Res.argumentInteger);
            e.popStackFrame();
            return e;
        }
    }

    return null;
}
Error.__typeName = 'Error';
Error.__class = true;

Error.create = function Error$create(message, errorInfo) {
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="errorInfo" optional="true" mayBeNull="true"></param>
    /// <returns type="Error"></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true},
        {name: "errorInfo", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


            var e = new Error(message);
    e.message = message;

    if (errorInfo) {
        for (var v in errorInfo) {
            e[v] = errorInfo[v];
        }
    }

    e.popStackFrame();
    return e;
}

Error.argument = function Error$argument(paramName, message) {
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentException: " + (message ? message : Sys.Res.argument);
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, { name: "Sys.ArgumentException", paramName: paramName });
    e.popStackFrame();
    return e;
}

Error.argumentNull = function Error$argumentNull(paramName, message) {
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentNullException: " + (message ? message : Sys.Res.argumentNull);
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, { name: "Sys.ArgumentNullException", paramName: paramName });
    e.popStackFrame();
    return e;
}

Error.argumentOutOfRange = function Error$argumentOutOfRange(paramName, actualValue, message) {
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="actualValue" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "actualValue", mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentOutOfRangeException: " + (message ? message : Sys.Res.argumentOutOfRange);
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

                if (typeof(actualValue) !== "undefined" && actualValue !== null) {
        displayMessage += "\n" + String.format(Sys.Res.actualValue, actualValue);
    }

    var e = Error.create(displayMessage, {
        name: "Sys.ArgumentOutOfRangeException",
        paramName: paramName,
        actualValue: actualValue
    });
    e.popStackFrame();
    return e;
}

Error.argumentType = function Error$argumentType(paramName, actualType, expectedType, message) {
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="actualType" type="Type" optional="true" mayBeNull="true"></param>
    /// <param name="expectedType" type="Type" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "actualType", type: Type, mayBeNull: true, optional: true},
        {name: "expectedType", type: Type, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentTypeException: ";
    if (message) {
        displayMessage += message;
    }
    else if (actualType && expectedType) {
        displayMessage +=
            String.format(Sys.Res.argumentTypeWithTypes, actualType.getName(), expectedType.getName());
    }
    else {
        displayMessage += Sys.Res.argumentType;
    }

    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, {
        name: "Sys.ArgumentTypeException",
        paramName: paramName,
        actualType: actualType,
        expectedType: expectedType
    });
    e.popStackFrame();
    return e;
}

Error.argumentUndefined = function Error$argumentUndefined(paramName, message) {
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentUndefinedException: " + (message ? message : Sys.Res.argumentUndefined);
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, { name: "Sys.ArgumentUndefinedException", paramName: paramName });
    e.popStackFrame();
    return e;
}

Error.format = function Error$format(message) {
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var displayMessage = "Sys.FormatException: " + (message ? message : Sys.Res.format);
    var e = Error.create(displayMessage, {name: 'Sys.FormatException'});
    e.popStackFrame();
    return e;
}

Error.invalidOperation = function Error$invalidOperation(message) {
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var displayMessage = "Sys.InvalidOperationException: " + (message ? message : Sys.Res.invalidOperation);

    var e = Error.create(displayMessage, {name: 'Sys.InvalidOperationException'});
    e.popStackFrame();
    return e;
}

Error.notImplemented = function Error$notImplemented(message) {
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var displayMessage = "Sys.NotImplementedException: " + (message ? message : Sys.Res.notImplemented);

    var e = Error.create(displayMessage, {name: 'Sys.NotImplementedException'});
    e.popStackFrame();
    return e;
}

Error.parameterCount = function Error$parameterCount(message) {
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ParameterCountException: " + (message ? message : Sys.Res.parameterCount);
    var e = Error.create(displayMessage, {name: 'Sys.ParameterCountException'});
    e.popStackFrame();
    return e;
}

Error.prototype.popStackFrame = function Error$popStackFrame() {
    if (arguments.length !== 0) throw Error.parameterCount();

                            
    if (typeof(this.stack) === "undefined" || this.stack === null ||
        typeof(this.fileName) === "undefined" || this.fileName === null ||
        typeof(this.lineNumber) === "undefined" || this.lineNumber === null) {
        return;
    }

    var stackFrames = this.stack.split("\n");

                var currentFrame = stackFrames[0];
    var pattern = this.fileName + ":" + this.lineNumber;
    while(typeof(currentFrame) !== "undefined" &&
          currentFrame !== null &&
          currentFrame.indexOf(pattern) === -1) {
        stackFrames.shift();
        currentFrame = stackFrames[0];
    }

    var nextFrame = stackFrames[1];

        if (typeof(nextFrame) === "undefined" || nextFrame === null) {
        return;
    }

        var nextFrameParts = nextFrame.match(/@(.*):(\d+)$/);
    if (typeof(nextFrameParts) === "undefined" || nextFrameParts === null) {
        return;
    }

    this.fileName = nextFrameParts[1];

        this.lineNumber = parseInt(nextFrameParts[2]);

    stackFrames.shift();
    this.stack = stackFrames.join("\n");
}
if (!window) this.window = this;

window.Type = Function;

window.__rootNamespaces = [];
window.__registeredTypes = {};

Type.__fullyQualifiedIdentifierRegExp = new RegExp("^[^.0-9 \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\]([^ \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\]*[^. \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\])?$", "i");
Type.__identifierRegExp = new RegExp("^[^.0-9 \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\][^. \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\]*$", "i");

Type.prototype.callBaseMethod = function Type$callBaseMethod(instance, name, baseArguments) {
    /// <param name="instance"></param>
    /// <param name="name" type="String"></param>
    /// <param name="baseArguments" type="Array" optional="true" mayBeNull="true" elementMayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"},
        {name: "name", type: String},
        {name: "baseArguments", type: Array, mayBeNull: true, optional: true, elementMayBeNull: true}
    ]);
    if (e) throw e;

    var baseMethod = this.getBaseMethod(instance, name);
    if (!baseMethod) throw Error.invalidOperation(String.format(Sys.Res.methodNotFound, name));
    if (!baseArguments) {
        return baseMethod.apply(instance);
    }
    else {
        return baseMethod.apply(instance, baseArguments);
    }
}

Type.prototype.getBaseMethod = function Type$getBaseMethod(instance, name) {
    /// <param name="instance"></param>
    /// <param name="name" type="String"></param>
    /// <returns type="Function" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"},
        {name: "name", type: String}
    ]);
    if (e) throw e;

    if (!this.isInstanceOfType(instance)) throw Error.argumentType('instance', Object.getType(instance), this);
    var baseType = this.getBaseType();
    if (baseType) {
        var baseMethod = baseType.prototype[name];
        return (baseMethod instanceof Function) ? baseMethod : null;
    }

    return null;
}

Type.prototype.getBaseType = function Type$getBaseType() {
    /// <returns type="Type" mayBeNull="true"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return (typeof(this.__baseType) === "undefined") ? null : this.__baseType;
}

Type.prototype.getInterfaces = function Type$getInterfaces() {
    /// <returns type="Array" elementType="Type" mayBeNull="false" elementMayBeNull="false"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    var result = [];
    var type = this;
    while(type) {
        var interfaces = type.__interfaces;
        if (interfaces) {
            for (var i = 0, l = interfaces.length; i < l; i++) {
                var interfaceType = interfaces[i];
                if (!Array.contains(result, interfaceType)) {
                    result[result.length] = interfaceType;
                }
            }
        }
        type = type.__baseType;
    }
    return result;
}

Type.prototype.getName = function Type$getName() {
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return (typeof(this.__typeName) === "undefined") ? "" : this.__typeName;
}

Type.prototype.implementsInterface = function Type$implementsInterface(interfaceType) {
    /// <param name="interfaceType" type="Type"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "interfaceType", type: Type}
    ]);
    if (e) throw e;

    this.resolveInheritance();

    var interfaceName = interfaceType.getName();
    var cache = this.__interfaceCache;
    if (cache) {
        var cacheEntry = cache[interfaceName];
        if (typeof(cacheEntry) !== 'undefined') return cacheEntry;
    }
    else {
        cache = this.__interfaceCache = {};
    }

    var baseType = this;
    while (baseType) {
        var interfaces = baseType.__interfaces;
        if (interfaces) {
            if (Array.indexOf(interfaces, interfaceType) !== -1) {
                return cache[interfaceName] = true;
            }
        }

        baseType = baseType.__baseType;
    }

    return cache[interfaceName] = false;
}

Type.prototype.inheritsFrom = function Type$inheritsFrom(parentType) {
    /// <param name="parentType" type="Type"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "parentType", type: Type}
    ]);
    if (e) throw e;

    this.resolveInheritance();
    var baseType = this.__baseType;
    while (baseType) {
        if (baseType === parentType) {
            return true;
        }
        baseType = baseType.__baseType;
    }

    return false;
}

Type.prototype.initializeBase = function Type$initializeBase(instance, baseArguments) {
    /// <param name="instance"></param>
    /// <param name="baseArguments" type="Array" optional="true" mayBeNull="true" elementMayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"},
        {name: "baseArguments", type: Array, mayBeNull: true, optional: true, elementMayBeNull: true}
    ]);
    if (e) throw e;

    if (!this.isInstanceOfType(instance)) throw Error.argumentType('instance', Object.getType(instance), this);

    this.resolveInheritance();
    if (this.__baseType) {
        if (!baseArguments) {
            this.__baseType.apply(instance);
        }
        else {
            this.__baseType.apply(instance, baseArguments);
        }
    }

    return instance;
}

Type.prototype.isImplementedBy = function Type$isImplementedBy(instance) {
    /// <param name="instance" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", mayBeNull: true}
    ]);
    if (e) throw e;

    if (typeof(instance) === "undefined" || instance === null) return false;

    var instanceType = Object.getType(instance);
    return !!(instanceType.implementsInterface && instanceType.implementsInterface(this));
}

Type.prototype.isInstanceOfType = function Type$isInstanceOfType(instance) {
    /// <param name="instance" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", mayBeNull: true}
    ]);
    if (e) throw e;

    if (typeof(instance) === "undefined" || instance === null) return false;

    if (instance instanceof this) return true;

    var instanceType = Object.getType(instance);
    return !!(instanceType === this) ||
           (instanceType.inheritsFrom && instanceType.inheritsFrom(this)) ||
           (instanceType.implementsInterface && instanceType.implementsInterface(this));
}

Type.prototype.registerClass = function Type$registerClass(typeName, baseType, interfaceTypes) {
    /// <param name="typeName" type="String"></param>
    /// <param name="baseType" type="Type" optional="true" mayBeNull="true"></param>
    /// <param name="interfaceTypes" parameterArray="true" type="Type"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "typeName", type: String},
        {name: "baseType", type: Type, mayBeNull: true, optional: true},
        {name: "interfaceTypes", type: Type, parameterArray: true}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(typeName)) throw Error.argument('typeName', Sys.Res.notATypeName);
        var parsedName;
    try {
        parsedName = eval(typeName);
    }
    catch(e) {
        throw Error.argument('typeName', Sys.Res.argumentTypeName);
    }
    if (parsedName !== this) throw Error.argument('typeName', Sys.Res.badTypeName);
        if (window.__registeredTypes[typeName]) throw Error.invalidOperation(String.format(Sys.Res.typeRegisteredTwice, typeName));

            if ((arguments.length > 1) && (typeof(baseType) === 'undefined')) throw Error.argumentUndefined('baseType');
    if (baseType && !baseType.__class) throw Error.argument('baseType', Sys.Res.baseNotAClass);

    this.prototype.constructor = this;
    this.__typeName = typeName;
    this.__class = true;
    if (baseType) {
        this.__baseType = baseType;
        this.__basePrototypePending = true;
    }
        if (!window.__classes) window.__classes = {};
    window.__classes[typeName.toUpperCase()] = this;

                if (interfaceTypes) {
        this.__interfaces = [];
        for (var i = 2; i < arguments.length; i++) {
            var interfaceType = arguments[i];
            if (!interfaceType.__interface) throw Error.argument('interfaceTypes[' + (i - 2) + ']', Sys.Res.notAnInterface);
            this.resolveInheritance();
            for (var methodName in interfaceType.prototype) {
                var method = interfaceType.prototype[methodName];
                if (!this.prototype[methodName]) {
                    this.prototype[methodName] = method;
                }
            }
            this.__interfaces.push(interfaceType);
        }
    }
    window.__registeredTypes[typeName] = true;

    return this;
}

Type.prototype.registerInterface = function Type$registerInterface(typeName) {
    /// <param name="typeName" type="String"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "typeName", type: String}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(typeName)) throw Error.argument('typeName', Sys.Res.notATypeName);
        var parsedName;
    try {
        parsedName = eval(typeName);
    }
    catch(e) {
        throw Error.argument('typeName', Sys.Res.argumentTypeName);
    }
    if (parsedName !== this) throw Error.argument('typeName', Sys.Res.badTypeName);
        if (window.__registeredTypes[typeName]) throw Error.invalidOperation(String.format(Sys.Res.typeRegisteredTwice, typeName));
    this.prototype.constructor = this;
    this.__typeName = typeName;
    this.__interface = true;
    window.__registeredTypes[typeName] = true;

    return this;
}

Type.prototype.resolveInheritance = function Type$resolveInheritance() {
    if (arguments.length !== 0) throw Error.parameterCount();

    if (this.__basePrototypePending) {
        var baseType = this.__baseType;

        baseType.resolveInheritance();

        for (var memberName in baseType.prototype) {
            var memberValue = baseType.prototype[memberName];
            if (!this.prototype[memberName]) {
                this.prototype[memberName] = memberValue;
            }
        }
        delete this.__basePrototypePending;
    }
}

Type.getRootNamespaces = function Type$getRootNamespaces() {
    /// <returns type="Array"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return Array.clone(window.__rootNamespaces);
}

Type.isClass = function Type$isClass(type) {
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
    return !!type.__class;
}

Type.isInterface = function Type$isInterface(type) {
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
    return !!type.__interface;
}

Type.isNamespace = function Type$isNamespace(object) {
    /// <param name="object" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "object", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(object) === 'undefined') || (object === null)) return false;
    return !!object.__namespace;
}

Type.parse = function Type$parse(typeName, ns) {
    /// <param name="typeName" type="String" mayBeNull="true"></param>
    /// <param name="ns" optional="true" mayBeNull="true"></param>
    /// <returns type="Type" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "typeName", type: String, mayBeNull: true},
        {name: "ns", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var fn;
    if (ns) {
        if (!window.__classes) return null;
        fn = window.__classes[ns.getName().toUpperCase() + '.' + typeName.toUpperCase()];
        return fn || null;
    }
    if (!typeName) return null;
    if (!Type.__htClasses) {
        Type.__htClasses = {};
    }
    fn = Type.__htClasses[typeName];
    if (!fn) {
        fn = eval(typeName);
        if (typeof(fn) !== 'function') throw Error.argument('typeName', Sys.Res.notATypeName);
        Type.__htClasses[typeName] = fn;
    }
    return fn;
}

Type.registerNamespace = function Type$registerNamespace(namespacePath) {
    /// <param name="namespacePath" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "namespacePath", type: String}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(namespacePath)) throw Error.argument('namespacePath', Sys.Res.invalidNameSpace);
    var rootObject = window;
    var namespaceParts = namespacePath.split('.');

    for (var i = 0; i < namespaceParts.length; i++) {
        var currentPart = namespaceParts[i];
        var ns = rootObject[currentPart];
        if (ns && !ns.__namespace) {
            throw Error.invalidOperation(String.format(Sys.Res.namespaceContainsObject, namespaceParts.splice(0, i + 1).join('.')));
        }
        if (!ns) {
            ns = rootObject[currentPart] = {};
            if (i === 0) {
                window.__rootNamespaces[window.__rootNamespaces.length] = ns;
            }
            ns.__namespace = true;
            ns.__typeName = namespaceParts.slice(0, i + 1).join('.');
            var parsedName;
            try {
                parsedName = eval(ns.__typeName);
            }
            catch(e) {
                parsedName = null;
            }
            if (parsedName !== ns) {
                delete rootObject[currentPart];
                throw Error.argument('namespacePath', Sys.Res.invalidNameSpace);
            }
            ns.getName = function ns$getName() {return this.__typeName;}
        }
        rootObject = ns;
    }
}
Object.__typeName = 'Object';
Object.__class = true;

Object.getType = function Object$getType(instance) {
    /// <param name="instance"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"}
    ]);
    if (e) throw e;

    var ctor = instance.constructor;
    if (!ctor || (typeof(ctor) !== "function") || !ctor.__typeName || (ctor.__typeName === 'Object')) {
        return Object;
    }
    return ctor;
}

Object.getTypeName = function Object$getTypeName(instance) {
    /// <param name="instance"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"}
    ]);
    if (e) throw e;

    return Object.getType(instance).getName();
}
Boolean.__typeName = 'Boolean';
Boolean.__class = true;

Boolean.parse = function Boolean$parse(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    var v = value.trim().toLowerCase();
    if (v === 'false') return false;
    if (v === 'true') return true;
    throw Error.argumentOutOfRange('value', value, Sys.Res.boolTrueOrFalse);
}
Date.__typeName = 'Date';
Date.__class = true;

Date._appendPreOrPostMatch = function Date$_appendPreOrPostMatch(preMatch, strBuilder) {
                var quoteCount = 0;
    var escaped = false;
    for (var i = 0, il = preMatch.length; i < il; i++) {
        var c = preMatch.charAt(i);
        switch (c) {
        case '\'':
            if (escaped) strBuilder.append("'");
            else quoteCount++;
            escaped = false;
            break;
        case '\\':
            if (escaped) strBuilder.append("\\");
            escaped = !escaped;
            break;
        default:
            strBuilder.append(c);
            escaped = false;
            break;
        }
    }
    return quoteCount;
}

Date._expandFormat = function Date$_expandFormat(dtf, format) {
        if (!format) {
        format = "F";
    }
    if (format.length === 1) {
        switch (format) {
        case "d":
            return dtf.ShortDatePattern;
        case "D":
            return dtf.LongDatePattern;
        case "t":
            return dtf.ShortTimePattern;
        case "T":
            return dtf.LongTimePattern;
        case "F":
            return dtf.FullDateTimePattern;
        case "M": case "m":
            return dtf.MonthDayPattern;
        case "s":
            return dtf.SortableDateTimePattern;
        case "Y": case "y":
            return dtf.YearMonthPattern;
        default:
            throw Error.format(Sys.Res.formatInvalidString);
        }
    }
    return format;
}

Date._expandYear = function Date$_expandYear(dtf, year) {
        if (year < 100) {
        var curr = new Date().getFullYear();
        year += curr - (curr % 100);
        if (year > dtf.Calendar.TwoDigitYearMax) {
            return year - 100;
        }
    }
    return year;
}

Date._getParseRegExp = function Date$_getParseRegExp(dtf, format) {
                if (!dtf._parseRegExp) {
        dtf._parseRegExp = {};
    }
    else if (dtf._parseRegExp[format]) {
        return dtf._parseRegExp[format];
    }

        var expFormat = Date._expandFormat(dtf, format);
    expFormat = expFormat.replace(/([\^\$\.\*\+\?\|\[\]\(\)\{\}])/g, "\\\\$1");

    var regexp = new Sys.StringBuilder("^");
    var groups = [];
    var index = 0;
    var quoteCount = 0;
    var tokenRegExp = Date._getTokenRegExp();
    var match;

        while ((match = tokenRegExp.exec(expFormat)) !== null) {
        var preMatch = expFormat.slice(index, match.index);
        index = tokenRegExp.lastIndex;

                quoteCount += Date._appendPreOrPostMatch(preMatch, regexp);
        if ((quoteCount%2) === 1) {
            regexp.append(match[0]);
            continue;
        }

                switch (match[0]) {
            case 'dddd': case 'ddd':
            case 'MMMM': case 'MMM':
                regexp.append("(\\D+)");
                break;
            case 'tt': case 't':
                regexp.append("(\\D*)");
                break;
            case 'yyyy':
                regexp.append("(\\d{4})");
                break;
            case 'fff':
                regexp.append("(\\d{3})");
                break;
            case 'ff':
                regexp.append("(\\d{2})");
                break;
            case 'f':
                regexp.append("(\\d)");
                break;
            case 'dd': case 'd':
            case 'MM': case 'M':
            case 'yy': case 'y':
            case 'HH': case 'H':
            case 'hh': case 'h':
            case 'mm': case 'm':
            case 'ss': case 's':
                regexp.append("(\\d\\d?)");
                break;
            case 'zzz':
                regexp.append("([+-]?\\d\\d?:\\d{2})");
                break;
            case 'zz': case 'z':
                regexp.append("([+-]?\\d\\d?)");
                break;
        }
        Array.add(groups, match[0]);
    }
    Date._appendPreOrPostMatch(expFormat.slice(index), regexp);
    regexp.append("$");
        var regexpStr = regexp.toString().replace(/\s+/g, "\\s+");
    var parseRegExp = {'regExp': regexpStr, 'groups': groups};
        dtf._parseRegExp[format] = parseRegExp;
    return parseRegExp;
}

Date._getTokenRegExp = function Date$_getTokenRegExp() {
        return /dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy|y|hh|h|HH|H|mm|m|ss|s|tt|t|fff|ff|f|zzz|zz|z/g;
}

Date.parseLocale = function Date$parseLocale(value, formats) {
    /// <param name="value" type="String"></param>
    /// <param name="formats" parameterArray="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Date"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String},
        {name: "formats", mayBeNull: true, optional: true, parameterArray: true}
    ]);
    if (e) throw e;

    return Date._parse(value, Sys.CultureInfo.CurrentCulture, arguments);
}

Date.parseInvariant = function Date$parseInvariant(value, formats) {
    /// <param name="value" type="String"></param>
    /// <param name="formats" parameterArray="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Date"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String},
        {name: "formats", mayBeNull: true, optional: true, parameterArray: true}
    ]);
    if (e) throw e;

    return Date._parse(value, Sys.CultureInfo.InvariantCulture, arguments);
}

Date._parse = function Date$_parse(value, cultureInfo, args) {
            var custom = false;
    for (var i = 1, il = args.length; i < il; i++) {
        var format = args[i];
        if (format) {
            custom = true;
            var date = Date._parseExact(value, format, cultureInfo);
            if (date) return date;
        }
    }
        if (! custom) {
        var formats = cultureInfo._getDateTimeFormats();
        for (var i = 0, il = formats.length; i < il; i++) {
            var date = Date._parseExact(value, formats[i], cultureInfo);
            if (date) return date;
        }
    }
    return null;
}

Date._parseExact = function Date$_parseExact(value, format, cultureInfo) {
            value = value.trim();
    var dtf = cultureInfo.dateTimeFormat;

            var parseInfo = Date._getParseRegExp(dtf, format);
    var match = new RegExp(parseInfo.regExp).exec(value);
        if (match !== null) {
        var groups = parseInfo.groups;
        var year = null, month = null, date = null, weekDay = null;
        var hour = 0, min = 0, sec = 0, msec = 0, tzMinOffset = null;
        var pmHour = false;
                for (var j = 0, jl = groups.length; j < jl; j++) {
            var matchGroup = match[j+1];
            if (matchGroup) {
                switch (groups[j]) {
                    case 'dd': case 'd':
                                                date = Date._parseInt(matchGroup);
                                                if ((date < 1) || (date > 31)) return null;
                        break;
                    case 'MMMM':
                                                month = cultureInfo._getMonthIndex(matchGroup);
                        if ((month < 0) || (month > 11)) return null;
                        break;
                    case 'MMM':
                                                month = cultureInfo._getAbbrMonthIndex(matchGroup);
                        if ((month < 0) || (month > 11)) return null;
                        break;
                    case 'M': case 'MM':
                                                var month = Date._parseInt(matchGroup) - 1;
                        if ((month < 0) || (month > 11)) return null;
                        break;
                    case 'y': case 'yy':
                                                year = Date._expandYear(dtf,Date._parseInt(matchGroup));
                        if ((year < 0) || (year > 9999)) return null;
                        break;
                    case 'yyyy':
                                                year = Date._parseInt(matchGroup);
                        if ((year < 0) || (year > 9999)) return null;
                        break;
                    case 'h': case 'hh':
                                                hour = Date._parseInt(matchGroup);
                        if (hour === 12) hour = 0;
                        if ((hour < 0) || (hour > 11)) return null;
                        break;
                    case 'H': case 'HH':
                                                hour = Date._parseInt(matchGroup);
                        if ((hour < 0) || (hour > 23)) return null;
                        break;
                    case 'm': case 'mm':
                                                min = Date._parseInt(matchGroup);
                        if ((min < 0) || (min > 59)) return null;
                        break;
                    case 's': case 'ss':
                                                sec = Date._parseInt(matchGroup);
                        if ((sec < 0) || (sec > 59)) return null;
                        break;
                    case 'tt': case 't':
                                                var upperToken = matchGroup.toUpperCase();
                        pmHour = (upperToken === dtf.PMDesignator.toUpperCase());
                        if (!pmHour && (upperToken !== dtf.AMDesignator.toUpperCase())) return null;
                        break;
                    case 'f':
                                                msec = Date._parseInt(matchGroup) * 100;
                        if ((msec < 0) || (msec > 999)) return null;
                        break;
                    case 'ff':
                                                msec = Date._parseInt(matchGroup) * 10;
                        if ((msec < 0) || (msec > 999)) return null;
                        break;
                    case 'fff':
                                                msec = Date._parseInt(matchGroup);
                        if ((msec < 0) || (msec > 999)) return null;
                        break;
                    case 'dddd':
                                                weekDay = cultureInfo._getDayIndex(matchGroup);
                        if ((weekDay < 0) || (weekDay > 6)) return null;
                        break;
                    case 'ddd':
                                                weekDay = cultureInfo._getAbbrDayIndex(matchGroup);
                        if ((weekDay < 0) || (weekDay > 6)) return null;
                        break;
                    case 'zzz':
                                                var offsets = matchGroup.split(/:/);
                        if (offsets.length !== 2) return null;
                        var hourOffset = Date._parseInt(offsets[0]);
                        if ((hourOffset < -12) || (hourOffset > 13)) return null;
                        var minOffset = Date._parseInt(offsets[1]);
                        if ((minOffset < 0) || (minOffset > 59)) return null;
                        tzMinOffset = (hourOffset * 60) + (matchGroup.startsWith('-')? -minOffset : minOffset);
                        break;
                    case 'z': case 'zz':
                                                var hourOffset = Date._parseInt(matchGroup);
                        if ((hourOffset < -12) || (hourOffset > 13)) return null;
                        tzMinOffset = hourOffset * 60;
                        break;
                }
            }
        }
        var result = new Date();
        if (year === null) {
            year = result.getFullYear();
        }
        if (month === null) {
            month = result.getMonth();
        }
        if (date === null) {
            date = result.getDate();
        }
                result.setFullYear(year, month, date);
                if (result.getDate() !== date) return null;
                if ((weekDay !== null) && (result.getDay() !== weekDay)) {
            return null;
        }
                if (pmHour && (hour < 12)) {
            hour += 12;
        }
        result.setHours(hour, min, sec, msec);
        if (tzMinOffset !== null) {
                        var adjustedMin = result.getMinutes() - (tzMinOffset + result.getTimezoneOffset());
                                                result.setHours(result.getHours() + parseInt(adjustedMin/60), adjustedMin%60);
        }
        return result;
    }
}

Date._parseInt = function Date$_parseInt(value) {
        return parseInt(value.replace(/^[\s0]+(\d+)$/,"$1"));
}

Date.prototype.format = function Date$format(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.InvariantCulture);
}

Date.prototype.localeFormat = function Date$localeFormat(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.CurrentCulture);
}

Date.prototype._toFormattedString = function Date$_toFormattedString(format, cultureInfo) {
    if (!format || (format.length === 0) || (format === 'i')) {
        if (cultureInfo && (cultureInfo.name.length > 0)) {
            return this.toLocaleString();
        }
        else {
            return this.toString();
        }
    }

    var dtf = cultureInfo.dateTimeFormat;
    format = Date._expandFormat(dtf, format);

        var ret = new Sys.StringBuilder();
    var hour;

    function addLeadingZero(num) {
        if (num < 10) {
            return '0' + num;
        }
        return num.toString();
    }

    function addLeadingZeros(num) {
        if (num < 10) {
            return '00' + num;
        }
        if (num < 100) {
            return '0' + num;
        }
        return num.toString();
    }

    var quoteCount = 0;
    var tokenRegExp = Date._getTokenRegExp();
    for (;;) {

                var index = tokenRegExp.lastIndex;

                var ar = tokenRegExp.exec(format);

                var preMatch = format.slice(index, ar ? ar.index : format.length);
        quoteCount += Date._appendPreOrPostMatch(preMatch, ret);

        if (!ar) break;

                if ((quoteCount%2) === 1) {
            ret.append(ar[0]);
            continue;
        }

        switch (ar[0]) {
        case "dddd":
                        ret.append(dtf.DayNames[this.getDay()]);
            break;
        case "ddd":
                        ret.append(dtf.AbbreviatedDayNames[this.getDay()]);
            break;
        case "dd":
                        ret.append(addLeadingZero(this.getDate()));
            break;
        case "d":
                        ret.append(this.getDate());
            break;
        case "MMMM":
                        ret.append(dtf.MonthNames[this.getMonth()]);
            break;
        case "MMM":
                        ret.append(dtf.AbbreviatedMonthNames[this.getMonth()]);
            break;
        case "MM":
                        ret.append(addLeadingZero(this.getMonth() + 1));
            break;
        case "M":
                        ret.append(this.getMonth() + 1);
            break;
        case "yyyy":
                        ret.append(this.getFullYear());
            break;
        case "yy":
                        ret.append(addLeadingZero(this.getFullYear() % 100));
            break;
        case "y":
                        ret.append(this.getFullYear() % 100);
            break;
        case "hh":
                        hour = this.getHours() % 12;
            if (hour === 0) hour = 12;
            ret.append(addLeadingZero(hour));
            break;
        case "h":
                        hour = this.getHours() % 12;
            if (hour === 0) hour = 12;
            ret.append(hour);
            break;
        case "HH":
                        ret.append(addLeadingZero(this.getHours()));
            break;
        case "H":
                        ret.append(this.getHours());
            break;
        case "mm":
                        ret.append(addLeadingZero(this.getMinutes()));
            break;
        case "m":
                        ret.append(this.getMinutes());
            break;
        case "ss":
                        ret.append(addLeadingZero(this.getSeconds()));
            break;
        case "s":
                        ret.append(this.getSeconds());
            break;
        case "tt":
                        ret.append((this.getHours() < 12) ? dtf.AMDesignator : dtf.PMDesignator);
            break;
        case "t":
                        ret.append(((this.getHours() < 12) ? dtf.AMDesignator : dtf.PMDesignator).charAt(0));
            break;
        case "f":
                        ret.append(addLeadingZeros(this.getMilliseconds()).charAt(0));
            break;
        case "ff":
                        ret.append(addLeadingZeros(this.getMilliseconds()).substr(0, 2));
            break;
        case "fff":
                        ret.append(addLeadingZeros(this.getMilliseconds()));
            break;
        case "z":
                        hour = this.getTimezoneOffset() / 60;
            ret.append(((hour >= 0) ? '+' : '-') + Math.floor(Math.abs(hour)));
            break;
        case "zz":
                        hour = this.getTimezoneOffset() / 60;
            ret.append(((hour >= 0) ? '+' : '-') + addLeadingZero(Math.floor(Math.abs(hour))));
            break;
        case "zzz":
                        hour = this.getTimezoneOffset() / 60;
            ret.append(((hour >= 0) ? '+' : '-') + addLeadingZero(Math.floor(Math.abs(hour))) +
                dtf.TimeSeparator + addLeadingZero(Math.abs(this.getTimezoneOffset() % 60)));
            break;
        }
    }
    return ret.toString();
}
Number.__typeName = 'Number';
Number.__class = true;

Number.parseLocale = function Number$parseLocale(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Number"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    return Number._parse(value, Sys.CultureInfo.CurrentCulture);
}
Number.parseInvariant = function Number$parseInvariant(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Number"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    return Number._parse(value, Sys.CultureInfo.InvariantCulture);
}
Number._parse = function Number$_parse(value, cultureInfo) {
            var valueStr = value.trim();
    if (valueStr.match(/infinity/i) !== null) {
        return parseFloat(valueStr);
    }
    if (valueStr.match(/^0x[a-f0-9]+$/i) !== null) {
        return parseInt(valueStr);
    }
    var numFormat = cultureInfo.numberFormat;
    var decSeparator = numFormat.NumberDecimalSeparator;
    var grpSeparator = numFormat.NumberGroupSeparator;

            var numberFormatRegex = new RegExp("^[+-]?[\\d\\" + grpSeparator + "]*\\" + decSeparator + "?\\d*([eE][+-]?\\d+)?$");
    if (!valueStr.match(numberFormatRegex)) {
        return Number.NaN;
    }

        valueStr = valueStr.split(grpSeparator).join("");

        valueStr = valueStr.replace(decSeparator, ".");

    return parseFloat(valueStr);
}

Number.prototype.format = function Number$format(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.InvariantCulture);
}
Number.prototype.localeFormat = function Number$localeFormat(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.CurrentCulture);
}
Number.prototype._toFormattedString = function Number$_toFormattedString(format, cultureInfo) {
    if (!format || (format.length === 0) || (format === 'i')) {
        if (cultureInfo && (cultureInfo.name.length > 0)) {
            return this.toLocaleString();
        }
        else {
            return this.toString();
        }
    }

        var _percentPositivePattern = ["n %", "n%", "%n" ];
    var _percentNegativePattern = ["-n %", "-n%", "-%n"];
    var _numberNegativePattern = ["(n)","-n","- n","n-","n -"];
    var _currencyPositivePattern = ["$n","n$","$ n","n $"];
    var _currencyNegativePattern = ["($n)","-$n","$-n","$n-","(n$)","-n$","n-$","n$-","-n $","-$ n","n $-","$ n-","$ -n","n- $","($ n)","(n $)"];

        function expandNumber(number, precision, groupSizes, sep, decimalChar) {
        
        var curSize = groupSizes[0];
        var curGroupIndex = 1;

                var numberString = number.toString();
        var right = "";
        var exponent = "";
                var decimalSplit = numberString.split('.');
        if (decimalSplit.length > 1) {
            numberString = decimalSplit[0];
            right = decimalSplit[1];
                        var exponentSplit = right.split(/e/i);
            if (exponentSplit.length > 1) {
                right = exponentSplit[0];
                exponent = "e" + exponentSplit[1];
            }
        }

                if (precision > 0) {
                        var rightDifference = right.length - precision;
            if (rightDifference > 0) {
                right = right.slice(0, precision);
            } else if (rightDifference < 0) {
                for (var i=0; i<Math.abs(rightDifference); i++) {
                    right += '0';
                }
            }

                        right = decimalChar + right;
        }
        else {             right = "";
        }
        right += exponent;

        var stringIndex = numberString.length-1;
        var ret = "";
        while (stringIndex >= 0) {

                        if (curSize === 0 || curSize > stringIndex) {
                if (ret.length > 0)
                    return numberString.slice(0, stringIndex + 1) + sep + ret + right;
                else
                    return numberString.slice(0, stringIndex + 1) + right;
            }

            if (ret.length > 0)
                ret = numberString.slice(stringIndex - curSize + 1, stringIndex+1) + sep + ret;
            else
                ret = numberString.slice(stringIndex - curSize + 1, stringIndex+1);

            stringIndex -= curSize;

            if (curGroupIndex < groupSizes.length) {
                curSize = groupSizes[curGroupIndex];
                curGroupIndex++;
            }
        }
        return numberString.slice(0, stringIndex + 1) + sep + ret + right;
    }
    var nf = cultureInfo.numberFormat;

        var number = Math.abs(this);

        if (!format)
        format = "D";

    var precision = -1;
    if (format.length > 1) precision = parseInt(format.slice(1));

    var pattern;
    switch (format.charAt(0)) {
    case "d":
    case "D":
        pattern = 'n';

                if (precision !== -1) {
            var numberStr = ""+number;
            var zerosToAdd = precision - numberStr.length;
            if (zerosToAdd > 0) {
                for (var i=0; i<zerosToAdd; i++) {
                    numberStr = '0'+numberStr;
                }
            }
            number = numberStr;
        }

                if (this < 0) number = -number;
        break;
    case "c":
    case "C":
        if (this < 0) pattern = _currencyNegativePattern[nf.CurrencyNegativePattern];
        else pattern = _currencyPositivePattern[nf.CurrencyPositivePattern];
        if (precision === -1) precision = nf.CurrencyDecimalDigits;
        number = expandNumber(Math.abs(this), precision, nf.CurrencyGroupSizes, nf.CurrencyGroupSeparator, nf.CurrencyDecimalSeparator);
        break;
    case "n":
    case "N":
        if (this < 0) pattern = _numberNegativePattern[nf.NumberNegativePattern];
        else pattern = 'n';
        if (precision === -1) precision = nf.NumberDecimalDigits;
        number = expandNumber(Math.abs(this), precision, nf.NumberGroupSizes, nf.NumberGroupSeparator, nf.NumberDecimalSeparator);
        break;
    case "p":
    case "P":
        if (this < 0) pattern = _percentNegativePattern[nf.PercentNegativePattern];
        else pattern = _percentPositivePattern[nf.PercentPositivePattern];
        if (precision === -1) precision = nf.PercentDecimalDigits;
        number = expandNumber(Math.abs(this), precision, nf.PercentGroupSizes, nf.PercentGroupSeparator, nf.PercentDecimalSeparator);
        break;
    default:
        throw Error.format(Sys.Res.formatBadFormatSpecifier);
    }

    var regex = /n|\$|-|%/g;

        var ret = "";

    for (;;) {

                var index = regex.lastIndex;

                var ar = regex.exec(pattern);

                ret += pattern.slice(index, ar ? ar.index : pattern.length);

        if (!ar)
            break;

        switch (ar[0]) {
        case "n":
            ret += number;
            break;
        case "$":
            ret += nf.CurrencySymbol;
            break;
        case "-":
            ret += nf.NegativeSign;
            break;
        case "%":
            ret += nf.PercentSymbol;
            break;
        }
    }

    return ret;
}
RegExp.__typeName = 'RegExp';
RegExp.__class = true;
Array.__typeName = 'Array';
Array.__class = true;

Array.add = Array.enqueue = function Array$enqueue(array, item) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;


        array[array.length] = item;
}

Array.addRange = function Array$addRange(array, items) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="items" type="Array" elementMayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "items", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;


        array.push.apply(array, items);
}

Array.clear = function Array$clear(array) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;

    array.length = 0;
}

Array.clone = function Array$clone(array) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <returns type="Array" elementMayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;

    if (array.length === 1) {
        return [array[0]];
    }
    else {
                        return Array.apply(null, array);
    }
}

Array.contains = function Array$contains(array, item) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;

    return (Array.indexOf(array, item) >= 0);
}

Array.dequeue = function Array$dequeue(array) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <returns mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;

    return array.shift();
}

Array.forEach = function Array$forEach(array, method, instance) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="method" type="Function"></param>
    /// <param name="instance" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "method", type: Function},
        {name: "instance", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    for (var i = 0, l = array.length; i < l; i++) {
        var elt = array[i];
        if (typeof(elt) !== 'undefined') method.call(instance, elt, i, array);
    }
}

Array.indexOf = function Array$indexOf(array, item, start) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" optional="true" mayBeNull="true"></param>
    /// <param name="start" optional="true" mayBeNull="true"></param>
    /// <returns type="Number"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true, optional: true},
        {name: "start", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if (typeof(item) === "undefined") return -1;
    var length = array.length;
    if (length !== 0) {
                start = start - 0;
                if (isNaN(start)) {
            start = 0;
        }
        else {
                                    if (isFinite(start)) {
                                start = start - (start % 1);
            }
                        if (start < 0) {
                start = Math.max(0, length + start);
            }
        }

                for (var i = start; i < length; i++) {
            if ((typeof(array[i]) !== "undefined") && (array[i] === item)) {
                return i;
            }
        }
    }
    return -1;
}

Array.insert = function Array$insert(array, index, item) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="index" mayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "index", mayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;

    array.splice(index, 0, item);
}

Array.parse = function Array$parse(value) {
    /// <param name="value" type="String" mayBeNull="true"></param>
    /// <returns type="Array" elementMayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String, mayBeNull: true}
    ]);
    if (e) throw e;

    if (!value) return [];
    var v = eval(value);
    if (!Array.isInstanceOfType(v)) throw Error.argument('value', Sys.Res.arrayParseBadFormat);
    return v;
}

Array.remove = function Array$remove(array, item) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;

    var index = Array.indexOf(array, item);
    if (index >= 0) {
        array.splice(index, 1);
    }
    return (index >= 0);
}

Array.removeAt = function Array$removeAt(array, index) {
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="index" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "index", mayBeNull: true}
    ]);
    if (e) throw e;

    array.splice(index, 1);
}
String.__typeName = 'String';
String.__class = true;

String.prototype.endsWith = function String$endsWith(suffix) {
    /// <param name="suffix" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "suffix", type: String}
    ]);
    if (e) throw e;

    return (this.substr(this.length - suffix.length) === suffix);
}

String.prototype.startsWith = function String$startsWith(prefix) {
    /// <param name="prefix" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "prefix", type: String}
    ]);
    if (e) throw e;

    return (this.substr(0, prefix.length) === prefix);
}

String.prototype.trim = function String$trim() {
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return this.replace(/^\s+|\s+$/g, '');
}

String.prototype.trimEnd = function String$trimEnd() {
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return this.replace(/\s+$/, '');
}

String.prototype.trimStart = function String$trimStart() {
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return this.replace(/^\s+/, '');
}

String.format = function String$format(format, args) {
    /// <param name="format" type="String"></param>
    /// <param name="args" parameterArray="true" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String},
        {name: "args", mayBeNull: true, parameterArray: true}
    ]);
    if (e) throw e;

    return String._toFormattedString(false, arguments);
}

String.localeFormat = function String$localeFormat(format, args) {
    /// <param name="format" type="String"></param>
    /// <param name="args" parameterArray="true" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String},
        {name: "args", mayBeNull: true, parameterArray: true}
    ]);
    if (e) throw e;

    return String._toFormattedString(true, arguments);
}

String._toFormattedString = function String$_toFormattedString(useLocale, args) {
    var result = '';
    var format = args[0];

    for (var i=0;;) {
                var open = format.indexOf('{', i);
        var close = format.indexOf('}', i);
        if ((open < 0) && (close < 0)) {
                        result += format.slice(i);
            break;
        }
        if ((close > 0) && ((close < open) || (open < 0))) {
                        if (format.charAt(close + 1) !== '}') {
                throw Error.argument('format', Sys.Res.stringFormatBraceMismatch);
            }
            result += format.slice(i, close + 1);
            i = close + 2;
            continue;
        }

                result += format.slice(i, open);
        i = open + 1;

                if (format.charAt(i) === '{') {
            result += '{';
            i++;
            continue;
        }

                if (close < 0) throw Error.argument('format', Sys.Res.stringFormatBraceMismatch);

        
                var brace = format.substring(i, close);
        var colonIndex = brace.indexOf(':');
        var argNumber = parseInt((colonIndex < 0)? brace : brace.substring(0, colonIndex)) + 1;
        if (isNaN(argNumber)) throw Error.argument('format', Sys.Res.stringFormatInvalid);
        var argFormat = (colonIndex < 0)? '' : brace.substring(colonIndex + 1);

        var arg = args[argNumber];
        if (typeof(arg) === "undefined" || arg === null) {
            arg = '';
        }

                if (arg.toFormattedString) {
            result += arg.toFormattedString(argFormat);
        }
        else if (useLocale && arg.localeFormat) {
            result += arg.localeFormat(argFormat);
        }
        else if (arg.format) {
            result += arg.format(argFormat);
        }
        else
            result += arg.toString();

        i = close + 1;
    }

    return result;
}

Type.registerNamespace('Sys');
Sys.IDisposable = function Sys$IDisposable() {
    throw Error.notImplemented();
}

    function Sys$IDisposable$dispose() {
        throw Error.notImplemented();
    }
Sys.IDisposable.prototype = {
    dispose: Sys$IDisposable$dispose
}
Sys.IDisposable.registerInterface('Sys.IDisposable');
Sys.StringBuilder = function Sys$StringBuilder(initialText) {
    /// <param name="initialText" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "initialText", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    this._parts = (typeof(initialText) !== 'undefined' && initialText !== null && initialText !== '') ?
        [initialText.toString()] : [];
    this._value = {};
    this._len = 0;
}


    function Sys$StringBuilder$append(text) {
        /// <param name="text" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "text", mayBeNull: true}
        ]);
        if (e) throw e;

        this._parts[this._parts.length] = text;
    }

    function Sys$StringBuilder$appendLine(text) {
        /// <param name="text" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "text", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        this._parts[this._parts.length] =
            ((typeof(text) === 'undefined') || (text === null) || (text === '')) ?
            '\r\n' : text + '\r\n';
    }

    function Sys$StringBuilder$clear() {
        if (arguments.length !== 0) throw Error.parameterCount();
        this._parts = [];
        this._value = {};
        this._len = 0;
    }

    function Sys$StringBuilder$isEmpty() {
        /// <returns type="Boolean"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._parts.length === 0) return true;
        return this.toString() === '';
    }



    function Sys$StringBuilder$toString(separator) {
        /// <param name="separator" type="String" optional="true" mayBeNull="true"></param>
        /// <returns type="String"></returns>
        var e = Function._validateParams(arguments, [
            {name: "separator", type: String, mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        separator = separator || '';
        var parts = this._parts;
        if (this._len !== parts.length) {
            this._value = {};
            this._len = parts.length;
        }
        var val = this._value;
        if (typeof(val[separator]) === 'undefined') {
                        if (separator !== '') {
                for (var i = 0; i < parts.length;) {
                    if ((typeof(parts[i]) === 'undefined') || (parts[i] === '') || (parts[i] === null)) {
                        parts.splice(i, 1);
                    }
                    else {
                        i++;
                    }
                }
            }
            val[separator] = this._parts.join(separator);
        }
        return val[separator];
    }
Sys.StringBuilder.prototype = {
    append: Sys$StringBuilder$append,

    appendLine: Sys$StringBuilder$appendLine,

    clear: Sys$StringBuilder$clear,

    isEmpty: Sys$StringBuilder$isEmpty,

            toString: Sys$StringBuilder$toString
}
Sys.StringBuilder.registerClass('Sys.StringBuilder');
if (!window.XMLHttpRequest) {
    window.XMLHttpRequest = function window$XMLHttpRequest() {
        var progIDs = [ 'Msxml2.XMLHTTP', 'Microsoft.XMLHTTP' ];
	    
        for (var i = 0; i < progIDs.length; i++) {
            try {
                var xmlHttp = new ActiveXObject(progIDs[i]);
                return xmlHttp;
            }
            catch (ex) {
            }
        }
	    
        return null;
    }
}

Sys.Browser = {};

Sys.Browser.InternetExplorer = {};
Sys.Browser.Firefox = {};
Sys.Browser.Safari = {};
Sys.Browser.Opera = {};

Sys.Browser.agent = null;
Sys.Browser.hasDebuggerStatement = false;
Sys.Browser.name = navigator.appName;
Sys.Browser.version = parseFloat(navigator.appVersion);

if (navigator.userAgent.indexOf(' MSIE ') > -1) {
    Sys.Browser.agent = Sys.Browser.InternetExplorer;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/MSIE (\d+\.\d+)/)[1]);
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(' Firefox/') > -1) {
    Sys.Browser.agent = Sys.Browser.Firefox;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Firefox\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Firefox';
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(' Safari/') > -1) {
    Sys.Browser.agent = Sys.Browser.Safari;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Safari\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Safari';
}
else if (navigator.userAgent.indexOf('Opera/') > -1) {
    Sys.Browser.agent = Sys.Browser.Opera;
}


Type.registerNamespace('Sys.UI');

Sys._Debug = function Sys$_Debug() {
    if (arguments.length !== 0) throw Error.parameterCount();
}


    function Sys$_Debug$_appendConsole(text) {
                if ((typeof(Debug) !== 'undefined') && Debug.writeln) {
            Debug.writeln(text);
        }
                if (window.console && window.console.log) {
            window.console.log(text);
        }
                if (window.opera) {
            window.opera.postError(text);
        }
                if (window.debugService) {
            window.debugService.trace(text);
        }
    }

    function Sys$_Debug$_appendTrace(text) {
        var traceElement = document.getElementById('TraceConsole');
        if (traceElement && (traceElement.tagName.toUpperCase() === 'TEXTAREA')) {
            traceElement.value += text + '\n';
        }
    }

    function Sys$_Debug$assert(condition, message, displayCaller) {
        /// <param name="condition" type="Boolean"></param>
        /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="displayCaller" type="Boolean" optional="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "condition", type: Boolean},
            {name: "message", type: String, mayBeNull: true, optional: true},
            {name: "displayCaller", type: Boolean, optional: true}
        ]);
        if (e) throw e;

        if (!condition) {
            message = (displayCaller && this.assert.caller) ?
                String.format(Sys.Res.assertFailedCaller, message, this.assert.caller) :
                String.format(Sys.Res.assertFailed, message);

            if (confirm(String.format(Sys.Res.breakIntoDebugger, message))) {
                this.fail(message);
            }
        }
    }

    function Sys$_Debug$clearTrace() {
        var traceElement = document.getElementById('TraceConsole');
        if (traceElement && (traceElement.tagName.toUpperCase() === 'TEXTAREA')) {
            traceElement.value = '';
        }
    }

    function Sys$_Debug$fail(message) {
        /// <param name="message" type="String" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "message", type: String, mayBeNull: true}
        ]);
        if (e) throw e;

        this._appendConsole(message);

                if (Sys.Browser.hasDebuggerStatement) {
            eval('debugger');
        }
    }

    function Sys$_Debug$trace(text) {
        /// <param name="text"></param>
        var e = Function._validateParams(arguments, [
            {name: "text"}
        ]);
        if (e) throw e;

        this._appendConsole(text);
        this._appendTrace(text);
    }

    function Sys$_Debug$traceDump(object, name) {
        /// <param name="object" mayBeNull="true"></param>
        /// <param name="name" type="String" mayBeNull="true" optional="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "object", mayBeNull: true},
            {name: "name", type: String, mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var text = this._traceDump(object, name, true);
    }

    function Sys$_Debug$_traceDump(object, name, recursive, indentationPadding, loopArray) {
        name = name? name : 'traceDump';
        indentationPadding = indentationPadding? indentationPadding : '';
        if (object === null) {
            this.trace(indentationPadding + name + ': null');
            return;
        }
        switch(typeof(object)) {
            case 'undefined':
                this.trace(indentationPadding + name + ': Undefined');
                break;
            case 'number': case 'string': case 'boolean':
                this.trace(indentationPadding + name + ': ' + object);
                break;
            default:
                if (Date.isInstanceOfType(object) || RegExp.isInstanceOfType(object)) {
                    this.trace(indentationPadding + name + ': ' + object.toString());
                    break;
                }
                if (!loopArray) {
                    loopArray = [];
                }
                else if (Array.contains(loopArray, object)) {
                    this.trace(indentationPadding + name + ': ...');
                    return;
                }
                Array.add(loopArray, object);

                                                                if ((object == window) || (object === document) ||
                    (window.HTMLElement && (object instanceof HTMLElement)) ||
                    (typeof(object.nodeName) === 'string')) {
                    var tag = object.tagName? object.tagName : 'DomElement';
                    if (object.id) {
                        tag += ' - ' + object.id;
                    }
                    this.trace(indentationPadding + name + ' {' +  tag + '}');
                }
                                else {
                    var typeName = Object.getTypeName(object);
                    this.trace(indentationPadding + name + (typeof(typeName) === 'string' ? ' {' + typeName + '}' : ''));
                    if ((indentationPadding === '') || recursive) {
                        indentationPadding += "    ";
                        var i, length, properties, p, v;
                        if (Array.isInstanceOfType(object)) {
                            length = object.length;
                            for (i = 0; i < length; i++) {
                                this._traceDump(object[i], '[' + i + ']', recursive, indentationPadding, loopArray);
                            }
                        }
                        else {
                            for (p in object) {
                                v = object[p];
                                if (!Function.isInstanceOfType(v)) {
                                    this._traceDump(v, p, recursive, indentationPadding, loopArray);
                                }
                            }
                        }
                    }
                }
                Array.remove(loopArray, object);
        }
    }
Sys._Debug.prototype = {

    _appendConsole: Sys$_Debug$_appendConsole,

    _appendTrace: Sys$_Debug$_appendTrace,

    assert: Sys$_Debug$assert,

    clearTrace: Sys$_Debug$clearTrace,

    fail: Sys$_Debug$fail,

    trace: Sys$_Debug$trace,

    traceDump: Sys$_Debug$traceDump,

    _traceDump: Sys$_Debug$_traceDump
}
Sys._Debug.registerClass('Sys._Debug');

Sys.Debug = new Sys._Debug();
    Sys.Debug.isDebug = true;
function Sys$Enum$parse(value, ignoreCase) {
    /// <param name="value" type="String"></param>
    /// <param name="ignoreCase" type="Boolean" optional="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String},
        {name: "ignoreCase", type: Boolean, optional: true}
    ]);
    if (e) throw e;

    var values, parsed, val;
    if (ignoreCase) {
        values = this.__lowerCaseValues;
        if (!values) {
            this.__lowerCaseValues = values = {};
            var prototype = this.prototype;
            for (var name in prototype) {
                values[name.toLowerCase()] = prototype[name];
            }
        }
    }
    else {
        values = this.prototype;
    }
    if (!this.__flags) {
        val = (ignoreCase ? value.toLowerCase() : value);
        parsed = values[val.trim()];
        if (typeof(parsed) !== 'number') throw Error.argument('value', String.format(Sys.Res.enumInvalidValue, value, this.__typeName));
        return parsed;
    }
    else {
        var parts = (ignoreCase ? value.toLowerCase() : value).split(',');
        var v = 0;

        for (var i = parts.length - 1; i >= 0; i--) {
            var part = parts[i].trim();
            parsed = values[part];
            if (typeof(parsed) !== 'number') throw Error.argument('value', String.format(Sys.Res.enumInvalidValue, value.split(',')[i].trim(), this.__typeName));
            v |= parsed;
        }
        return v;
    }
}

function Sys$Enum$toString(value) {
    /// <param name="value" optional="true" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

            if ((typeof(value) === 'undefined') || (value === null)) return this.__string;
    if ((typeof(value) != 'number') || ((value % 1) !== 0)) throw Error.argumentType('value', Object.getType(value), this);
    var values = this.prototype;
    var i;
    if (!this.__flags || (value === 0)) {
        for (i in values) {
            if (values[i] === value) {
                return i;
            }
        }
    }
    else {
        var sorted = this.__sortedValues;
        if (!sorted) {
            sorted = [];
            for (i in values) {
                sorted[sorted.length] = {key: i, value: values[i]};
            }
            sorted.sort(function(a, b) {
                return a.value - b.value;
            });
            this.__sortedValues = sorted;
        }
        var parts = [];
        var v = value;
        for (i = sorted.length - 1; i >= 0; i--) {
            var kvp = sorted[i];
            var vali = kvp.value;
            if (vali === 0) continue;
            if ((vali & value) === vali) {
                parts[parts.length] = kvp.key;
                v -= vali;
                if (v === 0) break;
            }
        }
        if (parts.length && v === 0) return parts.reverse().join(', ');
    }
    throw Error.argumentOutOfRange('value', value, String.format(Sys.Res.enumInvalidValue, value, this.__typeName));
}

Type.prototype.registerEnum = function Type$registerEnum(name, flags) {
    /// <param name="name" type="String"></param>
    /// <param name="flags" type="Boolean" optional="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "name", type: String},
        {name: "flags", type: Boolean, optional: true}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(name)) throw Error.argument('name', Sys.Res.notATypeName);
        var parsedName;
    try {
        parsedName = eval(name);
    }
    catch(e) {
        throw Error.argument('name', Sys.Res.argumentTypeName);
    }
    if (parsedName !== this) throw Error.argument('name', Sys.Res.badTypeName);
    if (window.__registeredTypes[name]) throw Error.invalidOperation(String.format(Sys.Res.typeRegisteredTwice, name));
    for (var i in this.prototype) {
        var val = this.prototype[i];
        if (!Type.__identifierRegExp.test(i)) throw Error.invalidOperation(String.format(Sys.Res.enumInvalidValueName, i));
        if (typeof(val) !== 'number' || (val % 1) !== 0) throw Error.invalidOperation(Sys.Res.enumValueNotInteger);
        if (typeof(this[i]) !== 'undefined') throw Error.invalidOperation(String.format(Sys.Res.enumReservedName, i));
    }
    for (var i in this.prototype) {
        this[i] = this.prototype[i];
    }
    this.__typeName = name;
    this.parse = Sys$Enum$parse;
    this.__string = this.toString();
    this.toString = Sys$Enum$toString;
    this.__flags = flags;
    this.__enum = true;
    window.__registeredTypes[name] = true;
}

Type.isEnum = function Type$isEnum(type) {
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
    return !!type.__enum;
}

Type.isFlags = function Type$isFlags(type) {
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
    return !!type.__flags;
}
Sys.EventHandlerList = function Sys$EventHandlerList() {
    if (arguments.length !== 0) throw Error.parameterCount();
    this._list = {};
}


    function Sys$EventHandlerList$addHandler(id, handler) {
        /// <param name="id" type="String"></param>
        /// <param name="handler" type="Function"></param>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String},
            {name: "handler", type: Function}
        ]);
        if (e) throw e;

        Array.add(this._getEvent(id, true), handler);
    }
    function Sys$EventHandlerList$removeHandler(id, handler) {
        /// <param name="id" type="String"></param>
        /// <param name="handler" type="Function"></param>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String},
            {name: "handler", type: Function}
        ]);
        if (e) throw e;

        var evt = this._getEvent(id);
        if (!evt) return;
        Array.remove(evt, handler);
    }
    function Sys$EventHandlerList$getHandler(id) {
        /// <param name="id" type="String"></param>
        /// <returns type="Function"></returns>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String}
        ]);
        if (e) throw e;

        var evt = this._getEvent(id);
        if (!evt || (evt.length === 0)) return null;
        evt = Array.clone(evt);
        if (!evt._handler) {
            evt._handler = function(source, args) {
                for (var i = 0, l = evt.length; i < l; i++) {
                    evt[i](source, args);
                }
            };
        }
        return evt._handler;
    }

    function Sys$EventHandlerList$_getEvent(id, create) {
        if (!this._list[id]) {
            if (!create) return null;
            this._list[id] = [];
        }
        return this._list[id];
    }
Sys.EventHandlerList.prototype = {
    addHandler: Sys$EventHandlerList$addHandler,
    removeHandler: Sys$EventHandlerList$removeHandler,
    getHandler: Sys$EventHandlerList$getHandler,

    _getEvent: Sys$EventHandlerList$_getEvent
}
Sys.EventHandlerList.registerClass('Sys.EventHandlerList');
Sys.EventArgs = function Sys$EventArgs() {
    if (arguments.length !== 0) throw Error.parameterCount();
}
Sys.EventArgs.registerClass('Sys.EventArgs');

Sys.EventArgs.Empty = new Sys.EventArgs();
Sys.CancelEventArgs = function Sys$CancelEventArgs() {
    if (arguments.length !== 0) throw Error.parameterCount();
    Sys.CancelEventArgs.initializeBase(this);

    this._cancel = false;
}


    function Sys$CancelEventArgs$get_cancel() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._cancel;
    }
    function Sys$CancelEventArgs$set_cancel(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        this._cancel = value;
    }
Sys.CancelEventArgs.prototype = {
    get_cancel: Sys$CancelEventArgs$get_cancel,
    set_cancel: Sys$CancelEventArgs$set_cancel
}

Sys.CancelEventArgs.registerClass('Sys.CancelEventArgs', Sys.EventArgs);
Sys.INotifyPropertyChange = function Sys$INotifyPropertyChange() {
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}

    function Sys$INotifyPropertyChange$add_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        throw Error.notImplemented();
    }
    function Sys$INotifyPropertyChange$remove_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        throw Error.notImplemented();
    }
Sys.INotifyPropertyChange.prototype = {
    add_propertyChanged: Sys$INotifyPropertyChange$add_propertyChanged,
    remove_propertyChanged: Sys$INotifyPropertyChange$remove_propertyChanged
}
Sys.INotifyPropertyChange.registerInterface('Sys.INotifyPropertyChange');
Sys.PropertyChangedEventArgs = function Sys$PropertyChangedEventArgs(propertyName) {
    /// <param name="propertyName" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "propertyName", type: String}
    ]);
    if (e) throw e;

    Sys.PropertyChangedEventArgs.initializeBase(this);
    this._propertyName = propertyName;
}
 
    function Sys$PropertyChangedEventArgs$get_propertyName() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._propertyName;
    }
Sys.PropertyChangedEventArgs.prototype = {
    get_propertyName: Sys$PropertyChangedEventArgs$get_propertyName
}
Sys.PropertyChangedEventArgs.registerClass('Sys.PropertyChangedEventArgs', Sys.EventArgs);
Sys.INotifyDisposing = function Sys$INotifyDisposing() {
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}

    function Sys$INotifyDisposing$add_disposing(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        throw Error.notImplemented();
    }
    function Sys$INotifyDisposing$remove_disposing(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        throw Error.notImplemented();
    }
Sys.INotifyDisposing.prototype = {
    add_disposing: Sys$INotifyDisposing$add_disposing,
    remove_disposing: Sys$INotifyDisposing$remove_disposing
}
Sys.INotifyDisposing.registerInterface("Sys.INotifyDisposing");
Sys.Component = function Sys$Component() {
    if (arguments.length !== 0) throw Error.parameterCount();
    if (!window.IsMultiForm && Sys.Application) {
		Sys.Application.registerDisposableObject(this);
		this._application = Sys.Application;
	}
}





    function Sys$Component$get_events() {
        /// <value type="Sys.EventHandlerList"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }
    function Sys$Component$get_id() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._id;
    }
    function Sys$Component$set_id(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        if (this._idSet) throw Error.invalidOperation(Sys.Res.componentCantSetIdTwice);
        this._idSet = true;
        var oldId = this.get_id();
        if (oldId && this._application.findComponent(oldId)) throw Error.invalidOperation(Sys.Res.componentCantSetIdAfterAddedToApp);
        this._id = value;
    }
    function Sys$Component$get_isInitialized() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._initialized;
    }
    function Sys$Component$get_isUpdating() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._updating;
    }
    function Sys$Component$add_disposing(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("disposing", handler);
    }
    function Sys$Component$remove_disposing(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("disposing", handler);
    }
    function Sys$Component$add_propertyChanged(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("propertyChanged", handler);
    }
    function Sys$Component$remove_propertyChanged(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("propertyChanged", handler);
    }
    function Sys$Component$beginUpdate() {
        this._updating = true;
    }
    function Sys$Component$dispose() {
        if (this._events) {
            var handler = this._events.getHandler("disposing");
            if (handler) {
                handler(this, Sys.EventArgs.Empty);
            }
        }
        delete this._events;
        this._application.unregisterDisposableObject(this);
        this._application.removeComponent(this);
    }
    function Sys$Component$endUpdate() {
        this._updating = false;
        if (!this._initialized) this.initialize();
        this.updated();
    }
    function Sys$Component$initialize() {
        this._initialized = true;
    }
    function Sys$Component$raisePropertyChanged(propertyName) {
        /// <param name="propertyName" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyName", type: String}
        ]);
        if (e) throw e;

        if (!this._events) return;
        var handler = this._events.getHandler("propertyChanged");
        if (handler) {
            handler(this, new Sys.PropertyChangedEventArgs(propertyName));
        }
    }
    function Sys$Component$updated() {
    }
Sys.Component.prototype = {
	_application: null,
    _id: null,
    _idSet: false,
    _initialized: false,
    _updating: false,
    get_events: Sys$Component$get_events,
    get_id: Sys$Component$get_id,
    set_id: Sys$Component$set_id,
    get_isInitialized: Sys$Component$get_isInitialized,
    get_isUpdating: Sys$Component$get_isUpdating,
    add_disposing: Sys$Component$add_disposing,
    remove_disposing: Sys$Component$remove_disposing,
    add_propertyChanged: Sys$Component$add_propertyChanged,
    remove_propertyChanged: Sys$Component$remove_propertyChanged,
    beginUpdate: Sys$Component$beginUpdate,
    dispose: Sys$Component$dispose,
    endUpdate: Sys$Component$endUpdate,
    initialize: Sys$Component$initialize,
    raisePropertyChanged: Sys$Component$raisePropertyChanged,
    updated: Sys$Component$updated
}
Sys.Component.registerClass('Sys.Component', null, Sys.IDisposable, Sys.INotifyPropertyChange, Sys.INotifyDisposing);

function Sys$Component$_setProperties(target, properties) {
    /// <param name="target"></param>
    /// <param name="properties"></param>
    var e = Function._validateParams(arguments, [
        {name: "target"},
        {name: "properties"}
    ]);
    if (e) throw e;

    var current;
    var targetType = Object.getType(target);
    var isObject = (targetType === Object) || (targetType === Sys.UI.DomElement);
    var isComponent = Sys.Component.isInstanceOfType(target) && !target.get_isUpdating();
    if (isComponent) target.beginUpdate();
    for (var name in properties) {
        var val = properties[name];
        var getter = isObject ? null : target["get_" + name];
        if (isObject || typeof(getter) !== 'function') {
                        var targetVal = target[name];
            if (!isObject && typeof(targetVal) === 'undefined') throw Error.invalidOperation(String.format(Sys.Res.propertyUndefined, name));
            if (!val || (typeof(val) !== 'object') || (isObject && !targetVal)) {
                target[name] = val;
            }
            else {
                Sys$Component$_setProperties(targetVal, val);
            }
        }
        else {
            var setter = target["set_" + name];
            if (typeof(setter) === 'function') {
                                setter.apply(target, [val]);
            }
            else if (val instanceof Array) {
                                current = getter.apply(target);
                if (!(current instanceof Array)) throw new Error.invalidOperation(String.format(Sys.Res.propertyNotAnArray, name));
                for (var i = 0, j = current.length, l= val.length; i < l; i++, j++) {
                    current[j] = val[i];
                }
            }
            else if ((typeof(val) === 'object') && (Object.getType(val) === Object)) {
                                current = getter.apply(target);
                if ((typeof(current) === 'undefined') || (current === null)) throw new Error.invalidOperation(String.format(Sys.Res.propertyNullOrUndefined, name));
                Sys$Component$_setProperties(current, val);
            }
            else {
                                throw new Error.invalidOperation(String.format(Sys.Res.propertyNotWritable, name));
            }
        }
    }
    if (isComponent) target.endUpdate();
}

function Sys$Component$_setReferences(component, references) {
    for (var name in references) {
        var setter = component["set_" + name];
        var reference = component._application.findComponent(references[name]);
        if (typeof(setter) !== 'function') throw new Error.invalidOperation(String.format(Sys.Res.propertyNotWritable, name));
        if (!reference) throw Error.invalidOperation(String.format(Sys.Res.referenceNotFound, references[name]));
        setter.apply(component, [reference]);
    }
}

var $create = Sys.Component.create = function Sys$Component$create(type, properties, events, references, element) {
    /// <param name="type" type="Type"></param>
    /// <param name="properties" optional="true" mayBeNull="true"></param>
    /// <param name="events" optional="true" mayBeNull="true"></param>
    /// <param name="references" optional="true" mayBeNull="true"></param>
    /// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Sys.UI.Component"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "properties", mayBeNull: true, optional: true},
        {name: "events", mayBeNull: true, optional: true},
        {name: "references", mayBeNull: true, optional: true},
        {name: "element", mayBeNull: true, domElement: true, optional: true}
    ]);
    if (e) throw e;
    
    return Sys.Component._createInternal(Sys.Application, type, properties, events, references, element);
}

Sys.Component._createInternal = function Sys$Component$_createInternal(app, type, properties, events, references, element) {

    if (!type.inheritsFrom(Sys.Component)) {
        throw Error.argument('type', String.format(Sys.Res.createNotComponent, type.getName()));
    }
    if (type.inheritsFrom(Sys.UI.Behavior) || type.inheritsFrom(Sys.UI.Control)) {
        if (!element) throw Error.argument('element', Sys.Res.createNoDom);
    }
    else if (element) throw Error.argument('element', Sys.Res.createComponentOnDom);
    var component = (element ? new type(element): new type());
    if(window.IsMultiForm)
		component.registerWithApplication(app);
    var creatingComponents = app.get_isCreatingComponents();

    component.beginUpdate();
    if (properties) {
        Sys$Component$_setProperties(component, properties);
    }
    if (events) {
        for (var name in events) {
            if (!(component["add_" + name] instanceof Function)) throw new Error.invalidOperation(String.format(Sys.Res.undefinedEvent, name));
            if (!(events[name] instanceof Function)) throw new Error.invalidOperation(Sys.Res.eventHandlerNotFunction);
            component["add_" + name](events[name]);
        }
    }

    app._createdComponents[app._createdComponents.length] = component;
    if (component.get_id()) {
        app.addComponent(component);
    }
    if (creatingComponents) {
        if (references) {
            app._addComponentToSecondPass(component, references);
        }
        else {
            component.endUpdate();
        }
    }
    else {
        if (references) {
            Sys$Component$_setReferences(component, references);
        }
        component.endUpdate();
    }

    return component;
}
Sys.UI.MouseButton = function Sys$UI$MouseButton() {
    /// <field name="leftButton" type="Number" integer="true" static="true"></field>
    /// <field name="middleButton" type="Number" integer="true" static="true"></field>
    /// <field name="rightButton" type="Number" integer="true" static="true"></field>
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}




Sys.UI.MouseButton.prototype = {
    leftButton: 0,
    middleButton: 1,
    rightButton: 2
}
Sys.UI.MouseButton.registerEnum("Sys.UI.MouseButton");
Sys.UI.Key = function Sys$UI$Key() {
    /// <field name="backspace" type="Number" integer="true" static="true"></field>
    /// <field name="tab" type="Number" integer="true" static="true"></field>
    /// <field name="enter" type="Number" integer="true" static="true"></field>
    /// <field name="esc" type="Number" integer="true" static="true"></field>
    /// <field name="space" type="Number" integer="true" static="true"></field>
    /// <field name="pageUp" type="Number" integer="true" static="true"></field>
    /// <field name="pageDown" type="Number" integer="true" static="true"></field>
    /// <field name="end" type="Number" integer="true" static="true"></field>
    /// <field name="home" type="Number" integer="true" static="true"></field>
    /// <field name="left" type="Number" integer="true" static="true"></field>
    /// <field name="up" type="Number" integer="true" static="true"></field>
    /// <field name="right" type="Number" integer="true" static="true"></field>
    /// <field name="down" type="Number" integer="true" static="true"></field>
    /// <field name="del" type="Number" integer="true" static="true"></field>
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}















Sys.UI.Key.prototype = {
    backspace: 8,
    tab: 9,
    enter: 13,
    esc: 27,
    space: 32,
    pageUp: 33,
    pageDown: 34,
    end: 35,
    home: 36,
    left: 37,
    up: 38,
    right: 39,
    down: 40,
    del: 127
}
Sys.UI.Key.registerEnum("Sys.UI.Key");
Sys.UI.DomEvent = function Sys$UI$DomEvent(eventObject) {
    /// <param name="eventObject"></param>
    /// <field name="altKey" type="Boolean"></field>
    /// <field name="button" type="Sys.UI.MouseButton"></field>
    /// <field name="charCode" type="Number" integer="true"></field>
    /// <field name="clientX" type="Number" integer="true"></field>
    /// <field name="clientY" type="Number" integer="true"></field>
    /// <field name="ctrlKey" type="Boolean"></field>
    /// <field name="keyCode" type="Number" integer="true"></field>
    /// <field name="offsetX" type="Number" integer="true"></field>
    /// <field name="offsetY" type="Number" integer="true"></field>
    /// <field name="screenX" type="Number" integer="true"></field>
    /// <field name="screenY" type="Number" integer="true"></field>
    /// <field name="shiftKey" type="Boolean"></field>
    /// <field name="target" domElement="true"></field>
    /// <field name="type" type="String"></field>
    var e = Function._validateParams(arguments, [
        {name: "eventObject"}
    ]);
    if (e) throw e;

    var e = eventObject;
    this.rawEvent = e;
    this.altKey = e.altKey;
    if (typeof(e.button) !== 'undefined') {
        this.button = (typeof(e.which) !== 'undefined') ? e.button :
            (e.button === 4) ? Sys.UI.MouseButton.middleButton :
            (e.button === 2) ? Sys.UI.MouseButton.rightButton :
            Sys.UI.MouseButton.leftButton;
    }
    if (e.type === 'keypress') {
        this.charCode = e.charCode || e.keyCode;
    }
    else if (e.keyCode && (e.keyCode === 46)) {
        this.keyCode = 127;
    }
    else {
        this.keyCode = e.keyCode;
    }
    this.clientX = e.clientX;
    this.clientY = e.clientY;
    this.ctrlKey = e.ctrlKey;
    this.target = e.target ? e.target : e.srcElement;
    if (this.target) {
        var loc = Sys.UI.DomElement.getLocation(this.target);
        this.offsetX = (typeof(e.offsetX) !== 'undefined') ? e.offsetX : window.pageXOffset + (e.clientX || 0) - loc.x;
        this.offsetY = (typeof(e.offsetY) !== 'undefined') ? e.offsetY : window.pageYOffset + (e.clientY || 0) - loc.y;
    }
    this.screenX = e.screenX;
    this.screenY = e.screenY;
    this.shiftKey = e.shiftKey;
    this.type = e.type;
}

    function Sys$UI$DomEvent$preventDefault() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this.rawEvent.preventDefault) {
            this.rawEvent.preventDefault();
        }
        else if (window.event) {
            window.event.returnValue = false;
        }
    }
    function Sys$UI$DomEvent$stopPropagation() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this.rawEvent.stopPropagation) {
            this.rawEvent.stopPropagation();
        }
        else if (window.event) {
            window.event.cancelBubble = true;
        }
    }
Sys.UI.DomEvent.prototype = {
    preventDefault: Sys$UI$DomEvent$preventDefault,
    stopPropagation: Sys$UI$DomEvent$stopPropagation
}
Sys.UI.DomEvent.registerClass('Sys.UI.DomEvent');

var $addHandler = Sys.UI.DomEvent.addHandler = function Sys$UI$DomEvent$addHandler(element, eventName, handler) {
    /// <param name="element" domElement="true"></param>
    /// <param name="eventName" type="String"></param>
    /// <param name="handler" type="Function"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "eventName", type: String},
        {name: "handler", type: Function}
    ]);
    if (e) throw e;

    if (!element._events) {
        element._events = {};
    }
    var eventCache = element._events[eventName];
    if (!eventCache) {
        element._events[eventName] = eventCache = [];
    }
    var browserHandler;
    if (element.addEventListener) {
        browserHandler = function(e) {
            return handler.call(element, new Sys.UI.DomEvent(e));
        }
        element.addEventListener(eventName, browserHandler, false);
    }
    else if (element.attachEvent) {
        browserHandler = function() {
            return handler.call(element, new Sys.UI.DomEvent(window.event));
        }
        element.attachEvent('on' + eventName, browserHandler);
    }
    eventCache[eventCache.length] = {handler: handler, browserHandler: browserHandler};
}

var $addHandlers = Sys.UI.DomEvent.addHandlers = function Sys$UI$DomEvent$addHandlers(element, events, handlerOwner) {
    /// <param name="element" domElement="true"></param>
    /// <param name="events" type="Object"></param>
    /// <param name="handlerOwner" optional="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "events", type: Object},
        {name: "handlerOwner", optional: true}
    ]);
    if (e) throw e;

    for (var name in events) {
        var handler = events[name];
        if (typeof(handler) !== 'function') throw Error.invalidOperation(Sys.Res.cantAddNonFunctionhandler);
        if (handlerOwner) {
            handler = Function.createDelegate(handlerOwner, handler);
        }
        $addHandler(element, name, handler);
    }
}

var $clearHandlers = Sys.UI.DomEvent.clearHandlers = function Sys$UI$DomEvent$clearHandlers(element) {
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    if (element._events) {
        var cache = element._events;
        for (var name in cache) {
            var handlers = cache[name];
            for (var i = handlers.length - 1; i >= 0; i--) {
                $removeHandler(element, name, handlers[i].handler);
            }
        }
        element._events = null;
    }
}

var $removeHandler = Sys.UI.DomEvent.removeHandler = function Sys$UI$DomEvent$removeHandler(element, eventName, handler) {
    /// <param name="element" domElement="true"></param>
    /// <param name="eventName" type="String"></param>
    /// <param name="handler" type="Function"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "eventName", type: String},
        {name: "handler", type: Function}
    ]);
    if (e) throw e;

    var browserHandler = null;
    if ((typeof(element._events) !== 'object') || (element._events == null)) throw Error.invalidOperation(Sys.Res.eventHandlerInvalid);
    var cache = element._events[eventName];
    if (!(cache instanceof Array)) throw Error.invalidOperation(Sys.Res.eventHandlerInvalid);
    var browserHandler = null;
    for (var i = 0, l = cache.length; i < l; i++) {
        if (cache[i].handler === handler) {
            browserHandler = cache[i].browserHandler;
            break;
        }
    }
    if (typeof(browserHandler) !== 'function') throw Error.invalidOperation(Sys.Res.eventHandlerInvalid);
    if (element.removeEventListener) {
        element.removeEventListener(eventName, browserHandler, false);
    }
    else if (element.detachEvent) {
        element.detachEvent('on' + eventName, browserHandler);
    }
    cache.splice(i, 1);
}
Sys.IContainer = function Sys$IContainer() {
    throw Error.notImplemented();
}

    function Sys$IContainer$addComponent(component) {
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

        throw Error.notImplemented();
    }
    function Sys$IContainer$removeComponent(component) {
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

        throw Error.notImplemented();
    }
    function Sys$IContainer$findComponent(id) {
        /// <param name="id" type="String"></param>
        /// <returns type="Sys.Component"></returns>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String}
        ]);
        if (e) throw e;

        throw Error.notImplemented();
    }
    function Sys$IContainer$getComponents() {
        /// <returns type="Array" elementType="Sys.Component"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
Sys.IContainer.prototype = {
    addComponent: Sys$IContainer$addComponent,
    removeComponent: Sys$IContainer$removeComponent,
    findComponent: Sys$IContainer$findComponent,
    getComponents: Sys$IContainer$getComponents
}
Sys.IContainer.registerInterface("Sys.IContainer");


Sys._ScriptLoader = function Sys$_ScriptLoader() {
    this._scriptsToLoad = null;
    this._scriptLoadedDelegate = Function.createDelegate(this, this._scriptLoadedHandler);
}

    function Sys$_ScriptLoader$dispose() {
        this._stopLoading();
        if(this._events) {
            delete this._events;
        }
        this._scriptLoadedDelegate = null;        
    }

    function Sys$_ScriptLoader$loadScripts(scriptTimeout, allScriptsLoadedCallback, scriptLoadFailedCallback, scriptLoadTimeoutCallback) {
        /// <param name="scriptTimeout" type="Number" integer="true"></param>
        /// <param name="allScriptsLoadedCallback" type="Function" mayBeNull="true"></param>
        /// <param name="scriptLoadFailedCallback" type="Function" mayBeNull="true"></param>
        /// <param name="scriptLoadTimeoutCallback" type="Function" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptTimeout", type: Number, integer: true},
            {name: "allScriptsLoadedCallback", type: Function, mayBeNull: true},
            {name: "scriptLoadFailedCallback", type: Function, mayBeNull: true},
            {name: "scriptLoadTimeoutCallback", type: Function, mayBeNull: true}
        ]);
        if (e) throw e;

        if(this._loading) {
            throw Error.invalidOperation(Sys.Res.scriptLoaderAlreadyLoading);
        }
        this._loading = true;
        this._allScriptsLoadedCallback = allScriptsLoadedCallback;
        this._scriptLoadFailedCallback = scriptLoadFailedCallback;
        this._scriptLoadTimeoutCallback = scriptLoadTimeoutCallback;
        
        this._loadScriptsInternal();
    }

    function Sys$_ScriptLoader$notifyScriptLoaded() {
        if (arguments.length !== 0) throw Error.parameterCount();
        
                        if(!this._loading) {
                                    return;
        }

        this._currentTask._notified++;
        
        if(Sys.Browser.agent === Sys.Browser.Safari) {           
            if(this._currentTask._notified === 1) {
                                                                                                                window.setTimeout(Function.createDelegate(this, function() {
                    this._scriptLoadedHandler(this._currentTask.get_scriptElement(), true);
                }), 0);
            }
        }

                            }

    function Sys$_ScriptLoader$queueCustomScriptTag(scriptAttributes) {
        /// <param name="scriptAttributes" mayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptAttributes"}
        ]);
        if (e) throw e;

        if(!this._scriptsToLoad) {
            this._scriptsToLoad = [];
        }
        Array.add(this._scriptsToLoad, scriptAttributes);
    }

    function Sys$_ScriptLoader$queueScriptBlock(scriptContent) {
        /// <param name="scriptContent" type="String" mayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptContent", type: String}
        ]);
        if (e) throw e;

        if(!this._scriptsToLoad) {
            this._scriptsToLoad = [];
        }
        Array.add(this._scriptsToLoad, {text: scriptContent});
    }

    function Sys$_ScriptLoader$queueScriptReference(scriptUrl) {
        /// <param name="scriptUrl" type="String" mayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptUrl", type: String}
        ]);
        if (e) throw e;

        if(!this._scriptsToLoad) {
            this._scriptsToLoad = [];
        }
        Array.add(this._scriptsToLoad, {src: scriptUrl});
    }

    function Sys$_ScriptLoader$_createScriptElement(queuedScript) {
        var scriptElement = document.createElement('SCRIPT');

                        scriptElement.type = 'text/javascript';

                for (var attr in queuedScript) {
            scriptElement[attr] = queuedScript[attr];
        }
        
        return scriptElement;
    }

    function Sys$_ScriptLoader$_loadScriptsInternal() {
                if (this._scriptsToLoad && this._scriptsToLoad.length > 0) {
            var nextScript = Array.dequeue(this._scriptsToLoad);
                        var scriptElement = this._createScriptElement(nextScript);
            
            if (scriptElement.text && Sys.Browser.agent === Sys.Browser.Safari) {
                                scriptElement.innerHTML = scriptElement.text;
                delete scriptElement.text;
            }            

                                                            if (typeof(nextScript.src) === "string") {
                                this._currentTask = new Sys._ScriptLoaderTask(scriptElement, this._scriptLoadedDelegate);
                                                                                this._currentTask.execute();
            }
            else {
                                                document.getElementsByTagName('HEAD')[0].appendChild(scriptElement);
                
                                Sys._ScriptLoader._clearScript(scriptElement);

                                                                this._loadScriptsInternal();
            }
        }
        else {
                        var callback = this._allScriptsLoadedCallback;
            this._stopLoading();
            if(callback) {
                callback(this);
            }
        }
    }

    function Sys$_ScriptLoader$_raiseError(multipleCallbacks) {
                var callback = this._scriptLoadFailedCallback;
        var scriptElement = this._currentTask.get_scriptElement();
        this._stopLoading();
        
        if(callback) {
            callback(this, scriptElement, multipleCallbacks);
        }
        else {
            throw Sys._ScriptLoader._errorScriptLoadFailed(scriptElement.src, multipleCallbacks);
        }
    }

    function Sys$_ScriptLoader$_scriptLoadedHandler(scriptElement, loaded) {
                                if(loaded && this._currentTask._notified) {
            if(this._currentTask._notified > 1) {
                                this._raiseError(true);
            }
            else {
                                Array.add(Sys._ScriptLoader._getLoadedScripts(), scriptElement.src);
                this._currentTask.dispose();
                this._currentTask = null;
                this._loadScriptsInternal();
            }
        }
        else {
                        this._raiseError(false);
        }
    }

    function Sys$_ScriptLoader$_scriptLoadTimeoutHandler() {
        var callback = this._scriptLoadTimeoutCallback;
        this._stopLoading();

        if(callback) {
            callback(this);
        }
    }

    function Sys$_ScriptLoader$_stopLoading() {
        if(this._timeoutCookie) {
            window.clearTimeout(this._timeoutCookie);
            this._timeoutCookie = null;
        }

        if(this._currentTask) {
            this._currentTask.dispose();
            this._currentTask = null;
        }

        this._scriptsToLoad = null;
        this._loading = null;
        
        this._allScriptsLoadedCallback = null;
        this._scriptLoadFailedCallback = null;
        this._scriptLoadTimeoutCallback = null;
    }
Sys._ScriptLoader.prototype = {
    dispose: Sys$_ScriptLoader$dispose,
    
    loadScripts: Sys$_ScriptLoader$loadScripts,
    
    notifyScriptLoaded: Sys$_ScriptLoader$notifyScriptLoaded,
    
    queueCustomScriptTag: Sys$_ScriptLoader$queueCustomScriptTag,

    queueScriptBlock: Sys$_ScriptLoader$queueScriptBlock,

    queueScriptReference: Sys$_ScriptLoader$queueScriptReference,
    
    _createScriptElement: Sys$_ScriptLoader$_createScriptElement,   

    _loadScriptsInternal: Sys$_ScriptLoader$_loadScriptsInternal,
    
    _raiseError: Sys$_ScriptLoader$_raiseError,
    
    _scriptLoadedHandler: Sys$_ScriptLoader$_scriptLoadedHandler,
    
    _scriptLoadTimeoutHandler: Sys$_ScriptLoader$_scriptLoadTimeoutHandler,
    
    _stopLoading: Sys$_ScriptLoader$_stopLoading    
}
Sys._ScriptLoader.registerClass('Sys._ScriptLoader', null, Sys.IDisposable);

Sys._ScriptLoader.getInstance = function Sys$_ScriptLoader$getInstance() {
    var sl = Sys._ScriptLoader._activeInstance;
    if(!sl) {
        sl = Sys._ScriptLoader._activeInstance = new Sys._ScriptLoader();
    }
    return sl;
}

Sys._ScriptLoader.isScriptLoaded = function Sys$_ScriptLoader$isScriptLoaded(scriptSrc) {
                    var dummyScript = document.createElement('script');
    dummyScript.src = scriptSrc;
    return Array.contains(Sys._ScriptLoader._getLoadedScripts(), dummyScript.src);
}

Sys._ScriptLoader.readLoadedScripts = function Sys$_ScriptLoader$readLoadedScripts() {
        if(!Sys._ScriptLoader._referencedScripts) {
        var referencedScripts = Sys._ScriptLoader._referencedScripts = [];

        var existingScripts = document.getElementsByTagName('SCRIPT');
        for (i = existingScripts.length - 1; i >= 0; i--) {
            var scriptNode = existingScripts[i];
            var scriptSrc = scriptNode.src;
            if (scriptSrc.length) {
                if (!Array.contains(referencedScripts, scriptSrc)) {
                    Array.add(referencedScripts, scriptSrc);
                }
            }
        }
    }
}

Sys._ScriptLoader._clearScript = function Sys$_ScriptLoader$_clearScript(scriptElement) {
    if (!Sys.Debug.isDebug) {
                        scriptElement.parentNode.removeChild(scriptElement);
    }
}

Sys._ScriptLoader._errorScriptLoadFailed = function Sys$_ScriptLoader$_errorScriptLoadFailed(scriptUrl, multipleCallbacks) {
    var errorMessage;
    if(multipleCallbacks) {
        errorMessage = Sys.Res.scriptLoadMultipleCallbacks;
    }
    else {
                errorMessage = Sys.Res.scriptLoadFailedDebug;
    }

    var displayMessage = "Sys.ScriptLoadFailedException: " + String.format(errorMessage, scriptUrl);
    var e = Error.create(displayMessage, {name: 'Sys.ScriptLoadFailedException', 'scriptUrl': scriptUrl });
    e.popStackFrame();
    return e;
}

Sys._ScriptLoader._getLoadedScripts = function Sys$_ScriptLoader$_getLoadedScripts() {
    if(!Sys._ScriptLoader._referencedScripts) {
        Sys._ScriptLoader._referencedScripts = [];
        Sys._ScriptLoader.readLoadedScripts();
    }
    return Sys._ScriptLoader._referencedScripts;
}


Sys._ScriptLoaderTask = function Sys$_ScriptLoaderTask(scriptElement, completedCallback) {
    /// <param name="scriptElement" domElement="true"></param>
    /// <param name="completedCallback" type="Function"></param>
    var e = Function._validateParams(arguments, [
        {name: "scriptElement", domElement: true},
        {name: "completedCallback", type: Function}
    ]);
    if (e) throw e;

    this._scriptElement = scriptElement;
    this._completedCallback = completedCallback;
    this._notified = 0;
}

    function Sys$_ScriptLoaderTask$get_scriptElement() {
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._scriptElement;
    }

    function Sys$_ScriptLoaderTask$dispose() {
                if(this._disposed) {
                        return;
        }
        this._disposed = true;
        this._removeScriptElementHandlers();
                Sys._ScriptLoader._clearScript(this._scriptElement);
        this._scriptElement = null;
    }

    function Sys$_ScriptLoaderTask$execute() {
        if (arguments.length !== 0) throw Error.parameterCount();
        this._addScriptElementHandlers();
        document.getElementsByTagName('HEAD')[0].appendChild(this._scriptElement);
    }

    function Sys$_ScriptLoaderTask$_addScriptElementHandlers() {
                this._scriptLoadDelegate = Function.createDelegate(this, this._scriptLoadHandler);
        
        if (Sys.Browser.agent !== Sys.Browser.InternetExplorer) {
            this._scriptElement.readyState = 'loaded';
            $addHandler(this._scriptElement, 'load', this._scriptLoadDelegate);
        }
        else {
            $addHandler(this._scriptElement, 'readystatechange', this._scriptLoadDelegate);
        }    
                        this._scriptErrorDelegate = Function.createDelegate(this, this._scriptErrorHandler);
        $addHandler(this._scriptElement, 'error', this._scriptErrorDelegate);
    }

    function Sys$_ScriptLoaderTask$_removeScriptElementHandlers() {
                if(this._scriptLoadDelegate) {
            var scriptElement = this.get_scriptElement();
            if (Sys.Browser.agent !== Sys.Browser.InternetExplorer) {
                $removeHandler(scriptElement, 'load', this._scriptLoadDelegate);
            }
            else {
                $removeHandler(scriptElement, 'readystatechange', this._scriptLoadDelegate);
            }
            $removeHandler(scriptElement, 'error', this._scriptErrorDelegate);
            this._scriptErrorDelegate = null;
            this._scriptLoadDelegate = null;
        }
    }

    function Sys$_ScriptLoaderTask$_scriptErrorHandler() {
                if(this._disposed) {
            return;
        }
        
                this._completedCallback(this.get_scriptElement(), false);
    }

    function Sys$_ScriptLoaderTask$_scriptLoadHandler() {
                if(this._disposed) {
            return;
        }

        var scriptElement = this.get_scriptElement();
        if ((scriptElement.readyState !== 'loaded') &&
            (scriptElement.readyState !== 'complete')) {
            return;
        }
        
                                        var _this = this;
        window.setTimeout(function() {
            _this._completedCallback(scriptElement, true);
        }, 0);
    }
Sys._ScriptLoaderTask.prototype = {
    get_scriptElement: Sys$_ScriptLoaderTask$get_scriptElement,
    
    dispose: Sys$_ScriptLoaderTask$dispose,
        
    execute: Sys$_ScriptLoaderTask$execute,
       
    _addScriptElementHandlers: Sys$_ScriptLoaderTask$_addScriptElementHandlers,    
    
    _removeScriptElementHandlers: Sys$_ScriptLoaderTask$_removeScriptElementHandlers,    

    _scriptErrorHandler: Sys$_ScriptLoaderTask$_scriptErrorHandler,
           
    _scriptLoadHandler: Sys$_ScriptLoaderTask$_scriptLoadHandler  
}
Sys._ScriptLoaderTask.registerClass("Sys._ScriptLoaderTask", null, Sys.IDisposable);
Sys.ApplicationLoadEventArgs = function Sys$ApplicationLoadEventArgs(components, isPartialLoad) {
    /// <param name="components" type="Array" elementType="Sys.Component"></param>
    /// <param name="isPartialLoad" type="Boolean"></param>
    var e = Function._validateParams(arguments, [
        {name: "components", type: Array, elementType: Sys.Component},
        {name: "isPartialLoad", type: Boolean}
    ]);
    if (e) throw e;

    Sys.ApplicationLoadEventArgs.initializeBase(this);
    this._components = components;
    this._isPartialLoad = isPartialLoad;
}
 
    function Sys$ApplicationLoadEventArgs$get_components() {
        /// <value type="Array" elementType="Sys.Component"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._components;
    }
    function Sys$ApplicationLoadEventArgs$get_isPartialLoad() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._isPartialLoad;
    }
Sys.ApplicationLoadEventArgs.prototype = {
    get_components: Sys$ApplicationLoadEventArgs$get_components,
    get_isPartialLoad: Sys$ApplicationLoadEventArgs$get_isPartialLoad
}
Sys.ApplicationLoadEventArgs.registerClass('Sys.ApplicationLoadEventArgs', Sys.EventArgs);
Sys._Application = function Sys$_Application() {
    Sys._Application.initializeBase(this);

	this._application = this;
    this._disposableObjects = [];
    this._components = {};
    this._createdComponents = [];
    this._secondPassComponents = [];

    this._unloadHandlerDelegate = Function.createDelegate(this, this._unloadHandler);
    this._loadHandlerDelegate = Function.createDelegate(this, this._loadHandler);

    Sys.UI.DomEvent.addHandler(window, "unload", this._unloadHandlerDelegate);
    Sys.UI.DomEvent.addHandler(window, "load", this._loadHandlerDelegate);
}




    function Sys$_Application$get_isCreatingComponents() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._creatingComponents;
    }
    function Sys$_Application$add_load(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("load", handler);
    }
    function Sys$_Application$remove_load(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("load", handler);
    }
    function Sys$_Application$add_init(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        if (this._initialized) {
            handler(this, Sys.EventArgs.Empty);
        }
        else {
            this.get_events().addHandler("init", handler);
        }
    }
    function Sys$_Application$remove_init(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("init", handler);
    }
    function Sys$_Application$add_unload(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("unload", handler);
    }
    function Sys$_Application$remove_unload(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("unload", handler);
    }
    function Sys$_Application$addComponent(component) {
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

        var id = component.get_id();
        if (!id) throw Error.invalidOperation(Sys.Res.cantAddWithoutId);
        if (typeof(this._components[id]) !== 'undefined') throw Error.invalidOperation(String.format(Sys.Res.appDuplicateComponent, id));
        this._components[id] = component;
    }
    function Sys$_Application$beginCreateComponents() {
        this._creatingComponents = true;
    }
    function Sys$_Application$dispose() {
        if (!this._disposing) {
            this._disposing = true;
            if (window.pageUnload) {
                window.pageUnload(this, Sys.EventArgs.Empty);
            }
            var unloadHandler = this.get_events().getHandler("unload");
            if (unloadHandler) {
                unloadHandler(this, Sys.EventArgs.Empty);
            }
            var disposableObjects = Array.clone(this._disposableObjects);
            for (var i = 0, l = disposableObjects.length; i < l; i++) {
                disposableObjects[i].dispose();
            }
            Array.clear(this._disposableObjects);

            Sys.UI.DomEvent.removeHandler(window, "unload", this._unloadHandlerDelegate);
            if(this._loadHandlerDelegate) {
                Sys.UI.DomEvent.removeHandler(window, "load", this._loadHandlerDelegate);
                this._loadHandlerDelegate = null;
            }

            var sl = Sys._ScriptLoader.getInstance();
            if(sl) {
                sl.dispose();
            }

            Sys._Application.callBaseMethod(this, 'dispose');
        }
    }
    function Sys$_Application$endCreateComponents() {
        var components = this._secondPassComponents;
        for (var i = 0, l = components.length; i < l; i++) {
            var component = components[i].component;
            Sys$Component$_setReferences(component, components[i].references);
            component.endUpdate();
        }
        this._secondPassComponents = [];
        this._creatingComponents = false;
    }
    function Sys$_Application$findComponent(id, parent) {
        /// <param name="id" type="String"></param>
        /// <param name="parent" optional="true" mayBeNull="true"></param>
        /// <returns type="Sys.Component" mayBeNull="true"></returns>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String},
            {name: "parent", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

                        return (parent ?
            ((Sys.IContainer.isInstanceOfType(parent)) ?
                parent.findComponent(id) :
                parent[id] || null) :
            this._components[id] || null);
    }
    function Sys$_Application$getComponents() {
        /// <returns type="Array" elementType="Sys.Component"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        var res = [];
        var components = this._components;
        for (var name in components) {
            res[res.length] = components[name];
        }
        return res;
    }
    function Sys$_Application$initialize() {
        if(!this._initialized && !this._initializing) {
            this._initializing = true;
                                                window.setTimeout(Function.createDelegate(this, this._doInitialize), 0);
        }
    }
    function Sys$_Application$notifyScriptLoaded() {
        if (arguments.length !== 0) throw Error.parameterCount();
        var sl = Sys._ScriptLoader.getInstance();
        if(sl) {
            sl.notifyScriptLoaded();
        }
    }
    function Sys$_Application$registerDisposableObject(object) {
        /// <param name="object" type="Sys.IDisposable"></param>
        var e = Function._validateParams(arguments, [
            {name: "object", type: Sys.IDisposable}
        ]);
        if (e) throw e;

        if (!this._disposing) {
            this._disposableObjects[this._disposableObjects.length] = object;
        }
    }
    function Sys$_Application$raiseLoad() {
        var h = this.get_events().getHandler("load");
        var args = new Sys.ApplicationLoadEventArgs(Array.clone(this._createdComponents), !this._initializing);
        if (h) {
            h(this, args);
        }

        if (window.pageLoad) {
            window.pageLoad(this, args);
        }
        this._createdComponents = [];
    }
    function Sys$_Application$removeComponent(component) {
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

        var id = component.get_id();
        if (id) delete this._components[id];
    }
    function Sys$_Application$unregisterDisposableObject(object) {
        /// <param name="object" type="Sys.IDisposable"></param>
        var e = Function._validateParams(arguments, [
            {name: "object", type: Sys.IDisposable}
        ]);
        if (e) throw e;

        if (!this._disposing) {
            Array.remove(this._disposableObjects, object);
        }
    }
    function Sys$_Application$_addComponentToSecondPass(component, references) {
        this._secondPassComponents[this._secondPassComponents.length] = {component: component, references: references};
    }
    function Sys$_Application$_doInitialize() {
        Sys._Application.callBaseMethod(this, 'initialize');

        var handler = this.get_events().getHandler("init");
        if (handler) {
            this.beginCreateComponents();
            handler(this, Sys.EventArgs.Empty);
            this.endCreateComponents();
        }
        this.raiseLoad();
        this._initializing = false;
    }
    function Sys$_Application$_loadHandler() {
                        if(this._loadHandlerDelegate) {
            Sys.UI.DomEvent.removeHandler(window, "load", this._loadHandlerDelegate);
            this._loadHandlerDelegate = null;
        }
        this.initialize();
    }
    function Sys$_Application$_unloadHandler(event) {
        this.dispose();
    }
Sys._Application.prototype = {
    _creatingComponents: false,
    _disposing: false,

    get_isCreatingComponents: Sys$_Application$get_isCreatingComponents,
    add_load: Sys$_Application$add_load,
    remove_load: Sys$_Application$remove_load,
    add_init: Sys$_Application$add_init,
    remove_init: Sys$_Application$remove_init,
    add_unload: Sys$_Application$add_unload,
    remove_unload: Sys$_Application$remove_unload,
    addComponent: Sys$_Application$addComponent,
    beginCreateComponents: Sys$_Application$beginCreateComponents,
    dispose: Sys$_Application$dispose,
    endCreateComponents: Sys$_Application$endCreateComponents,
    findComponent: Sys$_Application$findComponent,
    getComponents: Sys$_Application$getComponents,
    initialize: Sys$_Application$initialize,
    notifyScriptLoaded: Sys$_Application$notifyScriptLoaded,
    registerDisposableObject: Sys$_Application$registerDisposableObject,
    raiseLoad: Sys$_Application$raiseLoad,
    removeComponent: Sys$_Application$removeComponent,
    unregisterDisposableObject: Sys$_Application$unregisterDisposableObject,
    _addComponentToSecondPass: Sys$_Application$_addComponentToSecondPass,
    _doInitialize: Sys$_Application$_doInitialize,    
    _loadHandler: Sys$_Application$_loadHandler,
    _unloadHandler: Sys$_Application$_unloadHandler
}
Sys._Application.registerClass('Sys._Application', Sys.Component, Sys.IContainer);

Sys.Application = new Sys._Application();

var $find = Function.createDelegate(Sys.Application, Sys.Application.findComponent);

Type.registerNamespace('Sys.Net');

Sys.Net.WebRequestExecutor = function Sys$Net$WebRequestExecutor() {
    if (arguments.length !== 0) throw Error.parameterCount();
    this._webRequest = null;
    this._resultObject = null;
}


    function Sys$Net$WebRequestExecutor$get_webRequest() {
        /// <value type="Sys.Net.WebRequest"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._webRequest;
    }

    function Sys$Net$WebRequestExecutor$_set_webRequest(value) {
        if (this.get_started()) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOnceStarted, 'set_webRequest'));
        }

        this._webRequest = value;
    }


    function Sys$Net$WebRequestExecutor$get_started() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }

    function Sys$Net$WebRequestExecutor$get_responseAvailable() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }

    function Sys$Net$WebRequestExecutor$get_timedOut() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_aborted() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_responseData() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_statusCode() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_statusText() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_xml() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_object() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._resultObject) {
            this._resultObject = Sys.Serialization.JavaScriptSerializer.deserialize(this.get_responseData());
        }
        return this._resultObject;
    }


    function Sys$Net$WebRequestExecutor$executeRequest() {
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$abort() {
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$getResponseHeader(header) {
        /// <param name="header" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "header", type: String}
        ]);
        if (e) throw e;

        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$getAllResponseHeaders() {
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
Sys.Net.WebRequestExecutor.prototype = {
    get_webRequest: Sys$Net$WebRequestExecutor$get_webRequest,

    _set_webRequest: Sys$Net$WebRequestExecutor$_set_webRequest,

        get_started: Sys$Net$WebRequestExecutor$get_started,

    get_responseAvailable: Sys$Net$WebRequestExecutor$get_responseAvailable,

    get_timedOut: Sys$Net$WebRequestExecutor$get_timedOut,
    get_aborted: Sys$Net$WebRequestExecutor$get_aborted,
    get_responseData: Sys$Net$WebRequestExecutor$get_responseData,
    get_statusCode: Sys$Net$WebRequestExecutor$get_statusCode,
    get_statusText: Sys$Net$WebRequestExecutor$get_statusText,
    get_xml: Sys$Net$WebRequestExecutor$get_xml,
    get_object: Sys$Net$WebRequestExecutor$get_object,

        executeRequest: Sys$Net$WebRequestExecutor$executeRequest,
    abort: Sys$Net$WebRequestExecutor$abort,
    getResponseHeader: Sys$Net$WebRequestExecutor$getResponseHeader,
    getAllResponseHeaders: Sys$Net$WebRequestExecutor$getAllResponseHeaders
}
Sys.Net.WebRequestExecutor.registerClass('Sys.Net.WebRequestExecutor');
window.XMLDOM = function window$XMLDOM(markup) {
    if (!window.DOMParser) {
        var progIDs = [ 'Msxml2.DOMDocument.3.0', 'Msxml2.DOMDocument' ];
        for (var i = 0; i < progIDs.length; i++) {
            try {
                var xmlDOM = new ActiveXObject(progIDs[i]);
                xmlDOM.async = false;
                xmlDOM.loadXML(markup);
                xmlDOM.setProperty('SelectionLanguage', 'XPath');
                return xmlDOM;
            }
            catch (ex) {
            }
        }
        return null;
    }
        else {
        try {
            var domParser = new window.DOMParser();
            return domParser.parseFromString(markup, 'text/xml');
        } catch (ex) {
            return null;
        }
    }
    return null;
    }

Sys.Net.XMLHttpExecutor = function Sys$Net$XMLHttpExecutor() {
    if (arguments.length !== 0) throw Error.parameterCount();

    Sys.Net.XMLHttpExecutor.initializeBase(this);

    var _this = this;
    this._xmlHttpRequest = null;
    this._webRequest = null;
    this._responseAvailable = false;
    this._timedOut = false;
    this._timer = null;
    this._aborted = false;
    this._started = false;

    this._onReadyStateChange = function () {
        
        if (_this._xmlHttpRequest.readyState === 4 ) {

            _this._clearTimer();
            _this._responseAvailable = true;
            _this._webRequest.completed(Sys.EventArgs.Empty);
            if (_this._xmlHttpRequest != null) {
                _this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
                _this._xmlHttpRequest = null;
            }
        }
    }

    this._clearTimer = function this$_clearTimer() {
        if (_this._timer != null) {
            window.clearTimeout(_this._timer);
            _this._timer = null;
        }
    }

    this._onTimeout = function this$_onTimeout() {
        if (!_this._responseAvailable) {
            _this._clearTimer();
            _this._timedOut = true;
            _this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
            _this._xmlHttpRequest.abort();
            _this._webRequest.completed(Sys.EventArgs.Empty);
            _this._xmlHttpRequest = null;
        }
    }

}



    function Sys$Net$XMLHttpExecutor$get_timedOut() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timedOut;
    }

    function Sys$Net$XMLHttpExecutor$get_started() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._started;
    }

    function Sys$Net$XMLHttpExecutor$get_responseAvailable() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
    return this._responseAvailable;
    }

    function Sys$Net$XMLHttpExecutor$get_aborted() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._aborted;
    }

    function Sys$Net$XMLHttpExecutor$executeRequest() {
        if (arguments.length !== 0) throw Error.parameterCount();
        this._webRequest = this.get_webRequest();

        if (this._started) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOnceStarted, 'executeRequest'));
        }
        if (this._webRequest === null) {
            throw Error.invalidOperation(Sys.Res.nullWebRequest);
        }

        var body = this._webRequest.get_body();
        var headers = this._webRequest.get_headers();
        this._xmlHttpRequest = new XMLHttpRequest();
        this._xmlHttpRequest.onreadystatechange = this._onReadyStateChange;
        var verb = this._webRequest.get_httpVerb();
        this._xmlHttpRequest.open(verb, this._webRequest.getResolvedUrl(), true );
        if (headers) {
            for (var header in headers) {
                var val = headers[header];
                if (typeof(val) !== "function")
                    this._xmlHttpRequest.setRequestHeader(header, val);
            }
        }

        if (verb.toLowerCase() === "post") {
                        if ((headers === null) || !headers['Content-Type']) {
                this._xmlHttpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            }

                        if (!body) {
                body = "";
            }
        }

        var timeout = this._webRequest.get_timeout();
        if (timeout > 0) {
            this._timer = window.setTimeout(Function.createDelegate(this, this._onTimeout), timeout);
        }
        this._xmlHttpRequest.send(body);
        this._started = true;
    }

    function Sys$Net$XMLHttpExecutor$getResponseHeader(header) {
        /// <param name="header" type="String"></param>
        /// <returns type="String"></returns>
        var e = Function._validateParams(arguments, [
            {name: "header", type: String}
        ]);
        if (e) throw e;

        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'getResponseHeader'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'getResponseHeader'));
        }

        var result;
        try {
            result = this._xmlHttpRequest.getResponseHeader(header);
        } catch (e) {
        }
        if (!result) result = "";
        return result;
    }

    function Sys$Net$XMLHttpExecutor$getAllResponseHeaders() {
        /// <returns type="String"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'getAllResponseHeaders'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'getAllResponseHeaders'));
        }

        return this._xmlHttpRequest.getAllResponseHeaders();
    }

    function Sys$Net$XMLHttpExecutor$get_responseData() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_responseData'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_responseData'));
        }

        return this._xmlHttpRequest.responseText;
    }

    function Sys$Net$XMLHttpExecutor$get_statusCode() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_statusCode'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_statusCode'));
        }

        return this._xmlHttpRequest.status;
    }

    function Sys$Net$XMLHttpExecutor$get_statusText() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_statusText'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_statusText'));
        }

        return this._xmlHttpRequest.statusText;
    }

    function Sys$Net$XMLHttpExecutor$get_xml() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_xml'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_xml'));
        }

        var xml = this._xmlHttpRequest.responseXML;
        if (!xml || !xml.documentElement) {

                        xml = new XMLDOM(this._xmlHttpRequest.responseText);

                        if (!xml || !xml.documentElement)
                return null;
        }
                else if (navigator.userAgent.indexOf('MSIE') !== -1) {
            xml.setProperty('SelectionLanguage', 'XPath');
        }

                if (xml.documentElement.namespaceURI === "http://www.mozilla.org/newlayout/xml/parsererror.xml" &&
            xml.documentElement.tagName === "parsererror") {
            return null;
        }
        
                if (xml.documentElement.firstChild && xml.documentElement.firstChild.tagName === "parsererror") {
            return null;
        }
        
        return xml;
    }

    function Sys$Net$XMLHttpExecutor$abort() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._started) {
            throw Error.invalidOperation(Sys.Res.cannotAbortBeforeStart);
        }

                if (this._aborted || this._responseAvailable || this._timedOut)
            return;

        this._aborted = true;

        this._clearTimer();

        if (this._xmlHttpRequest && !this._responseAvailable) {

                        this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
            this._xmlHttpRequest.abort();

            this._xmlHttpRequest = null;
            var handler = this._webRequest._get_eventHandlerList().getHandler("completed");
            if (handler) {
                handler(this, Sys.EventArgs.Empty);
            }
        }
    }
Sys.Net.XMLHttpExecutor.prototype = {

    get_timedOut: Sys$Net$XMLHttpExecutor$get_timedOut,

    get_started: Sys$Net$XMLHttpExecutor$get_started,

    get_responseAvailable: Sys$Net$XMLHttpExecutor$get_responseAvailable,

    get_aborted: Sys$Net$XMLHttpExecutor$get_aborted,

    executeRequest: Sys$Net$XMLHttpExecutor$executeRequest,

    getResponseHeader: Sys$Net$XMLHttpExecutor$getResponseHeader,

    getAllResponseHeaders: Sys$Net$XMLHttpExecutor$getAllResponseHeaders,

    get_responseData: Sys$Net$XMLHttpExecutor$get_responseData,

    get_statusCode: Sys$Net$XMLHttpExecutor$get_statusCode,

    get_statusText: Sys$Net$XMLHttpExecutor$get_statusText,

    get_xml: Sys$Net$XMLHttpExecutor$get_xml,

    abort: Sys$Net$XMLHttpExecutor$abort
}
Sys.Net.XMLHttpExecutor.registerClass('Sys.Net.XMLHttpExecutor', Sys.Net.WebRequestExecutor);
Sys.Net._WebRequestManager = function Sys$Net$_WebRequestManager() {
    this._this = this;
    this._defaultTimeout = 0;
    this._defaultExecutorType = "Sys.Net.XMLHttpExecutor";
}


    function Sys$Net$_WebRequestManager$add_invokingRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this._get_eventHandlerList().addHandler("invokingRequest", handler);
    }
    function Sys$Net$_WebRequestManager$remove_invokingRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this._get_eventHandlerList().removeHandler("invokingRequest", handler);
    }

    function Sys$Net$_WebRequestManager$add_completedRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this._get_eventHandlerList().addHandler("completedRequest", handler);
    }
    function Sys$Net$_WebRequestManager$remove_completedRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this._get_eventHandlerList().removeHandler("completedRequest", handler);
    }

    function Sys$Net$_WebRequestManager$_get_eventHandlerList() {
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Sys$Net$_WebRequestManager$get_defaultTimeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultTimeout;
    }
    function Sys$Net$_WebRequestManager$set_defaultTimeout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        if (value < 0) {
            throw Error.argumentOutOfRange("value", value, Sys.Res.invalidTimeout);
        }

        this._defaultTimeout = value;
    }

    function Sys$Net$_WebRequestManager$get_defaultExecutorType() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultExecutorType;
    }
    function Sys$Net$_WebRequestManager$set_defaultExecutorType(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        this._defaultExecutorType = value;
    }

    function Sys$Net$_WebRequestManager$executeRequest(webRequest) {
        /// <param name="webRequest" type="Sys.Net.WebRequest"></param>
        var e = Function._validateParams(arguments, [
            {name: "webRequest", type: Sys.Net.WebRequest}
        ]);
        if (e) throw e;

        var executor = webRequest.get_executor();
                if (!executor) {
            
            var failed = false;
            try {
                var executorType = eval(this._defaultExecutorType);
                executor = new executorType();
            } catch (e) {
                failed = true;
            }

            if (failed  || !Sys.Net.WebRequestExecutor.isInstanceOfType(executor) || !executor) {
                throw Error.argument("defaultExecutorType", String.format(Sys.Res.invalidExecutorType, this._defaultExecutorType));
            }

            webRequest.set_executor(executor);
        }

                if (executor.get_aborted()) {
            return;
        }

        var evArgs = new Sys.Net.NetworkRequestEventArgs(webRequest);
        var handler = this._get_eventHandlerList().getHandler("invokingRequest");
        if (handler) {
            handler(this, evArgs);
        }

        if (!evArgs.get_cancel()) {
            executor.executeRequest();
        }
    }
Sys.Net._WebRequestManager.prototype = {
    add_invokingRequest: Sys$Net$_WebRequestManager$add_invokingRequest,
    remove_invokingRequest: Sys$Net$_WebRequestManager$remove_invokingRequest,

    add_completedRequest: Sys$Net$_WebRequestManager$add_completedRequest,
    remove_completedRequest: Sys$Net$_WebRequestManager$remove_completedRequest,

    _get_eventHandlerList: Sys$Net$_WebRequestManager$_get_eventHandlerList,

    get_defaultTimeout: Sys$Net$_WebRequestManager$get_defaultTimeout,
    set_defaultTimeout: Sys$Net$_WebRequestManager$set_defaultTimeout,

    get_defaultExecutorType: Sys$Net$_WebRequestManager$get_defaultExecutorType,
    set_defaultExecutorType: Sys$Net$_WebRequestManager$set_defaultExecutorType,

    executeRequest: Sys$Net$_WebRequestManager$executeRequest
}

Sys.Net._WebRequestManager.registerClass('Sys.Net._WebRequestManager');

Sys.Net.WebRequestManager = new Sys.Net._WebRequestManager();
Sys.Net.NetworkRequestEventArgs = function Sys$Net$NetworkRequestEventArgs(webRequest) {
    /// <param name="webRequest" type="Sys.Net.WebRequest"></param>
    var e = Function._validateParams(arguments, [
        {name: "webRequest", type: Sys.Net.WebRequest}
    ]);
    if (e) throw e;

    Sys.Net.NetworkRequestEventArgs.initializeBase(this);
    this._webRequest = webRequest;
}


    function Sys$Net$NetworkRequestEventArgs$get_webRequest() {
        /// <value type="Sys.Net.WebRequest"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._webRequest;
    }
Sys.Net.NetworkRequestEventArgs.prototype = {
    get_webRequest: Sys$Net$NetworkRequestEventArgs$get_webRequest
}

Sys.Net.NetworkRequestEventArgs.registerClass('Sys.Net.NetworkRequestEventArgs', Sys.CancelEventArgs);
Sys.Net.WebRequest = function Sys$Net$WebRequest() {
    if (arguments.length !== 0) throw Error.parameterCount();
    this._url = "";
    this._headers = { };
    this._body = null;
    this._userContext = null;
    this._httpVerb = null;
    this._executor = null;
    this._invokeCalled = false;
    this._timeout = 0;
}


    function Sys$Net$WebRequest$add_completed(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        this._get_eventHandlerList().addHandler("completed", handler);
    }
    function Sys$Net$WebRequest$remove_completed(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        this._get_eventHandlerList().removeHandler("completed", handler);
    }

    function Sys$Net$WebRequest$completed(eventArgs) {
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        var handler = Sys.Net.WebRequestManager._get_eventHandlerList().getHandler("completedRequest");
        if (handler) {
            handler(this._executor, eventArgs);
        }

        handler = this._get_eventHandlerList().getHandler("completed");
        if (handler) {
            handler(this._executor, eventArgs);
        }
    }

    function Sys$Net$WebRequest$_get_eventHandlerList() {
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Sys$Net$WebRequest$get_url() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._url;
    }
    function Sys$Net$WebRequest$set_url(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        this._url = value;
    }

    function Sys$Net$WebRequest$get_headers() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._headers;
    }

    function Sys$Net$WebRequest$get_httpVerb() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
                if (this._httpVerb === null) {
            if (this._body === null) {
                return "GET";
            }
            return "POST";
        }
        return this._httpVerb;
    }
    function Sys$Net$WebRequest$set_httpVerb(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        if (value.length === 0) {
            throw Error.argument('value', Sys.Res.invalidHttpVerb);
        }

        this._httpVerb = value;
    }

    function Sys$Net$WebRequest$get_body() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._body;
    }
    function Sys$Net$WebRequest$set_body(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._body = value;
    }

    function Sys$Net$WebRequest$get_userContext() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._userContext;
    }
    function Sys$Net$WebRequest$set_userContext(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._userContext = value;
    }

    function Sys$Net$WebRequest$get_executor() {
        /// <value type="Sys.Net.WebRequestExecutor"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._executor;
    }
    function Sys$Net$WebRequest$set_executor(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.Net.WebRequestExecutor}]);
        if (e) throw e;

        if (this._executor !== null && this._executor.get_started()) {
            throw Error.invalidOperation(Sys.Res.setExecutorAfterActive);
        }

        this._executor = value;
        this._executor._set_webRequest(this);
    }

    function Sys$Net$WebRequest$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._timeout === 0) {
            return Sys.Net.WebRequestManager.get_defaultTimeout();
        }
        return this._timeout;
    }
    function Sys$Net$WebRequest$set_timeout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        if (value < 0) {
            throw Error.argumentOutOfRange("value", value, Sys.Res.invalidTimeout);
        }

        this._timeout = value;
    }

    function Sys$Net$WebRequest$getResolvedUrl() {
        /// <returns type="String"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        return Sys.Net.WebRequest._resolveUrl(this._url);
    }

    function Sys$Net$WebRequest$invoke() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._invokeCalled) {
            throw Error.invalidOperation(Sys.Res.invokeCalledTwice);
        }

        Sys.Net.WebRequestManager.executeRequest(this);
        this._invokeCalled = true;
    }
Sys.Net.WebRequest.prototype = {
    add_completed: Sys$Net$WebRequest$add_completed,
    remove_completed: Sys$Net$WebRequest$remove_completed,

    completed: Sys$Net$WebRequest$completed,

    _get_eventHandlerList: Sys$Net$WebRequest$_get_eventHandlerList,

    get_url: Sys$Net$WebRequest$get_url,
    set_url: Sys$Net$WebRequest$set_url,

    get_headers: Sys$Net$WebRequest$get_headers,

    get_httpVerb: Sys$Net$WebRequest$get_httpVerb,
    set_httpVerb: Sys$Net$WebRequest$set_httpVerb,

    get_body: Sys$Net$WebRequest$get_body,
    set_body: Sys$Net$WebRequest$set_body,

    get_userContext: Sys$Net$WebRequest$get_userContext,
    set_userContext: Sys$Net$WebRequest$set_userContext,

    get_executor: Sys$Net$WebRequest$get_executor,
    set_executor: Sys$Net$WebRequest$set_executor,

    get_timeout: Sys$Net$WebRequest$get_timeout,
    set_timeout: Sys$Net$WebRequest$set_timeout,

    getResolvedUrl: Sys$Net$WebRequest$getResolvedUrl,

    invoke: Sys$Net$WebRequest$invoke
}

Sys.Net.WebRequest._resolveUrl = function Sys$Net$WebRequest$_resolveUrl(url, baseUrl) {
        if (url && url.indexOf('://') !== -1) {
        return url;
    }

        if (!baseUrl || baseUrl.length === 0) {
        var baseElement = document.getElementsByTagName('base')[0];
        if (baseElement && baseElement.href && baseElement.href.length > 0) {
            baseUrl = baseElement.href;
        }
        else {
            baseUrl = document.URL;
        }
    }

        var qsStart = baseUrl.indexOf('?');
    if (qsStart !== -1) {
        baseUrl = baseUrl.substr(0, qsStart);
    }
    baseUrl = baseUrl.substr(0, baseUrl.lastIndexOf('/') + 1);

        if (!url || url.length === 0) {
        return baseUrl;
    }

        if (url.charAt(0) === '/') {
        var slashslash = baseUrl.indexOf('://');
        if (slashslash === -1) {
            throw Error.argument("baseUrl", Sys.Res.badBaseUrl1);
        }

        var nextSlash = baseUrl.indexOf('/', slashslash + 3);
        if (nextSlash === -1) {
            throw Error.argument("baseUrl", Sys.Res.badBaseUrl2);
        }

        return baseUrl.substr(0, nextSlash) + url;
    }
            else {
        var lastSlash = baseUrl.lastIndexOf('/');
        if (lastSlash === -1) {
            throw Error.argument("baseUrl", Sys.Res.badBaseUrl3);
        }

        return baseUrl.substr(0, lastSlash+1) + url;
    }
}

Sys.Net.WebRequest._createQueryString = function Sys$Net$WebRequest$_createQueryString(queryString, encodeMethod) {
        if (!encodeMethod)
        encodeMethod = encodeURIComponent;

    var sb = new Sys.StringBuilder();

    var i = 0;
    for (var arg in queryString) {
        var obj = queryString[arg];
        if (typeof(obj) === "function") continue;
        var val = Sys.Serialization.JavaScriptSerializer.serialize(obj);
        if (i !== 0) {
            sb.append('&');
        }

        sb.append(arg);
        sb.append('=');
        sb.append(encodeMethod(val));

        i++;
    }

    return sb.toString();
}

Sys.Net.WebRequest._createUrl = function Sys$Net$WebRequest$_createUrl(url, queryString) {
    if (!queryString) {
        return url;
    }

    var qs = Sys.Net.WebRequest._createQueryString(queryString);
    if (qs.length > 0) {
        var sep = '?';
        if (url && url.indexOf('?') !== -1)
            sep = '&';
        return url + sep + qs;
    } else {
        return url;
    }
}

Sys.Net.WebRequest.registerClass('Sys.Net.WebRequest');
Sys.Net.WebServiceProxy = function Sys$Net$WebServiceProxy() {
}


    function Sys$Net$WebServiceProxy$set_timeout(value) {
        this._timeout = value;
    }
    function Sys$Net$WebServiceProxy$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeout;
    }
    function Sys$Net$WebServiceProxy$set_defaultUserContext(value) {
        this._userContext = value;
    }
    function Sys$Net$WebServiceProxy$get_defaultUserContext() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._userContext;
    }
    function Sys$Net$WebServiceProxy$set_defaultSucceededCallback(value) {
        this._succeeded = value;
    }
    function Sys$Net$WebServiceProxy$get_defaultSucceededCallback() {
        /// <value type="Function"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._succeeded;
    }
    function Sys$Net$WebServiceProxy$set_defaultFailedCallback(value) {
        this._failed = value;
    }
    function Sys$Net$WebServiceProxy$get_defaultFailedCallback() {
        /// <value type="Function"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._failed;
    }
    function Sys$Net$WebServiceProxy$set_path(value) {
        this._path = value;
    }
    function Sys$Net$WebServiceProxy$get_path() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._path;
    }

    function Sys$Net$WebServiceProxy$_invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext) {
        /// <param name="servicePath" type="String"></param>
        /// <param name="methodName" type="String"></param>
        /// <param name="useGet" type="Boolean"></param>
        /// <param name="params"></param>
        /// <param name="onSuccess" type="Function" mayBeNull="true" optional="true"></param>
        /// <param name="onFailure" type="Function" mayBeNull="true" optional="true"></param>
        /// <param name="userContext" mayBeNull="true" optional="true"></param>
        /// <returns type="Sys.Net.WebRequest"></returns>
        var e = Function._validateParams(arguments, [
            {name: "servicePath", type: String},
            {name: "methodName", type: String},
            {name: "useGet", type: Boolean},
            {name: "params"},
            {name: "onSuccess", type: Function, mayBeNull: true, optional: true},
            {name: "onFailure", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;


                if (onSuccess === null || typeof onSuccess === 'undefined') onSuccess = this.get_defaultSucceededCallback();
        if (onFailure === null || typeof onFailure === 'undefined') onFailure = this.get_defaultFailedCallback();
        if (userContext === null || typeof userContext === 'undefined') userContext = this.get_defaultUserContext();
        
        return Sys.Net.WebServiceProxy.invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext, this.get_timeout());
    }
Sys.Net.WebServiceProxy.prototype = {
    set_timeout: Sys$Net$WebServiceProxy$set_timeout,
    get_timeout: Sys$Net$WebServiceProxy$get_timeout,
    set_defaultUserContext: Sys$Net$WebServiceProxy$set_defaultUserContext,
    get_defaultUserContext: Sys$Net$WebServiceProxy$get_defaultUserContext,
    set_defaultSucceededCallback: Sys$Net$WebServiceProxy$set_defaultSucceededCallback,
    get_defaultSucceededCallback: Sys$Net$WebServiceProxy$get_defaultSucceededCallback,
    set_defaultFailedCallback: Sys$Net$WebServiceProxy$set_defaultFailedCallback,
    get_defaultFailedCallback: Sys$Net$WebServiceProxy$get_defaultFailedCallback,
    set_path: Sys$Net$WebServiceProxy$set_path,
    get_path: Sys$Net$WebServiceProxy$get_path,

    _invoke: Sys$Net$WebServiceProxy$_invoke
}
Sys.Net.WebServiceProxy.registerClass('Sys.Net.WebServiceProxy');

Sys.Net.WebServiceProxy.invoke = function Sys$Net$WebServiceProxy$invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext, timeout) {
    /// <param name="servicePath" type="String"></param>
    /// <param name="methodName" type="String"></param>
    /// <param name="useGet" type="Boolean" optional="true"></param>
    /// <param name="params" mayBeNull="true" optional="true"></param>
    /// <param name="onSuccess" type="Function" mayBeNull="true" optional="true"></param>
    /// <param name="onFailure" type="Function" mayBeNull="true" optional="true"></param>
    /// <param name="userContext" mayBeNull="true" optional="true"></param>
    /// <param name="timeout" type="Number" optional="true"></param>
    /// <returns type="Sys.Net.WebRequest"></returns>
    var e = Function._validateParams(arguments, [
        {name: "servicePath", type: String},
        {name: "methodName", type: String},
        {name: "useGet", type: Boolean, optional: true},
        {name: "params", mayBeNull: true, optional: true},
        {name: "onSuccess", type: Function, mayBeNull: true, optional: true},
        {name: "onFailure", type: Function, mayBeNull: true, optional: true},
        {name: "userContext", mayBeNull: true, optional: true},
        {name: "timeout", type: Number, optional: true}
    ]);
    if (e) throw e;


        var request = new Sys.Net.WebRequest();

if(window.TARGET_J2EE)
    request.get_headers()['Content-Type'] = 'application/json; charset=utf-8; action='+methodName;
else
    request.get_headers()['Content-Type'] = 'application/json; charset=utf-8';
    if (!params) params = {};
    var urlParams = params;
        if (!useGet || !urlParams) urlParams = {};
if(window.TARGET_J2EE)
    request.set_url(Sys.Net.WebRequest._createUrl(servicePath, urlParams));
else
    request.set_url(Sys.Net.WebRequest._createUrl(servicePath+"/"+methodName, urlParams));

    var body = null;
        if (!useGet) {
        body = Sys.Serialization.JavaScriptSerializer.serialize(params);

                if (body === "{}") body = "";
    }

        request.set_body(body);
    request.add_completed(onComplete);
    if (timeout && timeout > 0) request.set_timeout(timeout);
    request.invoke();

    function onComplete(response, eventArgs) {
        if (response.get_responseAvailable()) {
            var statusCode = response.get_statusCode();
            var result = null;

            try {
                var contentType = response.getResponseHeader("Content-Type");
                if (contentType.startsWith("application/json")) {
                    result = response.get_object();
                }
                else if (contentType.startsWith("text/xml")) {
                    result = response.get_xml();
                }
                                else {
                    result = response.get_responseData();
                }
            } catch (ex) {
            }

            var error = response.getResponseHeader("jsonerror");
            var errorObj = (error === "true");
            if (errorObj) {
                result = new Sys.Net.WebServiceError(false, result.Message, result.StackTrace, result.ExceptionType);
            }
            if (((statusCode < 200) || (statusCode >= 300)) || errorObj) {
                if (onFailure) {
                    if (!result || !errorObj) {
                        result = new Sys.Net.WebServiceError(false , String.format(Sys.Res.webServiceFailedNoMsg, methodName), "", "");
                    }
                    result._statusCode = statusCode;
                    onFailure(result, userContext, methodName);
                }
                else {
                                        var error;
                    if (result && errorObj) {
                                                error = result.get_exceptionType() + "-- " + result.get_message();
                    }
                    else {
                                                                        error = response.get_responseData();
                    }
                    alert(String.format(Sys.Res.webServiceFailed, methodName, error));
                }
            }
            else if (onSuccess) {
                onSuccess(result, userContext, methodName);
            }
        }
        else {
            var msg;
            if (response.get_timedOut()) {
                msg = String.format(Sys.Res.webServiceTimedOut, methodName);
            }
            else {
                msg = String.format(Sys.Res.webServiceFailedNoMsg, methodName)
            }
            if (onFailure) {
                onFailure(new Sys.Net.WebServiceError(response.get_timedOut(), msg, "", ""), userContext, methodName);
            }
            else {
                                alert(msg);
            }
        }
    }

    return request;
}

Sys.Net.WebServiceProxy._generateTypedConstructor = function Sys$Net$WebServiceProxy$_generateTypedConstructor(type) {
    return function(properties) {
                if (properties) {
            for (var name in properties) {
                this[name] = properties[name];
            }
        }
        this.__type = type;
    }
}
Sys.Net.WebServiceError = function Sys$Net$WebServiceError(timedOut, message, stackTrace, exceptionType) {
    /// <param name="timedOut" type="Boolean"></param>
    /// <param name="message" type="String" mayBeNull="true"></param>
    /// <param name="stackTrace" type="String" mayBeNull="true"></param>
    /// <param name="exceptionType" type="String" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "timedOut", type: Boolean},
        {name: "message", type: String, mayBeNull: true},
        {name: "stackTrace", type: String, mayBeNull: true},
        {name: "exceptionType", type: String, mayBeNull: true}
    ]);
    if (e) throw e;

    this._timedOut = timedOut;
    this._message = message;
    this._stackTrace = stackTrace;
    this._exceptionType = exceptionType;
    this._statusCode = -1;
}


    function Sys$Net$WebServiceError$get_timedOut() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timedOut;
    }

    function Sys$Net$WebServiceError$get_statusCode() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._statusCode;
    }

    function Sys$Net$WebServiceError$get_message() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._message;
    }

    function Sys$Net$WebServiceError$get_stackTrace() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._stackTrace;
    }

    function Sys$Net$WebServiceError$get_exceptionType() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._exceptionType;
    }
Sys.Net.WebServiceError.prototype = {
    get_timedOut: Sys$Net$WebServiceError$get_timedOut,

    get_statusCode: Sys$Net$WebServiceError$get_statusCode,

    get_message: Sys$Net$WebServiceError$get_message,

    get_stackTrace: Sys$Net$WebServiceError$get_stackTrace,

    get_exceptionType: Sys$Net$WebServiceError$get_exceptionType
}
Sys.Net.WebServiceError.registerClass('Sys.Net.WebServiceError');

Type.registerNamespace('Sys.Services');

Sys.Services._ProfileService = function Sys$Services$_ProfileService() {
    Sys.Services._ProfileService.initializeBase(this);
    this.properties = {};
}
Sys.Services._ProfileService.DefaultWebServicePath = '';








    function Sys$Services$_ProfileService$get_defaultFailedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultFailedCallback;
    }
    function Sys$Services$_ProfileService$set_defaultFailedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultFailedCallback = value;
    }

    function Sys$Services$_ProfileService$get_defaultLoadCompletedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultLoadCompletedCallback;
    }
    function Sys$Services$_ProfileService$set_defaultLoadCompletedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultLoadCompletedCallback = value;
    }

    function Sys$Services$_ProfileService$get_defaultSaveCompletedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultSaveCompletedCallback;
    }
    function Sys$Services$_ProfileService$set_defaultSaveCompletedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultSaveCompletedCallback = value;
    }


    function Sys$Services$_ProfileService$get_path() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._path;
    }
    function Sys$Services$_ProfileService$set_path(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if((!value) || (!value.length)) {
            value = '';
        }
        this._path = value;
    }

    function Sys$Services$_ProfileService$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeout;
    }
    function Sys$Services$_ProfileService$set_timeout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._timeout = value;
    }

    function Sys$Services$_ProfileService$load(propertyNames, loadCompletedCallback, failedCallback, userContext) {
        /// <param name="propertyNames" type="Array" elementType="String" optional="true" elementMayBeNull="false" mayBeNull="true"></param>
        /// <param name="loadCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyNames", type: Array, mayBeNull: true, optional: true, elementType: String},
            {name: "loadCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var parameters = {};
        var methodName;
        if(!propertyNames) {
            methodName = "GetAllPropertiesForCurrentUser";
        }
        else {
            methodName = "GetPropertiesForCurrentUser";
            parameters = { properties: this._clonePropertyNames(propertyNames) };
        }
                this._invoke(this._get_path(),
                                        methodName,
                                        false,
                                        parameters,
                                        Function.createDelegate(this, this._onLoadComplete),
                                        Function.createDelegate(this, this._onLoadFailed),                                         [loadCompletedCallback, failedCallback, userContext]);
    }

    function Sys$Services$_ProfileService$save(propertyNames, saveCompletedCallback, failedCallback, userContext) {
        /// <param name="propertyNames" type="Array" elementType="String" optional="true" elementMayBeNull="false" mayBeNull="true"></param>
        /// <param name="saveCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyNames", type: Array, mayBeNull: true, optional: true, elementType: String},
            {name: "saveCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var flattenedProperties = this._flattenProperties(propertyNames, this.properties);
                this._invoke(this._get_path(),
                                        "SetPropertiesForCurrentUser",
                                        false,
                                        { values: flattenedProperties },
                                        Function.createDelegate(this, this._onSaveComplete),
                                        Function.createDelegate(this, this._onSaveFailed),
                                        [saveCompletedCallback, failedCallback, userContext]);
    }


    function Sys$Services$_ProfileService$_clonePropertyNames(arr) {
        var nodups = [];
        var seen = {};
        for(var i=0; i < arr.length; i++) {
            var prop = arr[i];
            if(!seen[prop]) { Array.add(nodups, prop); seen[prop]=true; };
        }
        return nodups;
    }





    function Sys$Services$_ProfileService$_flattenProperties(propertyNames, properties, groupName) {
        var flattenedProperties = {};
        var val;
        var key;
        if(propertyNames && propertyNames.length === 0) {
            return flattenedProperties;
        }

        for (var property in properties) {
            val = properties[property];
            key = groupName ? groupName + "." + property : property;
                        if(Sys.Services.ProfileGroup.isInstanceOfType(val)) {
                var groupProperties = this._flattenProperties(propertyNames, val, key);
                                                                                                                for(var subKey in groupProperties) {
                    var subVal = groupProperties[subKey];
                    flattenedProperties[subKey] = subVal;
                }
            }
            else {
                                if(!propertyNames || Array.indexOf(propertyNames, key) !== -1) {
                    flattenedProperties[key] = val;
                }
            }
        }
        return flattenedProperties;
    }

    function Sys$Services$_ProfileService$_get_path() {
        var path = this.get_path();
        if(!path.length) {
            path = Sys.Services._ProfileService.DefaultWebServicePath;
        }
        if(!path || !path.length) {
            throw Error.invalidOperation(Sys.Res.servicePathNotSet);
        }
        return path;
    }

    function Sys$Services$_ProfileService$_onLoadComplete(result, context, methodName) {
        if(typeof(result) !== "object") {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "Object"));
        }

        var unflattened = this._unflattenProperties(result);
        for(var name in unflattened) {
            this.properties[name] = unflattened[name];
        }
        
        var userCallback = context[0];
        var callback = userCallback ? userCallback : this._defaultLoadCompletedCallback;
        if(callback) {
            callback(result.length, context[2], "Sys.Services.ProfileService.load");
        }
    }

    function Sys$Services$_ProfileService$_onLoadFailed(err, context, methodName) {
        var userCallback = context[1];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, context[2], "Sys.Services.ProfileService.load");
        }
    }

    function Sys$Services$_ProfileService$_onSaveComplete(result, context, methodName) {
        if(typeof(result) !== "number") {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "Number"));
        }
        
        var userCallback = context[0];
        var userContext = context[2];
        var callback = userCallback ? userCallback : this._defaultSaveCompletedCallback;
        if(callback) {
            callback(result, userContext, "Sys.Services.ProfileService.save");
        }
    }

    function Sys$Services$_ProfileService$_onSaveFailed(err, context, methodName) {
        var userCallback = context[1];
        var userContext = context[2];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, userContext, "Sys.Services.ProfileService.save");
        }
    }

    function Sys$Services$_ProfileService$_unflattenProperties(properties) {
        var unflattenedProperties = {};
        var dotIndex;
        var val;
        var count = 0;
        for(var key in properties) {
            count++;
            val = properties[key];

            dotIndex = key.indexOf('.');
            if(dotIndex !== -1) {
                var groupName = key.substr(0, dotIndex);
                key = key.substr(dotIndex+1);
                var group = unflattenedProperties[groupName];
                if((!group) || (!Sys.Services.ProfileGroup.isInstanceOfType(group))) {
                    group = new Sys.Services.ProfileGroup();
                    unflattenedProperties[groupName] = group;
                }
                group[key] = val;
            }
            else {
                unflattenedProperties[key] = val;
            }
        }
        properties.length = count;
        return unflattenedProperties;
    }
Sys.Services._ProfileService.prototype = {
    _defaultFailedCallback: null,
    _defaultLoadCompletedCallback: null,
    _defaultSaveCompletedCallback: null,
    _path: '',
    _timeout: 0,

    get_defaultFailedCallback: Sys$Services$_ProfileService$get_defaultFailedCallback,
    set_defaultFailedCallback: Sys$Services$_ProfileService$set_defaultFailedCallback,

    get_defaultLoadCompletedCallback: Sys$Services$_ProfileService$get_defaultLoadCompletedCallback,
    set_defaultLoadCompletedCallback: Sys$Services$_ProfileService$set_defaultLoadCompletedCallback,

    get_defaultSaveCompletedCallback: Sys$Services$_ProfileService$get_defaultSaveCompletedCallback,
    set_defaultSaveCompletedCallback: Sys$Services$_ProfileService$set_defaultSaveCompletedCallback,
    
    
    get_path: Sys$Services$_ProfileService$get_path,
    set_path: Sys$Services$_ProfileService$set_path,
        
    get_timeout: Sys$Services$_ProfileService$get_timeout,
    set_timeout: Sys$Services$_ProfileService$set_timeout,
        
    load: Sys$Services$_ProfileService$load,

    save: Sys$Services$_ProfileService$save,
    
        _clonePropertyNames: Sys$Services$_ProfileService$_clonePropertyNames,    

                    _flattenProperties: Sys$Services$_ProfileService$_flattenProperties,
    
    _get_path: Sys$Services$_ProfileService$_get_path,    

    _onLoadComplete: Sys$Services$_ProfileService$_onLoadComplete,
    
    _onLoadFailed: Sys$Services$_ProfileService$_onLoadFailed,
    
    _onSaveComplete: Sys$Services$_ProfileService$_onSaveComplete,
    
    _onSaveFailed: Sys$Services$_ProfileService$_onSaveFailed,
    
    _unflattenProperties: Sys$Services$_ProfileService$_unflattenProperties
}
Sys.Services._ProfileService.registerClass('Sys.Services._ProfileService', Sys.Net.WebServiceProxy);
Sys.Services.ProfileService = new Sys.Services._ProfileService();

Sys.Services.ProfileGroup = function Sys$Services$ProfileGroup(properties) {
    /// <param name="properties" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "properties", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if(properties) {
        for(var property in properties) {
            this[property] = properties[property];
        }
    }
}
Sys.Services.ProfileGroup.registerClass('Sys.Services.ProfileGroup');








Sys.Services._AuthenticationService = function Sys$Services$_AuthenticationService() {
    if (arguments.length !== 0) throw Error.parameterCount();
    Sys.Services._AuthenticationService.initializeBase(this);
}
Sys.Services._AuthenticationService.DefaultWebServicePath = '';








    function Sys$Services$_AuthenticationService$get_defaultFailedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultFailedCallback;
    }
    function Sys$Services$_AuthenticationService$set_defaultFailedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultFailedCallback = value;
    }

    function Sys$Services$_AuthenticationService$get_defaultLoginCompletedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultLoginCompletedCallback;
    }
    function Sys$Services$_AuthenticationService$set_defaultLoginCompletedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultLoginCompletedCallback = value;
    }

    function Sys$Services$_AuthenticationService$get_defaultLogoutCompletedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultLogoutCompletedCallback;
    }
    function Sys$Services$_AuthenticationService$set_defaultLogoutCompletedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultLogoutCompletedCallback = value;
    }

    function Sys$Services$_AuthenticationService$get_isLoggedIn() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._authenticated;
    }

    function Sys$Services$_AuthenticationService$get_path() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._path;
    }
    function Sys$Services$_AuthenticationService$set_path(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if((!value) || (!value.length)) {
            value = '';
        }
        this._path = value;
    }

    function Sys$Services$_AuthenticationService$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeout;
    }
    function Sys$Services$_AuthenticationService$set_timeout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._timeout = value;
    }

    function Sys$Services$_AuthenticationService$login(username, password, isPersistent, customInfo, redirectUrl, loginCompletedCallback, failedCallback, userContext) {
        /// <param name="username" type="String" mayBeNull="false"></param>
        /// <param name="password" type="String" mayBeNull="true"></param>
        /// <param name="isPersistent" type="Boolean" optional="true" mayBeNull="true"></param>
        /// <param name="customInfo" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="redirectUrl" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="loginCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "username", type: String},
            {name: "password", type: String, mayBeNull: true},
            {name: "isPersistent", type: Boolean, mayBeNull: true, optional: true},
            {name: "customInfo", type: String, mayBeNull: true, optional: true},
            {name: "redirectUrl", type: String, mayBeNull: true, optional: true},
            {name: "loginCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

                this._invoke(this._get_path(), "Login", false,
                                        { userName: username, password: password, createPersistentCookie: isPersistent },
                                        Function.createDelegate(this, this._onLoginComplete),
                                        Function.createDelegate(this, this._onLoginFailed),
                                        [username, password, isPersistent, redirectUrl, loginCompletedCallback, failedCallback, userContext]);
    }

    function Sys$Services$_AuthenticationService$logout(redirectUrl, logoutCompletedCallback, failedCallback, userContext) {
        /// <param name="redirectUrl" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="logoutCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "redirectUrl", type: String, mayBeNull: true, optional: true},
            {name: "logoutCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

                this._invoke(this._get_path(), "Logout", false, {}, 
                                        Function.createDelegate(this, this._onLogoutComplete),
                                        Function.createDelegate(this, this._onLogoutFailed),
                                        [redirectUrl, logoutCompletedCallback, failedCallback, userContext]);
    }

    function Sys$Services$_AuthenticationService$_get_path() {
        var path = this.get_path();
        if(!path.length) {
            path = Sys.Services._AuthenticationService.DefaultWebServicePath;
        }
        if(!path || !path.length) {
            throw Error.invalidOperation(Sys.Res.servicePathNotSet);
        }
        return path;
    }

    function Sys$Services$_AuthenticationService$_onLoginComplete(result, context, methodName) {
        if(typeof(result) !== "boolean") {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "Boolean"));
        }
        
        var redirectUrl = context[3];
        var userCallback = context[4];
        var userContext = context[6];
        var callback = userCallback ? userCallback : this._defaultLoginCompletedCallback;
        
        if(result) {
            this._authenticated = true;

            if(callback) {
                callback(true, userContext, "Sys.Services.AuthenticationService.login");
            }
            
            if(typeof(redirectUrl) !== "undefined" && redirectUrl !== null) {
                                window.location.href = redirectUrl;
            }
        }
        else if (callback) {
            callback(false, userContext, "Sys.Services.AuthenticationService.login");
        }
    }

    function Sys$Services$_AuthenticationService$_onLoginFailed(err, context, methodName) {
        var userCallback = context[5];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, context[6], "Sys.Services.AuthenticationService.login");
        }
    }

    function Sys$Services$_AuthenticationService$_onLogoutComplete(result, context, methodName) {
        if(result !== null) {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "null"));
        }
        
        var redirectUrl = context[0];
        var userCallback = context[1];
        var userContext = context[3];
        var callback = userCallback ? userCallback : this._defaultLogoutCompletedCallback;

        this._authenticated = false;
        
        if (callback) {
            callback(null, userContext, "Sys.Services.AuthenticationService.logout");
        }
        
                if(!redirectUrl) {
            window.location.reload();
        }
        else {
            window.location.href = redirectUrl;
        }
    }

    function Sys$Services$_AuthenticationService$_onLogoutFailed(err, context, methodName) {
        var userCallback = context[2];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, context[3], "Sys.Services.AuthenticationService.logout");
        }
    }

    function Sys$Services$_AuthenticationService$_setAuthenticated(authenticated) {
        this._authenticated = authenticated;
    }
Sys.Services._AuthenticationService.prototype = {
    _defaultFailedCallback: null,
    _defaultLoginCompletedCallback: null,
    _defaultLogoutCompletedCallback: null,
    _path: '',
    _timeout: 0,
    _authenticated: false,
    
    get_defaultFailedCallback: Sys$Services$_AuthenticationService$get_defaultFailedCallback,
    set_defaultFailedCallback: Sys$Services$_AuthenticationService$set_defaultFailedCallback,

    get_defaultLoginCompletedCallback: Sys$Services$_AuthenticationService$get_defaultLoginCompletedCallback,
    set_defaultLoginCompletedCallback: Sys$Services$_AuthenticationService$set_defaultLoginCompletedCallback,

    get_defaultLogoutCompletedCallback: Sys$Services$_AuthenticationService$get_defaultLogoutCompletedCallback,
    set_defaultLogoutCompletedCallback: Sys$Services$_AuthenticationService$set_defaultLogoutCompletedCallback,

    get_isLoggedIn: Sys$Services$_AuthenticationService$get_isLoggedIn,

    get_path: Sys$Services$_AuthenticationService$get_path,
    set_path: Sys$Services$_AuthenticationService$set_path,
    
    get_timeout: Sys$Services$_AuthenticationService$get_timeout,
    set_timeout: Sys$Services$_AuthenticationService$set_timeout,    
    
    login: Sys$Services$_AuthenticationService$login,
    
    logout: Sys$Services$_AuthenticationService$logout,
    
    _get_path: Sys$Services$_AuthenticationService$_get_path,
    
    _onLoginComplete: Sys$Services$_AuthenticationService$_onLoginComplete,
    
    _onLoginFailed: Sys$Services$_AuthenticationService$_onLoginFailed,
    
    _onLogoutComplete: Sys$Services$_AuthenticationService$_onLogoutComplete,
    
    _onLogoutFailed: Sys$Services$_AuthenticationService$_onLogoutFailed,
    
    _setAuthenticated: Sys$Services$_AuthenticationService$_setAuthenticated    
}

Sys.Services._AuthenticationService.registerClass('Sys.Services._AuthenticationService', Sys.Net.WebServiceProxy);
Sys.Services.AuthenticationService = new Sys.Services._AuthenticationService();

Type.registerNamespace('Sys.Serialization');


Sys.Serialization.JavaScriptSerializer = function Sys$Serialization$JavaScriptSerializer() {
    if (arguments.length !== 0) throw Error.parameterCount();
}
Sys.Serialization.JavaScriptSerializer.registerClass('Sys.Serialization.JavaScriptSerializer');

Sys.Serialization.JavaScriptSerializer._stringRegEx = new RegExp('["\b\f\n\r\t\\\\\x00-\x1F]', 'i');

Sys.Serialization.JavaScriptSerializer._serializeWithBuilder = function Sys$Serialization$JavaScriptSerializer$_serializeWithBuilder(object, stringBuilder, sort) {
    var i;
    switch (typeof object) {
    case 'object':
        if (object) {
                        if (Array.isInstanceOfType(object)) {
                stringBuilder.append('[');
                for (i = 0; i < object.length; ++i) {
                    if (i > 0) {
                        stringBuilder.append(',');
                    }
                    Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(object[i], stringBuilder);
                }
                stringBuilder.append(']');
            }
            else {
                                                                if (Date.isInstanceOfType(object)) {
                    stringBuilder.append('"\\/Date(');
                    stringBuilder.append(object.getTime());
                    stringBuilder.append(')\\/"');
                    break;
                }

                var properties = [];
                var propertyCount = 0;
                for (var name in object) {
                                        if (name.startsWith('$')) {
                        continue;
                    }
                    properties[propertyCount++] = name;
                }
                if (sort) properties.sort();

                stringBuilder.append('{');
                var needComma = false;
                for (i=0; i<propertyCount; i++) {
                    var value = object[properties[i]];
                    if (typeof value !== 'undefined' && typeof value !== 'function') {
                        if (needComma) {
                            stringBuilder.append(',');
                        }
                        else {
                            needComma = true;
                        }

                                                Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(properties[i], stringBuilder, sort);
                        stringBuilder.append(':');
                        Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(value, stringBuilder, sort);
                    }
                }
                stringBuilder.append('}');
            }
        }
        else {
            stringBuilder.append('null');
        }
        break;

    case 'number':
        if (isFinite(object)) {
            stringBuilder.append(String(object));
        }
        else {
            throw Error.invalidOperation(Sys.Res.cannotSerializeNonFiniteNumbers);
        }
        break;

    case 'string':
        stringBuilder.append('"');

                if (Sys.Browser.agent === Sys.Browser.Safari || Sys.Serialization.JavaScriptSerializer._stringRegEx.test(object)) {
            var length = object.length;
            for (i = 0; i < length; ++i) {
                var curChar = object.charAt(i);
                                if (curChar >= ' ') {
                                        if (curChar === '\\' || curChar === '"') {
                        stringBuilder.append('\\');
                    }
                    stringBuilder.append(curChar);
                }
                else {
                    switch (curChar) {
                        case '\b':
                            stringBuilder.append('\\b');
                            break;
                        case '\f':
                            stringBuilder.append('\\f');
                            break;
                        case '\n':
                            stringBuilder.append('\\n');
                            break;
                        case '\r':
                            stringBuilder.append('\\r');
                            break;
                        case '\t':
                            stringBuilder.append('\\t');
                            break;
                        default:
                                                        stringBuilder.append('\\u00');
                            if (curChar.charCodeAt() < 16) stringBuilder.append('0');
                            stringBuilder.append(curChar.charCodeAt().toString(16));
                    }
                }
            }
        } else {
            stringBuilder.append(object);
        }
        stringBuilder.append('"');
        break;

    case 'boolean':
        stringBuilder.append(object.toString());
        break;

    default:
        stringBuilder.append('null');
        break;
    }
}

Sys.Serialization.JavaScriptSerializer.serialize = function Sys$Serialization$JavaScriptSerializer$serialize(object) {
    /// <param name="object" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "object", mayBeNull: true}
    ]);
    if (e) throw e;

    var stringBuilder = new Sys.StringBuilder();
    Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(object, stringBuilder, false);
    return stringBuilder.toString();
}

Sys.Serialization.JavaScriptSerializer.deserialize = function Sys$Serialization$JavaScriptSerializer$deserialize(data) {
    /// <param name="data" type="String"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "data", type: String}
    ]);
    if (e) throw e;

    if (data.length === 0) throw Error.argument('data', Sys.Res.cannotDeserializeEmptyString);
                                                            
    try {    
        var exp = data.replace(new RegExp('(^|[^\\\\])\\"\\\\/Date\\((-?[0-9]+)\\)\\\\/\\"', 'g'), "$1new Date($2)");
        return eval('(' + exp + ')');
    }
    catch (e) {
         throw Error.argument('data', Sys.Res.cannotDeserializeInvalidJson);
    }
}

Sys.CultureInfo = function Sys$CultureInfo(name, numberFormat, dateTimeFormat) {
    /// <param name="name" type="String"></param>
    /// <param name="numberFormat" type="Object"></param>
    /// <param name="dateTimeFormat" type="Object"></param>
    var e = Function._validateParams(arguments, [
        {name: "name", type: String},
        {name: "numberFormat", type: Object},
        {name: "dateTimeFormat", type: Object}
    ]);
    if (e) throw e;

    this.name = name;
    this.numberFormat = numberFormat;
    this.dateTimeFormat = dateTimeFormat;
}

    function Sys$CultureInfo$_getDateTimeFormats() {
        if (! this._dateTimeFormats) {
            var dtf = this.dateTimeFormat;
            this._dateTimeFormats =
              [ dtf.MonthDayPattern,
                dtf.YearMonthPattern,
                dtf.ShortDatePattern,
                dtf.ShortTimePattern,
                dtf.LongDatePattern,
                dtf.LongTimePattern,
                dtf.FullDateTimePattern,
                dtf.RFC1123Pattern,
                dtf.SortableDateTimePattern,
                dtf.UniversalSortableDateTimePattern ];
        }
        return this._dateTimeFormats;
    }
    function Sys$CultureInfo$_getMonthIndex(value) {
        if (!this._upperMonths) {
            this._upperMonths = this._toUpperArray(this.dateTimeFormat.MonthNames);
        }
        return Array.indexOf(this._upperMonths, this._toUpper(value));
    }
    function Sys$CultureInfo$_getAbbrMonthIndex(value) {
        if (!this._upperAbbrMonths) {
            this._upperAbbrMonths = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthNames);
        }
        return Array.indexOf(this._upperMonths, this._toUpper(value));
    }
    function Sys$CultureInfo$_getDayIndex(value) {
        if (!this._upperDays) {
            this._upperDays = this._toUpperArray(this.dateTimeFormat.DayNames);
        }
        return Array.indexOf(this._upperDays, this._toUpper(value));
    }
    function Sys$CultureInfo$_getAbbrDayIndex(value) {
        if (!this._upperAbbrDays) {
            this._upperAbbrDays = this._toUpperArray(this.dateTimeFormat.AbbreviatedDayNames);
        }
        return Array.indexOf(this._upperAbbrDays, this._toUpper(value));
    }
    function Sys$CultureInfo$_toUpperArray(arr) {
        var result = [];
        for (var i = 0, il = arr.length; i < il; i++) {
            result[i] = this._toUpper(arr[i]);
        }
        return result;
    }
    function Sys$CultureInfo$_toUpper(value) {
                        return value.split("\u00A0").join(' ').toUpperCase();
    }
Sys.CultureInfo.prototype = {
    _getDateTimeFormats: Sys$CultureInfo$_getDateTimeFormats,
    _getMonthIndex: Sys$CultureInfo$_getMonthIndex,
    _getAbbrMonthIndex: Sys$CultureInfo$_getAbbrMonthIndex,
    _getDayIndex: Sys$CultureInfo$_getDayIndex,
    _getAbbrDayIndex: Sys$CultureInfo$_getAbbrDayIndex,
    _toUpperArray: Sys$CultureInfo$_toUpperArray,
    _toUpper: Sys$CultureInfo$_toUpper
}
Sys.CultureInfo._parse = function Sys$CultureInfo$_parse(value) {
    var cultureInfo = Sys.Serialization.JavaScriptSerializer.deserialize(value);
    return new Sys.CultureInfo(cultureInfo.name, cultureInfo.numberFormat, cultureInfo.dateTimeFormat);
}
Sys.CultureInfo.registerClass('Sys.CultureInfo');

Sys.CultureInfo.InvariantCulture = Sys.CultureInfo._parse('{"name":"","numberFormat":{"CurrencyDecimalDigits":2,"CurrencyDecimalSeparator":".","IsReadOnly":true,"CurrencyGroupSizes":[3],"NumberGroupSizes":[3],"PercentGroupSizes":[3],"CurrencyGroupSeparator":",","CurrencySymbol":"\u00A4","NaNSymbol":"NaN","CurrencyNegativePattern":0,"NumberNegativePattern":1,"PercentPositivePattern":0,"PercentNegativePattern":0,"NegativeInfinitySymbol":"-Infinity","NegativeSign":"-","NumberDecimalDigits":2,"NumberDecimalSeparator":".","NumberGroupSeparator":",","CurrencyPositivePattern":0,"PositiveInfinitySymbol":"Infinity","PositiveSign":"+","PercentDecimalDigits":2,"PercentDecimalSeparator":".","PercentGroupSeparator":",","PercentSymbol":"%","PerMilleSymbol":"\u2030","NativeDigits":["0","1","2","3","4","5","6","7","8","9"],"DigitSubstitution":1},"dateTimeFormat":{"AMDesignator":"AM","Calendar":{"MinSupportedDateTime":"@-62135568000000@","MaxSupportedDateTime":"@253402300799999@","AlgorithmType":1,"CalendarType":1,"Eras":[1],"TwoDigitYearMax":2029,"IsReadOnly":true},"DateSeparator":"/","FirstDayOfWeek":0,"CalendarWeekRule":0,"FullDateTimePattern":"dddd, dd MMMM yyyy HH:mm:ss","LongDatePattern":"dddd, dd MMMM yyyy","LongTimePattern":"HH:mm:ss","MonthDayPattern":"MMMM dd","PMDesignator":"PM","RFC1123Pattern":"ddd, dd MMM yyyy HH\':\'mm\':\'ss \'GMT\'","ShortDatePattern":"MM/dd/yyyy","ShortTimePattern":"HH:mm","SortableDateTimePattern":"yyyy\'-\'MM\'-\'dd\'T\'HH\':\'mm\':\'ss","TimeSeparator":":","UniversalSortableDateTimePattern":"yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'","YearMonthPattern":"yyyy MMMM","AbbreviatedDayNames":["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],"ShortestDayNames":["Su","Mo","Tu","We","Th","Fr","Sa"],"DayNames":["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],"AbbreviatedMonthNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthNames":["January","February","March","April","May","June","July","August","September","October","November","December",""],"IsReadOnly":true,"NativeCalendarName":"Gregorian Calendar","AbbreviatedMonthGenitiveNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthGenitiveNames":["January","February","March","April","May","June","July","August","September","October","November","December",""]}}');

if (typeof(__cultureInfo) === 'undefined') {
    var __cultureInfo = '{"name":"en-US","numberFormat":{"CurrencyDecimalDigits":2,"CurrencyDecimalSeparator":".","IsReadOnly":false,"CurrencyGroupSizes":[3],"NumberGroupSizes":[3],"PercentGroupSizes":[3],"CurrencyGroupSeparator":",","CurrencySymbol":"$","NaNSymbol":"NaN","CurrencyNegativePattern":0,"NumberNegativePattern":1,"PercentPositivePattern":0,"PercentNegativePattern":0,"NegativeInfinitySymbol":"-Infinity","NegativeSign":"-","NumberDecimalDigits":2,"NumberDecimalSeparator":".","NumberGroupSeparator":",","CurrencyPositivePattern":0,"PositiveInfinitySymbol":"Infinity","PositiveSign":"+","PercentDecimalDigits":2,"PercentDecimalSeparator":".","PercentGroupSeparator":",","PercentSymbol":"%","PerMilleSymbol":"\u2030","NativeDigits":["0","1","2","3","4","5","6","7","8","9"],"DigitSubstitution":1},"dateTimeFormat":{"AMDesignator":"AM","Calendar":{"MinSupportedDateTime":"@-62135568000000@","MaxSupportedDateTime":"@253402300799999@","AlgorithmType":1,"CalendarType":1,"Eras":[1],"TwoDigitYearMax":2029,"IsReadOnly":false},"DateSeparator":"/","FirstDayOfWeek":0,"CalendarWeekRule":0,"FullDateTimePattern":"dddd, MMMM dd, yyyy h:mm:ss tt","LongDatePattern":"dddd, MMMM dd, yyyy","LongTimePattern":"h:mm:ss tt","MonthDayPattern":"MMMM dd","PMDesignator":"PM","RFC1123Pattern":"ddd, dd MMM yyyy HH\':\'mm\':\'ss \'GMT\'","ShortDatePattern":"M/d/yyyy","ShortTimePattern":"h:mm tt","SortableDateTimePattern":"yyyy\'-\'MM\'-\'dd\'T\'HH\':\'mm\':\'ss","TimeSeparator":":","UniversalSortableDateTimePattern":"yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'","YearMonthPattern":"MMMM, yyyy","AbbreviatedDayNames":["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],"ShortestDayNames":["Su","Mo","Tu","We","Th","Fr","Sa"],"DayNames":["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],"AbbreviatedMonthNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthNames":["January","February","March","April","May","June","July","August","September","October","November","December",""],"IsReadOnly":false,"NativeCalendarName":"Gregorian Calendar","AbbreviatedMonthGenitiveNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthGenitiveNames":["January","February","March","April","May","June","July","August","September","October","November","December",""]}}';
}
Sys.CultureInfo.CurrentCulture = Sys.CultureInfo._parse(__cultureInfo);
delete __cultureInfo;

Sys.UI.Point = function Sys$UI$Point(x, y) {
    /// <param name="x" type="Number" integer="true"></param>
    /// <param name="y" type="Number" integer="true"></param>
    /// <field name="x" type="Number" integer="true"></field>
    /// <field name="y" type="Number" integer="true"></field>
    var e = Function._validateParams(arguments, [
        {name: "x", type: Number, integer: true},
        {name: "y", type: Number, integer: true}
    ]);
    if (e) throw e;

    this.x = x;
    this.y = y;
}
Sys.UI.Point.registerClass('Sys.UI.Point');
Sys.UI.Bounds = function Sys$UI$Bounds(x, y, width, height) {
    /// <param name="x" type="Number" integer="true"></param>
    /// <param name="y" type="Number" integer="true"></param>
    /// <param name="height" type="Number" integer="true"></param>
    /// <param name="width" type="Number" integer="true"></param>
    /// <field name="x" type="Number" integer="true"></field>
    /// <field name="y" type="Number" integer="true"></field>
    /// <field name="height" type="Number" integer="true"></field>
    /// <field name="width" type="Number" integer="true"></field>
    var e = Function._validateParams(arguments, [
        {name: "x", type: Number, integer: true},
        {name: "y", type: Number, integer: true},
        {name: "height", type: Number, integer: true},
        {name: "width", type: Number, integer: true}
    ]);
    if (e) throw e;

    this.x = x;
    this.y = y;
    this.height = height;
    this.width = width;
}
Sys.UI.Bounds.registerClass('Sys.UI.Bounds');
Sys.UI.DomElement = function Sys$UI$DomElement() {
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}
Sys.UI.DomElement.registerClass('Sys.UI.DomElement');

Sys.UI.DomElement.addCssClass = function Sys$UI$DomElement$addCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    if (!Sys.UI.DomElement.containsCssClass(element, className)) {
        if (element.className === '') {
            element.className = className;
        }
        else {
            element.className += ' ' + className;
        }
    }
}

Sys.UI.DomElement.containsCssClass = function Sys$UI$DomElement$containsCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    return Array.contains(element.className.split(' '), className);
}

Sys.UI.DomElement.getBounds = function Sys$UI$DomElement$getBounds(element) {
    /// <param name="element" domElement="true"></param>
    /// <returns type="Sys.UI.Bounds"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    var offset = Sys.UI.DomElement.getLocation(element);

    return new Sys.UI.Bounds(offset.x, offset.y, element.offsetWidth || 0, element.offsetHeight || 0);
}

var $get = Sys.UI.DomElement.getElementById = function Sys$UI$DomElement$getElementById(id, element) {
    /// <param name="id" type="String"></param>
    /// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
    /// <returns domElement="true" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "id", type: String},
        {name: "element", mayBeNull: true, domElement: true, optional: true}
    ]);
    if (e) throw e;

    if (!element) return document.getElementById(id);
    if (element.getElementById) return element.getElementById(id);

        var nodeQueue = [];
    var childNodes = element.childNodes;
    for (var i = 0; i < childNodes.length; i++) {
        var node = childNodes[i];
        if (node.nodeType == 1) {
            nodeQueue[nodeQueue.length] = node;
        }
    }

    while (nodeQueue.length) {
        node = nodeQueue.shift();
        if (node.id == id) {
            return node;
        }
        childNodes = node.childNodes;
        for (i = 0; i < childNodes.length; i++) {
            node = childNodes[i];
            if (node.nodeType == 1) {
                nodeQueue[nodeQueue.length] = node;
            }
        }
    }

    return null;
}



switch(Sys.Browser.agent) {
    case Sys.Browser.InternetExplorer:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if (element.self || element.nodeType === 9) return new Sys.UI.Point(0,0);

                                                var clientRects = element.getClientRects();
            if (!clientRects || !clientRects.length) {
                return new Sys.UI.Point(0,0);
            }

            var w = element.ownerDocument.parentWindow;
                                                var offsetL = w.screenLeft - top.screenLeft - top.document.documentElement.scrollLeft + 2;
            var offsetT = w.screenTop - top.screenTop - top.document.documentElement.scrollTop + 2;

                                                                        var f = w.frameElement || null;
            if (f) {
                                                                                var fstyle = f.currentStyle;
                offsetL += (f.frameBorder || 1) * 2 +
                    (parseInt(fstyle.paddingLeft) || 0) +
                    (parseInt(fstyle.borderLeftWidth) || 0) -
                    element.ownerDocument.documentElement.scrollLeft;
                offsetT += (f.frameBorder || 1) * 2 +
                    (parseInt(fstyle.paddingTop) || 0) +
                    (parseInt(fstyle.borderTopWidth) || 0) -
                    element.ownerDocument.documentElement.scrollTop;
            }

            var clientRect = clientRects[0];

            return new Sys.UI.Point(
                clientRect.left - offsetL,
                clientRect.top - offsetT);
        }
        break;
    case Sys.Browser.Safari:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0,0);

                                                var offsetX = 0;
            var offsetY = 0;

            var previous = null;
            var previousStyle = null;
            var currentStyle;
            for (var parent = element; parent; previous = parent, previousStyle = currentStyle, parent = parent.offsetParent) {
                currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
                var tagName = parent.tagName;

                                                                                if ((parent.offsetLeft || parent.offsetTop) &&
                    ((tagName !== "BODY") || (!previousStyle || previousStyle.position !== "absolute"))) {

                    offsetX += parent.offsetLeft;
                    offsetY += parent.offsetTop;
                }
            }

            currentStyle = Sys.UI.DomElement._getCurrentStyle(element);
            var elementPosition = currentStyle ? currentStyle.position : null;
            var elementPositioned = elementPosition && (elementPosition !== "static");
                        if (!elementPosition || (elementPosition !== "absolute")) {
                                for (var parent = element.parentNode; parent; parent = parent.parentNode) {
                    tagName = parent.tagName;

                    if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop)) {
                        offsetX -= (parent.scrollLeft || 0);
                        offsetY -= (parent.scrollTop || 0);
                    }
                    currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
                    var parentPosition = currentStyle ? currentStyle.position : null;

                                        if (parentPosition && (parentPosition === "absolute")) break;
                }
            }

            return new Sys.UI.Point(offsetX, offsetY);
        }
        break;
    case Sys.Browser.Opera:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0,0);

                                                var offsetX = 0;
            var offsetY = 0;

            var previous = null;
            for (var parent = element; parent; previous = parent, parent = parent.offsetParent) {

                var tagName = parent.tagName;

                offsetX += parent.offsetLeft || 0;
                offsetY += parent.offsetTop || 0;
            }

                        var elementPosition = element.style.position;
            var elementPositioned = elementPosition && (elementPosition !== "static");

                        for (var parent = element.parentNode; parent; parent = parent.parentNode) {
                tagName = parent.tagName;

                if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop) &&
                    ((elementPositioned &&
                    ((parent.style.overflow === "scroll") || (parent.style.overflow === "auto"))))) {
                                        
                    offsetX -= (parent.scrollLeft || 0);
                    offsetY -= (parent.scrollTop || 0);
                }
                var parentPosition = (parent && parent.style) ? parent.style.position : null;

                                elementPositioned = elementPositioned || (parentPosition && (parentPosition !== "static"));
            }


            return new Sys.UI.Point(offsetX, offsetY);
        }
        break;
    default:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0,0);

            var offsetX = 0;
            var offsetY = 0;
            var previous = null;
            var previousStyle = null;
            var currentStyle = null;
            for (var parent = element; parent; previous = parent, previousStyle = currentStyle, parent = parent.offsetParent) {
                var tagName = parent.tagName;
                currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);

                                                if ((parent.offsetLeft || parent.offsetTop) &&
                    !((tagName === "BODY") &&
                    (!previousStyle || previousStyle.position !== "absolute"))) {

                    offsetX += parent.offsetLeft;
                    offsetY += parent.offsetTop;
                }

                                                if (previous !== null && currentStyle) {
                                                                                                                                            if ((tagName !== "TABLE") && (tagName !== "TD") && (tagName !== "HTML")) {
                        offsetX += parseInt(currentStyle.borderLeftWidth) || 0;
                        offsetY += parseInt(currentStyle.borderTopWidth) || 0;
                    }
                    if (tagName === "TABLE" &&
                        (currentStyle.position === "relative" || currentStyle.position === "absolute")) {
                        offsetX += parseInt(currentStyle.marginLeft) || 0;
                        offsetY += parseInt(currentStyle.marginTop) || 0;
                    }
                }
            }

            currentStyle = Sys.UI.DomElement._getCurrentStyle(element);
            var elementPosition = currentStyle ? currentStyle.position : null;
            var elementPositioned = elementPosition && (elementPosition !== "static");
                        if (!elementPosition || (elementPosition !== "absolute")) {
                                                                                for (var parent = element.parentNode; parent; parent = parent.parentNode) {
                                                                                tagName = parent.tagName;

                    if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop)) {

                        offsetX -= (parent.scrollLeft || 0);
                        offsetY -= (parent.scrollTop || 0);

                        currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
                        offsetX += parseInt(currentStyle.borderLeftWidth) || 0;
                        offsetY += parseInt(currentStyle.borderTopWidth) || 0;
                    }
                }
            }

            return new Sys.UI.Point(offsetX, offsetY);
        }
        break;

}


Sys.UI.DomElement.removeCssClass = function Sys$UI$DomElement$removeCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    var currentClassName = ' ' + element.className + ' ';
    var index = currentClassName.indexOf(' ' + className + ' ');
    if (index >= 0) {
        element.className = (currentClassName.substr(0, index) + ' ' +
            currentClassName.substring(index + className.length + 1, currentClassName.length)).trim();
    }
}

Sys.UI.DomElement.setLocation = function Sys$UI$DomElement$setLocation(element, x, y) {
    /// <param name="element" domElement="true"></param>
    /// <param name="x" type="Number" integer="true"></param>
    /// <param name="y" type="Number" integer="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "x", type: Number, integer: true},
        {name: "y", type: Number, integer: true}
    ]);
    if (e) throw e;

    var style = element.style;
    style.position = 'absolute';
    style.left = x + "px";
    style.top = y + "px";
}

Sys.UI.DomElement.toggleCssClass = function Sys$UI$DomElement$toggleCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    if (Sys.UI.DomElement.containsCssClass(element, className)) {
        Sys.UI.DomElement.removeCssClass(element, className);
    }
    else {
        Sys.UI.DomElement.addCssClass(element, className);
    }
}

Sys.UI.DomElement._getCurrentStyle = function Sys$UI$DomElement$_getCurrentStyle(element) {
    var w = (element.ownerDocument ? element.ownerDocument : element.documentElement).defaultView;
    return ((w && (element !== w) && w.getComputedStyle) ? w.getComputedStyle(element, null) : element.style);
}
Sys.UI.Behavior = function Sys$UI$Behavior(element) {
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    Sys.UI.Behavior.initializeBase(this);

    this._element = element;

    var behaviors = element._behaviors;
    if (!behaviors) {
        element._behaviors = [this];
    }
    else {
        behaviors[behaviors.length] = this;
    }
}


    function Sys$UI$Behavior$get_element() {
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._element;
    }
    function Sys$UI$Behavior$get_id() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        var baseId = Sys.UI.Behavior.callBaseMethod(this, 'get_id');
        if (baseId) return baseId;
        if (!this._element || !this._element.id) return '';
        return this._element.id + '$' + this.get_name();
    }
    function Sys$UI$Behavior$get_name() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._name) return this._name;
        var name = Object.getTypeName(this);
        var i = name.lastIndexOf('.');
        if (i != -1) name = name.substr(i + 1);
        if (!this.get_isInitialized()) this._name = name;
        return name;
    }
    function Sys$UI$Behavior$set_name(value) {
        if ((value === '') || (value.charAt(0) === ' ') || (value.charAt(value.length - 1) === ' '))
            throw Error.argument('value', Sys.Res.invalidId);
        if (typeof(this._element[value]) !== 'undefined')
            throw Error.invalidOperation(String.format(Sys.Res.behaviorDuplicateName, value));
        if (this.get_isInitialized()) throw Error.invalidOperation(Sys.Res.cantSetNameAfterInit);
        this._name = value;
    }
    function Sys$UI$Behavior$initialize() {
        Sys.UI.Behavior.callBaseMethod(this, 'initialize');
        var name = this.get_name();
        if (name) this._element[name] = this;
    }
    function Sys$UI$Behavior$dispose() {
        Sys.UI.Behavior.callBaseMethod(this, 'dispose');
        if (this._element) {
            var name = this.get_name();
            if (name) {
                this._element[name] = null;
            }
            Array.remove(this._element._behaviors, this);
            delete this._element;
        }
    }
Sys.UI.Behavior.prototype = {
    _name: null,
    get_element: Sys$UI$Behavior$get_element,
    get_id: Sys$UI$Behavior$get_id,
    get_name: Sys$UI$Behavior$get_name,
    set_name: Sys$UI$Behavior$set_name,
    initialize: Sys$UI$Behavior$initialize,
    dispose: Sys$UI$Behavior$dispose
}
Sys.UI.Behavior.registerClass('Sys.UI.Behavior', Sys.Component);

Sys.UI.Behavior.getBehaviorByName = function Sys$UI$Behavior$getBehaviorByName(element, name) {
    /// <param name="element" domElement="true"></param>
    /// <param name="name" type="String"></param>
    /// <returns type="Sys.UI.Behavior" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "name", type: String}
    ]);
    if (e) throw e;

    var b = element[name];
    return (b && Sys.UI.Behavior.isInstanceOfType(b)) ? b : null;
}

Sys.UI.Behavior.getBehaviors = function Sys$UI$Behavior$getBehaviors(element) {
    /// <param name="element" domElement="true"></param>
    /// <returns type="Array" elementType="Sys.UI.Behavior"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    if (!element._behaviors) return [];
    return Array.clone(element._behaviors);
}

Sys.UI.Behavior.getBehaviorsByType = function Sys$UI$Behavior$getBehaviorsByType(element, type) {
    /// <param name="element" domElement="true"></param>
    /// <param name="type" type="Type"></param>
    /// <returns type="Array" elementType="Sys.UI.Behavior"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "type", type: Type}
    ]);
    if (e) throw e;

    var behaviors = element._behaviors;
    var results = [];
    if (behaviors) {
        for (var i = 0, l = behaviors.length; i < l; i++) {
            if (type.isInstanceOfType(behaviors[i])) {
                results[results.length] = behaviors[i];
            }
        }
    }
    return results;
}
Sys.UI.VisibilityMode = function Sys$UI$VisibilityMode() {
    /// <field name="hide" type="Number" integer="true" static="true"></field>
    /// <field name="collapse" type="Number" integer="true" static="true"></field>
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}



Sys.UI.VisibilityMode.prototype = {
    hide: 0,
    collapse: 1
}
Sys.UI.VisibilityMode.registerEnum("Sys.UI.VisibilityMode");

Sys.UI.Control = function Sys$UI$Control(element) {
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    if (typeof(element.control) != 'undefined') throw Error.invalidOperation(Sys.Res.controlAlreadyDefined);
    Sys.UI.Control.initializeBase(this);

    this._element = element;
    element.control = this;

    this._oldDisplayMode = this._element.style.display;
    if (!this._oldDisplayMode || (this._oldDisplayMode == 'none')) {
        this._oldDisplayMode = '';
    }
}




    function Sys$UI$Control$get_element() {
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._element;
    }
    function Sys$UI$Control$get_id() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._element) return '';
        return this._element.id;
    }
    function Sys$UI$Control$set_id(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        throw Error.invalidOperation(Sys.Res.cantSetId);
    }
    function Sys$UI$Control$get_parent() {
        /// <value type="Sys.UI.Control"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._parent) {
            return this._parent;
        }
        else {
            var parentElement = this._element.parentNode;
            while (parentElement) {
                if (parentElement.control) {
                    return parentElement.control;
                }
                parentElement = parentElement.parentNode;
            }
            return null;
        }
    }
    function Sys$UI$Control$set_parent(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.UI.Control}]);
        if (e) throw e;

        var parents = [this];
        var current = value;
        while (current) {
            if (Array.contains(parents, current)) throw Error.invalidOperation(Sys.Res.circularParentChain);
            parents[parents.length] = current;
            current = current.get_parent();
        }
        this._parent = value;
    }
    function Sys$UI$Control$get_visibilityMode() {
        /// <value type="Sys.UI.VisibilityMode"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._visibilityMode;
    }
    function Sys$UI$Control$set_visibilityMode(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.UI.VisibilityMode}]);
        if (e) throw e;

        if (this._visibilityMode !== value) {
            this._visibilityMode = value;
            if (this.get_visible() === false) {
                if (this._visibilityMode === Sys.UI.VisibilityMode.hide) {
                    this._element.style.display = this._oldDisplayMode;
                }
                else {
                    this._element.style.display = 'none';
                }
            }
        }
        this._visibilityMode = value;
    }
    function Sys$UI$Control$get_visible() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return (this._element.style.visibility != 'hidden');
    }
    function Sys$UI$Control$set_visible(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        if (value != this.get_visible()) {
            this._element.style.visibility = value ? 'visible' : 'hidden';
            if (value || (this._visibilityMode === Sys.UI.VisibilityMode.hide)) {
                this._element.style.display = this._oldDisplayMode;
            }
            else {
                this._element.style.display = 'none';
            }
        }
    }
    function Sys$UI$Control$addCssClass(className) {
        /// <param name="className" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "className", type: String}
        ]);
        if (e) throw e;

        Sys.UI.DomElement.addCssClass(this._element, className);
    }
    function Sys$UI$Control$dispose() {
        Sys.UI.Control.callBaseMethod(this, 'dispose');
        if (this._element) {
            this._element.control = undefined;
            delete this._element;
        }
    }
    function Sys$UI$Control$initialize() {
        Sys.UI.Control.callBaseMethod(this, 'initialize');
        var elt = this._element;
    }
    function Sys$UI$Control$onBubbleEvent(source, args) {
        /// <param name="source"></param>
        /// <param name="args" type="Sys.EventArgs"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "source"},
            {name: "args", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        return false;
    }
    function Sys$UI$Control$raiseBubbleEvent(source, args) {
        /// <param name="source"></param>
        /// <param name="args" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "source"},
            {name: "args", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        var currentTarget = this.get_parent();
        while (currentTarget) {
            if (currentTarget.onBubbleEvent(source, args)) {
                return;
            }
            currentTarget = currentTarget.get_parent();
        }
    }
    function Sys$UI$Control$removeCssClass(className) {
        /// <param name="className" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "className", type: String}
        ]);
        if (e) throw e;

        Sys.UI.DomElement.removeCssClass(this._element, className);
    }
    function Sys$UI$Control$toggleCssClass(className) {
        /// <param name="className" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "className", type: String}
        ]);
        if (e) throw e;

        Sys.UI.DomElement.toggleCssClass(this._element, className);
    }
Sys.UI.Control.prototype = {
    _parent: null,
    _visibilityMode: Sys.UI.VisibilityMode.hide,

    get_element: Sys$UI$Control$get_element,
    get_id: Sys$UI$Control$get_id,
    set_id: Sys$UI$Control$set_id,
    get_parent: Sys$UI$Control$get_parent,
    set_parent: Sys$UI$Control$set_parent,
    get_visibilityMode: Sys$UI$Control$get_visibilityMode,
    set_visibilityMode: Sys$UI$Control$set_visibilityMode,
    get_visible: Sys$UI$Control$get_visible,
    set_visible: Sys$UI$Control$set_visible,
    addCssClass: Sys$UI$Control$addCssClass,
    dispose: Sys$UI$Control$dispose,
    initialize: Sys$UI$Control$initialize,
    onBubbleEvent: Sys$UI$Control$onBubbleEvent,
    raiseBubbleEvent: Sys$UI$Control$raiseBubbleEvent,
    removeCssClass: Sys$UI$Control$removeCssClass,
    toggleCssClass: Sys$UI$Control$toggleCssClass
}
Sys.UI.Control.registerClass('Sys.UI.Control', Sys.Component);

Sys.Res={
'argumentTypeName':'Value is not the name of an existing type.',
'methodRegisteredTwice':'Method {0} has already been registered.',
'cantSetIdAfterInit':'The id property can\'t be set on this object after initialization.',
'componentCantSetIdAfterAddedToApp':'The id property of a component can\'t be set after it\'s been added to the Application object.',
'behaviorDuplicateName':'A behavior with name \'{0}\' already exists or it is the name of an existing property on the target element.',
'notATypeName':'Value is not a valid type name.',
'typeShouldBeTypeOrString':'Value is not a valid type or a valid type name.',
'boolTrueOrFalse':'Value must be \'true\' or \'false\'.',
'stringFormatInvalid':'The format string is invalid.',
'referenceNotFound':'Component \'{0}\' was not found.',
'enumReservedName':'\'{0}\' is a reserved name that can\'t be used as an enum value name.',
'eventHandlerNotFound':'Handler not found.',
'circularParentChain':'The chain of control parents can\'t have circular references.',
'undefinedEvent':'\'{0}\' is not an event.',
'notAMethod':'{0} is not a method.',
'propertyUndefined':'\'{0}\' is not a property or an existing field.',
'eventHandlerInvalid':'Handler was not added through the Sys.UI.DomEvent.addHandler method.',
'scriptLoadFailedDebug':'The script \'{0}\' failed to load. Check for:\r\n Inaccessible path.\r\n Script errors. (IE) Enable \'Display a notification about every script error\' under advanced settings.\r\n Missing call to Sys.Application.notifyScriptLoaded().',
'propertyNotWritable':'\'{0}\' is not a writable property.',
'enumInvalidValueName':'\'{0}\' is not a valid name for an enum value.',
'controlAlreadyDefined':'A control is already associated with the element.',
'namespaceContainsObject':'Object {0} already exists and is not a namespace.',
'cantAddNonFunctionhandler':'Can\'t add a handler that is not a function.',
'scriptLoaderAlreadyLoading':'ScriptLoader.loadScripts cannot be called while the ScriptLoader is already loading scripts.',
'invalidNameSpace':'Value is not a valid namespace identifier.',
'notAnInterface':'Value is not a valid interface.',
'eventHandlerNotFunction':'Handler must be a function.',
'propertyNotAnArray':'\'{0}\' is not an Array property.',
'typeRegisteredTwice':'Type {0} has already been registered.',
'cantSetNameAfterInit':'The name property can\'t be set on this object after initialization.',
'appDuplicateComponent':'Two components with the same id \'{0}\' can\'t be added to the application.',
'appComponentMustBeInitialized':'Components must be initialized before they are added to the Application object.',
'baseNotAClass':'Value is not a class.',
'methodNotFound':'No method found with name \'{0}\'.',
'arrayParseBadFormat':'Value must be a valid string representation for an array. It must start with a \'[\' and end with a \']\'.',
'cantSetId':'The id property can\'t be set on this object.',
'stringFormatBraceMismatch':'The format string contains an unmatched opening or closing brace.',
'enumValueNotInteger':'An enumeration definition can only contain integer values.',
'propertyNullOrUndefined':'Cannot set the properties of \'{0}\' because it returned a null value.',
'componentCantSetIdTwice':'The id property of a component can\'t be set more than once.',
'createComponentOnDom':'Value must be null for Components that are not Controls or Behaviors.',
'createNotComponent':'{0} does not derive from Sys.Component.',
'createNoDom':'Value must not be null for Controls and Behaviors.',
'cantAddWithoutId':'Can\'t add a component that doesn\'t have an id.',
'badTypeName':'Value is not the name of the type being registered or the name is a reserved word.',
'argumentInteger':'Value must be an integer.',
'scriptLoadMultipleCallbacks':'The script \'{0}\' contains multiple calls to Sys.Application.notifyScriptLoaded(). Only one is allowed.',
'invokeCalledTwice':'Cannot call invoke more than once.',
'webServiceFailed':'The server method \'{0}\' failed with the following error: {1}',
'argumentType':'Object cannot be converted to the required type.',
'argumentNull':'Value cannot be null.',
'controlCantSetId':'The id property can\'t be set on a control.',
'formatBadFormatSpecifier':'Format specifier was invalid.',
'webServiceFailedNoMsg':'The server method \'{0}\' failed.',
'argumentDomElement':'Value must be a DOM element.',
'invalidExecutorType':'Could not create a valid Sys.Net.WebRequestExecutor from: {0}.',
'cannotCallBeforeResponse':'Cannot call {0} when responseAvailable is false.',
'actualValue':'Actual value was {0}.',
'enumInvalidValue':'\'{0}\' is not a valid value for enum {1}.',
'scriptLoadFailed':'The script \'{0}\' could not be loaded.',
'parameterCount':'Parameter count mismatch.',
'cannotDeserializeEmptyString':'Cannot deserialize empty string.',
'formatInvalidString':'Input string was not in a correct format.',
'invalidTimeout':'Value must be greater than or equal to zero.',
'cannotAbortBeforeStart':'Cannot abort when executor has not started.',
'argument':'Value does not fall within the expected range.',
'cannotDeserializeInvalidJson':'Cannot deserialize. The data does not correspond to valid JSON.',
'invalidHttpVerb':'httpVerb cannot be set to an empty or null string.',
'nullWebRequest':'Cannot call executeRequest with a null webRequest.',
'eventHandlerInvalid':'Handler was not added through the Sys.UI.DomEvent.addHandler method.',
'cannotSerializeNonFiniteNumbers':'Cannot serialize non finite numbers.',
'argumentUndefined':'Value cannot be undefined.',
'webServiceInvalidReturnType':'The server method \'{0}\' returned an invalid type. Expected type: {1}',
'servicePathNotSet':'The path to the web service has not been set.',
'argumentTypeWithTypes':'Object of type \'{0}\' cannot be converted to type \'{1}\'.',
'cannotCallOnceStarted':'Cannot call {0} once started.',
'badBaseUrl1':'Base URL does not contain ://.',
'badBaseUrl2':'Base URL does not contain another /.',
'badBaseUrl3':'Cannot find last / in base URL.',
'setExecutorAfterActive':'Cannot set executor after it has become active.',
'paramName':'Parameter name: {0}',
'cannotCallOutsideHandler':'Cannot call {0} outside of a completed event handler.',
'format':'One of the identified items was in an invalid format.',
'assertFailedCaller':'Assertion Failed: {0}\r\nat {1}',
'argumentOutOfRange':'Specified argument was out of the range of valid values.',
'webServiceTimedOut':'The server method \'{0}\' timed out.',
'notImplemented':'The method or operation is not implemented.',
'assertFailed':'Assertion Failed: {0}',
'invalidOperation':'Operation is not valid due to the current state of the object.',
'breakIntoDebugger':'{0}\r\n\r\nBreak into debugger?'
};

if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();
