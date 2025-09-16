$Version = Read-Host -Prompt 'Version'
try
{
	dotnet pack ../src/TruePeople.SharePreview/TruePeople.SharePreview.csproj -c Release -o ../packages -p:PackageVersion=$Version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
	Write-Host "Succesfully created Nuget package with version $($Version)"
}
catch {
	Write-Host "Error occured:";
	Write-Host $_;
	 $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp") > $null
}