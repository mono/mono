//describe, beforeAll, it, expect - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Http Test Suite",function(){
    
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
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestStream", [true, "base/publish/netstandard2.0/NowIsTheTime.txt"]).then(
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
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestByteArray", [true, "base/publish/netstandard2.0/NowIsTheTime.txt"]).then(
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
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestStream", [false, "base/publish/netstandard2.0/NowIsTheTime.txt"]).then(
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
      
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestByteArray", [false, "base/publish/netstandard2.0/NowIsTheTime.txt"]).then(
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

    it('RequestMessageWith: should override credentials with FetchCredentialsOption omit.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["FetchCredentialsOption", "Omit"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should override credentials with FetchCredentialsOption same-origin.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["FetchCredentialsOption", "same-origin"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should override credentials with FetchCredentialsOption include.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["FetchCredentialsOption", "include"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should throw invalid credentials with FetchCredentialsOption foo.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["FetchCredentialsOption", "foo"]).then(
        (result) => 
        {
          done.fail("Should have thrown invalid value.");
        },
        (error) => {
          expect(error).toContain("Http.HttpClient.FetchCredentialsOption");
          done();
        }

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should override cache with RequestCache default.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "DEFAULT"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should override cache with RequestCache no-store.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "no-store"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)    

    it('RequestMessageWith: should override cache with RequestCache reload.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "ReLoad"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)    

    it('RequestMessageWith: should override cache with RequestCache no-cache.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "no-cache"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)    

    it('RequestMessageWith: should override cache with RequestCache force-cache.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "force-cache"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)    

    it('RequestMessageWith: should override cache with RequestCache only-if-cached.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestMode;RequestCache", "same-origin;only-if-cached"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)
    
    it('RequestMessageWith: should throw invalid cache with RequestCache only-if-cached and RequestMode cors default.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "only-if-cached"]).then(
        (result) => 
        {
          done.fail("Should have thrown invalid value.");
        },
        (error) => {
          expect(error).toContain("TypeError: Failed to execute 'fetch' on 'Window': 'only-if-cached' can be set only with 'same-origin' mode");
          done();
        }

      );
    }, DEFAULT_TIMEOUT)    

    it('RequestMessageWith: should throw invalid cache with RequestCache foo.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestCache", "foo"]).then(
        (result) => 
        {
          done.fail("Should have thrown invalid value.");
        },
        (error) => {
          expect(error).toContain("Http.HttpClient.RequestCache");
          done();
        }

      );
    }, DEFAULT_TIMEOUT)    

    it('RequestMessageWith: should override mode with RequestMode same-origin.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestMode", "same-origin"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should override mode with RequestMode no-cors.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestMode", "no-cors"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should override mode with RequestMode Cors.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestMode", "Cors"]).then(
        (result) => 
        {
            try {
              assert.equal(result, 500000, "result doesn't match length");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT)

    it('RequestMessageWith: should throw invalid cache with RequestMode Navigate.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestMode", "Navigate"]).then(
        (result) => 
        {
          done.fail("Should have thrown invalid value.");
        },
        (error) => {
          expect(error).toContain("TypeError: Failed to execute 'fetch' on 'Window': Cannot construct a Request with a RequestInit whose mode member is set as 'navigate'.");
          done();
        }

      );
    }, DEFAULT_TIMEOUT)   
    

    it('RequestMessageWith: should throw invalid cache with RequestMode foo.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;
      _document.Module.BINDING.call_static_method("[HttpTestSuite]TestSuite.Program:RequestMessageWith", ["RequestMode", "foo"]).then(
        (result) => 
        {
          done.fail("Should have thrown invalid value.");
        },
        (error) => {
          expect(error).toContain("Http.HttpClient.RequestMode");
          done();
        }

      );
    }, DEFAULT_TIMEOUT)   

  });
