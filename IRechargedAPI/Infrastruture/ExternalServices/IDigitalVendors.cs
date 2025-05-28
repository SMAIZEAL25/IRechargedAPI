using IRechargedAPI.Infrastruture.ExternalServices.Models;

namespace IRechargedAPI.Infrastruture.ExternalServices
{
    public interface IDigitalVendors
    {
        Task<DigitalVendorsReponseModel> VendAirtime(VendAirtimeRequestModel vendAirtimeRequestModel, string token);
    }
}
