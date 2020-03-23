//describe, beforeAll, it, expect - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Http Streaming Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;
    const DEFAULT_WS_TIMEOUT = 5000;

    beforeAll(function(done){
      //load DOM custom matchers from karma-jasmine-dom package
      jasmine.addMatchers(DOMCustomMatchers);
      
      //lets open our 'http-stream-spec.html' file in the browser by 'index' tag as you specified in 'karma.conf.js'
      karmaHTML.httpstreamspec.open();
      
      //karmaHTML.httpstreamspec.onstatechange fires when the Document is loaded
      //now the tests can be executed on the DOM
      karmaHTML.httpstreamspec.onstatechange = function(ready){
        //if the #Document is ready, fire tests
        //the done() callback is the jasmine native async-support function
        if(ready) {
          karmaHTML.httpstreamspec.document.onRuntimeDone = function ()
          {
            done();
          }

        }
      };

    });
    
    it("should be a real Document object",function(){
      var _document = karmaHTML.httpstreamspec.document;
      expect(_document.constructor.name).toEqual('HTMLDocument');
    });

    it('should support streaming', () => {
      //karmaHTML.httpstreamspec.document gives the access to the Document object of 'http-stream-spec.html' file
      var _document = karmaHTML.httpstreamspec.document;

      assert.equal(_document.Module.BINDING.call_static_method("[HttpStreamingTestSuite]TestSuite.Program:IsStreamingSupported", []), true);
    });    


    it('should have streaming enabled', () => {
      //karmaHTML.httpstreamspec.document gives the access to the Document object of 'http-stream-spec.html' file
      var _document = karmaHTML.httpstreamspec.document;

      assert.equal(_document.Module.BINDING.call_static_method("[HttpStreamingTestSuite]TestSuite.Program:IsStreamingEnabled", []), true);
    });    

 
  });
