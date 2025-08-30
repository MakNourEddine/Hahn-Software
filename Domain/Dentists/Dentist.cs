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
            FullName = string.IsNullOrWhiteSpace(fullName) ? throw new ArgumentException("Name required") : fullName.Trim();
        }
    }
}
