# Copyright (C) 2017 Schroedinger Entertainment
# Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

-nologo
-optimize+
-nowarn:0436,1685
-out:"..\..\Hqt.exe"
-platform:x64
-target:exe

# Preprocessor Defines
# Turn the following on if .NET 4.0 is installed, else minimum
# recommendet target should be .NET 4.5 or higher
# -define:net40;NET40;NET_4_0;NET_FRAMEWORK
-define:net45;NET45;NET_4_5;NET_FRAMEWORK

# References
# Turn the following on if .NET 4.5 is used
-reference:System.Net.Http.dll

# Files
# Adjust these paths to fit your directory structure, for example
# ..\..\Packages\se.sharp.common@1.0.0 to fulfill the default
# package installation target
-recurse:"..\..\Sharp\Actor\*.cs"
-recurse:"..\..\Sharp\Alchemy\*.cs"
-recurse:"..\..\Sharp\App\*.cs"
-recurse:"..\..\Sharp\CommandLine\*.cs"
-recurse:"..\..\Sharp\Common\*.cs"
-recurse:"..\..\Sharp\Config\*.cs"
-recurse:"..\..\Sharp\Flex\*.cs"
-recurse:"..\..\Sharp\Json\*.cs"
-recurse:"..\..\Sharp\Parsing\*.cs"
-recurse:"..\..\Sharp\Reactive\*.cs"
-recurse:"..\..\Sharp\SharpLang\*.cs"
-recurse:"..\..\Sharp\Tar\*.cs"
-recurse:"..\..\Sharp\Web\*.cs"
-recurse:"..\..\Apollo\Package\*.cs"
-recurse:"*.cs"