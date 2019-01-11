const puppeteer = require('puppeteer');
process.env.CHROME_BIN = puppeteer.executablePath();

module.exports = function(config) {
    config.set({

        mime: {
            'application/wasm': ['wasm']
        },
        files: [
            {pattern: './publish/http-spec.js',watched:true,served:true,included:true},
            {pattern: './publish/mono.wasm', included: false, served: true, type: 'wasm'},
            {pattern: './publish/mono.js', included: false, served: true},
            {pattern: './publish/mono-config.js', included: false, served: true},
            {pattern: './publish/runtime.js', included: false, served: true},
            {pattern: './publish/managed/*.dll', included: false, served: true},
            {pattern: './publish/**/*.txt', included: false, served: true, type: 'text'},
        ],
        //load karma-mocha-reporter and karma-html
        reporters: ['mocha','karmaHTML', 'dots', 'junit'], //, 'progress'],
        // the default configuration
        junitReporter: {
            outputDir: '', // results will be saved as $outputDir/$browserName.xml
            outputFile: 'test-results.xml', // if included, results will be saved as $outputDir/$browserName/$outputFile
            suite: '', // suite will become the package name attribute in xml testsuite element
            useBrowserName: false, // add browser name to report and classes names
            nameFormatter: undefined, // function (browser, result) to customize the name attribute in xml testcase element
            classNameFormatter: undefined, // function (browser, result) to customize the classname attribute in xml testcase element
            properties: {} // key value pair of properties to add to the <properties> section of the report
        },
        //load karma-jasmine-dom and karma-jasmine
        frameworks: ['jasmine-dom','jasmine','mocha', 'chai'],
        //load karma-chrome-launcher
        browsers: ['ChromeHeadless'],
        logLevel: config.LOG_INFO,
        client: {
            //If false, Karma will not remove iframes upon the completion of running the tests
            clearContext:false,
            //karma-html configuration
            karmaHTML: {
                source: [
                    //indicate 'index.html' file that will be loaded in the browser
                    //the 'index' tag will be used to get the access to the Document object of 'index.html'
                    {src:'./publish/http-spec.html', tag:'httpspec'}
                ],
                auto: true,
                timeout: 10000,
                abort: 60000,
                width: "730px",
                height: "30vw"        

            }
        }
    });
  };