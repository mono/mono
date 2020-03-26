//
//  main.m
//  test-runner
//
//  Created by Zoltan Varga on 11/12/17.
//  Copyright © 2017 Zoltan Varga. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <sys/utsname.h>
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

UILabel *lblTestResults, *lblSummary;
int passed = 0, skipped = 0, failed = 0;
char* summaryMessage = nil;

- (void)viewDidLoad {
	[super viewDidLoad];
	self.view.backgroundColor = [UIColor whiteColor];

	//
	// construct UI
	//
	UIStackView *mainStackView = [[UIStackView alloc] init];
	mainStackView.translatesAutoresizingMaskIntoConstraints = NO;
	mainStackView.axis = UILayoutConstraintAxisVertical;
	mainStackView.alignment = UIStackViewAlignmentFill;
	mainStackView.distribution = UIStackViewDistributionFill;
	mainStackView.spacing = 8;

	struct utsname systemInfo;
	uname(&systemInfo);
	UILabel *lblHeader = [[UILabel alloc] init];
	lblHeader.textColor = [UIColor blueColor];
	lblHeader.font = [UIFont boldSystemFontOfSize: 18];
	lblHeader.text = [NSString stringWithFormat:@"Mono iOS SDK on %s", systemInfo.machine];

	UILabel *lblInfo = [[UILabel alloc] init];
	lblInfo.font = [UIFont boldSystemFontOfSize: 15];
	lblInfo.text = @"Test results:";

	lblTestResults = [[UILabel alloc] init];
	lblTestResults.text = @"";
	lblTestResults.numberOfLines = 3;

	lblSummary = [[UILabel alloc] init];
	lblSummary.accessibilityIdentifier = @"SummaryLabel";
	lblSummary.text = @"Waiting for test to start.";

	[mainStackView addArrangedSubview:lblHeader];
	[mainStackView addArrangedSubview:lblInfo];
	[mainStackView addArrangedSubview:lblTestResults];
	[mainStackView addArrangedSubview:lblSummary];

	[self.view addSubview:mainStackView];

	[mainStackView.topAnchor constraintEqualToAnchor:self.view.topAnchor constant:25.0].active = YES;
	[mainStackView.rightAnchor constraintEqualToAnchor:self.view.rightAnchor constant:-15.0].active = YES;
	[mainStackView.leftAnchor constraintEqualToAnchor:self.view.leftAnchor constant:15.0].active = YES;

	NSTimer* timer = [NSTimer timerWithTimeInterval:0.5f target:self selector:@selector(updateTestResults) userInfo:nil repeats:YES];
	[[NSRunLoop mainRunLoop] addTimer:timer forMode:NSRunLoopCommonModes];

	//
	// Launch Mono runtime
	//
	dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
		[self startRuntime];
	});
}

- (void)updateTestResults {
	dispatch_async(dispatch_get_main_queue(), ^{
		lblTestResults.text = [NSString stringWithFormat: @"Passed: %i\nSkipped: %i\nFailed: %i", passed, skipped, failed];

		if (failed > 0) {
			lblTestResults.textColor = [UIColor redColor];
		}

		if (summaryMessage != nil) {
			lblSummary.text = [[NSString alloc] initWithUTF8String:summaryMessage];
		}
	});
}

- (void)startRuntime {
	mono_sdks_ui_register_test_result_fields (&passed, &skipped, &failed, &summaryMessage);
	mono_ios_runtime_init ();
}

@end

int main(int argc, char * argv[]) {
	@autoreleasepool {
		return UIApplicationMain(argc, argv, nil, NSStringFromClass([AppDelegate class]));
	}
}
