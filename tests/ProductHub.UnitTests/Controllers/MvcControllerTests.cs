using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProductHub.Web.Controllers.MVC;

namespace ProductHub.UnitTests.Controllers;

public class MvcControllerTests
{
    [Fact]
    public void HomeController_Index_RedirectsToProductsPage()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Index();

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("ProductsPage");
    }

    [Fact]
    public void AccountController_Login_ReturnsView()
    {
        // Arrange
        var controller = new AccountController();

        // Act
        var result = controller.Login();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void AccountController_Register_ReturnsView()
    {
        // Arrange
        var controller = new AccountController();

        // Act
        var result = controller.Register();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void ProductsPageController_Index_ReturnsView()
    {
        // Arrange
        var controller = new ProductsPageController();

        // Act
        var result = controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }
}
