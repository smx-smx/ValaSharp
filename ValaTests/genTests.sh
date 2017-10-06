#!/bin/bash
cat<<EOF
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
		
EOF

find . -type f -iname "*.vala" -print0 | while IFS= read -r -d '' path; do
	dir=$(basename $(dirname "$path"))
	file=$(basename "$path")
	name="${file%.*}"

	testMethod="${dir}_${name}"
	testMethod=$(echo "$testMethod" | sed 's/-/_/g')

	cat<<EOF
		[Test]
		public void ${testMethod}() {
			Assert.IsTrue(runner.RunValaTest("${dir}/${file}") == 0);
		}
EOF

done

cat<<EOF
	}
}
EOF
