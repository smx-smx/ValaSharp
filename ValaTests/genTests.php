<?php
$head=<<<EOF
using NUnit.Framework;
namespace ValaTests
{
	[TestFixture]
	public class ValaCompilerTests {


EOF;

$tail=<<<EOF
	}
}
EOF;

$scriptDir = realpath(dirname(__FILE__));
$it = new RecursiveDirectoryIterator($scriptDir);

print($head);
foreach(new RecursiveIteratorIterator($it) as $file)
{
	$fi = pathinfo($file);
	$ext = strtolower($fi["extension"]);
	if($ext != "vala")
		continue;

	$testDir = dirname($file);
	if($testDir == $scriptDir){
		continue;
	}

	$dir = basename($testDir);
	
	$file = basename($file);
	$name = $fi["filename"];

	$testMethod = "{$dir}_{$name}";
	$testMethod = str_replace("-", "_", $testMethod);
	
	echo<<<EOF
		[Test]
		public void ${testMethod}() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("${dir}/${file}") == 0);
		}

EOF;

}
print($tail);


?>
