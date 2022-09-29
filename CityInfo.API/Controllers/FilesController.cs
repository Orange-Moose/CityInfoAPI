using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new System.ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }
        
        [HttpGet("fileId")]
        public ActionResult GetFile(string FileId)
        {
            //Lookup the file by fileId
            //demo code
            var pathToFile = ".NET cheatsheet.docx";

            //Check if the file exists
            if(!System.IO.File.Exists(pathToFile)) { return NotFound(); };

            if(!_fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // Read file contents
            var bytes = System.IO.File.ReadAllBytes(pathToFile);

            //Return file 
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }
    }
}
