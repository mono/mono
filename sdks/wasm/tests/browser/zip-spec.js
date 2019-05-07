//describe, beforeAll, it, expect - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Zip Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;
    const DEFAULT_WS_TIMEOUT = 5000;

    beforeAll(function(done){
      //load DOM custom matchers from karma-jasmine-dom package
      jasmine.addMatchers(DOMCustomMatchers);
      
      //lets open our 'http-spec.html' file in the browser by 'index' tag as you specified in 'karma.conf.js'
      karmaHTML.zipspec.open();
      
      //karmaHTML.zipspec.onstatechange fires when the Document is loaded
      //now the tests can be executed on the DOM
      karmaHTML.zipspec.onstatechange = function(ready){
        //if the #Document is ready, fire tests
        //the done() callback is the jasmine native async-support function
        if(ready) {
          karmaHTML.zipspec.document.onRuntimeDone = function ()
          {
            done();
          }

        }
      };

    });
    
    it('ZipGetEntryReadMode: entry for foo.txt should exist', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryReadMode", ["foo.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isDefined(result, "result should not be undefined.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryReadMode: entry for foobar.txt should not exist', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryReadMode", ["foobar.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isUndefined(result, "result should be undefined but it seems we have an entry.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);
    
  });
