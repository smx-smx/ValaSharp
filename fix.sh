#!/bin/bash
find CCodeGen CLanguage -type f -iname "*.cs" -not -iname "TemporaryGeneratedFile*.cs" | while read file; do echo "VALA2CS $file"; php v2cs.php "$file" > gen.cs; mv gen.cs "$file"; done
