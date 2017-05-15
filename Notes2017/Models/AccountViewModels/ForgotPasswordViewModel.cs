﻿using System.ComponentModel.DataAnnotations;

namespace Notes2017.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
