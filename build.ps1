 $rootDir = $env:APPVEYOR_BUILD_FOLDER
 $buildNumber = $env:APPVEYOR_BUILD_NUMBER
 $solutionFile = "$rootDir\src\TITcs.SharePoint.SSOM.sln"
 $srcDir = "$rootDir\src\nuget\TITcs.SharePoint.SSOM"
 $slns = ls "$rootDir\src\*.sln"
 $packagesDir = "$rootDir\src\packages"
 $nuspecPath = "$rootDir\src\nuget\TITcs.SharePoint.SSOM\TITcs.SharePoint.SSOM.nuspec"
 $nugetExe = "$rootDir\src\.nuget\NuGet.exe"
 $nupkgPath = "$rootDir\src\NuGet\TITcs.SharePoint.SSOM\TITcs.SharePoint.SSOM.{0}.nupkg"

foreach($sln in $slns) {
   nuget restore $sln
}

[xml]$xml = cat $nuspecPath
$xml.package.metadata.version+=".$buildNumber"
$xml.Save($nuspecPath)

[xml]$xml = cat $nuspecPath
$nupkgPath = $nupkgPath -f $xml.package.metadata.version

nuget pack $nuspecPath -properties "Configuration=$env:configuration;Platform=AnyCPU;Version=$($env:appveyor_build_version)" -OutputDirectory $srcDir 
appveyor PushArtifact $nupkgPath
