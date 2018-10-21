using System.Collections.Generic;
using System.Threading.Tasks;
using uhttpsharp.Handlers;

namespace uhttpsharp.Controllers {
    public interface IErrorContainer {
        IEnumerable<string> Errors { get; }

        bool Any { get; }

        void Log(string description);

        Task<IControllerResponse> GetResponse();
    }
}