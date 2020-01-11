//
//  main.m
//  test-runner
//
//  Created by Zoltan Varga on 11/12/17.
//  Copyright © 2017 Zoltan Varga. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "runtime.h"

@interface ViewController : UIViewController

@end

@interface AppDelegate : UIResponder <UIApplicationDelegate>

@property (strong, nonatomic) UIWindow *window;
@property (strong, nonatomic) ViewController *controller;

@end

// ------------------------------------------

@implementation AppDelegate


- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    self.window = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
	self.controller = [[ViewController alloc] initWithNibName:nil bundle:nil];
	self.window.rootViewController = self.controller;
	[self.window makeKeyAndVisible];
    return YES;
}

- (void)applicationWillResignActive:(UIApplication *)application {
}

- (void)applicationDidEnterBackground:(UIApplication *)application {
}

- (void)applicationWillEnterForeground:(UIApplication *)application {
}

- (void)applicationDidBecomeActive:(UIApplication *)application {
}

- (void)applicationWillTerminate:(UIApplication *)application {
}


@end


@implementation ViewController

UIView *mainView;
UILabel *lblResults;
int passed = 0, skipped = 0, failed = 0;

- (void)viewDidLoad {
    [super viewDidLoad];

    mainView = [[UIView alloc] initWithFrame:self.view.frame];
    mainView.backgroundColor = [UIColor whiteColor];
	[self.view addSubview:mainView];

	UILabel *lblHeader = [[UILabel alloc] init];
    lblHeader.frame = CGRectMake(100, 100, 200, 200);
	lblHeader.backgroundColor = [UIColor clearColor];
	lblHeader.textColor = [UIColor redColor];
	lblHeader.font = [UIFont boldSystemFontOfSize: 20];
	lblHeader.numberOfLines = 2;
	lblHeader.text = @"Mono iOS SDK\nRunning tests...";
	[mainView addSubview:lblHeader];

	lblResults = [[UILabel alloc] init];
    lblResults.frame = CGRectMake(100, 200, 200, 200);
	lblResults.backgroundColor = [UIColor clearColor];
	lblResults.textColor = [UIColor redColor];
	lblResults.font = [UIFont boldSystemFontOfSize: 20];
	lblResults.numberOfLines = 3;
	lblResults.text = @"Passed: 0\nSkipped: 0\nFailed: 0";
	[mainView addSubview:lblResults];

	NSTimer* timer = [NSTimer timerWithTimeInterval:0.5f target:self selector:@selector(updateTestResults) userInfo:nil repeats:YES];
	[[NSRunLoop mainRunLoop] addTimer:timer forMode:NSRunLoopCommonModes];

	dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
		[self startRuntime];
	});
    // Do any additional setup after loading the view, typically from a nib.
}

- (void)updateTestResults {
	dispatch_async(dispatch_get_main_queue(), ^{
		lblResults.text = [NSString stringWithFormat: @"Passed: %i\nSkipped: %i\nFailed: %i", passed, skipped, failed];
	});
}

- (void)startRuntime {
	mono_sdks_ui_register_testcase_result_fields (&passed, &skipped, &failed);
	mono_ios_runtime_init ();
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}


@end


int main(int argc, char * argv[]) {
    @autoreleasepool {
        return UIApplicationMain(argc, argv, nil, NSStringFromClass([AppDelegate class]));
    }
}
