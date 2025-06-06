﻿using Microsoft.AspNetCore.Identity;

namespace IRechargedAPI.Infrastruture.BLL.AuthService
{
    public class APIUser : IdentityUser
    {

        public string Email { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }
    }
}
