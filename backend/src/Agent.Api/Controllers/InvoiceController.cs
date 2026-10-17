using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Agent.Core.Interfaces;
using Agent.Core.Entities;

namespace Agent.Api.Controllers
{
    public class UploadDto
    {
        public required IFormFile File { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IInvoiceRepository _repository;

        public InvoiceController(IMessagePublisher messagePublisher, IInvoiceRepository repository)
        {
            _messagePublisher = messagePublisher;
            _repository = repository;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadInvoice([FromForm] UploadDto request)
        {
            var rawInvoice = new RawInvoice 
            { 
                Id = Guid.NewGuid(),
                FileName = request.File.FileName,
                Status = "Processing",
                Color = "None",
                Explanation = "En cola de análisis"
            };
            
            await _repository.SaveRawAsync(rawInvoice);
            await _messagePublisher.PublishAsync("invoice.received", new { InvoiceId = rawInvoice.Id });

            return Accepted(new { Message = "Factura encolada para análisis", Id = rawInvoice.Id });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var invoices = await _repository.GetAllAsync();
            return Ok(invoices);
        }
    }
}