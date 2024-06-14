using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
//TODO: Update this code if I have time
namespace StargateAPI.Business.Commands
{
    public class UpdateAstronautDuty: IRequest<UpdateAstronautDutyResult>
    {
    }

    public class UpdateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
