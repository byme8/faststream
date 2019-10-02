
ls .\src\**\bin\Release\*.nupkg | select-object -ExpandProperty FullName | rm

$projects = ls .\src\**\*.csproj
foreach ($project in $projects) {
    dotnet build -c release $project.FullName
}

$nugets = ls .\src\**\bin\Release\*.nupkg
if (($args.Count -gt 0) -and ($args.Contains("--publish"))) {
    $nugetToken = $Env:NUGET_TOKEN:

    foreach ($file in $nugets) {
        dotnet nuget push $file.FullName -k $nugetToken -s https://api.nuget.org/v3/index.json
    }
}

if (($args.Count -gt 0) -and ($args.Contains("--copy"))) {
    foreach ($file in $nugets) {
        cp  $file.FullName "C:/tools/local nugets/${$file.Name}"
    }
}
