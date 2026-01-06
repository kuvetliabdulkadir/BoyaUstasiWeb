using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using boya_usta_web.Data;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli iletişim mesajlarını ve mesaj detaylarını yöneten kontrolcü.
    [Route("usta-panel-2024/mesajlar")]
    public class AdminMessagesController : AdminBaseController
    {
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AdminMessagesController> _logger;

        public AdminMessagesController(ApplicationDbContext context, IImageService imageService, IWebHostEnvironment environment, ILogger<AdminMessagesController> logger, ISettingsService settingsService)
            : base(context, settingsService)
        {
            _imageService = imageService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(bool? unreadOnly = null)
        {
            var query = _context.ContactMessages.AsQueryable();

            if (unreadOnly == true)
            {
                query = query.Where(m => !m.IsRead);
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.UnreadOnly = unreadOnly;
            return View(messages);
        }

        [HttpGet("detay/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            // Okundu olarak işaretle
            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        [HttpPost("okundu/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("sil/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mesaj başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Güvenli resim erişim endpoint'i - Sadece admin erişebilir
        [HttpGet("resim/{messageId}/{fileName}")]
        public async Task<IActionResult> GetMessageImage(int messageId, string fileName)
        {
            try
            {
                _logger.LogInformation("Resim isteği alındı: MessageId={MessageId}, FileName={FileName}", messageId, fileName);

                // Mesajı bul
                var message = await _context.ContactMessages.FindAsync(messageId);
                if (message == null)
                {
                    _logger.LogWarning("Mesaj bulunamadı: MessageId={MessageId}", messageId);
                    return NotFound("Mesaj bulunamadı.");
                }

                // Dosya adını güvenli hale getir (path traversal saldırılarını önle)
                var originalFileName = fileName;
                fileName = Path.GetFileName(fileName);
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.LogWarning("Geçersiz dosya adı: OriginalFileName={OriginalFileName}", originalFileName);
                    return BadRequest("Geçersiz dosya adı.");
                }

                // Mesajın bu dosyaya sahip olduğunu kontrol et (büyük/küçük harf duyarsız)
                var hasFile = false;
                string? matchedFileName = null;
                
                // AttachmentPaths'den kontrol et
                if (!string.IsNullOrEmpty(message.AttachmentPaths))
                {
                    try
                    {
                        var fileNames = System.Text.Json.JsonSerializer.Deserialize<List<string>>(message.AttachmentPaths);
                        if (fileNames != null)
                        {
                            // Büyük/küçük harf duyarsız eşleştirme
                            matchedFileName = fileNames.FirstOrDefault(fn => 
                                string.Equals(fn, fileName, StringComparison.OrdinalIgnoreCase));
                            
                            if (matchedFileName != null)
                            {
                                hasFile = true;
                                _logger.LogInformation("Dosya eşleşmesi bulundu: Requested={FileName}, Matched={MatchedFileName}", fileName, matchedFileName);
                            }
                            else
                            {
                                // WebP uzantısı alternatif kontrolü
                                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                                var requestedExt = Path.GetExtension(fileName).ToLowerInvariant();
                                
                                // Eğer istenen dosya .jpg veya .png ise, .webp versiyonunu da kontrol et
                                if (requestedExt == ".jpg" || requestedExt == ".jpeg" || requestedExt == ".png")
                                {
                                    var webPFileName = fileNameWithoutExt + ".webp";
                                    matchedFileName = fileNames.FirstOrDefault(fn => 
                                        string.Equals(Path.GetFileNameWithoutExtension(fn) + ".webp", webPFileName, StringComparison.OrdinalIgnoreCase));
                                    
                                    if (matchedFileName != null)
                                    {
                                        hasFile = true;
                                        _logger.LogInformation("WebP alternatifi bulundu: Requested={FileName}, Matched={MatchedFileName}", fileName, matchedFileName);
                                    }
                                }
                                // Eğer istenen dosya .webp ise, orijinal uzantıyı da kontrol et
                                else if (requestedExt == ".webp")
                                {
                                    matchedFileName = fileNames.FirstOrDefault(fn => 
                                        string.Equals(Path.GetFileNameWithoutExtension(fn), fileNameWithoutExt, StringComparison.OrdinalIgnoreCase));
                                    
                                    if (matchedFileName != null)
                                    {
                                        hasFile = true;
                                        _logger.LogInformation("Orijinal uzantı alternatifi bulundu: Requested={FileName}, Matched={MatchedFileName}", fileName, matchedFileName);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "JSON parse hatası: MessageId={MessageId}, AttachmentPaths={AttachmentPaths}", messageId, message.AttachmentPaths);
                    }
                }

                if (!hasFile || matchedFileName == null)
                {
                    _logger.LogWarning("Mesaja ait resim bulunamadı: MessageId={MessageId}, RequestedFileName={FileName}, AttachmentPaths={AttachmentPaths}", 
                        messageId, fileName, message.AttachmentPaths);
                    return NotFound("Bu mesaja ait resim bulunamadı.");
                }

                // Private klasörden resmi oku (eşleşen dosya adını kullan)
                var filePath = _imageService.GetSecureImagePath(matchedFileName, "messages");
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogWarning("Resim path'i bulunamadı: MessageId={MessageId}, FileName={FileName}, MatchedFileName={MatchedFileName}", 
                        messageId, fileName, matchedFileName);
                    return NotFound($"Resim path'i bulunamadı. Dosya adı: {matchedFileName}");
                }
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Resim dosyası fiziksel olarak bulunamadı: MessageId={MessageId}, FilePath={FilePath}, MatchedFileName={MatchedFileName}", 
                        messageId, filePath, matchedFileName);
                    return NotFound($"Resim dosyası bulunamadı. Path: {filePath}, Dosya adı: {matchedFileName}");
                }

                _logger.LogInformation("Resim başarıyla bulundu ve gönderiliyor: MessageId={MessageId}, FilePath={FilePath}", messageId, filePath);

                var secureFileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var secureContentType = GetContentType(matchedFileName);
                
                return File(secureFileBytes, secureContentType, matchedFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim yüklenirken hata oluştu: MessageId={MessageId}, FileName={FileName}", messageId, fileName);
                return StatusCode(500, "Resim yüklenirken bir hata oluştu.");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}

