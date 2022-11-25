Set-Location APKInstaller/APKInstaller

dotnet restore

msbuild /p:Configuration=Release /p:Platform=x64 /p:AppxPackageDir=AppPackages/ /p:UapAppxPackageBuildMode=StoreUpload /p:AppxBundle=Never /p:GenerateAppxPackageOnBuild=true
msbuild /p:Configuration=Release /p:Platform=x86 /p:AppxPackageDir=AppPackages/ /p:UapAppxPackageBuildMode=StoreUpload /p:AppxBundle=Never /p:GenerateAppxPackageOnBuild=true
msbuild /p:Configuration=Release /p:Platform=ARM64 /p:AppxPackageDir=AppPackages/ /p:UapAppxPackageBuildMode=StoreUpload /p:AppxBundle=Never /p:GenerateAppxPackageOnBuild=true