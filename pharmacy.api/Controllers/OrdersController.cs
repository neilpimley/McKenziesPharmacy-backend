﻿using System;
using Pharmacy.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Pharmacy.Models.Pocos;
using Pharmacy.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Pharmacy.Controllers
{
    /// <summary>  
    /// Orders functions of McKenzies Pharmacy API
    /// </summary>  
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrdersService _service;
        private readonly ICustomersService _customersService;

        /// <summary>  
        /// Constructor for Orders functions of McKenzies Pharmacy API
        /// </summary>  
        public OrdersController(IOrdersService service, ICustomersService customersService)
        {
            _service = service;
            _customersService = customersService;
        }


        // GET: api/Orders
        [HttpGet]
        [Route("api/Order")]
        public async Task<IActionResult> GetOrder()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var customer = await _customersService.GetCustomerByUsername(userId);
            if (customer == null)
            {
                return NotFound();
            }
            Order order = await _service.GetCurrentOrder(customer.CustomerId, (int)Status.Inbasket);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // GET: api/Orders
        [HttpGet]
        [Route("api/Order/{id}")]
        public async Task<OrderPoco> GetOrder(Guid id)
        {
            return await _service.GetOrder(id);
        }

        [HttpGet]
        [Route("api/Orders")]
        public async Task<IEnumerable<Order>> GetOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var customer = _customersService.GetCustomerByUsername(userId).Result;
            if (customer == null)
            {
                throw new Exception("Customer doesn't exist");
            }
            return await _service.GetOrders(customer.CustomerId);
        }

        // GET: api/Orders/962ed775-a117-4e93-9d6c-7208bc5d484d
        [HttpGet]
        [Route("api/OrderLines/{id}", Name = "GetOrderLines")]
        public Task<IEnumerable<DrugPoco>> GetOrderLines(Guid id)
        {
            return _service.GetOrderLines(id);
        }

        // PUT: api/Orders/5
        [HttpPut]
        [Route("api/Order/{id}", Name = "SubmitOrder")]
        public async Task<IActionResult> SubmitOrder(Guid id, OrderPoco order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            try
            {
                await _service.SubmitOrder(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        // POST: api/Orders
        [HttpPost]
        [Route("api/OrderLines")]
        public async Task<IActionResult> AddOrderLineOrder([FromBody]OrderLine orderLine)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _service.AddToOrder(orderLine);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(orderLine);
        }

        // DELETE: api/Orders/2F714D7E-1CD6-4E73-98A7-98F875D558F6
        [HttpDelete]
        [Route("api/OrderLines/{id}")]
        public async Task<IActionResult> DeleteOrderLine(Guid id)
        {
            OrderLine orderLine = await _service.GetOrderLine(id);
            if (orderLine == null)
            {
                return NotFound();
            }

            try
            {
                await _service.DeleteFromOrder(orderLine);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(orderLine);
        }

        
    }
}