param(
    [string]$Message = "",
    [string]$Tag = "",
    [string]$Remote = "origin",
    [string]$Branch = ""
)

$ErrorActionPreference = "Stop"

function Run-Git {
    param([string[]]$Args)

    Write-Host "> git $($Args -join ' ')" -ForegroundColor Cyan
    & git @Args
    if ($LASTEXITCODE -ne 0) {
        throw "Git command failed: git $($Args -join ' ')"
    }
}

$repoRoot = (& git rev-parse --show-toplevel 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    throw "This folder is not inside a Git repository."
}

Set-Location $repoRoot

if ([string]::IsNullOrWhiteSpace($Branch)) {
    $Branch = (& git branch --show-current).Trim()
}

if ([string]::IsNullOrWhiteSpace($Branch)) {
    throw "Cannot detect current branch. Please pass -Branch main."
}

$remoteUrl = (& git remote get-url $Remote 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($remoteUrl)) {
    throw "Remote '$Remote' does not exist."
}

Write-Host "Repository: $repoRoot" -ForegroundColor Green
Write-Host "Remote: $Remote ($remoteUrl)" -ForegroundColor Green
Write-Host "Branch: $Branch" -ForegroundColor Green

$status = (& git status --porcelain)
if (-not [string]::IsNullOrWhiteSpace($status)) {
    if ([string]::IsNullOrWhiteSpace($Message)) {
        $Message = "Update project $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    }

    Run-Git @("add", "-A")
    Run-Git @("commit", "-m", $Message)
} else {
    Write-Host "No file changes to commit." -ForegroundColor Yellow
}

if (-not [string]::IsNullOrWhiteSpace($Tag)) {
    $existingTag = (& git tag --list $Tag).Trim()
    if (-not [string]::IsNullOrWhiteSpace($existingTag)) {
        throw "Tag '$Tag' already exists. Use a new tag name, for example v2.1."
    }

    Run-Git @("tag", $Tag)
}

Run-Git @("push", $Remote, $Branch)

if (-not [string]::IsNullOrWhiteSpace($Tag)) {
    Run-Git @("push", $Remote, $Tag)
}

Write-Host "Done. GitHub is updated." -ForegroundColor Green