using Microsoft.AspNetCore.Mvc;

namespace ComponentLibrary.ViewComponents
{
    public class NavComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
