using System.Threading.Tasks;

namespace System.Web.SessionState
{
  public interface ISessionStateModule : IHttpModule
  {
    void ReleaseSessionState(HttpContext context);

    Task ReleaseSessionStateAsync(HttpContext context);
  }
}