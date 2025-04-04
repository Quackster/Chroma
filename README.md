# Chroma
Chroma (Greek word χρώμα for paint) a furniture renderer for the Habbo SWF's

## How to use?

Compile the ChromaWebApp to the operating system of your choice (it runs on .NET 5 and works on Linux).

Put the swfs in the folder /swfs/hof_furni/ where the app is located, for example...

- chroma/
  - ChromaWebApp.exe
  - swfs/hof_furni/
    - rare_dragonlamp.swf
   
## Download

The latest builds for Linux and Windows are found on the [latest](https://github.com/Quackster/Chroma/releases/tag/latest) tag.

| OS | Download |
|---|---|
| Linux (64-bit) | [Chroma-linux-x64.zip](https://github.com/Quackster/Chroma/releases/download/latest/Chroma-linux-x64.zip) |
| Windows (64-bit) | [Chroma-win-x64.zip](https://github.com/Quackster/Chroma/releases/download/latest/Chroma-win-x64.zip) |

## Setup

To run Chroma, you need to install .NET 8 [runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) for your operating system.

Once downloaded, you may execute ``./ChromaWebApp`` (Linux) or ``ChromaWebApp.exe`` (Windows).
Once that's done, you should see the following console output.

```
Loading flash assets...
9762 flash assets loaded
Loading figure data...
301 figure sets loaded
11 figure set types loaded
3 figure palettes loaded
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path...
```

Run the app.

(On Linux for example)

``./ChromaWebApp --urls=http://*:8090/``

(On Windows for example)

``ChromaWebApp.exe --urls=http://*:8090/``

Then proxy it through PHP.

```php
<?php
header ('Content-Type: image/png');
echo file_get_contents("http://127.0.0.1:8090/?" . $_SERVER['QUERY_STRING']);
?>
```

And it should work like so, for example, using the furni imager hosted on Classic Habbo.

https://cdn.classichabbo.com/habbo-imaging/furni?sprite=rare_dragonlamp&color=2&direction=0&small=false

You can see the arguments used above to change how such furniture should be rendered to image.

![](https://cdn.classichabbo.com/habbo-imaging/furni?sprite=rare_dragonlamp&color=2&direction=0&small=false)

## A massive thanks to

- higoka
- Scottstamp
- Arachis
- Speaqer
- Parsnip
- Billsonnn
