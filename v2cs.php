<?php
/*
 * Small fix script to help port Vala to C#
 */
if($argc < 2){
	fprintf(STDERR, "Usage: %s\n", $argv[0]);
	return 1;
}

function preg_replace_all($pattern, $replacement, $subject){
	while(preg_match($pattern, $subject))
		$subject = preg_replace($pattern, $replacement, $subject);
	return $subject;
}

function fix_in_flags($d){
	$d = preg_replace("/(.*)(?:\s+)in(?:\s+)(.*)/", "$2.HasFlag($1)", $d);
	return $d;
}
function fix_in_list($d){
	$d = preg_replace("/(.*)(?:\s+)in(?:\s+)(.*)/", "$2.Contains($1)", $d);
	return $d;
}

$types = "class|enum";
$visibility = "public|private|protected";
$keywords = "$visibility|static|abstract";

$d = file_get_contents($argv[1]);

// Prepend default using(s)
if(false){
	$using = <<<EOT
using System;
using System.Collections.Generic;
using System.Text;
EOT;
	$d = $using . PHP_EOL . $d;
}

// 1 - Fix type names
$d = preg_replace("/($keywords)+\s+($types)+\s+(?:.*?)\.(.*)/", "$1 $2 $3", $d);

// 2 - Fix common types
$d = preg_replace_all("/ArrayList<(.*)>/", "List<$1>", $d);
$d = preg_replace_all("/(\s+)Set<(.*)>/", "$1HashSet<$2>", $d);
$d = preg_replace_all("/(\s+)Map<(.*)>/", "$1Dictionary<$2>", $d);
$d = preg_replace_all("/(?<!\s)Map<(.*)>/", "Dictionary<$1>", $d);
$d = preg_replace_all("/Iterator<(.*)>/", "IEnumerator<$1>", $d);

// 3 - unichar -> char
$d = preg_replace("/(\s+)(?:uni)char(\s+)/", "$1char$2", $d);

// 4 - Try to fix lists
$d = preg_replace("/\.add\s*\(/", ".Add(", $d);
$d = preg_replace("/\.append\s*\(/", ".Add(", $d);
$d = preg_replace("/\.prepend\s*\((.*)\)/", ".Insert(0, $1)", $d);

$d = preg_replace("/\.clear\s*\(/", ".Clear(", $d);
$d = preg_replace("/\.remove\s*\(/", ".Remove(", $d);
$d = preg_replace("/\.insert\s*\(/", ".Insert(", $d);
$d = preg_replace_callback("/\.get\s*\((.*)\)/", function($m){
	if(empty(trim($m[1]))) //enumerator
		return $m[0];
	return sprintf("[%s]", $m[1]);
}, $d);
$d = preg_replace("/\.set\s*\((.*),\s+(.*)\)/", "[$1] = $2", $d);
//TODO: Glib returns the one it removes
$d = preg_replace("/\.remove_at\s*\(/", ".RemoveAt(", $d);

$d = preg_replace("/\.iterator\s*\(/", ".GetEnumerator(", $d);
$d = preg_replace("/\.next\s*\(/", ".MoveNext(", $d);
$d = preg_replace("/\.get\s*\(.*\)/", ".Current", $d);

// 5 - Try fixing static constructors
// Invocation
$d = preg_replace("/new\s+([\w|\d|\s]+)\.(.*)\s+\((.*?)\)/", "$1.$2($3)", $d);
// Declaration
$d = preg_replace("/($visibility)\s+([\w|\d|\s]+)\.(.*)\s+\((.*?)\)/", "$1 static $2 $3($4)", $d);
// TODO: fix body aswell?

// 6 - Fix nullable
$d = preg_replace_callback("/([^\s]+)\?/",
	function($m){
		switch($m[1]){
			// allowed nullables
			case "bool":
			case "long":
			case "ulong":
			case "char":
				return $m[0];
			default:
				return $m[1];
		}
	},
	$d
);

function fixvar($search, $replace, $d){
	//(...$search...)
	// exclude foo.bar (e.g param.params_array)
	$d = preg_replace("/\((.*)[^.]{$search}(.*)\)/", "($1{$replace}$2)", $d);
	//...($search)...
	$d = preg_replace("/(.*)\(\s*{$search}\s*\)(.*)/", "$1({$replace})$2", $d);
	//... = $search
	$d = preg_replace("/(.*)(\s*)=(\s*)(.*){$search}(.*)/", "$1$2=$3$4{$replace}$5", $d);
	//$search = ...
	$d = preg_replace("/(.*){$search}(.*)=(.*)/", "$1{$replace}$2=$3", $d);
	//in $search
	$d = preg_replace("/in\s+{$search}/", "in $1{$replace}", $d);
	return $d;
}

// 7 - reserved keywords
$d = fixvar("checked", "is_checked", $d);
$d = fixvar("params", "_params", $d);
$d = fixvar("operator", "Operator", $d);

// 8 - remove specific keywords
$d = fixvar("unowned", "", $d);
$d = fixvar("owned", "", $d);
$d = fixvar("weak", "", $d);

// fix char methods
//TODO

// Fix {get; set; default = bar} --> {get; set;} = bar
$d = preg_replace("/default\s+=\s+(.*;).*\}/", "} = $1", $d);

print($d);
return 0;
?>