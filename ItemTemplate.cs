using System;

namespace Analysis
{
    public abstract class AbstractItemTemplate<T> : ItemTemplate<T> where T : Item
    {
        public string Content { get; set; }

        public Group Group { get; set; }

        public bool Open { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string Title { get; set; }

        public bool Valid { get => true; set => throw new NotImplementedException(); }

        public abstract T Build();

        Item ItemTemplate.Build() => Build();
    }

    public sealed class InvalidItemTemplate : ItemTemplate
    {
        public string Content { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Group Group { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Open { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTimeOffset Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Valid { get => false; set => throw new NotImplementedException(); }

        public Item Build() => throw new NotImplementedException();
    }

    public interface ItemTemplate
    {
        string Content { get; set; }

        Group Group { get; set; }

        bool Open { get; set; }

        DateTimeOffset Timestamp { get; set; }

        string Title { get; set; }

        bool Valid { get; set; }

        Item Build();
    }

    public interface ItemTemplate<T> : ItemTemplate where T : Item
    {
        new T Build();
    }

    public sealed class RangeItemTemplate : AbstractItemTemplate<RangeItem>
    {
        public ItemTemplate Base { get; set; }

        public override RangeItem Build() => new RangeItem(Content, Group.Name, Title, Base.Timestamp, Timestamp);
    }

    public sealed class SingleItemTemplate : AbstractItemTemplate<SingleItem>
    {
        public override SingleItem Build() => new SingleItem(Content, Group.Name, Title, Timestamp);
    }
}
