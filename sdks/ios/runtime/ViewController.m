/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
//
//  ViewController.m
//  test-runner
//
//  Created by Zoltan Varga on 11/12/17.
//  Copyright © 2017 Zoltan Varga. All rights reserved.
//

#import "ViewController.h"
#import "runtime.h"

@interface ViewController ()

@end

@implementation ViewController

UILabel *lblResults;
int passed = 0, skipped = 0, failed = 0;

- (void)viewDidLoad {
    [super viewDidLoad];

	UILabel *lblHeader = [[UILabel alloc] init];
    lblHeader.frame = CGRectMake(100, 100, 200, 200);
	lblHeader.backgroundColor = [UIColor clearColor];
	lblHeader.textColor = [UIColor redColor];
	lblHeader.font = [UIFont boldSystemFontOfSize: 20];
	lblHeader.numberOfLines = 2;
	lblHeader.text = @"Mono iOS SDK\nRunning tests...";
	[self.view addSubview:lblHeader];

	lblResults = [[UILabel alloc] init];
    lblResults.frame = CGRectMake(100, 200, 200, 200);
	lblResults.backgroundColor = [UIColor clearColor];
	lblResults.textColor = [UIColor redColor];
	lblResults.font = [UIFont boldSystemFontOfSize: 20];
	lblResults.numberOfLines = 3;
	lblResults.text = @"Passed: 0\nSkipped: 0\nFailed: 0";
	[self.view addSubview:lblResults];

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
