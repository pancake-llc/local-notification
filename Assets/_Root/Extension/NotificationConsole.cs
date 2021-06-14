using System;
using System.Collections.Generic;
#if UNITY_ANDROID
using Lance.Common.LocalNotification.Android;
#endif
#if UNITY_IOS
using Lance.Common.LocalNotification.iOS;
using Unity.Notifications.iOS;
#endif
using UnityEngine;
using UnityEngine.Events;
using MEC;

namespace Lance.Common.LocalNotification
{
    public enum TypeNoti
    {
        OnceTime = 0,
        Repeat = 1,
    }

    [Serializable]
    public struct NotificationData
    {
        public string title;
        public string message;

        public NotificationData(string title, string message)
        {
            this.title = title;
            this.message = message;
        }
    }

    [Serializable]
    public class NotificationStuctureData
    {
        public TypeNoti type;
        public string chanel;
        public int minute = 1;
        public bool autoSchedule;
        public NotificationData[] datas;
    }

    /// <summary>
    /// Manages the console on screen that displays information about notifications,
    /// and allows you to schedule more.
    /// </summary>
    [RequireComponent(typeof(GameNotificationsManager))]
    public class NotificationConsole : MonoBehaviour
    {
        // On iOS, this represents the notification's Category Identifier, and is optional
        // On Android, this represents the notification's channel, and is required (at least one).

        [SerializeField] private NotificationStuctureData[] structures =
        {
            new NotificationStuctureData() {type = TypeNoti.Repeat, chanel = "channel_repeat", minute = 1440, autoSchedule = true},
            new NotificationStuctureData() {type = TypeNoti.OnceTime, chanel = "channel_event", minute = 120, autoSchedule = false},
            new NotificationStuctureData() {type = TypeNoti.OnceTime, chanel = "channel_noti", minute = 120, autoSchedule = false},
        };

        public UnityEvent onUpdateDeliveryTime;
        public GameNotificationsManager Manager => _manager != null ? _manager : _manager = GetComponent<GameNotificationsManager>();

        // Update pending notifications in the next update.
        //private bool _updatePendingNotifications;
        private DateTime[] _dateTimes;
        private TimeSpan[] _timeSpans;
        private GameNotificationChannel[] _channels;
        private GameNotificationsManager _manager;

        private void Start()
        {
            // Set up channels (mostly for Android)
            // You need to have at least one of these

            _channels = new GameNotificationChannel[structures.Length];
            _dateTimes = new DateTime[structures.Length];
            _timeSpans = new TimeSpan[structures.Length];
            for (int i = 0; i < structures.Length; i++)
            {
                var chanelCache = structures[i];
                var chanelName = chanelCache.type == TypeNoti.OnceTime ? "Cygnus" : "Nova";
                var chanelDescription = chanelCache.type == TypeNoti.OnceTime ? "Newsletter Announcement" : "Daily Newsletter";
                _channels[i] = new GameNotificationChannel(structures[i].chanel, chanelName, chanelDescription);
            }

            Manager.Initialize(_channels);
            Manager.CancelAllNotifications();
            Manager.DismissAllNotifications();
        }

        private void UpdateDeliveryTime()
        {
            var currentNow = DateTime.Now.ToLocalTime();

            for (int i = 0; i < structures.Length; i++)
            {
                if (!structures[i].autoSchedule) continue;

                var chanelCache = structures[i];
                var data = chanelCache.datas.PickRandom();

                if (chanelCache.type == TypeNoti.OnceTime)
                {
                    var deliveryTime = currentNow.AddMinutes(chanelCache.minute);
                    _dateTimes[i] = new DateTime(deliveryTime.Year,
                        deliveryTime.Month,
                        deliveryTime.Day,
                        deliveryTime.Hour,
                        deliveryTime.Minute,
                        deliveryTime.Second,
                        DateTimeKind.Local);
                    SendNotification(data.title,
                        data.message,
                        _dateTimes[i],
                        channelId: _channels[i].Id,
                        smallIcon: "icon_0",
                        largeIcon: "icon_1");
                }
                else
                {
                    var deliveryTime = currentNow.AddMinutes(chanelCache.minute);
                    _dateTimes[i] = new DateTime(deliveryTime.Year,
                        deliveryTime.Month,
                        deliveryTime.Day,
                        deliveryTime.Hour,
                        deliveryTime.Minute,
                        deliveryTime.Second,
                        DateTimeKind.Local);
                    _timeSpans[i] = new TimeSpan(0, 0, chanelCache.minute, 0);
                    SendNotification(data.title,
                        data.message,
                        _dateTimes[i],
                        channelId: _channels[i].Id,
                        smallIcon: "icon_0",
                        largeIcon: "icon_1",
                        timeRepeatAt: _timeSpans[i]);
                }
            }
        }

