using System.Net;
using EventStore.ClientAPI;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShoppingCart.EventFeed
{
    public class EventStore : IEventStore
    {
        const int DEFAULTPORT = 1113;
        private static readonly Encoding ContentEncoding = Encoding.UTF8;

        public async Task Raise(string eventName, object content)
        {
            using (var conn = EventStoreConnection.Create(ConnectionSettings.Create(), new IPEndPoint(IPAddress.Loopback, DEFAULTPORT)))
            {
                conn.ConnectAsync().Wait();
                var contentJson = JsonConvert.SerializeObject(content);
                var metaDataJson = JsonConvert.SerializeObject(
                    new EventMetadata
                    {
                        OccuredAt = DateTimeOffset.Now,
                        Eventname = eventName
                    });

                var eventData = new EventData(
                    Guid.NewGuid(),
                    "ShoppingCart",
                    isJson: true,
                    data: ContentEncoding.GetBytes(contentJson),
                    metadata: ContentEncoding.GetBytes(metaDataJson));

                await conn.AppendToStreamAsync("ShoppingCart", ExpectedVersion.Any, eventData);
            }
        }

        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            using (var conn = EventStoreConnection.Create(
                ConnectionSettings.Create(),
                new IPEndPoint(IPAddress.Loopback, DEFAULTPORT)))
            {
                conn.ConnectAsync().Wait();

                var result = await conn.ReadStreamEventsForwardAsync(
                    "ShoppingCart",
                    start: (int)firstEventSequenceNumber,
                    count: (int)(lastEventSequenceNumber - firstEventSequenceNumber),
                    resolveLinkTos: false).ConfigureAwait(false);

                return result.Events.Select(
                    (ev, i) =>
                    {
                        var content = JsonConvert.DeserializeObject(ContentEncoding.GetString(ev.Event.Data));
                        var metadata =
                            JsonConvert.DeserializeObject<EventMetadata>(ContentEncoding.GetString(ev.Event.Metadata));
                        return new Event(i + firstEventSequenceNumber, metadata.OccuredAt, metadata.Eventname, content);
                    });
            }
        }

        private class EventMetadata
        {
            public DateTimeOffset OccuredAt { get; set; }
            public string Eventname { get; set; }
        }
    }
}
