using Microsoft.AspNetCore.Identity;

namespace Allup.Extensions
{
    public class IdentityErrorDescriberAZ : IdentityErrorDescriber
    {
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Reqem mutleqdir" };
        }
    }
}
