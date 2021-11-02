<img alt="APK Installer 로고" src="https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/logo.png" width="200px" />

# APK Installer
Windows용 안드로이드 앱 설치 프로그램

[![Crowdin](https://badges.crowdin.net/APKInstaller/localized.svg)](https://crowdin.com/project/APKInstaller)

<a href="https://github.com/Paving-Base/APK-Installer/blob/master/LICENSE"><img alt="깃헙" src="https://img.shields.io/github/license/Paving-Base/APK-Installer.svg?label=License&style=flat-square"></a>
<a href="https://github.com/Paving-Base/APK-Installer/issues"><img alt="깃헙" src="https://img.shields.io/github/issues/Paving-Base/APK-Installer.svg?label=Issues&style=flat-square"></a>
<a href="https://github.com/Paving-Base/APK-Installer/stargazers"><img alt="깃헙" src="https://img.shields.io/github/stars/Paving-Base/APK-Installer.svg?label=Stars&style=flat-square"></a>

<a href="https://github.com/Paving-Base/APK-Installer/releases/latest"><img alt="깃헙 전체 릴리즈" src="https://img.shields.io/github/downloads/Paving-Base/APK-Installer/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge"></a>
<a href="https://pan.baidu.com/s/1AgAvyemIIDA3pLEYeiWR7g"><img alt="바이두 넷디스크" src="https://img.shields.io/badge/download-%e5%af%86%e7%a0%81%ef%bc%9aAPKI-magenta.svg?label=%e4%b8%8b%e8%bd%bd&logo=baidu&style=for-the-badge"></a>

## 언어
 - [中文](README.md)
 - [English](README.en-US.md)

## 목차
- [APK Installer](#apk-installer)
  - [언어](#语言)
  - [목차](#目录)
  - [APK Installer 설치 가이드](#如何安装应用)
    - [시스템 최소 요구 사항](#最低需求)
    - [설치 스크립트로 APK Installer 설치](#使用应用安装脚本安装应用)
    - [Windows App Installer를 이용하여 APK Installer 설치 ⭐](#使用应用安装程序安装应用)
    - [앱 업데이트](#更新应用)
  - [스크린샷](#屏幕截图)
  - [사용된 모듈](#使用到的模块)
  - [감사의 말](#鸣谢)
  - [깃헙 Star 통계](#star-数量统计)

## APK Installer 설치 가이드
### 시스템 최소 요구 사항
- Windows 10 빌드 17763 이상
- ARM64/x86/x64 지원 기기
- 400MB 이상의 여유 공간 (앱 및 설치 패키지 저장에 사용)

### 설치 스크립트로 APK Installer 설치
- 下载并解压最新的[安装包`(APKInstaller (Package)_x.x.x.0_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest "下载安装包")
- 如果没有应用安装脚本，下载[`Install.ps1`](Install.ps1)到目标目录 ![Install.ps1](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-12_22-49-11.png)
- 右击`Install.ps1`，选择“使用PowerShell运行”
- 应用安装脚本将会引导您完成此过程的剩余部分

### Windows App Installer를 이용하여 APK Installer 설치 ⭐
- 下载并解压最新的[安装包`(APKInstaller (Package)_x.x.x.0_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest "下载安装包")
- [开启旁加载模式](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - 如果您想开发UWP应用，您可以开启[开发人员模式](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development)，**对于大多数不需要做UWP开发的用户来说，开发人员模式是没有必要的**
- 安装`Dependencies`文件夹下的适用于您的设备的所有依赖包 ![Dependencies](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-13_15-51-33.png)
- 安装`*.cer`证书到`本地计算机`→`受信任的根证书颁发机构`
  - 这项操作需要用到管理员权限，如果您安装证书时没有用到该权限，则可能是因为您将证书安装到了错误的位置或者您使用的是超级管理员账户 ![安装证书](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-12_22-46-37.png) ![导入本地计算机](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-19_15-28-58.png) ![储存到受信任的根证书颁发机构](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-20_23-36-44.png)
- 双击`*.appxbundle`，单击安装，坐和放宽 ![설치](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-13_12-42-40.png)

### 앱 업데이트
- 下载并解压最新的[安装包`(APKInstaller (Package)_x.x.x.0_x86_x64_arm_arm64.appxbundle)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest "下载安装包")
- 双击`*.appxbundle`，单击更新，坐和放宽 ![업데이트](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Guides/Snipaste_2019-10-13_16-01-09.png)

## 스크린샷
- 설치 ![설치](https://raw.githubusercontent.com/Paving-Base/APK-Installer/main/Images/Screenshots/Snipaste_2021-10-22_21-00-14.png)

## 사용된 모듈
- [WinUI](https://github.com/microsoft/microsoft-ui-xaml "WinUI")
- [AAPTForNet](https://github.com/canheo136/QuickLook.Plugin.ApkViewer "AAPTForNet")
- [Advanced Sharp Adb Client](https://github.com/yungd1plomat/AdvancedSharpAdbClient "Advanced Sharp Adb Client")
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit "Windows Community Toolkit")

## 감사의 말
- 所有为 APK Installer 做出贡献的同志们
- **铺路尚未成功，同志仍需努力！**

## 깃헙 Star 통계
[![깃헙 Star 통계](https://starchart.cc/Paving-Base/APK-Installer.svg)](https://starchart.cc/Paving-Base/APK-Installer "깃헙 Star 통계")