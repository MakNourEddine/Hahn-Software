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
            Rename(fullName);
            ChangeEmail(email);
        }

        public void Rename(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Name required", nameof(fullName));

            FullName = fullName.Trim();
        }

        public void ChangeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email required", nameof(email));

            Email = email.Trim();
        }

        public void Update(string fullName, string email)
        {
            Rename(fullName);
            ChangeEmail(email);
        }
    }
}
