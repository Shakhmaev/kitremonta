using MvcSiteMapProvider;
using Store.Domain.Abstract;
using Store.Domain.Concrete;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.WebUI.Infrastructure
{
    public class DynamicNodeProvider:DynamicNodeProviderBase
    {
        IItemRepository repos;
        public DynamicNodeProvider(IItemRepository repo)
        {
            repos = repo;
        }
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            IEnumerable<Category> categories = repos.Categories.Where(x => x.ParentID == null);
            foreach (var cat in categories)
            {
                DynamicNode Dnode = new DynamicNode(Convert.ToString(cat.CategoryId), "Home", cat.Description, cat.Description);
                Dnode.RouteValues.Add("category",cat.Name);
                DynamicNode all = new DynamicNode(Convert.ToString(cat.CategoryId) + "_all", Dnode.Key, "Все", "Все");
                all.RouteValues.Add("category", cat.Name);
                yield return all;
                foreach (var sub in cat.SubCategories)
                {
                    foreach (var dnd in GetSubCategNodes(sub))
                    {
                        yield return dnd;
                    }
                }
                yield return Dnode;
            }
        }

        IEnumerable<DynamicNode> GetSubCategNodes(Category subcateg)
        {   
            DynamicNode node = new DynamicNode(Convert.ToString(subcateg.CategoryId),
                    Convert.ToString(subcateg.Parent.CategoryId), subcateg.Description, subcateg.Description);
            node.RouteValues.Add("category", subcateg.Name);
            if (subcateg.Type == "Collection") node.Attributes["visibility"] = "!SiteMapHelper,*";
            yield return node;
            foreach (var sub in subcateg.SubCategories)
            {
                foreach (var dnd in GetSubCategNodes(sub))
                {
                    yield return dnd;
                }
            }
        }
    }

    public class DynamicItemsNodeProvider : DynamicNodeProviderBase
    {
        IItemRepository repos;
        public DynamicItemsNodeProvider(IItemRepository repo)
        {
            repos = repo;
        }
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            IEnumerable<Item> items = repos.Items.GroupBy(x=>x.article).Select(g=>g.FirstOrDefault());
            
            foreach (var item in items)
            {
                foreach (var ctg in item.ParentCategories)
                {
                    DynamicNode nodeitem = new DynamicNode("id_" + Convert.ToString(item.Id) 
                        + "-parentid_" + Convert.ToString(ctg.CategoryId),
                        item.Name);
                    nodeitem.ParentKey = Convert.ToString(ctg.CategoryId);
                    nodeitem.RouteValues.Add("id", item.Id);
                    yield return nodeitem;
                }
            }
        }
    }
}