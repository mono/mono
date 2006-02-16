#!/usr/bin/perl

use strict;

open(RBTS,"RedBlackTree.cs") || die "Can't open infile";

rename "RedBlackTreeBag.cs", "RedBlackTreeBag.cs.old" || die "Can't backup";

open(RBTB,">RedBlackTreeBag.cs")  || die "Can't open outfile";

my @cond=(1);
my $printing = 1;

#Assume all conditions on BAG symbol is '#if BAG'
while (<RBTS>) {
  if (/^#define BAG/) {
    print RBTB "#define BAG\r\n";
    next;
  }
  s/TreeSet/TreeBag/g;
  print RBTB;
}

close(RBTS) || die "Can't close infile";
close(RBTB) || die "Can't close outfile";
