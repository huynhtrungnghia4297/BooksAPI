using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BooksAPI.Models;
using BooksAPI.Data;

namespace BooksAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly ApiContext _context;

        public BookController(ApiContext context)
        {
            _context = context; 
        }
        //Create
        [HttpPost]
        public IActionResult Create (Books books)
        {

            if (books == null)
            {
                return BadRequest("Book cannot be null.");
            }

            if (!Request.Headers.TryGetValue("xAuth", out var xAuth) || string.IsNullOrEmpty(xAuth))
            {
                return Unauthorized("Unauthorized: xAuth header is missing or empty.");
            }

            if (books.Id != 0)
            {
                return BadRequest("New book's ID should be 0.");
            }

            _context.Books.Add(books);
            _context.SaveChanges();

            return new JsonResult(Ok(books));

        }
        //Edit
        [HttpPost]
        public IActionResult Edit(Books books)
        {
            if (!Request.Headers.TryGetValue("xAuth", out var xAuth) || string.IsNullOrEmpty(xAuth))
            {
                return Unauthorized("Unauthorized: xAuth header is missing or empty.");
            }

            if (books.Id == 0)
            {
                return BadRequest("Book ID is required for editing.");
            }

            var booksInDb = _context.Books.Find(books.Id);

            if (booksInDb == null)
            {
                return NotFound("Book not found.");
            }

            booksInDb.Title = books.Title;
            booksInDb.Author = books.Author;
            booksInDb.PublishedYear = books.PublishedYear;

            _context.SaveChanges();

            return new JsonResult(Ok(booksInDb));
        }
        //Get
        [HttpGet]
        public IActionResult Get(int id)
        {
            if (!Request.Headers.TryGetValue("xAuth", out var xAuth) || string.IsNullOrEmpty(xAuth))
            {
                return Unauthorized("Unauthorized: xAuth header is missing or empty.");
            }

            var result = _context.Books.Find(id);

            if (result == null)
            {
                return NotFound("Book not found.");
            }

            return Ok(result);
        }

        //Delete
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (!Request.Headers.TryGetValue("xAuth", out var xAuth) || string.IsNullOrEmpty(xAuth))
            {
                return Unauthorized("Unauthorized: xAuth header is missing or empty.");
            }

            var result = _context.Books.Find(id);

            if (result == null)
            {
                return NotFound("Book not found.");
            }

            _context.Books.Remove(result);
            _context.SaveChanges();

            return NoContent();
        }

        //Get All   
        [HttpGet("/GetAll")]
        public IActionResult GetAll()
        {
            if (!Request.Headers.TryGetValue("xAuth", out var xAuth) || string.IsNullOrEmpty(xAuth))
            {
                return Unauthorized("Unauthorized: xAuth header is missing or empty.");
            }

            var results = _context.Books.ToList();
            return new JsonResult(Ok(results));
        }
    }
}
        