﻿using System.ComponentModel.DataAnnotations;

namespace FileShare.Models
{
    public class LoginModel
    {
        public int Id { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "*", AllowEmptyStrings = false)]
        [EmailAddress]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "*", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember-me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
