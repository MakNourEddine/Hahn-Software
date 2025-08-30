using Domain.Common;

namespace Domain.Services
{
    public class Service : BaseEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; } = default!;
        public int DurationMinutes { get; private set; }


        private Service() { }
        public Service(string name, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required");
            if (durationMinutes is < 15 or > 240) throw new ArgumentException("Duration out of range");
            Name = name.Trim();
            DurationMinutes = durationMinutes;
        }
    }
}
