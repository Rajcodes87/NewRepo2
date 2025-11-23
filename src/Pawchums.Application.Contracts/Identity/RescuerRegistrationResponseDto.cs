using System;

namespace Pawchums.Identity;

public class RescuerRegistrationResponseDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
}
