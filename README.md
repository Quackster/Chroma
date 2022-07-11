# Chroma
Chroma (Greek word χρώμα for paint) a furniture renderer for the Habbo SWF's

## How to use?

Compile the ChromaWebApp to the operating system of your choice (it runs on .NET 5 and works on Linux).

Put the swfs in the folder /swfs/hof_furni/ where the app is located, for example...

- chroma/
  - ChromaWebApp.exe
  - swfs/hof_furni/
    - rare_dragonlamp.swf

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
