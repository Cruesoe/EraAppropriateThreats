# Checks translation keys and threat source data; exits non-zero on any problem
$root = Split-Path $PSScriptRoot
$problems = New-Object Collections.Generic.List[string]

[xml]$keyed = [IO.File]::ReadAllText((Join-Path $root 'Languages\English\Keyed\EAT_Settings.xml'))
$keys = @{}
foreach ($node in $keyed.LanguageData.ChildNodes) { if ($node.NodeType -eq 'Element') { $keys[$node.Name] = $true } }

$source = (Get-ChildItem (Join-Path $root 'Source') -Recurse -Filter *.cs |
  Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
  ForEach-Object { [IO.File]::ReadAllText($_.FullName) }) -join "`n"

$used = @{}
foreach ($match in [regex]::Matches($source, '"(EAT_[A-Za-z]+)"')) { $used[$match.Groups[1].Value] = $true }
# Rule button labels are built from the FactionRule names at runtime
$used.Remove('EAT_Rule')
$rules = [IO.File]::ReadAllText((Join-Path $root 'Source\FactionRule.cs'))
foreach ($match in [regex]::Matches($rules, '(?m)^\s+([A-Za-z]+),?\s*$')) { $used['EAT_Rule' + $match.Groups[1].Value] = $true }

foreach ($key in $used.Keys) { if (-not $keys[$key]) { $problems.Add("Key used in code but missing: $key") } }
foreach ($key in $keys.Keys) { if (-not $used[$key]) { $problems.Add("Key defined but never used: $key") } }

[xml]$defs = [IO.File]::ReadAllText((Join-Path $root 'Common\Defs\ThreatSourceDefs\EAT_ThreatSources.xml'))
$names = @{}
$incidents = @{}
foreach ($def in $defs.SelectNodes('//EraAppropriateThreats.ThreatSourceDef')) {
  if (-not $def.defName) { $problems.Add('Threat source without a defName'); continue }
  if ($names[$def.defName]) { $problems.Add("Duplicate threat source defName: $($def.defName)") }
  $names[$def.defName] = $true
  if (-not $def.incident) { $problems.Add("$($def.defName) has no incident"); continue }
  if ($incidents[$def.incident]) { $problems.Add("Duplicate threat source incident: $($def.incident)") }
  $incidents[$def.incident] = $true
  if (-not $def.faction -and -not $def.techLevel) { $problems.Add("$($def.defName) has neither a faction nor a tech level") }
}

if ($problems.Count) { $problems | ForEach-Object { Write-Output $_ }; exit 1 }
Write-Output "Test-All passed: $($keys.Count) keys, $($names.Count) threat sources"
