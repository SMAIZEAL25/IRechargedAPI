﻿using System.Drawing;
using System.Net;

namespace IRechargedAPI.Infrastruture.BLL.AuthService
{
    public class APIResponse<T>
    {
        // here is the API response implemented in the controller class 
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string Token { get; set; }
        public DateTime ExpiresIn { get; set; }
        public T Data { get; set; }

    }
}
