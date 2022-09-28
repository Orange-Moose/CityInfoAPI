using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpGet("fileId")]
        public ActionResult GetFile(string FileId)
        {
            //Lookup the file by fileId
            //demo code
            var pathToFile = ".NET cheatsheet.docx";

            //Check if the file exists
            if(!System.IO.File.Exists(pathToFile)) { return NotFound(); };

            // Read file contents
            var bytes = System.IO.File.ReadAllBytes(pathToFile);

            //Return file 
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Path.GetFileName(pathToFile));
        }
    }
}
