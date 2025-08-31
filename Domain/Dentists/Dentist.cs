using Domain.Common;

namespace Domain.Dentists
{
    public class Dentist : BaseEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string FullName { get; private set; } = default!;

        private Dentist() { }

        public Dentist(string fullName)
        {
            Rename(fullName);
        }

        public void Rename(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));

            FullName = fullName.Trim();
        }
    }
}
