using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SMTIA.WebAPI.Abstractions
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        public readonly IMediator _mediator;
        protected ApiController(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