        private void OnEnable()
        {
            if (Manager != null)
            {
                Manager.LocalNotificationDelivered += OnDelivered;
                Manager.LocalNotificationExpired += OnExpired;
            }
        }

        private void OnDisable()
        {
            if (Manager != null)
            {
                Manager.LocalNotificationDelivered -= OnDelivered;
                Manager.LocalNotificationExpired -= OnExpired;
            }
        }

        /// <summary>
        /// Queue a notification with the given parameters.
        /// </summary>
        /// <param name="title">The title for the notification.</param>
        /// <param name="body">The body text for the notification.</param>
        /// <param name="deliveryTime">The time to deliver the notification.</param>
        /// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
        /// <param name="reschedule">
        /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
        /// </param>
        /// <param name="channelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
        /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>
        /// <param name="smallIcon">Notification small icon.</param>
        /// <param name="largeIcon">Notification large icon.</param>
        /// <param name="timeRepeatAt">time repeat fire notification</param>
        public void SendNotification(
            string title,
            string body,
            DateTime deliveryTime,
            int? badgeNumber = null,
            bool reschedule = false,
            string channelId = null,
            string smallIcon = null,
            string largeIcon = null,
            TimeSpan? timeRepeatAt = null)
        {
            IGameNotification notification = Manager.CreateNotification();

            if (notification == null) return;

            notification.Title = title;
            notification.Body = body;
            notification.Group = channelId;
#if UNITY_ANDROID
            if (timeRepeatAt != null)
            {
                if (notification is AndroidGameNotification notiAndroid)
                {
                    notiAndroid.RepeatInterval = timeRepeatAt;
                }
            }

#elif UNITY_IOS
            if (notification is iOSGameNotification notiIOS)
            {
                if (notiIOS.InternalNotification.Trigger is iOSNotificationTimeIntervalTrigger triggerTime)
                {
                    triggerTime.Repeats = timeRepeatAt != null;
                }
                else if (notiIOS.InternalNotification.Trigger is iOSNotificationCalendarTrigger triggerCalender)
                {
                    triggerCalender.Repeats = timeRepeatAt != null;
                }
            }
#endif
            notification.DeliveryTime = deliveryTime;
            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;

            if (badgeNumber != null) notification.BadgeNumber = badgeNumber;

            PendingNotification notificationToDisplay = Manager.ScheduleNotification(notification);
            notificationToDisplay.Reschedule = reschedule;
        }

        /// <summary>
        /// Cancel a given pending notification
        /// </summary>
        public void CancelPendingNotificationItem(PendingNotification itemToCancel) { Manager.CancelNotification(itemToCancel.Notification.Id.Value); }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                if (Manager.Initialized)
                {
                    Manager.CancelAllNotifications();
                    Manager.DismissAllNotifications();
                }

                UpdatePendingNotificationsNextFrame().RunCoroutine();
            }
            else
            {
                JobScheduleNotification();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                JobScheduleNotification();
            }
            else
            {
                if (Manager.Initialized)
                {
                    Manager.CancelAllNotifications();
                    Manager.DismissAllNotifications();
                }
            }
        }

        private void JobScheduleNotification()
        {
            if (!Manager.Initialized) return;

            Manager.CancelAllNotifications();
            Manager.DismissAllNotifications();
            UpdateDeliveryTime();
            onUpdateDeliveryTime?.Invoke();
        }

        private void OnDelivered(PendingNotification deliveredNotification)
        {
            // Schedule this to run on the next frame (can't create UI elements from a Java callback)
            ShowDeliveryNotificationCoroutine(deliveredNotification.Notification).RunCoroutine();
        }

        private void OnExpired(PendingNotification obj) { }

        private IEnumerator<float> ShowDeliveryNotificationCoroutine(IGameNotification deliveredNotification) { yield return Timing.WaitForOneFrame; }

        private IEnumerator<float> UpdatePendingNotificationsNextFrame() { yield return Timing.WaitForOneFrame; }
    }


    internal static class NotificationUtility
    {
        /// <summary>
        /// Indicates the random value in the <paramref name="collection"/>
        /// if <paramref name="collection"/> is empty return default vaule of T
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T PickRandom<T>(this T[] collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.Length == 0 ? default : collection[UnityEngine.Random.Range(0, collection.Length - 1)];
        }
    }
}