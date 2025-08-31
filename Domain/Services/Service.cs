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
            Rename(name);
            ChangeDuration(durationMinutes);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name required", nameof(name));

            Name = name.Trim();
        }

        public void ChangeDuration(int durationMinutes)
        {
            if (durationMinutes is < 15 or > 240)
                throw new ArgumentException("Duration out of range", nameof(durationMinutes));

            DurationMinutes = durationMinutes;
        }

        public void Update(string name, int durationMinutes)
        {
            Rename(name);
            ChangeDuration(durationMinutes);
        }
    }
}
