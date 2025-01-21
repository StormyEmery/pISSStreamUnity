using System;
using System.Threading;
using com.lightstreamer.client;

namespace LeTai.pISSStream
{
public class pISSStream
{
    public class Data
    {
        public readonly float pISSPercentage;

        public Data(float pIssPercentage)
        {
            pISSPercentage = pIssPercentage;
        }
    }

    readonly LightstreamerClient client;
    readonly Subscription        subscription;

    Data data = null;

    public pISSStream()
    {
        var sub = new Subscription(subscriptionMode: "MERGE",
                                   items: new[] {
                                       "NODE3000005",
                                       // "TIME_000001"
                                   },
                                   fields: new[] { "Value" });
        subscription = sub;
        subscription.addListener(new pISSListener(this));

        client = new LightstreamerClient("https://push.lightstreamer.com", "ISSLIVE");
        client.subscribe(subscription);
        client.connect();
    }

    public bool TryGetLatestData(out Data latest)
    {
        latest = Interlocked.Exchange(ref data, null);
        return latest != null;
    }

    public void Dispose()
    {
        client.unsubscribe(subscription);
        client.disconnect();
    }

    class pISSListener : SubscriptionListener
    {
        readonly pISSStream stream;

        public pISSListener(pISSStream stream)
        {
            this.stream = stream;
        }

        void SubscriptionListener.onItemUpdate(ItemUpdate itemUpdate)
        {
            if (itemUpdate.ItemName == "NODE3000005")
            {
                var newData = new Data(pIssPercentage: Convert.ToSingle(itemUpdate.Fields["Value"]));
                // var newData = new Data(pIssPercentage: (float) new Random().NextDouble() * 100);
                Interlocked.Exchange(ref stream.data, newData);
            }
        }

        void SubscriptionListener.onClearSnapshot(string                    itemName,    int    itemPos)             { }
        void SubscriptionListener.onCommandSecondLevelItemLostUpdates(int   lostUpdates, string key)                 { }
        void SubscriptionListener.onCommandSecondLevelSubscriptionError(int code,        string message, string key) { }
        void SubscriptionListener.onEndOfSnapshot(string                    itemName,    int    itemPos)                  { }
        void SubscriptionListener.onItemLostUpdates(string                  itemName,    int    itemPos, int lostUpdates) { }
        void SubscriptionListener.onListenEnd()                                 { }
        void SubscriptionListener.onListenStart()                               { }
        void SubscriptionListener.onRealMaxFrequency(string frequency)          { }
        void SubscriptionListener.onSubscription()                              { }
        void SubscriptionListener.onSubscriptionError(int code, string message) { }
        void SubscriptionListener.onUnsubscription() { }
    }
}
}
