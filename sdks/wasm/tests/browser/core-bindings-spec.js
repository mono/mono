//describe, beforeAll, it, expext - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Core Bindings Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;

    beforeAll(function(done){
      //load DOM custom matchers from karma-jasmine-dom package
      jasmine.addMatchers(DOMCustomMatchers);
      
      //lets open our 'http-spec.html' file in the browser by 'index' tag as you specified in 'karma.conf.js'
      karmaHTML.corebindingsspec.open();
      
      //karmaHTML.corebindingsspec.onstatechange fires when the Document is loaded
      //now the tests can be executed on the DOM
      karmaHTML.corebindingsspec.onstatechange = function(ready){
        //if the #Document is ready, fire tests
        //the done() callback is the jasmine native async-support function
        if(ready) {
          karmaHTML.corebindingsspec.document.onRuntimeDone = function ()
          {
            done();
          }

        }
      };

    });

    it('BindingTestSuite: Should return new Uint8ClampedArray from a c# byte array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;


      var clamped = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint8ClampedArrayFrom", []);
      assert.equal(clamped.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(clamped), "[object Uint8ClampedArray]", "TypedArray is not of type Uint8ClampedArray" )

    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return new Uint8Array from a c# byte array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint8ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint8Array]", "TypedArray is not of type Uint8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint16Array from a c# ushort array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint16ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint16Array]", "TypedArray is not of type Uint16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint32Array from a c# uint array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint32Array]", "TypedArray is not of type Uint32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int8Array from a c# sbyte array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int8ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int8Array]", "TypedArray is not of type Int8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int16Array from a c# short array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int16ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int16Array]", "TypedArray is not of type Int16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int32Array from a c# int array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int32Array]", "TypedArray is not of type Int32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float32Array from a c# float array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Float32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float32Array]", "TypedArray is not of type Float32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float64Array from a c# double array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Float64ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float64Array]", "TypedArray is not of type Float64Array" )
    }, DEFAULT_TIMEOUT);  
    
    it('BindingTestSuite: Should return Int8Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Int8Array()]);
      assert.equal(type, "Int8Array", "result does not match Int8Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint8Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint8Array()]);
      assert.equal(type, "Uint8Array", "result does not match Uint8Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint8ClampedArray type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint8ClampedArray()]);
      assert.equal(type, "Uint8ClampedArray", "result does not match Uint8ClampedArray.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Int16Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Int16Array()]);
      assert.equal(type, "Int16Array", "result does not match Int16Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint16Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint16Array()]);
      assert.equal(type, "Uint16Array", "result does not match Uint16Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Int32Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Int32Array()]);
      assert.equal(type, "Int32Array", "result does not match Int32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint32Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint32Array()]);
      assert.equal(type, "Uint32Array", "result does not match Uint32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Float32Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Float32Array()]);
      assert.equal(type, "Float32Array", "result does not match Float32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Float64Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Float64Array()]);
      assert.equal(type, "Float64Array", "result does not match Float64Array.");

    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return new Uint8ClampedArray from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;


      var clamped = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint8ClampedArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
      assert.equal(clamped.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(clamped), "[object Uint8ClampedArray]", "TypedArray is not of type Uint8ClampedArray" )

    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return new Uint8Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint8ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint8Array]", "TypedArray is not of type Uint8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint16Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint16ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
      assert.equal(arr.length, 25, "result does not match length of 25.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint16Array]", "TypedArray is not of type Uint16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint32Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint32ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
      assert.equal(arr.length, 10, "result does not match length of 10.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint32Array]", "TypedArray is not of type Uint32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int8Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int8ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int8Array]", "TypedArray is not of type Int8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int16Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int16ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
      assert.equal(arr.length, 20, "result does not match length of 20.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int16Array]", "TypedArray is not of type Int16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int32Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int32ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
      assert.equal(arr.length, 10, "result does not match length of 10.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int32Array]", "TypedArray is not of type Int32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float32Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Float32ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
      assert.equal(arr.length, 10, "result does not match length of 10.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float32Array]", "TypedArray is not of type Float32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float64Array from a SharedArrayBuffer.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Float64ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
      assert.equal(arr.length, 5, "result does not match length of 5.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float64Array]", "TypedArray is not of type Float64Array" )
    }, DEFAULT_TIMEOUT);  
    
    it('BindingTestSuite: Should return Sum of two int values from Function.Call.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:FunctionSumCall", [5,3]);
      assert.equal(result, 8, "result does not match value 8.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return Sum of two double values from Function.Call.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:FunctionSumCallD", [2,1.14]);
      assert.equal(result, 3.14, "result does not match value 3.14.");
    }, DEFAULT_TIMEOUT);  
    
    it('BindingTestSuite: Should return Sum of two int values from Function.Apply.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:FunctionSumApply", [5,3]);
      assert.equal(result, 8, "result does not match value 8.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return Sum of two double values from Function.Apply.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:FunctionSumApplyD", [2,1.14]);
      assert.equal(result, 3.14, "result does not match value 3.14.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return lowest-valued number passed to JavaScript Math.min.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:FunctionMathMin", [[5, 6, 2, 3, 7]]);
      assert.equal(result, 2, "result does not match value 2.");
    }, DEFAULT_TIMEOUT);  
  });
