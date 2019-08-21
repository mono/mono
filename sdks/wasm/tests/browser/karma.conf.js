const puppeteer = require('puppeteer');
process.env.CHROME_BIN = puppeteer.executablePath();

module.exports = function(config) {
    config.set({

        mime: {
            'application/wasm': ['wasm']
        },
        files: [
            {pattern: './publish/netstandard2.0/http-spec.js',watched:true,served:true,included:true},
            {pattern: './publish/netstandard2.0/core-bindings-spec.js',watched:true,served:true,included:true},
            {pattern: './publish/netstandard2.0/issues-spec.js',watched:true,served:true,included:true},
            {pattern: './publish/netstandard2.0/zip-spec.js',watched:true,served:true,included:true},
            {pattern: './publish/netstandard2.0/mono.wasm', included: false, served: true, type: 'wasm'},
            {pattern: './publish/netstandard2.0/mono.worker.js', included: false, served: true, type: 'wasm'},
            {pattern: './publish/netstandard2.0/mono.js.mem', included: false, served: true, type: 'wasm'},
            {pattern: './publish/netstandard2.0/mono.js', included: false, served: true},
            {pattern: './publish/netstandard2.0/mono-config.js', included: false, served: true},
            {pattern: './publish/netstandard2.0/runtime.js', included: false, served: true},
            {pattern: './publish/netstandard2.0/managed/*.dll', included: false, served: true},
            {pattern: './publish/netstandard2.0/managed/*.pdb', included: false, served: true},
            {pattern: './publish/netstandard2.0/**/*.txt', included: false, served: true, type: 'text'},
            {pattern: './publish/netstandard2.0/**/*.zip', included: false, served: true, type: 'zip'},
            {pattern: './publish/netstandard2.0/**/*.nupkg', included: false, served: true, type: 'zip'},
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
        frameworks: ['jasmine-dom','jasmine','mocha', 'chai', 'websocket-server'],
        //load karma-chrome-launcher
        browsers: ['ChromeHeadless', 'NoSandBoxHeadless'],
        customLaunchers: {
            NoSandBoxHeadless: {
                base: 'ChromeHeadless',
                flags: ['--no-sandbox']
            }
        },
        logLevel: config.LOG_INFO,
        client: {
            //If false, Karma will not remove iframes upon the completion of running the tests
            clearContext:false,
            //karma-html configuration
            karmaHTML: {
                source: [
                    //indicate 'index.html' file that will be loaded in the browser
                    //the 'index' tag will be used to get the access to the Document object of 'index.html'
                    {src:'./publish/netstandard2.0/http-spec.html', tag:'httpspec'},
                    {src:'./publish/netstandard2.0/core-bindings-spec.html', tag:'corebindingsspec'},
                    {src:'./publish/netstandard2.0/issues-spec.html', tag:'issuesspec'},
                    {src:'./publish/netstandard2.0/zip-spec.html', tag:'zipspec'}
                ],
                timeout: 10000,
                abort: 60000,
                width: "730px",
                height: "30vw"        

            }
        },
        websocketServer: {
            port: 8889,
            beforeStart: (server) => {
                function originIsAllowed(origin) {
                    // put logic here to detect whether the specified origin is allowed.
                    return true;
                  }
            server.on('request', (request) => {
            //     console.log(new Date() + ' new websocket request...');
                if (!originIsAllowed(request.origin)) {
                    // Make sure we only accept requests from an allowed origin
                    request.reject();
                    console.log((new Date()) + ' Connection from origin ' + request.origin + ' rejected.');
                    return;
                }
                
                try 
                {
                    var connection = request.accept('echo-protocol', request.origin);
                    console.log((new Date()) + ' Connection accepted.');
                    connection.on('message', function(message) {
                        if (message.type === 'utf8') {
                            if (message.utf8Data === "closeme")
                            {
                                connection.close(1000, "bye!");
                            }
                            else {
                                console.log('Received Message: ' + message.utf8Data);
                                connection.sendUTF(message.utf8Data);
                            }
                        }
                        else if (message.type === 'binary') {
                            console.log('Received Binary Message of ' + message.binaryData.length + ' bytes');
                            connection.sendBytes(message.binaryData);
                        }
                    });
                    connection.on('close', function(reasonCode, description) {
                        console.log((new Date()) + ' Peer ' + connection.remoteAddress + ' disconnected.');
                    });            
                }
                catch (error)
                {
                    console.log(error.message);
                }
              });
            },
            afterStart: (server) => {
              console.log('Server now listening!');
            }
          }        
    });
  };