using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Agent.Core.Interfaces;
using Agent.Core.Entities;
using Agent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agent.Api.Controllers
{
    public class UploadDto
    {
        public IFormFile File { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _repository;
        private readonly IMessagePublisher _publisher;
        private readonly AgentDbContext _context;

        public InvoiceController(IInvoiceRepository repository, IMessagePublisher publisher, AgentDbContext context)
        {
            _repository = repository;
            _publisher = publisher;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _context.RawInvoices.ToListAsync();
            return Ok(invoices);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadInvoice([FromForm] UploadDto request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Archivo ausente o vacío.");

            var invoiceId = Guid.NewGuid();
            string base64File;

            using (var memoryStream = new MemoryStream())
            {
                await request.File.CopyToAsync(memoryStream);
                base64File = Convert.ToBase64String(memoryStream.ToArray());
            }

            var rawInvoice = new RawInvoice
            {
                Id = invoiceId,
                FileName = request.File.FileName,
                Status = "Encolada",
                Color = "Gris",
                Explanation = "Esperando análisis de IA...",
                Amount = 0m,
                IsDuplicate = false,
                IsIllegal = false,
                NeedsReview = false
            };

            await _repository.SaveRawAsync(rawInvoice);

            var payload = new
            {
                InvoiceId = invoiceId,
                FileName = request.File.FileName,
                FileBase64 = base64File
            };

            await _publisher.PublishAsync("invoice.received", payload);

            return Ok(new { message = "Factura encolada para análisis", id = invoiceId });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearInvoices()
        {
            await _context.RawInvoices.ExecuteDeleteAsync();
            return Ok(new { message = "Base de datos limpiada" });
        }
    }
}