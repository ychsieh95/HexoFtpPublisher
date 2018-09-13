# HexoFtpPublisher

基於 .NET Core 2.1 與 [FluentFTP](https://github.com/robinrodricks/FluentFTP)，部署 Hexo 至指定 FTP 的協助發行工具。

# 環境需求

* [Microsoft .NET Core 2.1 Runtime](https://www.microsoft.com/net/download/dotnet-core/2.1) or latest version

> 在安裝完成後請**首先檢查 Runtime 是否安裝成功**：
>
> ```shell
> $ dotnet --version
> ```

# 檔案結構

```
─┬─ HexoFtpPublisher.dll
 ├─ HexoFtpPublisher.runtimeconfig.json
 └─ FluentFTP.dll
```

# 參數說明

| Parameter       | Desciption                  | Required           | Options                 | Default Value  |
|:----------------|:----------------------------|:------------------:|:------------------------|:---------------|
| --host          | FTP Host                    | :white_check_mark: |                         |                |
| --port          | FTP Port                    |                    |                         | 21             |
| --user          | Login Username              |                    |                         | (Anonymous)    |
| --pass          | Login Password              |                    |                         |                |
| --source        | Source Folder               |                    |                         | .\public       |
| --remote        | Remote Folder               | :white_check_mark: |                         |                |
| --clean_remote  | Clean Remote Folder Content |                    | yes/no (or y/n)         | yes            |
| --exist_action  | The Action If File Exist    |                    | append/overwrite/skip   | overwrite      |

* exist_action
    * append\
      若檔案存在，透過檢查檔案長度並添加缺少的資料於檔案上。（Append to the file if it exists, by checking the length and adding the missing data.）
    * overwirte\
      若檔案存在，覆蓋檔案。（Overwrite the file if it exists.）
    * skip\
      若檔案存在，則跳過該檔案並不再進行任何檢查。（Skip the file if it exists, without any more checks.）

# 使用範例

## 以匿名使用者登入並上傳

```shell
$ dotnet HexoFtpPublisher.dll --host=127.0.0.1 --remote="/blog"
```

## 清除目標資料夾內容後上傳指定資料夾內容

```shell
$ dotnet HexoFtpPublisher.dll --host=127.0.0.1 --user=<USERNAME> --pass=<PASSWORD> --source="..\..\..\public" --remote="/blog"
```

## 不清除目標資料夾內容並以新增模式上傳檔案

```shell
$ dotnet HexoFtpPublisher.dll --host=127.0.0.1 --user=<USERNAME> --pass=<PASSWORD> --source="..\..\..\public" --remote="/blog" --clean_remote=no --exist_action=append
```

## 直接執行程式並依序輸入參數

```shell
$ dotnet HexoFtpPublisher.dll
Host:
Port:
User:
Pass:
Source Folder:
Remote Folder:
Clean Remote Path (Y/n):
Exist File Option (append/overwirte/skip):
```