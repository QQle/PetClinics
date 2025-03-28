﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace PetClinics.Models
{
    /// <summary>
    /// Класс, представляющий данные, необходимые для регистрации нового пользователя.
    /// </summary>
    public class RegistrationUser
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; }
    }
}
