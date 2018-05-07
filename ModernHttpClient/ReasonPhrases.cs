
namespace ModernHttpClient
{
    public enum ReasonPhrases
    {
        // 1xx: Informational - Request received, continuing process

        Continue = 100,
        Switching_Protocols = 101,
        Processing = 102,
        Early_Hints = 103,

        // 104-199   Unassigned

        // 2xx: Success - The action was successfully received, understood, and accepted

        OK = 200,
        Created = 201,
        Accepted = 202,
        Non_Authoritative_Information = 203,
        No_Content = 204,
        Reset_Content = 205,
        Partial_Content = 206,
        Multi_Status = 207,
        Already_Reported = 208,

        // 209-225 Unassigned

        IM_Used = 226,

        // 227-299   Unassigned

        // 3xx: Redirection - Further action must be taken in order to complete the request

        Multiple_Choices = 300,
        Moved_Permanently = 301,
        Found = 302,
        See_Other = 303,
        Not_Modified = 304,
        Use_Proxy = 305,
        Unused = 306,
        Temporary_Redirect = 307,
        Permanent_Redirect = 308,

        // 309-399  Unassigned

        // 4xx: Client Error - The request contains bad syntax or cannot be fulfilled

        Bad_Request = 400,
        Unauthorized = 401,
        Payment_Required = 402,
        Forbidden = 403,
        Not_Found = 404,
        Method_Not_Allowed = 405,
        Not_Acceptable = 406,
        Proxy_Authentication_Required = 407,
        Request_Timeout = 408,
        Conflict = 409,
        Gone = 410,
        Length_Required = 411,
        Precondition_Failed = 412,
        Payload_Too_Large = 413,
        URI_Too_Long = 414,
        Unsupported_Media_Type = 415,
        Range_Not_Satisfiable = 416,
        Expectation_Failed = 417,

        // 418-420  Unassigned

        Misdirected_Request = 421,
        Unprocessable_Entity = 422,
        Locked = 423,
        Failed_Dependency = 424,
        //Unassigned = 425,
        Upgrade_Required = 426,
        //Unassigned = 427,
        Precondition_Required = 428,
        Too_Many_Requests = 429,
        //Unassigned = 430,
        Request_Header_Fields_Too_Large = 431,

        // 432-450 Unassigned

        Unavailable_For_Legal_Reasons = 451,

        // 452-499 Unassigned

        Internal_Server_Error = 500,
        Not_Implemented = 501,
        Bad_Gateway = 502,
        Service_Unavailable = 503,
        Gateway_Timeout = 504,
        HTTP_Version_Not_Supported = 505,
        Variant_Also_Negotiates = 506,
        Insufficient_Storage = 507,
        Loop_Detected = 508,
        //Unassigned  509
        Not_Extended = 510,
        Network_Authentication_Required = 511

        // 512-599  Unassigned
    }
}
