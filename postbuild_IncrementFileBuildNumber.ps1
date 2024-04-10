Remove-Variable * -ErrorAction SilentlyContinue

if (!(Test-Path -Path "..\..\Properties\AssemblyInfo.cs"))
{
    Write-Output "Unable to find AssemblyInfo.cs";
    exit -1;
}

$asmContent = Get-Content "..\..\Properties\AssemblyInfo.cs" -ErrorAction Stop;

[String]$AssemblyVersion = "";
[String]$AssemblyFileVersion = "";
[String]$AssemblyInformationalVersion = "";

[UInt16]$AssemblyFileVersionIndex | Out-Null;
[String]$AssemblyFileVersionMatches = "";

for ([UInt16]$i = 0; $i -lt $asmContent.Length; ++$i)
{
    if ($AssemblyVersion -eq "")
    {
        if ($asmContent[$i] -match '\[assembly: AssemblyVersion\("(\d+\.\d+\.\d+\.\d+)"\)\]')
        {
            $AssemblyVersion = "AssemblyVersion: $($Matches[1])";
        }
    }

    if ($AssemblyFileVersion -eq "")
    {
        if ($asmContent[$i] -match '\[assembly: AssemblyFileVersion\("(\d+\.\d+\.\d+\.\d+)"\)\]')
        {
            $AssemblyFileVersion = "AssemblyFileVersion: $($Matches[1])";
            
            $AssemblyFileVersionMatches = $Matches[1]
            $AssemblyFileVersionIndex = $i
        }
    }

    if ($AssemblyInformationalVersion -eq "")
    {
        if ($asmContent[$i] -match '\[assembly: AssemblyInformationalVersion\("(\d+\.\d+[-+].+)"\)\]')
        {
            $AssemblyInformationalVersion = "AssemblyInformationalVersion: $($Matches[1])";
        }
    }
}

if ($AssemblyVersion -eq "")
{   
    Write-Output "Unable to find AssemblyVersion";
    exit -1;
}

if ($AssemblyFileVersion -eq "")
{
    Write-Output "Unable to find AssemblyFileVersion";
    exit -1;
}

if ($AssemblyInformationalVersion -eq "")
{
    Write-Output "Unable to find AssemblyInformationalVersion";
    exit -1;
}

if ($AssemblyFileVersionIndex -eq $null)
{
    Write-Output "Failed to obtain the line index of 'AssemblyInformationalVersion'";
    exit -1;
}

[String[]]$VersionParts = $AssemblyFileVersionMatches.Split('.');

if ($VersionParts.Length -ne 4)
{
    Write-Output "Invalid AssemblyInformationalVersion format, use x.x.x.x for autoincrement";
    exit -1;
}

[UInt32]$BuildUInt = $VersionParts[3];
++$BuildUInt;

[String]$NewVersionString = "$($VersionParts[0]).$($VersionParts[1]).$($VersionParts[2]).$($BuildUInt)";

$asmContent[$AssemblyFileVersionIndex] = $asmContent[$AssemblyFileVersionIndex].Replace($AssemblyFileVersionMatches, $NewVersionString);

Set-Content -Path "..\..\Properties\AssemblyInfo.cs" -Value $asmContent -ErrorAction Stop;

Write-Output $AssemblyVersion;
Write-Output $AssemblyFileVersion;
Write-Output $AssemblyInformationalVersion;

exit 0;