﻿using IRechargedAPI.Domian.Entities;
using IRechargedAPI.Domian.Interface;
using IRechargedAPI.Infrastruture.ExternalServices.Models;
using IRechargedAPI.Presentation.DTO;
using System.Net.Http.Headers;

namespace IRechargedAPI.Infrastruture.BLL
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IUserRepository _userRepository;
        private readonly AirtimeService _airtimeService; // Changed from IDigitalVendors
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(
            IUserRepository userRepository,
            AirtimeService airtimeService, // Changed injection
            ILogger<PurchaseService> logger)
        {
            _userRepository = userRepository;
            _airtimeService = airtimeService;
            _logger = logger;
        }

        public async Task<ResponseModel> PurchaseAirtimeService(PurchaseAirtimeRequestDTO purchaseAirtimeRequestDTO, string username)
        {
            try
            {
                _logger.LogInformation($"Starting airtime purchase for user: {username}");

                // Validate request
                if (purchaseAirtimeRequestDTO == null)
                {
                    _logger.LogWarning("Null request received");
                    return new ResponseModel { Message = "Invalid Request", IsSuccess = false };
                }

                // Validate user
                var user = _userRepository.GetByUserName(username);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {username}");
                    return new ResponseModel { IsSuccess = false, Message = "User not found" };
                }

                // Validate balance
                if (purchaseAirtimeRequestDTO.Amount > user.WalletBalance)
                {
                    _logger.LogWarning($"Insufficient balance for user: {username}");
                    return new ResponseModel { IsSuccess = false, Message = "Insufficient balance" };
                }

                // Prepare vendor request
                var vendRequest = new VendAirtimeRequestModel
                {
                    amount = purchaseAirtimeRequestDTO.Amount,
                    number = purchaseAirtimeRequestDTO.PhoneNumber,
                    network = purchaseAirtimeRequestDTO.NetworkType
                };

                // Make purchase through AirtimeService
                var vendorResponse = await _airtimeService.PurchaseAirtime(vendRequest);

                if (!vendorResponse.isSuccessful)
                {
                    _logger.LogError($"Airtime purchase failed: {vendorResponse.responsemessage}");
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = vendorResponse.responsemessage
                    };
                }

                // Deduct balance only after successful purchase
                user.WalletBalance -= purchaseAirtimeRequestDTO.Amount;
                _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"Airtime purchase successful. Transaction ID: {vendorResponse.tran_Id}");

                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Airtime purchased successfully",
                    Data = new
                    {
                        TransactionId = vendorResponse.tran_Id,
                        purchaseAirtimeRequestDTO.Amount
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing airtime purchase for user: {username}");
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "An error occurred while processing your request"
                };
            }
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var wallet = await _userRepository.GetWalletAsync(userId);
            return wallet?.Balance ?? 0.00m;
        }

        public async Task<Wallet> TopUpWalletAsync(Guid userId, decimal amount)
        {
            var wallet = await _userRepository.GetWalletAsync(userId);
            if (wallet == null)
            {
                wallet = await _userRepository.CreateWalletAsync(userId);
            }
            wallet.Balance += amount;
            _userRepository.UpdateWalletAsync(wallet); // Corrected method to update Wallet instead of User  
            return wallet;
        }


    }
}