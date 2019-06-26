#!/usr/bin/env ruby
require 'open-uri'
require 'fileutils'
require 'rubygems'
require 'zip'

$testDataUri = "https://github.com/dotnet/corefx-testdata/archive/master.zip"
$testDataZip = "testdata.zip"
$testDataDir = "TestData"

$testDataCertificatePath = "corefx-testdata-master/System.Net.TestData"
$testDataCertificate = "TestData/testclient1_at_contoso.com.cer"

$testData = "corefx-testdata"

def download(url, path)
	return if File.exists?(path)

	puts "Dowloading #{url} ..."
	
	case io = open(url)
		when StringIO then File.open(path, 'w') { |f| f.write(io) }
		when Tempfile then io.close; FileUtils.mv(io.path, path)
	end
	
	puts "Wrote #{path}."
end

def deleteTestData()
	FileUtils.rm_rf($testDataDir)
end

def downloadTestData()
	download($testDataUri, $testDataZip)
	FileUtils.mkdir_p($testDataDir)
	
	Zip::File.open($testDataZip) do |zip|
		zip.glob("**/System.Net.TestData/*") do |entry|
			f_path=File.join($testDataDir, File.basename(entry.name))
			unless File.exists?(f_path)
				entry.extract(f_path)
				puts "Extracted #{f_path}"
			end
		end
	end
end

def installCertificate()
	downloadTestData()
	system("sudo security add-trusted-cert -d -r trustAsRoot -p ssl -u 1 -k /Library/Keychains/System.keychain #{$testDataCertificate}")
end

def removeCertificate()
	downloadTestData()
	system("sudo security remove-trusted-cert -d #{$testDataCertificate}")
end

# deleteTestData
downloadTestData

installCertificate()

# sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain TestData/testclient1_at_contoso.com.cer 

    
#$script:certificatePath = "$($script:testData)\corefx-testdata-master\System.Net.TestData"

#$script:clientPrivateKeyPath = Join-Path $script:certificatePath "testclient1_at_contoso.com.pfx"
#$script:clientPrivateKeyPassword = "testcertificate"

##$script:serverPrivateKeyPath = Join-Path $script:certificatePath "contoso.com.pfx"
#$script:serverPrivateKeyPassword = "testcertificate""
