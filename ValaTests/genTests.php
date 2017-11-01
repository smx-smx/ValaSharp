<?php
$head=<<<EOF
using NUnit.Framework;
namespace ValaTests
{
	[TestFixture]
	public class ValaCompilerTests {
		private ValaTestRunner runner;

		[SetUp]
		public void Init() {
			runner = new ValaTestRunner();
		}

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
	
	$dir = basename(dirname($file));
	$file = basename($file);
	$name = $fi["filename"];

	$testMethod = "{$dir}_{$name}";
	$testMethod = str_replace("-", "_", $testMethod);
	
	echo<<<EOF
		[Test]
		public void ${testMethod}() {
			Assert.IsTrue(runner.RunValaTest("${dir}/${file}") == 0);
		}

EOF;

}
print($tail);


?>