﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Core.Utils;
using System.Collections.Generic;
using SSCMS.Configuration;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Contents
{
    public partial class ContentsCheckController
    {
        [HttpPost, Route(RouteTree)]
        public async Task<ActionResult<TreeResult>> Tree([FromBody] SiteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    MenuUtils.SitePermissions.ContentsCheck))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error(Constants.ErrorNotFound);

            var channel = await _channelRepository.GetAsync(request.SiteId);
            var root = await _channelRepository.GetCascadeAsync(site, channel);

            var siteUrl = await _pathManager.GetSiteUrlAsync(site, true);
            var groupNames = await _contentGroupRepository.GetGroupNamesAsync(request.SiteId);
            var tagNames = await _contentTagRepository.GetTagNamesAsync(request.SiteId);
            var allLevels = CheckManager.GetCheckedLevels(site, true, site.CheckContentLevel, true);
            var levels = new List<KeyValuePair<int, string>>();
            foreach (var level in allLevels)
            {
                if (level.Key == CheckManager.LevelInt.CaoGao || level.Key > 0)
                {
                    continue;
                }
                levels.Add(level);
            }
            var checkedLevels = ElementUtils.GetCheckBoxes(levels);

            var columnsManager = new ColumnsManager(_databaseManager, _pathManager);
            var columns = await columnsManager.GetContentListColumnsAsync(site, channel, ColumnsManager.PageType.CheckContents);

            return new TreeResult
            {
                Root = root,
                SiteUrl = siteUrl,
                GroupNames = groupNames,
                TagNames = tagNames,
                CheckedLevels = checkedLevels,
                Columns = columns
            };
        }
    }
}
