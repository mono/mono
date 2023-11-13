//describe, beforeAll, it, expect - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Issues Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;

    beforeAll(function(done){
      //load DOM custom matchers from karma-jasmine-dom package
      jasmine.addMatchers(DOMCustomMatchers);
      
      //lets open our 'http-spec.html' file in the browser by 'index' tag as you specified in 'karma.conf.js'
      karmaHTML.issuesspec.open();
      
      //karmaHTML.issuesspec.onstatechange fires when the Document is loaded
      //now the tests can be executed on the DOM
      karmaHTML.issuesspec.onstatechange = function(ready){
        //if the #Document is ready, fire tests
        //the done() callback is the jasmine native async-support function
        if(ready) {
          karmaHTML.issuesspec.document.onRuntimeDone = function ()
          {
            done();
          }

        }
      };

    });

    it('IssuesTestSuite: https://github.com/mono/mono/issues/12981 Interpreter Recursion', () => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;


      _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:BugInterpRecursion", [null]);
      expect(_document.constructor.name).toEqual('HTMLDocument'); // really nothing to assert here except if a recursion occurs.

    }, DEFAULT_TIMEOUT);  

    it('IssuesTestSuite: https://github.com/mono/mono/issues/13881 32 bit enum flag values upon reflection', () => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;


      _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:Issue13881", []);
      expect(_document.constructor.name).toEqual('HTMLDocument'); // really nothing to assert here except if an error occurs.

    }, DEFAULT_TIMEOUT);  


    it('IssuesTestSuite: https://github.com/mono/mono/issues/13428 Mysterious phenomenon of using Math.Truncate.', () => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;


      var doublevalue = _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:Issue13428", []);
      assert.equal(doublevalue, 20, "result doesn't match 20");

    }, DEFAULT_TIMEOUT); 
    
    it('IssuesTestSuite: https://github.com/dotnet/try/issues/290 try.net Math.Truncate.', () => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;


      var doublevalue = _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:IssueTry290", []);
      assert.equal(doublevalue, 11, "result doesn't match 11");

    }, DEFAULT_TIMEOUT);

    it('IssuesTestSuite: https://github.com/mono/mono/issues/14940 should not crash when issuing two fetchs back to back.', (done) => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;
      _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:IssueDoubleFetch", []).then(
        (result) => 
        {
            try {
              assert.equal(result.length, 2, "result does not match Fetch Issue of 2.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT);  

    it('IssuesTestSuite: https://github.com/mono/mono/issues/14940 should not crash when retrieving headers issuing two fetchs back to back.', (done) => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;
      _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:IssueDoubleFetchHeaders", []).then(
        (result) => 
        {
            try {
              assert.equal(result.length, 2, "result does not match number of headers 2.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
    }, DEFAULT_TIMEOUT);  
    it('IssuesTestSuite: https://github.com/mono/mono/issues/12917 IL Linker not working correctly with IQueryable extensions.', () => {
      //karmaHTML.issuesspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.issuesspec.document;


      var ret = _document.Module.BINDING.call_static_method("[IssuesTestSuite]TestSuite.Program:IssueIQueryable", []);
      assert.equal(ret, 4, "result doesn't match 4");

    }, DEFAULT_TIMEOUT); 


  });
