using System;

namespace ShoppingCart.EventFeed
{
    public struct Event
    {
        public long SequenceNumber { get; }
        public DateTimeOffset OccuredAt { get; }
        public string Name { get; }
        public object Content { get; }

        public Event(long sequenceNumber, DateTimeOffset occuredAt, string name, object content)
        {
            SequenceNumber = sequenceNumber;
            OccuredAt = occuredAt;
            Name = name;
            Content = content;
        }
    }
}