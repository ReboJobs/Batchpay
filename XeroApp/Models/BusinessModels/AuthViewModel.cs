﻿using System.ComponentModel.DataAnnotations;

namespace XeroApp.Models.BusinessModels
{
	public class AuthViewModel
	{
		[Required(ErrorMessage = "Email is required")]
		[EmailAddress]
		public string Email { get; set; }

		[Required(ErrorMessage = "Password is required")]
		public string Password { get; set; }
	}
}