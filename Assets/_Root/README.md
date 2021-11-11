## Local Notification


UPM Package
---
### Install via git URL

Requires a version of unity that supports path query parameter for git packages (Unity >= 2019.3.4f1, Unity >= 2020.1a21).
You can add 

```cs
https://github.com/yenmoc/local-notification.git?path=Assets/_Root
```
 to PackageManager

![image](https://user-images.githubusercontent.com/44673303/112711380-791f6180-8efa-11eb-953f-d92d2bc93f0f.png)

![image](https://user-images.githubusercontent.com/44673303/112711396-99e7b700-8efa-11eb-9548-a6ab1487d887.png)

or add 
```cs
"com.lance.local-notification": "https://github.com/yenmoc/local-notification.git?path=Assets/_Root"
```
to `Packages/manifest.json`.

If you want to set a target version. lance uses the `year.month.day` release tag so you can specify a version like `#2021.5.25`. For example 

```cs
https://github.com/yenmoc/local-notification.git?path=Assets/_Root#2021.5.25
```


### Install via manifest.json

Add the following dependencies in manifest

```cs
"com.lance.local-notification": "https://github.com/yenmoc/local-notification.git?path=Assets/_Root#2021.5.25",
```


### Usages

- add component `NotificationConsole` into object has dont destroy to reschedule each time go to background and cancel when back to forceground
