﻿using System;
using Microsoft.AspNetCore.Authorization;
using Pharmacy.Models.Pocos;
using Pharmacy.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Pharmacy.Controllers
{
    /// <summary>  
    /// Customer functions of McKenzies Pharmacy API
    /// </summary>  
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomersService _service;


        /// <summary>  
        /// Constructor for Customer functions of McKenzies Pharmacy API
        /// </summary>  
        public CustomersController(ICustomersService service)
        {
            _service = service;
        }

        /// <summary>  
        /// Get a Customer
        /// </summary>  
        /// <param name="userid"></param>
        /// <returns code="200">Customer</returns>  
        // GET: api/Customers/5
        [Route("api/Customers")]
        public IActionResult GetCustomer(string userid)
        {
            CustomerPoco customer = _service.GetCustomerByUsername(userid);
            if (customer == null)
            {
                return BadRequest("Customer has not registered yet");
            }
            return Ok(customer);
        }

        /// <summary>  
        /// Update a Customer
        /// </summary>  
        /// <param name="id"></param>
        /// <param name="customer"></param>
        /// <returns code="200">Customer</returns>  
        // PUT: api/Customers/5
        [Route("api/Customers/{id}")]
        public IActionResult PutCustomer(Guid id, CustomerPoco customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customer.CustomerId)
            {
                return BadRequest();
            }
            
            try
            {
                _service.UpdateCustomerDetails(customer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(customer);
        }

        /// <summary>  
        /// Add a new Customer
        /// </summary>  
        /// <param name="customer"></param>
        /// <returns code="200">Customer</returns>  
        // POST: api/Customers
        [Route("api/Customers")]
        public IActionResult PostCustomer(CustomerPoco customer)
        {
            try
            {
                customer = _service.RegisterCustomer(customer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(customer);
        }
    }
}