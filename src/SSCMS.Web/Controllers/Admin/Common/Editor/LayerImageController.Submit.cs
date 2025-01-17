﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Core.Utils;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Common.Editor
{
    public partial class LayerImageController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<List<SubmitResult>>> Submit([FromBody] SubmitRequest request)
        {
            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var isAutoStorage = await _storageManager.IsAutoStorageAsync(request.SiteId, SyncType.Images);

            var result = new List<SubmitResult>();
            foreach (var filePath in request.FilePaths)
            {
                if (string.IsNullOrEmpty(filePath)) continue;

                var fileName = PathUtils.GetFileName(filePath);

                var fileExtName = StringUtils.ToLower(PathUtils.GetExtension(filePath));
                var localDirectoryPath = await _pathManager.GetUploadDirectoryPathAsync(site, fileExtName);

                var imageUrl = await _pathManager.GetSiteUrlByPhysicalPathAsync(site, filePath, true);
                if (isAutoStorage)
                {
                    var (success, url) = await _storageManager.StorageAsync(request.SiteId, filePath);
                    if (success)
                    {
                        imageUrl = url;
                    }
                }

                if (request.IsMaterial)
                {
                    var materialFileName = PathUtils.GetMaterialFileName(fileName);
                    var virtualDirectoryPath = PathUtils.GetMaterialVirtualDirectoryPath(UploadType.Image);

                    var directoryPath = _pathManager.ParsePath(virtualDirectoryPath);
                    var materialFilePath = PathUtils.Combine(directoryPath, materialFileName);
                    DirectoryUtils.CreateDirectoryIfNotExists(materialFilePath);

                    FileUtils.CopyFile(filePath, materialFilePath, true);

                    var image = new MaterialImage
                    {
                        GroupId = -request.SiteId,
                        Title = fileName,
                        Url = PageUtils.Combine(virtualDirectoryPath, materialFileName)
                    };

                    await _materialImageRepository.InsertAsync(image);
                }

                if (request.IsThumb)
                {
                    var localSmallFileName = Constants.SmallImageAppendix + fileName;
                    var localSmallFilePath = PathUtils.Combine(localDirectoryPath, localSmallFileName);

                    var thumbnailUrl = await _pathManager.GetSiteUrlByPhysicalPathAsync(site, localSmallFilePath, true);
                    if (isAutoStorage)
                    {
                        var (success, url) = await _storageManager.StorageAsync(request.SiteId, localSmallFilePath);
                        if (success)
                        {
                            thumbnailUrl = url;
                        }
                    }

                    var width = request.ThumbWidth;
                    var height = request.ThumbHeight;
                    ImageUtils.MakeThumbnail(filePath, localSmallFilePath, width, height, true);

                    if (request.IsLinkToOriginal)
                    {
                        result.Add(new SubmitResult
                        {
                            ImageUrl = thumbnailUrl,
                            PreviewUrl = imageUrl
                        });
                    }
                    else
                    {
                        FileUtils.DeleteFileIfExists(filePath);
                        result.Add(new SubmitResult
                        {
                            ImageUrl = thumbnailUrl
                        });
                    }
                }
                else
                {
                    result.Add(new SubmitResult
                    {
                        ImageUrl = imageUrl
                    });
                }
            }

            var options = TranslateUtils.JsonDeserialize(site.Get<string>(nameof(LayerImageController)), new Options
            {
                IsMaterial = true,
                IsThumb = false,
                ThumbWidth = 1024,
                ThumbHeight = 1024,
                IsLinkToOriginal = true,
            });

            options.IsMaterial = request.IsMaterial;
            options.IsThumb = request.IsThumb;
            options.ThumbWidth = request.ThumbWidth;
            options.ThumbHeight = request.ThumbHeight;
            options.IsLinkToOriginal = request.IsLinkToOriginal;
            site.Set(nameof(LayerImageController), TranslateUtils.JsonSerialize(options));

            await _siteRepository.UpdateAsync(site);

            return result;
        }
    }
}