using AutoMapper;
using Rollem.TaskRunnerService.Models;
using Rollem.TaskRunnerService.Tasks;

namespace Rollem.TaskRunnerService.Bootstrap
{
    internal class AppMapProfile : Profile
    {
        public AppMapProfile()
        {
            CreateMap<FileTaskConfig, FileTask>();
            CreateMap<BaseTask, TaskLog>();
        }
    }
}