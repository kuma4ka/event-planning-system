using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Interfaces;

public interface IEventService : IEventReadService, IEventWriteService
{
}