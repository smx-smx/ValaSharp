<?php
/*
 * This (quick and dirty) script runs the compiler through the tests with -C
 * It then runs the original vala compiler with the same options
 * and looks for differences
 * 
 * Copyright (C) 2017 Stefano Moioli <smxdev4@gmail.com>
 **/
$scriptDir = realpath(dirname(__FILE__));
$it = new RecursiveDirectoryIterator($scriptDir);

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

	if(!is_dir("testdir"))
		mkdir("testdir", 0777);

	$dir = basename($fi["dirname"]);
	$name = $dir . "/" . $fi["filename"];

	$valac = escapeshellarg(realpath("../ValaCompiler/bin/Debug/ValaCompiler.exe"));
	$args = [
		$valac,
		"--vapidir C:/msys64/mingw64//share/vala-0.38/vapi",
		"--pkg gio-2.0",
		"--main main",
		"--disable-warnings",
		"-C",
		escapeshellarg($file)
	];

	@mkdir("testdir/{$dir}", 0777);

	printf("Compiling %s\n", $file);
	$current = getcwd();
		chdir("testdir/{$dir}");
		exec(implode(" ", $args), $output, $code);
	chdir($current);

	foreach($output as $line){
		print($line . PHP_EOL);
	}
	printf("Result: %d\n", $code);

	chdir("testdir/{$dir}");
		$cfile = $fi["filename"] . ".c";
		if(file_exists($cfile))
			rename($cfile, $fi["filename"] . "_vsharp.c");
		else
			touch($fi["filename"] . "_vsharp.c");

		$args[0] = getenv("VALAC");
		if($args[0] === FALSE)
			$args[0] = "valac";

		/* Remove command line hacks before calling original valac */
		/*
		foreach($args as $i => &$arg){
			// Remove escaping for -X 'arg'
			if(strpos($arg, "-X") === 0)
				$arg = str_replace('\'', "", $arg);
		}
		*/

		// Array with exit codes
		$codes = [$code];

		exec(implode(" ", $args), $output, $code);
		$codes[1] = $code;
		
		$diff = $fi["filename"] . ".diff";
		shell_exec(
			sprintf("diff -bu %s %s > %s",
				$cfile,
				$fi["filename"] . "_vsharp.c",
				$diff
			)
		);
		if(
			!$codes[0] && !$codes[1] &&
			file_exists($diff) &&
			empty(file_get_contents($diff))
		){
			unlink($fi["filename"] . ".diff");
			printf("OK\n");
		} else {
			printf("FAIL\n");
		}
		print(PHP_EOL);

	chdir($current);
}
?>