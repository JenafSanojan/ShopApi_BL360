using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;

namespace ShopApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController(ApplicationDbContext dbContext) : Controller
    {
        private readonly ApplicationDbContext _context = dbContext;


        // GET, POST, PUT, DELETE
        [HttpGet]
        public IActionResult Index()
        {
            var messageObject = new { message = "API end points are working" };
            return Ok(messageObject);
        }


        // GET: Product/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.ProductsForApi.ToListAsync());
        }


        // GET: Product/GetOne/:id
        [HttpGet("GetOne")]
        public async Task<IActionResult> GetOne(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.ProductsForApi
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        // GET: ProductsApi/Ui   does not send an api but a html file
        [HttpGet("Ui")]
        public IActionResult Ui()
        {
            return View();
        }


        // POST: Product/AddNewProduct + <- Product
        [HttpPost("AddNewProduct")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] Product viewModel)
        {
            try
            {
                byte[]? imageData = null;

                using (var memoryStream = new MemoryStream())
                {
                    if (viewModel.ImageFile is not null)
                    {
                        await viewModel.ImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                if (ProductExists(viewModel.ProductId))
                {
                    return BadRequest(new { message = "Product with same ProductID already exists" });
                }

                if (viewModel.ProductId == 0)
                {
                    return BadRequest(new { message = "ProductId is required, Provide an appropriate ProductId" });
                }

                var product = new Product
                {
                    ProductId = viewModel.ProductId,
                    Name = viewModel.Name,
                    SKU = viewModel.SKU,
                    Quantity = viewModel.Quantity,
                    Category = viewModel.Category,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    Image = imageData,
                };

                await _context.ProductsForApi.AddAsync(product);

                await _context.SaveChangesAsync();

                var messageObject = new { message = "Product has been created" };
                return Created("Created", messageObject);
            }
            catch (Exception ex)
            {                
                // Take first 5 lines of error and log (for clarity)
                var exceptionMessageLines = ex.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var exceptionSummary = string.Join(Environment.NewLine, exceptionMessageLines.Take(5));

                // Log.Error("Error in Product/Add:\n" + exceptionSummary + "\n\n", "An error occurred in Product/Add.");

                return StatusCode(500, "Internal Error: \n" + exceptionSummary.ToString());
            }
        }


        // DELETE: Product/DeleteOne/:id
        [HttpDelete("DeleteOne/{id}")]
        public async Task<IActionResult> DeleteOne(int? id)
        {
            try {
                if (id == null)
                {
                    return NotFound();
                }

                var product = await _context.ProductsForApi
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (product == null)
                {
                    var messageObject = new { message = "Product doesn't exist" };
                    return NotFound(messageObject);
                }

                _context.ProductsForApi.Remove(product);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Take first 5 lines of error and log (for clarity)
                var exceptionMessageLines = ex.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var exceptionSummary = string.Join(Environment.NewLine, exceptionMessageLines.Take(5));

                // Log.Error("Error in Product/Delete:\n" + exceptionSummary + "\n\n", "An error occurred in Product/Delete.");

                return StatusCode(500, "Internal Error: \n" + exceptionSummary.ToString());
            }
        }


        // PUT: Product/EditOneByDatabaseId/:id + <- product
        [HttpPut("EditOneByDatabaseId/{id}")]
        public async Task<IActionResult> EditOne(int? id, [FromBody] ProductEditViewModel viewModel)
        {
            try
            {
                if (id == null)
                {
                    var messageObject = new { message = "Please provide an id" };
                    return NotFound(messageObject);
                }

                var product = await _context.ProductsForApi
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (product == null)
                {
                    var messageObject = new { message = "Product doesn't exist" };
                    return NotFound(messageObject);
                }
                
                if (viewModel.ProductId == product.ProductId || !ProductExists(viewModel.ProductId) || viewModel.ProductId == 0) {

                    product.ProductId = viewModel.ProductId != 0 ? viewModel.ProductId : product.ProductId;
                    product.Description = viewModel.Description is not null ? viewModel.Description : product.Description;
                    product.SKU = viewModel.SKU is not null ? viewModel.SKU : product.SKU;
                    product.Category = viewModel.Category is not null ? viewModel.Category : product.Category;
                    product.Quantity = viewModel.Quantity != 0 ? viewModel.Quantity : product.Quantity;
                    product.Price = viewModel.Price != 0 ? viewModel.Price : product.Price;
                    product.Name = viewModel.Name is not null ? viewModel.Name : product.Name;

// *****                    // image

                    await dbContext.SaveChangesAsync();

                    var messageObject = new { message = "Product has been edited" };
                    return Ok(messageObject);
                }
                else
                {
                    return BadRequest(new { message = "Provide an appropriate ProductId, " +
                        "another product exists in this ProductID" });
                }
            }
            catch (Exception ex)
            {
                // Take first 5 lines of error and log (for clarity)
                var exceptionMessageLines = ex.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var exceptionSummary = string.Join(Environment.NewLine, exceptionMessageLines.Take(5));

                // Log.Error("Error in Product/Delete:\n" + exceptionSummary + "\n\n", "An error occurred in Product/Delete.");

                return StatusCode(500, "Internal Error: \n" + exceptionSummary.ToString());
            }
        }


        // PUT: Product/EditOneByProductId/:id + <- product
        [HttpPut("EditOneByProductId/{productId}")]
        public async Task<IActionResult> EditOneByProductId(int? productId, [FromBody] ProductEditViewModel viewModel)
        {
            try
            {
                if (productId == null)
                {
                    return NotFound(new { message = "Please provide a product id" });
                }

                var product = await _context.ProductsForApi
                    .FirstOrDefaultAsync(m => m.ProductId == productId);

                if (product == null)
                {
                    return NotFound(new { message = "Product doesn't exist" });
                }

                product.ProductId = viewModel.ProductId != 0 ? viewModel.ProductId : product.ProductId;
                product.Description = viewModel.Description is not null ? viewModel.Description : product.Description;
                product.SKU = viewModel.SKU is not null ? viewModel.SKU : product.SKU;
                product.Category = viewModel.Category is not null ? viewModel.Category : product.Category;
                product.Quantity = viewModel.Quantity != 0 ? viewModel.Quantity : product.Quantity;
                product.Price = viewModel.Price != 0 ? viewModel.Price : product.Price;
                product.Name = viewModel.Name is not null ? viewModel.Name : product.Name;

// *****                    // image

                await dbContext.SaveChangesAsync();

                var messageObject = new { message = "Product has been edited" };
                return Ok(messageObject);
            }
            catch (Exception ex)
            {
                // Take first 5 lines of error and log (for clarity)
                var exceptionMessageLines = ex.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var exceptionSummary = string.Join(Environment.NewLine, exceptionMessageLines.Take(5));

                // Log.Error("Error in Product/Delete:\n" + exceptionSummary + "\n\n", "An error occurred in Product/Delete.");

                return StatusCode(500, "Internal Error: \n" + exceptionSummary.ToString());
            }
        }


        private bool ProductExists(long id)
        {
            return _context.ProductsForApi.Any(e => e.ProductId == id);
        }
    }
}
