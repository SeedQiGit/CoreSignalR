using FeiCheSignalR.Infrastructure.Helper;
using FeiCheSignalR.Models.EnumModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeiCheSignalR.Models.Response
{
    public class BaseResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }


        private object _data;
        public object Data
        {
            get { return _data ?? new object(); }
            set
            {
                if (value is IList || value is string)
                {
                    _data = new { Value = value };
                }
                else
                {
                    _data = value;
                }
            }
        }

        public BaseResponse()
        {

        }

        public BaseResponse(int code) : this(code, "")
        {

        }

        public BaseResponse(int code, string message) : this(code, message, null)
        {
        }

        public BaseResponse(int code, string message, object data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public static BaseResponse GetBaseResponse(int code)
        {
            return new BaseResponse(code);
        }

        public static BaseResponse GetBaseResponse(int code, string message)
        {
            return new BaseResponse(code, message);
        }

        public static BaseResponse GetBaseResponse(int code, string message, object data)
        {
            return new BaseResponse(code, message, data);
        }

        public static BaseResponse GetBaseResponse(BusinessStatusType code)
        {
            return new BaseResponse((int)code, EnumHelper.GetEnumDescription(code));
        }

        public static BaseResponse GetBaseResponse(BusinessStatusType code, string message)
        {
            return new BaseResponse((int)code, message);
        }

        public static BaseResponse GetBaseResponse(BusinessStatusType code, object data)
        {
            return new BaseResponse((int)code, EnumHelper.GetEnumDescription(code), data);
        }
    }
}
