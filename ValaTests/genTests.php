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
$out = fopen($scriptDir . "/ValaTests_Generated.cs", "w+") or die("Cannot open destination file for writing\n");

$it = new RecursiveDirectoryIterator($scriptDir);

fwrite($out, $head);
foreach(new RecursiveIteratorIterator($it) as $file)
{
	$fi = pathinfo($file);
	if($fi === FALSE || !isset($fi["extension"]))
		continue;

	$ext = strtolower($fi["extension"]);
	if($ext != "vala")
		continue;

	if($fi["dirname"] == $scriptDir){
		continue;
	}

	$dir = basename($fi["dirname"]);
	
	$fileRelative = $fi["basename"];
	$name = $fi["filename"];

	$testMethod = "{$dir}_{$name}";
	$testMethod = str_replace("-", "_", $testMethod);
	
	$testCode=<<<EOF
		[Test]
		public void ${testMethod}() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("${dir}/${fileRelative}") == 0);
		}

EOF;

	fwrite($out, $testCode);

}
fwrite($out, $tail);
fclose($out);


?>
