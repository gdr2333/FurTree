using Back.Types.DataBase;
using Back.Types.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Results;
using SkiaSharp;

namespace Back.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class CapchaController(IDbContextFactory<MainDataBase> dbContextFactory, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private ILogger<CapchaController> _logger = loggerFactory.CreateLogger<CapchaController>();
    [HttpGet]
    public IActionResult GetCapcha()
    {
        _logger.LogInformation("生成验证码流程开始");
        // Step 0: Initialize text
        string capchaCharList = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string capchaChars = new(Enumerable.Range(1, 6).Select(i => capchaCharList[Random.Shared.Next(capchaCharList.Length)]).ToArray());
        _logger.LogInformation($"新验证码：{capchaChars}");

        // Step 1: Initialize renderer
        using var typeface = SKTypeface.FromFile(Environment.CurrentDirectory + "/Fonts/IntelOneMono-Regular.otf");
        using var font = new SKFont(typeface);
        font.Size = 40f;
        using var paint = new SKPaint(font);
        paint.IsAntialias = true;
        paint.Typeface = typeface;
        paint.TextAlign = SKTextAlign.Center;

        // Step 2: Create bitmap
        SKFontMetrics fontMetrics = font.Metrics;
        float height = fontMetrics.Descent - fontMetrics.Ascent;
        using var bitmap = new SKBitmap((int)(font.MeasureText(capchaChars) + 20), (int)(height * 1.5f + 20), SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.DrawColor(SKColors.White);

        // Step 3: Draw text
        paint.Color = SKColors.Black;
        float x = bitmap.Width / 2;
        float y = (bitmap.Height - (fontMetrics.Descent - fontMetrics.Ascent)) / 2 - fontMetrics.Ascent;
        canvas.DrawText(capchaChars, x, y, SKTextAlign.Center, font, paint);

        // Step 4: Draw lines
        for (int i = 1; i <= 300; i++)
        {
            paint.Color = new SKColor((byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), 70);
            canvas.DrawLine(0f, Random.Shared.Next(bitmap.Height), bitmap.Width, Random.Shared.Next(bitmap.Height), paint);
        }

        // Step 5: Save
        using var img = SKImage.FromBitmap(bitmap);
        using var png = img.Encode(SKEncodedImageFormat.Png, 100);
        byte[] data = png.ToArray();
        var capcha = new Capcha(Guid.NewGuid().ToString(), capchaChars);

        // Save to MainDataBase
        using (var dbContext = dbContextFactory.CreateDbContext())
        {
            dbContext.Capchas.Add(capcha);
            dbContext.SaveChanges();
        }

        // Build return data
        var result = new GetCapchaResult
        {
            Id = capcha.Guid,
            Image = Convert.ToBase64String(data)
        };

        _logger.LogInformation($"新验证码Id：{capcha.Guid}");
        _logger.LogInformation("验证码生成完成");
        return Ok(result);
    }
}
