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

- (void)viewDidLoad {
    [super viewDidLoad];
	dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
			[self startRuntime];
		});
    // Do any additional setup after loading the view, typically from a nib.
}

- (void)startRuntime {
	mono_ios_runtime_init ();
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}


@end
