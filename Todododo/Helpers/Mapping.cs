using AutoMapper;
using Todododo.Data;
using Todododo.ViewModels;

namespace Todododo.Helpers
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ToDoViewModel, ToDo>().ReverseMap();
        }
    }
}