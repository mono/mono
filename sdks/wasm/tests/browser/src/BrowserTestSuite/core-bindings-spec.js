//describe, beforeAll, it, expect - are the Jasmine default methods
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


      var clamped = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint8ClampedArrayFrom", []);
      assert.equal(clamped.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(clamped), "[object Uint8ClampedArray]", "TypedArray is not of type Uint8ClampedArray" )

    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return new Uint8Array from a c# byte array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint8ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint8Array]", "TypedArray is not of type Uint8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint16Array from a c# ushort array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint16ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint16Array]", "TypedArray is not of type Uint16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint32Array from a c# uint array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint32Array]", "TypedArray is not of type Uint32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int8Array from a c# sbyte array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Int8ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int8Array]", "TypedArray is not of type Int8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int16Array from a c# short array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Int16ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int16Array]", "TypedArray is not of type Int16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int32Array from a c# int array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Int32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int32Array]", "TypedArray is not of type Int32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float32Array from a c# float array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Float32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float32Array]", "TypedArray is not of type Float32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float64Array from a c# double array.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Float64ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float64Array]", "TypedArray is not of type Float64Array" )
    }, DEFAULT_TIMEOUT);  
    
    it('BindingTestSuite: Should return Int8Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Int8Array()]);
      assert.equal(type, "Int8Array", "result does not match Int8Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint8Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Uint8Array()]);
      assert.equal(type, "Uint8Array", "result does not match Uint8Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint8ClampedArray type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Uint8ClampedArray()]);
      assert.equal(type, "Uint8ClampedArray", "result does not match Uint8ClampedArray.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Int16Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Int16Array()]);
      assert.equal(type, "Int16Array", "result does not match Int16Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint16Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Uint16Array()]);
      assert.equal(type, "Uint16Array", "result does not match Uint16Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Int32Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Int32Array()]);
      assert.equal(type, "Int32Array", "result does not match Int32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint32Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Uint32Array()]);
      assert.equal(type, "Uint32Array", "result does not match Uint32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Float32Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Float32Array()]);
      assert.equal(type, "Float32Array", "result does not match Float32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Float64Array type.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:TypedArrayType", [new Float64Array()]);
      assert.equal(type, "Float64Array", "result does not match Float64Array.");

    }, DEFAULT_TIMEOUT);  

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Uint8ClampedArray from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {
      it('BindingTestSuite: Should return new Uint8ClampedArray from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined") {}
          
        else {
          var _document = karmaHTML.corebindingsspec.document;


          var clamped = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint8ClampedArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
          assert.equal(clamped.length, 50, "result does not match length of 50.");
          assert.equal(Object.prototype.toString.call(clamped), "[object Uint8ClampedArray]", "TypedArray is not of type Uint8ClampedArray" )
        }
      }, DEFAULT_TIMEOUT);  
    }

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Uint8Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {

      it('BindingTestSuite: Should return new Uint8Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint8ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
        assert.equal(arr.length, 50, "result does not match length of 50.");
        assert.equal(Object.prototype.toString.call(arr), "[object Uint8Array]", "TypedArray is not of type Uint8Array" )

      }, DEFAULT_TIMEOUT);    
    }

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Uint16Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {

      it('BindingTestSuite: Should return new Uint16Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint16ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
        assert.equal(arr.length, 25, "result does not match length of 25.");
        assert.equal(Object.prototype.toString.call(arr), "[object Uint16Array]", "TypedArray is not of type Uint16Array" )

      }, DEFAULT_TIMEOUT);    

      it('BindingTestSuite: Should return new Uint32Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Uint32ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
        assert.equal(arr.length, 10, "result does not match length of 10.");
        assert.equal(Object.prototype.toString.call(arr), "[object Uint32Array]", "TypedArray is not of type Uint32Array" )

      }, DEFAULT_TIMEOUT); 
    }   

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Int8Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {

      it('BindingTestSuite: Should return new Int8Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Int8ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(50)]);
        assert.equal(arr.length, 50, "result does not match length of 50.");
        assert.equal(Object.prototype.toString.call(arr), "[object Int8Array]", "TypedArray is not of type Int8Array" )

      }, DEFAULT_TIMEOUT);  
    }  

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Int16Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {
      it('BindingTestSuite: Should return new Int16Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Int16ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
        assert.equal(arr.length, 20, "result does not match length of 20.");
        assert.equal(Object.prototype.toString.call(arr), "[object Int16Array]", "TypedArray is not of type Int16Array" )

      }, DEFAULT_TIMEOUT);   
    } 

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Int32Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {
      it('BindingTestSuite: Should return new Int32Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Int32ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
        assert.equal(arr.length, 10, "result does not match length of 10.");
        assert.equal(Object.prototype.toString.call(arr), "[object Int32Array]", "TypedArray is not of type Int32Array" )

      }, DEFAULT_TIMEOUT);    
    }

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Float32Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {
      it('BindingTestSuite: Should return new Float32Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();
        
        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Float32ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
        assert.equal(arr.length, 10, "result does not match length of 10.");
        assert.equal(Object.prototype.toString.call(arr), "[object Float32Array]", "TypedArray is not of type Float32Array" )

      }, DEFAULT_TIMEOUT);    
    }

    if (typeof SharedArrayBuffer === "undefined") 
    {
      xit('BindingTestSuite: Should return new Float32Array from a SharedArrayBuffer.', () => {
      }, DEFAULT_TIMEOUT); 
    }
    else {
      it('BindingTestSuite: Should return new Float64Array from a SharedArrayBuffer.', () => {
        //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
        if (typeof SharedArrayBuffer === "undefined")
          this.skip();

        var _document = karmaHTML.corebindingsspec.document;

        var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:Float64ArrayFromSharedArrayBuffer", [new SharedArrayBuffer(40)]);
        assert.equal(arr.length, 5, "result does not match length of 5.");
        assert.equal(Object.prototype.toString.call(arr), "[object Float64Array]", "TypedArray is not of type Float64Array" )
      }, DEFAULT_TIMEOUT);  
    }
    
    it('BindingTestSuite: Should return Sum of two int values from Function.Call.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:FunctionSumCall", [5,3]);
      assert.equal(result, 8, "result does not match value 8.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return Sum of two double values from Function.Call.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:FunctionSumCallD", [2,1.14]);
      assert.equal(result, 3.14, "result does not match value 3.14.");
    }, DEFAULT_TIMEOUT);  
    
    it('BindingTestSuite: Should return Sum of two int values from Function.Apply.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:FunctionSumApply", [5,3]);
      assert.equal(result, 8, "result does not match value 8.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return Sum of two double values from Function.Apply.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:FunctionSumApplyD", [2,1.14]);
      assert.equal(result, 3.14, "result does not match value 3.14.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return lowest-valued number passed to JavaScript Math.min.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:FunctionMathMin", [[5, 6, 2, 3, 7]]);
      assert.equal(result, 2, "result does not match value 2.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return new DataView.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view2 = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewConstructor", []);
      assert.equal(view2.getInt8(0), 42, "result does not match value 42.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: DataView ArrayBuffer should be equal.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
      var buffer = new ArrayBuffer(12);
      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewArrayBuffer", [buffer]);
      assert.isTrue(view.buffer === buffer, "result is not true.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: DataView byteLength.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
      var buffer = new ArrayBuffer(12);
      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewByteLength", [buffer]);
      assert.equal(view.byteLength, 2, "result does not match value 2.");
    }, DEFAULT_TIMEOUT);  
    it('BindingTestSuite: DataView byteOffset.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
      var buffer = new ArrayBuffer(12);
      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewByteOffset", [buffer]);
      assert.equal(view.byteOffset, 4, "result does not match value 4.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetFloat32.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setFloat32(1, Math.PI);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetFloat32", [x]);
      assert.equal(result.toFixed(5), Math.PI.toFixed(5), "result does not match value " + Math.PI.toFixed(5));
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetFloat64.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setFloat64(1, Math.PI);      
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetFloat64", [x]);
      assert.equal(result.toFixed(5), Math.PI.toFixed(5), "result does not match value " + Math.PI.toFixed(5));
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetInt16 positive.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setInt16(1, 1234);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetInt16", [x]);
      assert.equal(result, 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetInt16 negative.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setInt16(1, -1234);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetInt16", [x]);
      assert.equal(result, -1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetInt32 positive.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setInt32(1, 1234);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetInt32", [x]);
      assert.equal(result, 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetInt32 negative.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setInt32(1, -1234);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetInt32", [x]);
      assert.equal(result, -1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetInt8 positive.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setInt8(1, 123);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetInt8", [x]);
      assert.equal(result, 123, "result does not match value 123.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetInt8 negative.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setInt8(1, -123);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetInt8", [x]);
      assert.equal(result, -123, "result does not match value 123.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetUint16.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setUint16(1, 1234);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetUint16", [x]);
      assert.equal(result, 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetUint32.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setUint32(1, 1234);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetUint32", [x]);
      assert.equal(result, 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView GetUint8.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var x = new DataView(new ArrayBuffer(12), 0);
      x.setUint8(1, 123);
      var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewGetUint8", [x]);
      assert.equal(result, 123, "result does not match value 123.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetFloat32.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetFloat32", []);
      assert.equal(view.getFloat32(1).toFixed(5), Math.PI.toFixed(5), "result does not match value " + Math.PI.toFixed(5));
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetFloat64.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetFloat64", []);
      assert.equal(view.getFloat64(1).toFixed(5), Math.PI.toFixed(5), "result does not match value " + Math.PI.toFixed(5));
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetInt16.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetInt16", []);
      assert.equal(view.getInt16(1), 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetInt32.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetInt32", []);
      assert.equal(view.getInt32(1), 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetInt8.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetInt8", []);
      assert.equal(view.getInt8(1), 123, "result does not match value 123.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetUint16.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetUint16", []);
      assert.equal(view.getUint16(1), 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetUint32.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetUint32", []);
      assert.equal(view.getUint32(1), 1234, "result does not match value 1234.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return DataView SetUint8.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var view = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:DataViewSetUint8", []);
      assert.equal(view.getUint8(1), 123, "result does not match value 123.");
    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Pop should return undefined if Array is empty.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;

      var pop = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ArrayPop", []);
      assert.equal(pop, undefined, "result does not match expected value unidentified.");
      assert.equal(pop, null, "result does not match expected value null.");
    }, DEFAULT_TIMEOUT); 

    it('BindingTestSuite: Should error with Parameter count mismatch #1.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
      try {
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ParameterTest", ["hello"]);
        assert.equal(result, 0, "result should have been Parameter count mismatch.");
      } catch (e) {
      }
    }, DEFAULT_TIMEOUT); 

    it('BindingTestSuite: Should error with Parameter count mismatch #2.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
      try {
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ParameterTest2", []);
        assert.equal(result, 0, "result should have been Parameter count mismatch.");
      } catch (e) {
      }
    }, DEFAULT_TIMEOUT); 

    it('BindingTestSuite: Should error with Parameter count mismatch #3.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
      try {
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ParameterTest2");
        assert.equal(result, 0, "result should have been Parameter count mismatch.");
      } catch (e) {
      }
    }, DEFAULT_TIMEOUT); 

    it('BindingTestSuite: Should NOT error with Parameter count mismatch #1.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ParameterTest", []);
        assert.equal(result, -1, "result does not match expected result.");
    }, DEFAULT_TIMEOUT); 

    it('BindingTestSuite: Should NOT error with Parameter count mismatch #2.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ParameterTest");
        assert.equal(result, -1, "result does not match expected result.");
    }, DEFAULT_TIMEOUT); 

    it('BindingTestSuite: Should NOT error with Parameter count mismatch #3.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:ParameterTest2", [null]);
        assert.equal(result, -1, "result does not match expected result.");
    }, DEFAULT_TIMEOUT);

    it('BindingTestSuite: Should marshal null string argument as null.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:StringIsNull", [null]);
        assert.isTrue(result);
    }, DEFAULT_TIMEOUT);     

    it('BindingTestSuite: Should return true for string.IsNullOrEmpty.', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:StringIsNullOrEmpty", [null]);
        assert.isTrue(result);
    }, DEFAULT_TIMEOUT);     

    it('BindingTestSuite: Should return true for null string[].', () => {
      //karmaHTML.corebindingsspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.corebindingsspec.document;
        var result = _document.Module.BINDING.call_static_method("[BindingsTestSuite]BindingsTestSuite.Program:StringArrayIsNull", [null]);
        assert.isTrue(result);
    }, DEFAULT_TIMEOUT);     
  });
