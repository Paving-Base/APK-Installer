<img alt="APK Installer LOGO" src="logo.png" width="200px"/>

# APK Installer
An Android Application Installer for Windows

[![Crowdin](https://badges.crowdin.net/APKInstaller/localized.svg)](https://crowdin.com/project/APKInstaller "Crowdin")

[![LICENSE](https://img.shields.io/github/license/Paving-Base/APK-Installer.svg?label=License&style=flat-square)](https://github.com/Paving-Base/APK-Installer/blob/master/LICENSE "LICENSE")
[![Issues](https://img.shields.io/github/issues/Paving-Base/APK-Installer.svg?label=Issues&style=flat-square)](https://github.com/Paving-Base/APK-Installer/issues "Issues")
[![Stargazers](https://img.shields.io/github/stars/Paving-Base/APK-Installer.svg?label=Stars&style=flat-square)](https://github.com/Paving-Base/APK-Installer/stargazers "Stargazers")

[![Microsoft Store](https://img.shields.io/badge/download-%e4%b8%8b%e8%bd%bd-magenta.svg?label=Microsoft%20Store&logo=Microsoft&style=for-the-badge&color=11a2f8)](https://apps.microsoft.com/store/detail/9P2JFQ43FPPG "Microsoft Store")
[![GitHub All Releases](https://img.shields.io/github/downloads/Paving-Base/APK-Installer/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge)](https://github.com/Paving-Base/APK-Installer/releases/latest "GitHub All Releases")

## Language
 - [中文](README.md)
 - [English](README.en-US.md)

## Contents
- [APK Installer](#apk-installer)
  - [Language](#language)
  - [Contents](#contents)
  - [How to install the APK Installer](#how-to-install-the-apk-installer)
    - [Minimum requirements:](#minimum-requirements)
    - [Install the app using the app installation script](#install-the-app-using-the-app-installation-script)
    - [Install the app using the Windows App Installer ⭐](#install-the-app-using-the-windows-app-installer-)
    - [Update the app](#update-the-app)
  - [Screenshots](#screenshots)
  - [Modules used](#modules-used)
  - [Special thanks to](#special-thanks-to)
  - [GitHub repo stars statistics](#github-repo-stars-statistics)

## How to install the APK Installer
### Minimum requirements:
- Windows 10 Build 17763 and above
- The device needs to support ARM64/x86/x64
- At least 400MB of free storage space (used to store installation packages and install applications)

### Install the app using the app installation script
- Download and unzip the latest [installation package `(APKInstaller (Package)_x.x.x.0_Test.rar)`](https://github.com/Paving-Base/APK-Installer/releases/latest "Download Package")
- If you do not have the app installation script, you can download [`Install.ps1`] into the target directory.
![Install.ps1](Images/Guides/Snipaste_2019-10-12_22-49-11.png)
- Right click on`Install.ps1`, and select 'Open with Powershell'.
- The installation script will guide you through the rest of the process.

### Install the app using the Windows App Installer ⭐
- Download and unzip the latest [installation package `(APKInstaller (Package)_x.x.x.0_Test.rar)`](https://github.com/Paving-Base/APK-Installer/releases/latest "下载安装包")
- [Turn on side loading mode](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - If you want to develop UWP applications, you can turn on the [developer mode](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development). **For most users who do not need to do UWP development, the developer mode is not necessary**
- Install all packages under `Dependencies` folder
![Dependencies](Images/Guides/Snipaste_2019-10-13_15-51-33.png)
- Install the certificate to `Local Machine` > `Place all certificates in the following store` > `Trusted Root CA`
  - This operation requires administrator privileges. If you did not use this privilege when installing the certificate, it may be because you installed the certificate to the wrong location or you are using a super administrator account
  ![Install Certificate](Images/Guides/Snipaste_2019-10-12_22-46-37.png)
  ![Import to local computer](Images/Guides/Snipaste_2019-10-19_15-28-58.png)
  ![Store to a trusted root certification authority](Images/Guides/Snipaste_2019-10-20_23-36-44.png)
- Double click`*.appxbundle`, then click and install. 
![Install](Images/Guides/Snipaste_2019-10-13_12-42-40.png)

### Update the app
- Download and unzip the latest installation package [`(APKInstaller (Package)_x.x.x.0_x86_x64_arm_arm64.appxbundle)`](https://github.com/Paving-Base/APK-Installer/releases/latest "Download Package")
- Double click `*.appxbundle`, click Update, **sit and relax**
![Install](Images/Guides/Snipaste_2019-10-13_16-01-09.png)

## Screenshots
- Install popup
![Install](Images/Screenshots/Snipaste_2021-10-22_21-00-14.png)

## Modules used
- [MetroLog](https://github.com/roubachof/MetroLog "MetroLog")
- [Zeroconf](https://github.com/novotnyllc/Zeroconf "Zeroconf")
- [Windows UI](https://github.com/microsoft/microsoft-ui-xaml "Windows UI")
- [Downloader](https://github.com/bezzad/Downloader "Downloader")
- [AAPTForNet](https://github.com/canheo136/QuickLook.Plugin.ApkViewer "AAPTForNet")
- [Sharp Compress](https://github.com/adamhathcock/sharpcompress "Sharp Compress")
- [Advanced Sharp Adb Client](https://github.com/yungd1plomat/AdvancedSharpAdbClient "Advanced Sharp Adb Client")
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit "Windows Community Toolkit")

## Special thanks to
- All people who contributed to the APK Installer project ❤️
- **Paving the road has not been successful, comrades still need to work hard!**

## GitHub repo stars statistics
[![Star statistics](https://starchart.cc/Paving-Base/APK-Installer.svg)](https://starchart.cc/Paving-Base/APK-Installer "Star statistics")
