$hostname=$args[0]
$deviceId=$args[1]
$symmetrickey=$args[2]

If ((Test-Path "D:\temp") -eq $False) { New-Item -ItemType directory -Path D:\temp }

(New-Object System.Net.WebClient).DownloadFile("https://download.microsoft.com/download/B/1/D/B1D7D5BF-3920-47AA-94BD-7A6E48822F18/DotNetCore.2.0.0-WindowsHosting.exe", "D:\temp\dotnet-core-2.0.0-windows-hosting.exe")

(New-Object System.Net.WebClient).DownloadFile("https://tm2018.blob.core.windows.net/devicevms/install_fakedevice.ps1", "D:\temp\install_fakedevice.ps1")

(New-Object System.Net.WebClient).DownloadFile("https://tm2018.blob.core.windows.net/devicevms/FakeDevice.zip", "D:\temp\fakedevice.zip")

Start-Process -FilePath "D:\temp\dotnet-core-2.0.0-windows-hosting.exe" -Verb runAs -ArgumentList '/passive' -Wait

If ((Test-Path "c:\fakedevice") -eq $True) { Remove-Item -Path c:\fakedevice -Recurse }

Expand-Archive -LiteralPath D:\temp\fakedevice.zip -DestinationPath c:\fakedevice

Start-Process -FilePath "C:\Program Files\dotnet\dotnet" -Verb runAs -ArgumentList "C:\fakedevice\fakedevice.dll $hostname $deviceId $symmetrickey"