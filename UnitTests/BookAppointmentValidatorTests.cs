using Application.Features.Appointments;
using FluentAssertions;

namespace UnitTests
{
    public class BookAppointmentValidatorTests
    {
        [Fact]
        public void Rejects_past_start()
        {
            var v = new BookAppointment.Validator();
            var result = v.Validate(new BookAppointment.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-15)));
            result.IsValid.Should().BeFalse();
        }
    }
}
