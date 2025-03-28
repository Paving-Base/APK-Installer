<#
ULTIMATE ASSISTANT INSTALLER v3.0
#>

# Configuration
$RepoUrl = "https://github.com/badman576/UltimateAssistant"
$BackupRepo = "https://gitlab.com/badman576/UltimateAssistant-Mirror"
$InstallDir = "$env:ProgramFiles\UltimateAssistant"
$ConfigDir = "$env:APPDATA\UltimateAssistant"

# Create directories
New-Item -Path $InstallDir -ItemType Directory -Force | Out-Null
New-Item -Path $ConfigDir -ItemType Directory -Force | Out-Null

function Install-Assistant {
    param(
        [Parameter(Mandatory=$true)]
        [SecureString]$GitHubToken
    )

    try {
        # Convert secure token
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($GitHubToken)
        $PlainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

        # Verify GitHub access
        $headers = @{
            "Authorization" = "token $PlainToken"
            "Accept" = "application/vnd.github.v3+json"
        }

        try {
            $repoCheck = Invoke-RestMethod -Uri "https://api.github.com/user/repos" -Headers $headers
            Write-Host "✓ GitHub authentication successful" -ForegroundColor Green
        } catch {
            throw "GitHub authentication failed: $_"
        }

        # Download core files
        $files = @(
            "UltimateAssistant.exe",
            "VoiceTrainer.exe",
            "config.json"
        )

        foreach ($file in $files) {
            try {
                $url = "$RepoUrl/releases/latest/download/$file"
                Invoke-WebRequest $url -OutFile "$InstallDir\$file"
                Write-Host "Downloaded $file" -ForegroundColor Cyan
            } catch {
                Write-Warning "Failed to download $file from primary source, trying backup..."
                $url = "$BackupRepo/releases/latest/download/$file"
                Invoke-WebRequest $url -OutFile "$InstallDir\$file"
            }
        }

        # Create config
        $config = @{
            GitHub = @{
                Token = $PlainToken | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString
                Repositories = @("badman576/peter", "badman576/a-bot")
            }
        }

        $config | ConvertTo-Json | Out-File "$ConfigDir\config.json"
        attrib +h +s "$ConfigDir\config.json"

        # Create start menu shortcut
        $WshShell = New-Object -ComObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Ultimate Assistant.lnk")
        $Shortcut.TargetPath = "$InstallDir\UltimateAssistant.exe"
        $Shortcut.Save()

        Write-Host "`n🎉 Installation Complete!" -ForegroundColor Green
        Write-Host "Access methods:`n1. Say 'Hey Assistant'`n2. Start Menu shortcut`n3. Run 'UltimateAssistant.exe'"

    } catch {
        Write-Host "`n❌ Error: $_" -ForegroundColor Red
    } finally {
        # Clear sensitive data
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
        Remove-Variable PlainToken -ErrorAction SilentlyContinue
    }
}

# Start installation
$secureToken = Read-Host "Enter your GitHub PAT" -AsSecureString
Install-Assistant -GitHubToken $secureToken
