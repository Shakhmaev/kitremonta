using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Domain.Entities
{
    public class Cart
    {
        [Required]
        public string Id { get; set; }

        public decimal ComputeTotalValue()
        {
            int sum = 0;
            foreach (var line in Lines)
            {
                if (line.Item.ItemType == "keram")
                {
                    sum += (int) (line.Item.OnlyInPacks == true ? line.Quantity * line.Item.Price * line.Item.m2 : line.Quantity * line.Item.Price);
                }
                else
                {
                    sum += line.Item.Price * line.Quantity;
                }
            }
            return sum;
        }
        public DateTime expires { get; set; }

        public virtual ICollection<CartLine> Lines { get; set; }

        public class Mapping : EntityTypeConfiguration<Cart>
        {
            public Mapping()
            {

            }
        }
    }

    public class CartLine
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string qUnit { get; set; }
        public virtual Item Item { get; set; }
    }
}
