#!/usr/bin/perl

use XML::LibXML;

$parser = new XML::LibXML;

opendir DIR, ".";
foreach $file (readdir(DIR)) {
	if ($file !~ /([\d\.]+)\.xml/) { next; }
	$sec = $1;
	
	$xml = $parser->parse_file("$sec.xml");
	$titles{$sec} = $xml->findvalue('/clause/@title');
	
	if ($sec =~ /^([\d\.]+)\.(\d+)$/) {
		$parent = $1;
		$sub = $2;
	} else {
		$parent = "";
		$sub = $sec;
	}

	$sections{$parent}[$sub-1] = $sec;
}
closedir DIR;

$doc = new XML::LibXML::Document;
$toc = $doc->createElement('toc');
$doc->setDocumentElement($toc);

AddChildren($toc, '');

print $doc->toString(1);

sub AddChildren {
	my $x = $sections{$_[1]};
	my @x = @{ $x };
	foreach my $s (@x) {
		my $n = $doc->createElement('node');
		$n->setAttribute('number', $s);
		$n->setAttribute('name', $titles{$s});
		$_[0]->appendChild($n);
		
		AddChildren($n, $s);
	}
}

