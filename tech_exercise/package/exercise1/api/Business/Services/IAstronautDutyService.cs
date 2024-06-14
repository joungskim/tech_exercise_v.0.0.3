using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public interface IAstronautDutyService
    {
        Task<AstronautDuty?> GetLatestAstronautDutyAsync(int personId);
        Task UpdatePreviousDutyEndDateAsync(int personId, DateTime dutyStartDate);
        Task CreateOrUpdateAstronautDetailAsync(Person person, string dutyTitle, string rank, DateTime dutyStartDate);
        Task HandleRetiredDutyAsync(Person person, DateTime dutyStartDate, string rank);
    }
}
