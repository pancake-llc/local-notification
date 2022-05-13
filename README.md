## Local Notification


UPM Package
---

### Install via manifest.json

Add the following dependencies in manifest

```cs
"com.snorlax.local-notification": "https://github.com/snorluxe/local-notification.git?path=Assets/_Root#2022.5.13",
"com.pancake.common": "https://github.com/pancake-llc/common.git?path=Assets/_Root#1.1.7",

```


### Usages

- Add component `NotificationConsole` into object has dont destroy to reschedule each time go to background and cancel when back to forceground

- Type `Repeat` will fire notification after each number `Minute`

![image](https://user-images.githubusercontent.com/44673303/141402003-88e7e3f7-bde2-4513-a7bf-d4fc4539ca02.png)


- Event `OnUpdateDeliveryTime` to use when you want send notification one time with diffirent custom `Minute` each time such as fire notification in game idle when building house completed. In case you need write your custom method
to assign to event by call to API `public void UpdateDeliveryTimeBy(string id, int customTimeSchedule = -1)`

*Note :
- Version 2.+ require minimum android api support is 5.+ 