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
    [DisplayName("GetDialogRequestModel")]
Getdialogrequestmodel,
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
    [DisplayName("GetDialogResponseModel")]
Getdialogresponsemodel,
    [DisplayName("RequestResponseBase")]
Requestresponsebase
}

            [Serializable]
            public enum EndPoints
            {

    [DisplayName("com")]
Com,
    [DisplayName("v1/getDialog/1/1")]
V1Getdialog11
}
        }
    }
}