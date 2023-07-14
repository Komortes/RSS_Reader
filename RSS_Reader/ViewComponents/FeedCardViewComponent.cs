using Microsoft.AspNetCore.Mvc;
using RSS_Reader.Models.Domain;

public class FeedCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(FeedModel feed)
    {
        return View(feed);
    }
}
