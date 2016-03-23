using System;
using System.Text;
using System.Web.Mvc;
using Store.WebUI.Models;

namespace Store.WebUI.HtmlHelpers
{
    public static class PagingHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper html, PagingInfo pagingInfo, Func<int, string> pageUrl)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<div class='pagination pagination-centered'>");
            if (pagingInfo.CurrentPage == 1)
            {
                result.Append("<li class='disabled'><a href='#'>Пред.</a></li>");
            }
            else
            {
                result.Append("<li><a href=\"# "/*+ pageUrl(pagingInfo.CurrentPage-1) +*/+ " \" class=\"prev\">Пред.</a></li>");
            }


            int start = 1, end = pagingInfo.TotalPages;

            if (pagingInfo.TotalPages > 3)
            {
                if (pagingInfo.CurrentPage > 1 && pagingInfo.CurrentPage < pagingInfo.TotalPages )
                {
                    start = pagingInfo.CurrentPage - 1;
                    end = start+2;
                }
                else if (pagingInfo.CurrentPage > 1)
                {
                    start = pagingInfo.CurrentPage-2;
                    end = pagingInfo.TotalPages;
                }
                end = start + 2;
            }

            for (int i = start; i <= end; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                TagBuilder li = new TagBuilder("li");
                tag.AddCssClass("linkbtn");

                tag.MergeAttribute("href", "#"/*pageUrl(i)*/);
                tag.InnerHtml = i.ToString();
                tag.MergeAttribute("id", i.ToString());
                if (i == pagingInfo.CurrentPage)
                {
                    li.AddCssClass("active");
                }
                li.InnerHtml = tag.ToString();
                result.Append(li.ToString());
            }
            if (pagingInfo.CurrentPage == pagingInfo.TotalPages || pagingInfo.TotalItems==0)
            {
                result.Append("<li class='disabled'><a href='#'>След.</a></li>");
            }
            else
            {
                result.Append("<li><a href=\"#"+/* +pageUrl(pagingInfo.CurrentPage+1) + */" \" class=\"next\">След.</a></li>");
            }
            result.Append("</div>");
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString AdminPageLinks(this HtmlHelper html, PagingInfo pagingInfo, Func<int, string> pageUrl)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<div class='pagination pagination-centered'>");
            if (pagingInfo.CurrentPage == 1)
            {
                result.Append("<li class='disabled'><a href='#'>Пред.</a></li>");
            }
            else
            {
                result.Append("<li><a href=\"# "/*+ pageUrl(pagingInfo.CurrentPage-1) +*/+ " \" class=\"prev\">Пред.</a></li>");
            }


            int start = 1, end = pagingInfo.TotalPages;

            if (pagingInfo.TotalPages > 3)
            {
                if (pagingInfo.CurrentPage > 1 && pagingInfo.CurrentPage < pagingInfo.TotalPages)
                {
                    start = pagingInfo.CurrentPage - 1;
                    end = start + 2;
                }
                else if (pagingInfo.CurrentPage > 1)
                {
                    start = pagingInfo.CurrentPage - 2;
                    end = pagingInfo.TotalPages;
                }
                end = start + 2;
            }

            for (int i = start; i <= end; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                TagBuilder li = new TagBuilder("li");

                tag.MergeAttribute("href", pageUrl(i));
                tag.InnerHtml = i.ToString();
                tag.MergeAttribute("id", i.ToString());
                if (i == pagingInfo.CurrentPage)
                {
                    li.AddCssClass("active");
                }
                li.InnerHtml = tag.ToString();
                result.Append(li.ToString());
            }
            if (pagingInfo.CurrentPage == pagingInfo.TotalPages || pagingInfo.TotalItems == 0)
            {
                result.Append("<li class='disabled'><a href='#'>След.</a></li>");
            }
            else
            {
                result.Append("<li><a href=\"#" +/* +pageUrl(pagingInfo.CurrentPage+1) + */" \" class=\"next\">След.</a></li>");
            }
            result.Append("</div>");
            return MvcHtmlString.Create(result.ToString());
        }
    }
}