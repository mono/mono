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


    it('ReceiveHostCloseWebSocket: should return Closed.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:ReceiveHostCloseWebSocket", ["ws://localhost:8889", "echo-protocol"]).then(
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

    it('WebSocketSendTextPartial: should return echoed text.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:WebSocketSendTextPartial", ["ws://localhost:8889", "echo-protocol", "Hello WebSockets"]).then(
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

    it('WebSocketSendBinaryPartial: should return echoed text.', (done) => {
      //karmaHTML.httpspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.httpspec.document;

      _document.Module.BINDING.call_static_method("[WebSocketTestSuite]TestSuite.Program:WebSocketSendBinaryPartial", ["ws://localhost:8889", "echo-protocol", "Hello WebSockets"]).then(
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
  });
