param(
	[string]$SolutionPath = "..\OazaDlaAutyzmu.slnx"
)

Write-Host "Adding new projects to solution: $SolutionPath"

dotnet sln $SolutionPath add `
  "src\OazaDlaAutyzmu.Api\OazaDlaAutyzmu.Api.csproj" `
  "src\OazaDlaAutyzmu.BlazorWeb\OazaDlaAutyzmu.BlazorWeb.csproj" `
  "src\OazaDlaAutyzmu.Maui\OazaDlaAutyzmu.Maui.csproj"

Write-Host "Done."
