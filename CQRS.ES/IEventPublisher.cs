using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Common;

namespace CQRS.ES
{
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}
