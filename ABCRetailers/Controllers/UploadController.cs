using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add this using directive
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<UploadController> _logger; // Add this field

        public UploadController(IAzureStorageService storageService, ILogger<UploadController> logger)
        {
            _storageService = storageService;
            _logger = logger; // Initialize the logger
        }

        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                    {
                        // Validate file size (10MB max)
                        if (model.ProofOfPayment.Length > 10 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ProofOfPayment", "File size must be less than 10MB");
                            return View(model);
                        }

                        // Validate file type
                        var validTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                        var fileExtension = Path.GetExtension(model.ProofOfPayment.FileName).ToLower();
                        if (!validTypes.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ProofOfPayment", "Please select a valid file type (PDF, JPG, PNG, DOC, DOCX)");
                            return View(model);
                        }

                        var fileName = await _storageService.UploadFileAsync(model.ProofOfPayment, "payment-proofs");
                        await _storageService.UploadToFileShareAsync(model.ProofOfPayment, "contracts", "payments");

                        TempData["Success"] = $"File '{model.ProofOfPayment.FileName}' uploaded successfully!";
                        return View(new FileUploadModel());
                    }
                    else
                    {
                        ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file"); // Now this will work
                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                }
            }

            return View(model);
        }
    }
}