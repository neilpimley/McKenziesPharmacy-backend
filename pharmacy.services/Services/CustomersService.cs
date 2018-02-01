﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using NLog;
using Pharmacy.Models.Pocos;
using Pharmacy.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Models;

namespace Pharmacy.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IMapper _mapper;

        public CustomersService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<CustomerPoco> GetCustomerByUsername(string username)
        {
            logger.Info("GetCustomerByUsername - {0}", username);
            var _customers = await _unitOfWork.CustomerRepository.Get(c => c.UserId == username);
            var _customer = _customers.FirstOrDefault();
            if (_customer == null)
            {
                logger.Error("GetCustomerByUsername - User doesn't exist");
                return null;
            }
            var customer = _mapper.Map<CustomerPoco>(_customer);
            customer.Title = await _unitOfWork.TitleRepository.GetByID(_customer.TitleId);
            customer.Address = await _unitOfWork.AddressRepository.GetByID(_customer.AddressId);
            customer.Shop = await _unitOfWork.ShopRepository.GetByID(_customer.ShopId);
            customer.Doctor = await _unitOfWork.DoctorRepository.GetByID(_customer.DoctorId);
            return customer;
        }

        public async Task<CustomerPoco> GetCustomer(Guid id) {
            logger.Info("GetCustomer - {0}", id);
            var _customer = await _unitOfWork.CustomerRepository.GetByID(id);
            if (_customer == null)
            {
                logger.Error("GetCustomerByUsername - User doesn't exist");
                return null;
            }
            var customer = _mapper.Map<CustomerPoco>(_customer);
            customer.Title = await _unitOfWork.TitleRepository.GetByID(_customer.TitleId);
            customer.Address = await _unitOfWork.AddressRepository.GetByID(_customer.AddressId);
            customer.Shop = await _unitOfWork.ShopRepository.GetByID(_customer.ShopId);
            customer.Doctor = await _unitOfWork.DoctorRepository.GetByID(_customer.DoctorId);
            return customer;
        }

        public async Task<CustomerPoco> RegisterCustomer(CustomerPoco customer)
        {
            logger.Info("RegisterCustomer - {0}", customer.Fullname);

            IList<string> registerErrors = await ValidateCustomer(customer);
            if (registerErrors.Count > 0)
            {
                throw new Exception(registerErrors[0]);
            }

            customer.CustomerId = Guid.NewGuid();
            customer.CreatedOn = DateTime.Now;
            customer.AddressId = Guid.NewGuid();
            customer.Address.AddressId = customer.AddressId;
            customer.Address.CreatedOn = DateTime.Now;

            var _customer = _mapper.Map<Customer>(customer);
            _unitOfWork.CustomerRepository.Insert(_customer);
            _unitOfWork.AddressRepository.Insert(customer.Address);
            try
            {
                await _unitOfWork.SaveAsync();
                customer.Title = await _unitOfWork.TitleRepository.GetByID(_customer.TitleId);
                customer.Shop = await _unitOfWork.ShopRepository.GetByID(_customer.ShopId);
                customer.Doctor = await _unitOfWork.DoctorRepository.GetByID(_customer.DoctorId);
            }
            catch (Exception ex)
            {
                logger.Error("RegisterCustomer - {0}", ex.Message);
                throw new Exception(ex.Message);
            }
            return customer;
        }

        public async Task UpdateCustomerDetails(CustomerPoco customer) {
            logger.Info("UpdateCustomerDetails - {0}", customer.CustomerId);
            var _customer = _mapper.Map<Customer>(customer);
            _unitOfWork.CustomerRepository.Update(_customer);
            _unitOfWork.AddressRepository.Update(customer.Address);
            try
            {
                await _unitOfWork.SaveAsync();
                await _emailService.SendPersonalDetailsAmended(customer);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateCustomerDetails - {0}", ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task ActivateCustomer(Guid id, string mobileVerificationCode)
        {
            logger.Info("ActiveateCustomer - {0}", id);
            var customers = await _unitOfWork.CustomerRepository.Get(x => x.CustomerId == id);
            var customer = customers.FirstOrDefault();
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            //TODO: check mobileVerificationCode against value to be stored in DB

            customer.Active = true;
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                logger.Error("ActivateCustomer - {0}", ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task<List<string>> ValidateCustomer(CustomerPoco customer)
        {
            var errors = new List<string>();
            var existingEmail = await EmailExists(customer.Email);
            if (existingEmail)
            {
                logger.Info("Customer with email {0} has already been registered", customer.Email);
                errors.Add("Email address has already been registered");
                return errors;
            }

            var existingCustomer = await CustomerExists(customer);
            if (existingCustomer)
            {
                logger.Info("Customer ({0} - {1}) with email the same name, dob and doctor has already been registered", 
                    customer.Fullname, customer.Dob);
                errors.Add("Customer has already registered with a different email address");
                return errors;
            }

            var age = GetAge(customer.Dob);
            if (age < 18)
            {
                logger.Info("Customer ({0} - {1}) with email the same name, dob and doctor has already been registered",
                    customer.Fullname, customer.Dob);
                errors.Add("Customer has already registered with a different email address");
            }

            return errors;
        }

        private async Task<bool> EmailExists(string email)
        {
            var customers = await _unitOfWork.CustomerRepository.Get(c => c.Email == email);
            return customers.Any();
        }

        private async Task<bool> CustomerExists(CustomerPoco customer)
        {
            var customers = await _unitOfWork.CustomerRepository
                .Get(c => c.Firstname == customer.Firstname 
                    && c.Lastname == customer.Lastname
                          && c.Dob == customer.Dob
                          && c.DoctorId == customer.DoctorId);
            return customers.Any();
        }

        int GetAge(DateTime bornDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - bornDate.Year;
            if (bornDate > today.AddYears(-age))
                age--;

            return age;
        }
}
    
}