using EzAPI.RunTime.UserAccessible;
using System;

namespace EzAPI
{
    namespace Example
    {
        [Serializable]
        public class LoginData : RequestPayloadBase
        {
            public string email;
            public string password;
        }
    }
}