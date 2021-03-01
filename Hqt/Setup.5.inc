# Copyright (C) 2017 Schroedinger Entertainment
# Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

-nologo
-optimize+
-out:"..\..\Hqt.exe"
-platform:x64
-target:exe

# Preprocessor Defines
-define:net50;NET50;NET_5_0;NET_CORE

# References

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