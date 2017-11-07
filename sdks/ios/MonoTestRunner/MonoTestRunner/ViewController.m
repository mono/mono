//
//  ViewController.m
//  MonoTestRunner
//
//  Created by Rodrigo Kumpera on 3/30/17.
//  Copyright © 2017 Rodrigo Kumpera. All rights reserved.
//

#import "ViewController.h"
#include "runtime-bootstrap.h"


@interface ViewController ()
@property (weak, nonatomic) IBOutlet UITextField *TestNameField;
@property (weak, nonatomic) IBOutlet UILabel *OutputLabel;
@property (weak, nonatomic) IBOutlet UIButton *TapButton;

@end

@implementation ViewController

@synthesize TapButton;
@synthesize OutputLabel;
@synthesize TestNameField;

- (IBAction)WasTapped:(id)sender {
    NSString *text = self.TestNameField.text;

    char *res = runtime_send_message ("start", [text UTF8String]);
    NSString *res_str = [NSString stringWithUTF8String:res];

    self.OutputLabel.text = res_str;
}

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view, typically from a nib.
    init_runtime();

    
}


- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}


@end
