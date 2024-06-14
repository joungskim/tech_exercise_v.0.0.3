using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public interface IAstronautDutyService
    {
        Task<AstronautDuty?> GetLatestAstronautDutyAsync(int personId);
        Task<AstronautDuty?> UpdatePreviousDutyEndDateAsync(int personId, DateTime dutyStartDate);
        Task<AstronautDetail> CreateOrUpdateAstronautDetailAsync(Person person, string dutyTitle, string rank, DateTime dutyStartDate);
        Task<AstronautDuty?> HandleRetiredDutyAsync(Person person, DateTime dutyStartDate, string rank);
    }
}