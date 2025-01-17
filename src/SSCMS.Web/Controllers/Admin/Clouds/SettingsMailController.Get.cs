﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SSCMS.Web.Controllers.Admin.Clouds
{
    public partial class SettingsMailController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get()
        {
            if (!await _authManager.IsSuperAdminAsync())
            {
                return Unauthorized();
            }

            var config = await _configRepository.GetAsync();

            return new GetResult
            {
                IsCloudMail = config.IsCloudMail,
                IsCloudMailContentAdd = config.IsCloudMailContentAdd,
                IsCloudMailContentEdit = config.IsCloudMailContentEdit,
                CloudMailAddress = config.CloudMailAddress,
            };
        }
    }
}
