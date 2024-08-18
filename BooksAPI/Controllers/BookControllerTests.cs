using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using BooksAPI.Controllers;
using BooksAPI.Data;
using BooksAPI.Models;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using Moq.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BookControllerTests
{
    private readonly Mock<ApiContext> _mockContext;
    private readonly BookController _controller;

    public BookControllerTests()
    {
        _mockContext = new Mock<ApiContext>();
        _controller = new BookController(_mockContext.Object);
    }

    [Fact]
    public void Create_ShouldReturnBadRequest_WhenBookIsNull()
    {
        // Act
        var result = _controller.Create(null) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
        result.Value.Should().Be("Book cannot be null.");
    }

    [Fact]
    public void Create_ShouldReturnUnauthorized_WhenXAuthHeaderIsMissing()
    {
        // Arrange
        var book = new Books { Id = 0, Title = "New Book", Author = "New Author", PublishedYear = "2024" };

        // Act
        var controller = new BookController(_mockContext.Object);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        var result = controller.Create(book) as UnauthorizedObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Unauthorized: xAuth header is missing or empty.");
    }

    [Fact]
    public void Create_ShouldReturnBadRequest_WhenBookIdIsNotZero()
    {
        // Arrange
        var book = new Books { Id = 1, Title = "New Book", Author = "New Author", PublishedYear = "2024" };

        // Act
        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";
        var result = _controller.Create(book) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
        result.Value.Should().Be("New book's ID should be 0.");
    }

    [Fact]
    public void Create_ShouldReturnOk_WhenBookIsValid()
    {
        // Arrange
        var book = new Books { Id = 0, Title = "New Book", Author = "New Author", PublishedYear = "2024" };

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        var mockSet = new Mock<DbSet<Books>>();
        mockSet.Setup(m => m.Add(It.IsAny<Books>())).Verifiable();
        _mockContext.Setup(m => m.Books).Returns(mockSet.Object);

        // Act
        var result = _controller.Create(book) as JsonResult;

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Value as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var createdBook = okResult.Value as Books;
        createdBook.Should().NotBeNull();
        createdBook.Title.Should().Be("New Book");

        mockSet.Verify(m => m.Add(It.IsAny<Books>()), Times.Once);
        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
    }


    [Fact]
    public void Edit_ShouldReturnUnauthorized_WhenXAuthHeaderIsMissing()
    {
        // Arrange
        var book = new Books { Id = 1, Title = "Updated Title", Author = "Updated Author", PublishedYear = "2024" };

        // Act
        var controller = new BookController(_mockContext.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() 
        };
        var result = controller.Edit(book) as UnauthorizedObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Unauthorized: xAuth header is missing or empty.");
    }

    [Fact]
    public void Edit_ShouldReturnBadRequest_WhenBookIdIsZero()
    {
        // Arrange
        var book = new Books { Id = 0, Title = "Updated Title", Author = "Updated Author", PublishedYear = "2024" };

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        // Act
        var result = _controller.Edit(book) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
        result.Value.Should().Be("Book ID is required for editing.");
    }

    [Fact]
    public void Edit_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var book = new Books { Id = 1, Title = "Updated Title", Author = "Updated Author", PublishedYear = "2024" };

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        _mockContext.Setup(m => m.Books.Find(book.Id)).Returns((Books)null);

        // Act
        var result = _controller.Edit(book) as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(404);
        result.Value.Should().Be("Book not found.");
    }

    [Fact]
    public void Edit_ShouldReturnOk_WhenBookIsUpdated()
    {
        // Arrange
        var book = new Books { Id = 1, Title = "Updated Title", Author = "Updated Author", PublishedYear = "2024" };

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        var existingBook = new Books { Id = 1, Title = "Old Title", Author = "Old Author", PublishedYear = "2020" };
        _mockContext.Setup(m => m.Books.Find(book.Id)).Returns(existingBook);

        // Act
        var result = _controller.Edit(book) as JsonResult;

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Value as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var updatedBook = okResult.Value as Books;
        updatedBook.Should().NotBeNull();
        updatedBook.Title.Should().Be("Updated Title");
        updatedBook.Author.Should().Be("Updated Author");
        updatedBook.PublishedYear.Should().Be("2024");

        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
    }


    

    [Fact]
    public void Get_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        int bookId = 1;

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        _mockContext.Setup(m => m.Books.Find(bookId)).Returns((Books)null);

        // Act
        var result = _controller.Get(bookId) as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(404);
        result.Value.Should().Be("Book not found.");
    }

    [Fact]
    public void Get_ShouldReturnOk_WhenBookExists()
    {
        // Arrange
        int bookId = 1;

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        var existingBook = new Books { Id = bookId, Title = "Sample Title", Author = "Sample Author", PublishedYear = "2024" };
        _mockContext.Setup(m => m.Books.Find(bookId)).Returns(existingBook);

        // Act
        var result = _controller.Get(bookId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);

        var returnedBook = result.Value as Books;
        returnedBook.Should().NotBeNull();
        returnedBook.Id.Should().Be(bookId);
        returnedBook.Title.Should().Be("Sample Title");
        returnedBook.Author.Should().Be("Sample Author");
        returnedBook.PublishedYear.Should().Be("2024");
    }

    [Fact]
    public void Get_ShouldReturnUnauthorized_WhenXAuthHeaderIsMissing()
    {
        // Arrange
        int bookId = 1;
        var controller = new BookController(_mockContext.Object);
       
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext(); 

        // Act
       var result = controller.Get(bookId) as UnauthorizedObjectResult;
        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Unauthorized: xAuth header is missing or empty.");
    }
    [Fact]
    public void Delete_ShouldReturnUnauthorized_WhenXAuthHeaderIsMissing()
    {
        // Arrange
        int bookId = 1;

        // Act
        var controller = new BookController(_mockContext.Object);

        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        var result = controller.Delete(bookId) as UnauthorizedObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Unauthorized: xAuth header is missing or empty.");
    }

    [Fact]
    public void Delete_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        int bookId = 1;

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        _mockContext.Setup(m => m.Books.Find(bookId)).Returns((Books)null);

        // Act
        var result = _controller.Delete(bookId) as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(404);
        result.Value.Should().Be("Book not found.");
    }

    [Fact]
    public void Delete_ShouldReturnNoContent_WhenBookIsDeleted()
    {
        // Arrange
        int bookId = 1;

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        var bookToDelete = new Books { Id = bookId, Title = "Book to delete", Author = "Author", PublishedYear = "2024" };
        var booksList = new List<Books> { bookToDelete };
        var dbSetMock = new Mock<DbSet<Books>>();

        dbSetMock.As<IQueryable<Books>>().Setup(m => m.Provider).Returns(booksList.AsQueryable().Provider);
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.Expression).Returns(booksList.AsQueryable().Expression);
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.ElementType).Returns(booksList.AsQueryable().ElementType);
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.GetEnumerator()).Returns(booksList.AsQueryable().GetEnumerator());

        _mockContext.Setup(m => m.Books.Find(bookId)).Returns(bookToDelete);

        // Act
        var result = _controller.Delete(bookId) as StatusCodeResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(204); 

        _mockContext.Verify(m => m.Books.Remove(It.Is<Books>(b => b.Id == bookId)), Times.Once);
        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
    }


    [Fact]
    public void GetAll_ShouldReturnUnauthorized_WhenXAuthHeaderIsMissing()
    {
        // Arrange
        var controller = new BookController(_mockContext.Object);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext(); 

        // Act
        var result = controller.GetAll() as UnauthorizedObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Unauthorized: xAuth header is missing or empty.");
    }


    [Fact]
    public void GetAll_ShouldReturnOk_WithListOfBooks_WhenXAuthHeaderIsValid()
    {
        // Arrange
        var booksList = new List<Books>
    {
        new Books { Id = 1, Title = "Book 1", Author = "Author 1", PublishedYear = "2023" },
        new Books { Id = 2, Title = "Book 2", Author = "Author 2", PublishedYear = "2024" }
    };

        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["xAuth"] = "valid-auth-token";

        var dbSetMock = new Mock<DbSet<Books>>();
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.Provider).Returns(booksList.AsQueryable().Provider);
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.Expression).Returns(booksList.AsQueryable().Expression);
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.ElementType).Returns(booksList.AsQueryable().ElementType);
        dbSetMock.As<IQueryable<Books>>().Setup(m => m.GetEnumerator()).Returns(booksList.AsQueryable().GetEnumerator());

        _mockContext.Setup(m => m.Books).Returns(dbSetMock.Object);

        // Act
        var result = _controller.GetAll() as JsonResult;

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Value as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedBooks = okResult.Value as List<Books>;
        returnedBooks.Should().NotBeNull();
        returnedBooks.Count.Should().Be(2);
        returnedBooks[0].Title.Should().Be("Book 1");
        returnedBooks[1].Title.Should().Be("Book 2");
    }

}
