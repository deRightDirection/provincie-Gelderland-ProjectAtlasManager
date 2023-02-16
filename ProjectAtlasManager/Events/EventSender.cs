using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAtlasManager.Events
{
  class EventSender : CompositePresentationEvent<EventBase>
    {
      /// <summary>
      /// Allow subscribers to register for our custom event
      /// </summary>
      /// <param name="action">The callback which will be used to notify the subscriber</param>
      /// <param name="keepSubscriberReferenceAlive">Set to true to retain a strong reference</param>
      /// <returns><see cref="ArcGIS.Core.Events.SubscriptionToken"/></returns>
      public static SubscriptionToken Subscribe(Action<EventBase> action, bool keepSubscriberReferenceAlive = false)
      {
        return FrameworkApplication.EventAggregator.GetEvent<EventSender>()
            .Register(action, keepSubscriberReferenceAlive);
      }

    internal static void Subscribe(object update, bool v)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Allow subscribers to unregister from our custom event
    /// </summary>
    /// <param name="subscriber">The action that will be unsubscribed</param>
    public static void Unsubscribe(Action<EventBase> subscriber)
      {
        FrameworkApplication.EventAggregator.GetEvent<EventSender>().Unregister(subscriber);
      }
      /// <summary>
      /// Allow subscribers to unregister from our custom event
      /// </summary>
      /// <param name="token">The token that will be used to find the subscriber to unsubscribe</param>
      public static void Unsubscribe(SubscriptionToken token)
      {
        FrameworkApplication.EventAggregator.GetEvent<EventSender>().Unregister(token);
      }

      /// <summary>
      /// Event owner calls publish to raise the event and notify subscribers
      /// </summary>
      /// <param name="payload">The associated event information</param>
      internal static void Publish(EventBase payload)
      {
        FrameworkApplication.EventAggregator.GetEvent<EventSender>().Broadcast(payload);
      }
    }
}
