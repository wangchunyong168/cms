﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Web.Controllers.Admin.Clouds
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class SettingsMailController : ControllerBase
    {
        private const string Route = "clouds/settingsMail";

        private readonly IAuthManager _authManager;
        private readonly ICloudManager _cloudManager;
        private readonly IConfigRepository _configRepository;

        public SettingsMailController(IAuthManager authManager, ICloudManager cloudManager, IConfigRepository configRepository)
        {
            _authManager = authManager;
            _cloudManager = cloudManager;
            _configRepository = configRepository;
        }

        public class GetResult
        {
            public bool IsCloudMail { get; set; }
            public bool IsCloudMailContentAdd { get; set; }
            public bool IsCloudMailContentEdit { get; set; }
            public string CloudMailAddress { get; set; }
        }

        public class SubmitRequest
        {
            public bool IsCloudMail { get; set; }
            public bool IsCloudMailContentAdd { get; set; }
            public bool IsCloudMailContentEdit { get; set; }
            public string CloudMailAddress { get; set; }
        }
    }
}
