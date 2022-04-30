﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RdtClient.Data.Models.QBittorrent;
using RdtClient.Service.Services;


namespace RdtClient.Web.Controllers;

/// <summary>
/// This API behaves as a regular QBittorrent 4+ API
/// Documentation is found here: https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)
/// </summary>
[ApiController]
[Route("api/v2")]
public class QBittorrentController : Controller
{
    private readonly QBittorrent _qBittorrent;

    public QBittorrentController(QBittorrent qBittorrent)
    {
        _qBittorrent = qBittorrent;
    }

    [AllowAnonymous]
    [Route("auth/login")]
    [HttpGet]
    public async Task<ActionResult> AuthLogin([FromQuery] QBAuthLoginRequest request)
    {
        var result = await _qBittorrent.AuthLogin(request.UserName, request.Password);

        if (result)
        {
            return Ok("Ok.");
        }

        return Ok("Fails.");
    }
        
    [AllowAnonymous]
    [Route("auth/login")]
    [HttpPost]
    public async Task<ActionResult> AuthLoginPost([FromForm] QBAuthLoginRequest request)
    {
        return await AuthLogin(request);
    }

    [Authorize]
    [Route("auth/logout")]
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult> AuthLogout()
    {
        await _qBittorrent.AuthLogout();
        return Ok();
    }

    [Route("app/version")]
    [HttpGet]
    [HttpPost]
    public ActionResult AppVersion()
    {
        return Ok("v4.3.2");
    }

    [Route("app/webapiVersion")]
    [HttpGet]
    [HttpPost]
    public ActionResult AppWebVersion()
    {
        return Ok("2.7");
    }

    [Route("app/buildInfo")]
    [HttpGet]
    [HttpPost]
    public ActionResult AppBuildInfo()
    {
        var result = new AppBuildInfo
        {
            Bitness = 64,
            Boost = "1.75.0",
            Libtorrent = "1.2.11.0",
            Openssl = "1.1.1i",
            Qt = "5.15.2",
            Zlib = "1.2.11"
        };
        return Ok(result);
    }

    [Authorize]
    [Route("app/shutdown")]
    [HttpGet]
    [HttpPost]
    public ActionResult AppShutdown()
    {
        return Ok();
    }

    [AllowAnonymous]
    [Route("app/preferences")]
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult<AppPreferences>> AppPreferences()
    {
        var result = await _qBittorrent.AppPreferences();
        return Ok(result);
    }

    [Authorize]
    [Route("app/setPreferences")]
    [HttpGet]
    [HttpPost]
    public ActionResult AppSetPreferences()
    {
        return Ok();
    }

    [Authorize]
    [Route("app/defaultSavePath")]
    [HttpGet]
    [HttpPost]
    public ActionResult<AppPreferences> AppDefaultSavePath()
    {
        var result = _qBittorrent.AppDefaultSavePath();
        return Ok(result);
    }
        
    [Authorize]
    [Route("torrents/info")]
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult<IList<TorrentInfo>>> TorrentsInfo([FromQuery] QBTorrentsInfoRequest request)
    {
        var results = await _qBittorrent.TorrentInfo();

        if (!String.IsNullOrWhiteSpace(request.Category))
        {
            results = results.Where(m => m.Category == request.Category).ToList();
        }

        return Ok(results);
    }

    [Authorize]
    [Route("torrents/info")]
    [HttpPost]
    public async Task<ActionResult<IList<TorrentInfo>>> TorrentsFilesPost([FromForm] QBTorrentsInfoRequest request)
    {
        return await TorrentsInfo(request);
    }

    [Authorize]
    [Route("torrents/files")]
    [HttpGet]
    public async Task<ActionResult<IList<TorrentFileItem>>> TorrentsFiles([FromQuery] QBTorrentsHashRequest request)
    {
        var result = await _qBittorrent.TorrentFileContents(request.Hash);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize]
    [Route("torrents/files")]
    [HttpPost]
    public async Task<ActionResult<IList<TorrentFileItem>>> TorrentsFilesPost([FromForm] QBTorrentsHashRequest request)
    {
        return await TorrentsFiles(request);
    }

    [Authorize]
    [Route("torrents/properties")]
    [HttpGet]
    public async Task<ActionResult<IList<TorrentInfo>>> TorrentsProperties([FromQuery] QBTorrentsHashRequest request)
    {
        var result = await _qBittorrent.TorrentProperties(request.Hash);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize]
    [Route("torrents/properties")]
    [HttpPost]
    public async Task<ActionResult<IList<TorrentInfo>>> TorrentsPropertiesPost([FromForm] QBTorrentsHashRequest request)
    {
        return await TorrentsProperties(request);
    }

