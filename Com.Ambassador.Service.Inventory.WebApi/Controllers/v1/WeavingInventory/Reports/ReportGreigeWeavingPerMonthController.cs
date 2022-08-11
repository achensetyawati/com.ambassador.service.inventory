﻿using AutoMapper;
using Com.Ambassador.Service.Inventory.Lib.Services;
using Com.Ambassador.Service.Inventory.Lib.Services.InventoryWeaving.Reports.ReportGreigeWeavingPerMonth;
using Com.Ambassador.Service.Inventory.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Com.Ambassador.Service.Inventory.WebApi.Controllers.v1.WeavingInventory.Reports
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/weaving-stock-report")]
    [Authorize]
    public class ReportGreigeWeavingPerMonthController : Controller
    {
        protected IIdentityService IdentityService;
        protected readonly IValidateService ValidateService;
        protected readonly IReportGreigeWeavingPerMonthService Service;
        protected readonly string ApiVersion;
        public ReportGreigeWeavingPerMonthController(IIdentityService identityService, IValidateService validateService, IReportGreigeWeavingPerMonthService service)
        {
            IdentityService = identityService;
            ValidateService = validateService;
            Service = service;
            ApiVersion = "1.0.0";
        }

        [HttpGet]
        public IActionResult GetReportStock(DateTime? dateTo, int page = 1, int size = 25, string Order = "{}")
        {
            try
            {
                IdentityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                IdentityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());
                IdentityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");

                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                string accept = Request.Headers["Accept"];

                var data = Service.GetStockReportGreige(dateTo, offset, page, size, Order );



                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
        [HttpGet("download")]
        public IActionResult GetExcelAll([FromHeader(Name = "x-timezone-offset")] string timezone, DateTime? dateTo)
        {
            try
            {
                //VerifyUser();
                byte[] xlsInBytes;
                int clientTimeZoneOffset = Convert.ToInt32(timezone);
                var Result = Service.GenerateExcel( dateTo, clientTimeZoneOffset);
                string filename = "Monitoring Penerimaan Gudang Weaving.xlsx";

                xlsInBytes = Result.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
