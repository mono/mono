//describe, beforeAll, it, expext - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Browser Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;

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

    it('RequestByteArray: should return size of ByteArrray with streaming', (done) => {
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

    it('RequestByteArray: blob should return size of ByteArrray with streaming', (done) => {
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

    it('RequestByteArray: should return size of ByteArrray without streaming', (done) => {
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

    it('RequestByteArray: blob should return size of ByteArrray without streaming', (done) => {
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

  });
