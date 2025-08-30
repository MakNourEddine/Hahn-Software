using Domain.Common;

namespace Domain.Patients
{
    public class Patient : BaseEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string FullName { get; private set; } = default!;
        public string Email { get; private set; } = default!;


        private Patient() { }
        public Patient(string fullName, string email)
        {
            FullName = string.IsNullOrWhiteSpace(fullName) ? throw new ArgumentException("Name required") : fullName.Trim();
            Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentException("Email required") : email.Trim();
        }
    }
}
