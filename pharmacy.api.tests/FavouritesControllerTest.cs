﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Moq;
using Pharmacy.Controllers;
using Pharmacy.Models;
using Pharmacy.Models.Pocos;
using Pharmacy.Services.Interfaces;
using Xunit;

namespace Pharmacy.ControllerTests
{
    public class FavouritesControllerTest
    {


        [Fact]
        public void GetFavourites_test()
        {
            var customerID = Guid.NewGuid();
            var customer = new CustomerPoco()
            {
                CustomerId = customerID
            };

            // Arrange
            var mockFavouriteskService = new Mock<IFavouritesService>();
            mockFavouriteskService.Setup(x => x.GetFavouriteDrugs(customerID))
                .Returns(new List<DrugPoco> {
                    new DrugPoco() { DrugName = "drug1"},
                    new DrugPoco() { DrugName = "drug2"}
            });

            var mockCustomersService = new Mock<ICustomersService>();
            mockCustomersService.Setup(x => x.GetCustomer(customerID))
                .Returns(customer);

            // Arrange
            var controller = new FavouritesController(mockFavouriteskService.Object, mockCustomersService.Object);

            // Act
            var favourites = controller.Get();

            // Assert
            Assert.NotNull(favourites);
            Assert.Equal(2, favourites.Count());
        }

        [Fact]
        public void AddFavourite_Test()
        {
            Favourite favourite;
        }

        [Fact]
        public void DeleteFavourite_Test()
        {
            Guid id;
        }
    }
}