    [Authorize]
    [Route("torrents/pause")]
    [HttpGet]
    public async Task<ActionResult> TorrentsPause([FromQuery] QBTorrentsHashesRequest request)
    {
        var hashes = request.Hashes.Split("|");

        foreach (var hash in hashes)
        {
            await _qBittorrent.TorrentPause(hash);
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/topPrio")]
    [HttpPost]
    public async Task<ActionResult> TorrentsPausePost([FromForm] QBTorrentsHashesRequest request)
    {
        return await TorrentsPause(request);
    }

    [Authorize]
    [Route("torrents/resume")]
    [HttpGet]
    public async Task<ActionResult> TorrentsResume([FromQuery] QBTorrentsHashesRequest request)
    {
        var hashes = request.Hashes.Split("|");

        foreach (var hash in hashes)
        {
            await _qBittorrent.TorrentResume(hash);
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/topPrio")]
    [HttpPost]
    public async Task<ActionResult> TorrentsResumePost([FromForm] QBTorrentsHashesRequest request)
    {
        return await TorrentsResume(request);
    }

    [Authorize]
    [Route("torrents/setShareLimits")]
    [HttpGet]
    [HttpPost]
    public ActionResult TorrentsSetShareLimits()
    {
        return Ok();
    }

    [Authorize]
    [Route("torrents/delete")]
    [HttpGet]
    public async Task<ActionResult> TorrentsDelete([FromQuery] QBTorrentsDeleteRequest request)
    {
        var hashes = request.Hashes.Split("|");

        foreach (var hash in hashes)
        {
            await _qBittorrent.TorrentsDelete(hash, request.DeleteFiles);
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/delete")]
    [HttpPost]
    public async Task<ActionResult> TorrentsDeletePost([FromForm] QBTorrentsDeleteRequest request)
    {
        return await TorrentsDelete(request);
    }

    [Authorize]
    [Route("torrents/add")]
    [HttpGet]
    public async Task<ActionResult> TorrentsAdd([FromQuery] QBTorrentsAddRequest request)
    {
        var urls = request.Urls.Split("\n");

        foreach (var url in urls)
        {
            if (url.StartsWith("magnet"))
            {
                await _qBittorrent.TorrentsAddMagnet(url.Trim(), request.Category, null);
            }
            else if (url.StartsWith("http"))
            {
                var httpClient = new HttpClient();
                var result = await httpClient.GetByteArrayAsync(url);
                await _qBittorrent.TorrentsAddFile(result, request.Category, null);
            }
            else
            {
                throw new Exception($"Invalid torrent link format {url}");
            }
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/add")]
    [HttpPost]
    public async Task<ActionResult> TorrentsAddPost([FromForm] QBTorrentsAddRequest request)
    {
        foreach (var file in Request.Form.Files)
        {
            if (file.Length > 0)
            {
                await using var target = new MemoryStream();

                await file.CopyToAsync(target);
                var fileBytes = target.ToArray();

                await _qBittorrent.TorrentsAddFile(fileBytes, request.Category, request.Priority);
            }
        }
            
        if (request.Urls != null)
        {
            return await TorrentsAdd(request);
        }

        return Ok();
    }
        
    [Authorize]
    [Route("torrents/setCategory")]
    [HttpGet]
    public async Task<ActionResult> TorrentsSetCategory([FromQuery] QBTorrentsSetCategoryRequest request)
    {
        var hashes = request.Hashes.Split("|");

        foreach (var hash in hashes)
        {
            await _qBittorrent.TorrentsSetCategory(hash, request.Category);
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/setCategory")]
    [HttpPost]
    public async Task<ActionResult> TorrentsSetCategoryPost([FromForm] QBTorrentsSetCategoryRequest request)
    {
        return await TorrentsSetCategory(request);
    }

    [Authorize]
    [Route("torrents/categories")]
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult<IDictionary<String, TorrentCategory>>> TorrentsCategories()
    {
        var categories = await _qBittorrent.TorrentsCategories();

        return Ok(categories);
    }

    [Authorize]
    [Route("torrents/createCategory")]
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult> TorrentsCreateCategory([FromForm] QBTorrentsCreateCategoryRequest request)
    {
        if (String.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest("category name is empty");
        }

        await _qBittorrent.CategoryCreate(request.Category.Trim());

        return Ok();
    }
        
    [Authorize]
    [Route("torrents/removeCategories")]
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult> TorrentsRemoveCategories([FromForm] QBTorrentsRemoveCategoryRequest request)
    {
        if (String.IsNullOrWhiteSpace(request.Categories))
        {
            return Ok();
        }

        var categories = request.Categories.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var category in categories)
        {
            await _qBittorrent.CategoryRemove(category.Trim());
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/setForcestart")]
    [HttpGet]
    [HttpPost]
    public ActionResult TorrentsSetForceStart()
    {
        return Ok();
    }

    [Authorize]
    [Route("torrents/topPrio")]
    [HttpGet]
    public async Task<ActionResult> TorrentsTopPrio([FromQuery] QBTorrentsHashesRequest request)
    {
        var hashes = request.Hashes.Split("|");

        foreach (var hash in hashes)
        {
            await _qBittorrent.TorrentsTopPrio(hash);
        }

        return Ok();
    }

    [Authorize]
    [Route("torrents/topPrio")]
    [HttpPost]
    public async Task<ActionResult> TorrentsTopPrioPost([FromForm] QBTorrentsHashesRequest request)
    {
        return await TorrentsTopPrio(request);
    }

    [Authorize]
    [Route("sync/maindata")]
    [HttpGet]
    public async Task<ActionResult> SyncMainData()
    {
        var result = await _qBittorrent.SyncMainData();

        return Ok(result);
    }

    [Authorize]
    [Route("sync/maindata")]
    [HttpPost]
    public async Task<ActionResult> SyncMainDataPost()
    {
        return await SyncMainData();
    }
}

public class QBAuthLoginRequest
{
    public String UserName { get; set; }
    public String Password { get; set; }
}

public class QBTorrentsInfoRequest
{
    public String Category { get; set; }
}
    
public class QBTorrentsHashRequest
{
    public String Hash { get; set; }
    public String Category { get; set; }
}

public class QBTorrentsDeleteRequest
{
    public String Hashes { get; set; }
    public Boolean DeleteFiles { get; set; }
}

public class QBTorrentsAddRequest
{
    public String Urls { get; set; }
    public String Category { get; set; }
    public Int32? Priority { get; set; }
}

public class QBTorrentsSetCategoryRequest
{
    public String Hashes { get; set; }
    public String Category { get; set; }
}
    
public class QBTorrentsCreateCategoryRequest
{
    public String Category { get; set; }
}

public class QBTorrentsRemoveCategoryRequest
{
    public String Categories { get; set; }
}

public class QBTorrentsHashesRequest
{
    public String Hashes { get; set; }
}