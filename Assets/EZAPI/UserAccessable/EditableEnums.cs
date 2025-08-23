using System;
using static EzAPI.TypeFinder;
namespace EzAPI
{
    namespace RunTime
    {
        namespace UserAccessible
        {
            [Serializable]
            public enum PayLoadEnum
            {

                [DisplayName("None")]
                None,
                [DisplayName("LoginData")]
                Logindata,
                [DisplayName("RequestPayloadBase")]
                Requestpayloadbase
            }

            [Serializable]
            public enum ResponseEnum
            {

                [DisplayName("ExampleResponse")]
                Exampleresponse,
                [DisplayName("RequestResponseBase")]
                Requestresponsebase
            }

            [Serializable]
            public enum EndPoints
            {

                [DisplayName("com")]
                Com
            }
        }
    }
}