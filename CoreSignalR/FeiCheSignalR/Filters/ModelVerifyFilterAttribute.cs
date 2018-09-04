using FeiCheSignalR.Models.EnumModel;
using FeiCheSignalR.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeiCheSignalR.Filters
{
    public class ModelVerifyFilterAttribute : ActionFilterAttribute
    {
        private JsonResult GenerateJsonResult(string message)
        {
            var data = BaseResponse.GetBaseResponse(BusinessStatusType.ParameterError, message);
            return new JsonResult(data)
            {
                StatusCode = 200
            };
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            if (context.ActionArguments.Where(o => o.Value != null).Any())
            {
                if (!context.ModelState.IsValid)
                {
                    var listErrMsg = new List<string>();
                    foreach (var value in context.ModelState.Values)
                    {
                        if (value.Errors.AsEnumerable().Any())
                        {
                            var errMsg = value.Errors.FirstOrDefault().ErrorMessage;
                            if (string.IsNullOrEmpty(errMsg))
                            {
                                errMsg = value.Errors.FirstOrDefault().Exception.Message;
                            }
                            listErrMsg.Add(errMsg);
                        }
                    }
                    var errorMsg = string.Join(" ", listErrMsg);
                    context.Result = GenerateJsonResult(errorMsg);
                    return;
                }
            }
            //else
            //{
            //    context.Result = GenerateJsonResult("参数不能为空");
            //    return;
            //}
        }

    }
}
