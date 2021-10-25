# APK Installer
An Android Application Installer for Windows

[![Crowdin](https://badges.crowdin.net/APKInstaller/localized.svg)](https://crowdin.com/project/APKInstaller)

<a href="https://github.com/Paving-Base/APK-Installer/blob/master/LICENSE"><img alt="GitHub" src="https://img.shields.io/github/license/Paving-Base/APK-Installer.svg?label=License&style=flat-square"></a>
<a href="https://github.com/Paving-Base/APK-Installer/issues"><img alt="GitHub" src="https://img.shields.io/github/issues/Paving-Base/APK-Installer.svg?label=Issues&style=flat-square"></a>
<a href="https://github.com/Paving-Base/APK-Installer/stargazers"><img alt="GitHub" src="https://img.shields.io/github/stars/Paving-Base/APK-Installer.svg?label=Stars&style=flat-square"></a>

<a href="https://github.com/Paving-Base/APK-Installer/releases/latest"><img alt="GitHub All Releases" src="https://img.shields.io/github/downloads/Paving-Base/APK-Installer/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge"></a>
<a href="https://pan.baidu.com/s/1AgAvyemIIDA3pLEYeiWR7g"><img alt="Baidu Netdisk" src="https://img.shields.io/badge/download-%e5%af%86%e7%a0%81%ef%bc%9aAPKI-magenta.svg?label=%e4%b8%8b%e8%bd%bd&logo=baidu&style=for-the-badge"></a>

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
- Download and unzip the latest [installation package `(APKInstaller (Package)_x.x.x.0_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest "下载安装包")
- 如果没有应用安装脚本，下载[`Install.ps1`](Install.ps1)到目标目录
![Install.ps1](Images/Guides/Snipaste_2019-10-12_22-49-11.png)
- 右击`Install.ps1`，选择“使用PowerShell运行”
- 应用安装脚本将会引导您完成此过程的剩余部分

### Install the app using the Windows App Installer ⭐
- Download and unzip the latest installation package `APKInstaller (Package)_x.x.x.0_Test.rar)`](https://github.com/Paving-Base/APK-Installer/releases/latest "下载安装包")
- [Turn on side loading mode](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - If you want to develop UWP applications, you can turn on the [developer mode](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development). **For most users who do not need to do UWP development, the developer mode is not necessary**
- Install all packages under `Dependencies` folder
![Dependencies](Images/Guides/Snipaste_2019-10-13_15-51-33.png)
- Install the certificate to `Local Machine` > `Place all certificates in the following store` > `Trusted Root CA`
  - This operation requires administrator privileges. If you did not use this privilege when installing the certificate, it may be because you installed the certificate to the wrong location or you are using a super administrator account
  ![安装证书](Images/Guides/Snipaste_2019-10-12_22-46-37.png)
  ![导入本地计算机](Images/Guides/Snipaste_2019-10-19_15-28-58.png)
  ![储存到受信任的根证书颁发机构](Images/Guides/Snipaste_2019-10-20_23-36-44.png)
- 双击`*.appxbundle`，单击安装，坐和放宽
![安装](Images/Guides/Snipaste_2019-10-13_12-42-40.png)

### Update the app
- Download and unzip the latest installation package [`(APKInstaller (Package)_x.x.x.0_x86_x64_arm_arm64.appxbundle)`](https://github.com/Paving-Base/APK-Installer/releases/latest "下载安装包")
- Double click `*.appxbundle`, click Update, **sit and relax**
![安装](Images/Guides/Snipaste_2019-10-13_16-01-09.png)

## Screenshots
- Install popup
![安装](Images/Screenshots/Snipaste_2021-10-22_21-00-14.png)

## Modules used
- [WinUI](https://github.com/microsoft/microsoft-ui-xaml "WinUI")
- [AAPTForNet](https://github.com/canheo136/QuickLook.Plugin.ApkViewer "AAPTForNet")
- [Advanced Sharp Adb Client](https://github.com/yungd1plomat/AdvancedSharpAdbClient "Advanced Sharp Adb Client")
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit "Windows Community Toolkit")

## Special thanks to
- All people who contributed to the APK Installer project ❤️
- **Paving the road has not been successful, comrades still need to work hard!**

## GitHub repo stars statistics
[![Star 数量统计](https://starchart.cc/Paving-Base/APK-Installer.svg)](https://starchart.cc/Paving-Base/APK-Installer "Star 数量统计")
