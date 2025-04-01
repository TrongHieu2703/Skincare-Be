using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;



public class FileUploadRequest
{
     [FromForm] 
    public IFormFile? File { get; set; }
}
