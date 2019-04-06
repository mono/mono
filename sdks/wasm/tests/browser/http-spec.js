//describe, beforeAll, it, expext - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Browser Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;
    const DEFAULT_WS_TIMEOUT = 5000;

    beforeAll(function(done){
      //load DOM custom matchers from karma-jasmine-dom package
      jasmine.addMatchers(DOMCustomMatchers);
      
      //lets open our 'http-spec.html' file in the browser by 'index' tag as you specified in 'karma.conf.js'
      karmaHTML.httpspec.open();
      
      //karmaHTML.httpspec.onstatechange fires when the Document is loaded
      //now the tests can be executed on the DOM
      karmaHTML.httpspec.onstatechange = function(ready){
        //if the #Document is ready, fire tests
        //the done() callback is the jasmine native async-support function
        if(ready) {
          karmaHTML.httpspec.document.onRuntimeDone = function ()
          {
            done();
          }

        }
      };

    });
    
    it("should be a real Document object",function(){
      var _document = karmaHTML.httpspec.document;
      expect(_document.constructor.name).toEqual('HTMLDocument');
    });

    it('should support streaming', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      assert.equal(_document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:IsStreamingSupported", []), true);
    });    


    it('should have streaming enabled', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      assert.equal(_document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:IsStreamingEnabled", []), true);
    });    

    it('should have base path', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      assert.equal(_document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:BasePath", []), "http://localhost:9876/");
    });    

    it('RequestStream: should return size of Stream with streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestStream", [true, "base/publish/NowIsTheTime.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('RequestStream: blob should return size of Stream with streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      var blob = new Blob([JSON.stringify({hello: "world"}, null, 2)], {type : 'application/json'});
      var blobUrl = URL.createObjectURL(blob);
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestStream", [true, blobUrl]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 22, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
      URL.revokeObjectURL(blobUrl);
    }, DEFAULT_TIMEOUT);

    it('RequestByteArray: should return size of ByteArray with streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestByteArray", [true, "base/publish/NowIsTheTime.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('RequestByteArray: blob should return size of ByteArray with streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      var blob = new Blob([JSON.stringify({hello: "world"}, null, 2)], {type : 'application/json'});
      var blobUrl = URL.createObjectURL(blob);

      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestByteArray", [true, blobUrl]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 22, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
      URL.revokeObjectURL(blobUrl);
    }, DEFAULT_TIMEOUT);

    it('RequestStream: should return size of Stream without streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestStream", [false, "base/publish/NowIsTheTime.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('RequestStream: blob should return size of Stream without streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      var blob = new Blob([JSON.stringify({hello: "world"}, null, 2)], {type : 'application/json'});
      var blobUrl = URL.createObjectURL(blob);

      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestStream", [false, blobUrl]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 22, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
      URL.revokeObjectURL(blobUrl);
    }, DEFAULT_TIMEOUT);    

    it('RequestByteArray: should return size of ByteArray without streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestByteArray", [false, "base/publish/NowIsTheTime.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('RequestByteArray: blob should return size of ByteArray without streaming', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      var blob = new Blob([JSON.stringify({hello: "world"}, null, 2)], {type : 'application/json'});
      var blobUrl = URL.createObjectURL(blob);

      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestByteArray", [false, blobUrl]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 22, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
      URL.revokeObjectURL(blobUrl);
    }, DEFAULT_TIMEOUT);

    it('GetStreamAsync_ReadZeroBytes_Success: should return 0', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:GetStreamAsync_ReadZeroBytes_Success", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 0, "result doesn't match expected result 0");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);  

    it('ConnectWebSocketStatus: should return Error Code 1006 because unresolved host.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:ConnectWebSocketStatus", ["ws://localhost", ""]).then(
        (result) => 
        {
            try {
              assert.equal(result, '1006', "result doesn't match expected result 1006.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_WS_TIMEOUT); 
       
    it('ConnectWebSocketStatus: should return Error Code 1006 because of invalid protocol.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:ConnectWebSocketStatus", ["ws://localhost:8889", ""]).then(
        (result) => 
        {
            try {
              assert.equal(result, '1006', "result doesn't match expected result 1006.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_WS_TIMEOUT); 

    it('ConnectWebSocketStatusWithToken: should return Error Code 1006 because unresolved host.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:ConnectWebSocketStatusWithToken", ["ws://localhost", ""]).then(
        (result) => 
        {
            try {
              assert.equal(result, '1006', "result doesn't match expected result 1006.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_WS_TIMEOUT); 
       
    it('ConnectWebSocketStatusWithToken: should return Error Code 1006 because of invalid protocol.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:ConnectWebSocketStatusWithToken", ["ws://localhost:8889", ""]).then(
        (result) => 
        {
            try {
              assert.equal(result, '1006', "result doesn't match expected result 1006.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_WS_TIMEOUT); 

    it('OpenWebSocket: should return Open.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:OpenWebSocket", ["ws://localhost:8889", "echo-protocol"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 'Open', "result doesn't match expected result Open.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);  
    
    it('CloseWebSocket: should return Closed.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:CloseWebSocket", ["ws://localhost:8889", "echo-protocol"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 'Closed', "result doesn't match expected result Closed.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

        
    it('RecieveHostCloseWebSocket: should return Closed.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:RecieveHostCloseWebSocket", ["ws://localhost:8889", "echo-protocol"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 'Closed', "result doesn't match expected result Closed.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('CloseStatusDescCloseWebSocket: should return Close Code and Description.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:CloseStatusDescCloseWebSocket", ["ws://localhost:8889", "echo-protocol"]).then(
        (result) => 
        {
            try {
              var resultObj = JSON.parse(result);
              assert.equal(resultObj.code, 'NormalClosure', "code result doesn't match expected result NormalClosure.");
              assert.equal(resultObj.desc, 'bye!', "description result doesn't match expected result 'bye!'.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('WebSocketSendText: should return echoed text.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:WebSocketSendText", ["ws://localhost:8889", "echo-protocol", "Hello WebSockets"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 'Hello WebSockets', "result does not match Hello WebSockets.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('WebSocketSendBinary: should return echoed text.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      var binBuffer = new Uint8Array([49,50,51,52,53,54,55,56,57])
      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:WebSocketSendBinary", ["ws://localhost:8889", "echo-protocol", "Hello WebSockets"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 'Hello WebSockets', "result does not match Hello WebSockets.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_WS_TIMEOUT);  

    it('BindingTestSuite: Should return new Uint8ClampedArray from a c# byte array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;


      var clamped = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint8ClampedArrayFrom", []);
      assert.equal(clamped.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(clamped), "[object Uint8ClampedArray]", "TypedArray is not of type Uint8ClampedArray" )

    }, DEFAULT_TIMEOUT);  

    it('BindingTestSuite: Should return new Uint8Array from a c# byte array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint8ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint8Array]", "TypedArray is not of type Uint8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint16Array from a c# ushort array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint16ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint16Array]", "TypedArray is not of type Uint16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Uint32Array from a c# uint array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Uint32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Uint32Array]", "TypedArray is not of type Uint32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int8Array from a c# sbyte array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int8ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int8Array]", "TypedArray is not of type Int8Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int16Array from a c# short array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int16ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int16Array]", "TypedArray is not of type Int16Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Int32Array from a c# int array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Int32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Int32Array]", "TypedArray is not of type Int32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float32Array from a c# float array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Float32ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float32Array]", "TypedArray is not of type Float32Array" )

    }, DEFAULT_TIMEOUT);    

    it('BindingTestSuite: Should return new Float64Array from a c# double array.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var arr = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:Float64ArrayFrom", []);
      assert.equal(arr.length, 50, "result does not match length of 50.");
      assert.equal(Object.prototype.toString.call(arr), "[object Float64Array]", "TypedArray is not of type Float64Array" )
    }, DEFAULT_TIMEOUT);  
    
    it('BindingTestSuite: Should return Int8Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Int8Array()]);
      assert.equal(type, "Int8Array", "result does not match Int8Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint8Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint8Array()]);
      assert.equal(type, "Uint8Array", "result does not match Uint8Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint8ClampedArray type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint8ClampedArray()]);
      assert.equal(type, "Uint8ClampedArray", "result does not match Uint8ClampedArray.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Int16Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Int16Array()]);
      assert.equal(type, "Int16Array", "result does not match Int16Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint16Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint16Array()]);
      assert.equal(type, "Uint16Array", "result does not match Uint16Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Int32Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Int32Array()]);
      assert.equal(type, "Int32Array", "result does not match Int32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Uint32Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Uint32Array()]);
      assert.equal(type, "Uint32Array", "result does not match Uint32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Float32Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Float32Array()]);
      assert.equal(type, "Float32Array", "result does not match Float32Array.");

    }, DEFAULT_TIMEOUT);    
    
    it('BindingTestSuite: Should return Float64Array type.', () => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      var type = _document.Module.BINDING.call_static_method("[BindingsTestSuite]TestSuite.Program:TypedArrayType", [new Float64Array()]);
      assert.equal(type, "Float64Array", "result does not match Float64Array.");

    }, DEFAULT_TIMEOUT);    

    
  });
