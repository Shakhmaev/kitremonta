using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Store.WebUI.Controllers
{
    [AllowAnonymous]
    public class ImageViewController : Controller
    {
        IItemRepository repository;
        public ImageViewController(IItemRepository repo)
        {
            repository = repo;
        }

        //для получения списка адресов всех доп. фото данного товара
        public string GetExtraImages(int Id)
        {
            Item item = repository.Items.Where(i => i.Id == Id).FirstOrDefault();
            if (item != null)
            {
                string[] urls = new string[item.Photos.Count];
                if (item != null)
                {
                    for (int i = 0; i < item.Photos.Count; i++)
                    {
                        //urls[i] = Url.Action("GetImageById", new { id = item.Photos.ElementAt(i).PhotoId });
                        urls[i] = GetImageById(item.Photos.ElementAt(i).PhotoId);
                    }
                }
                JavaScriptSerializer objSerializer = new JavaScriptSerializer();
                return objSerializer.Serialize(urls);
            }
            else return null;
        }

        //для получения конкретной фотографии
        public string GetImageById(int id)
        {
            Photo photo = repository.Photos.Where(p=>p.PhotoId==id).FirstOrDefault();
            if (photo != null)
            {
               // return File(photo.ImageData, photo.ImageMimeType);
                return Url.Content("~/Uploads/Images/" + photo.url);
            }
            else return null;
        }

        public string GetCategoryImage(int id)
        {
            return Url.Content("~/Uploads/CategoryImages/" + repository.GetCategoryImageUrl(id));
        }

        public string GetCategoryImageMini(int id)
        {
            Category ctg = repository.Categories.FirstOrDefault(x => x.CategoryId == id);

            if (ctg != null && ctg.Photo != null)
            {
                return Url.Content("~/Uploads/CategoryImages/" + 
                         repository.GetCategoryImageMiniUrl(ctg.CategoryId));
            }
            else return null;
        }

        public string GetCategoryExtraImage(string url)
        {
            return Url.Content("~/Uploads/CategoryImages/" + url);
        }
	}
}