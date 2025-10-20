namespace eticaret.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(int productId, string productName, string brand, decimal price,
                           string imageUrl, int quantity = 1, string color = "", string size = "", string numara = "")
        {
            // Aynı ürün, aynı renk ve aynı bedenden varsa miktarını artır
            var item = Items.FirstOrDefault(i => i.ProductId == productId &&
                                                 i.SelectedColor == color &&
                                                 i.SelectedSize == size &&
                                                 i.SelectedNumara == numara);


            if (item == null)
            {
                Items.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Brand = brand,
                    Price = price,
                    ImageUrl = imageUrl,
                    Quantity = quantity,
                    SelectedColor = color,
                    SelectedSize = size,
                    SelectedNumara = numara
                });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId, string color, string size, string numara)
        {
            Items.RemoveAll(i => i.ProductId == productId &&
                               i.SelectedColor == color &&
                               i.SelectedSize == size &&
                               i.SelectedNumara == numara);
        }

        public void UpdateQuantity(int productId, string color, string size, int quantity, string numara)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId &&
                                                 i.SelectedColor == color &&
                                                 i.SelectedSize == size &&
                                                 i.SelectedNumara == numara);
            if (item != null)
            {
                if (quantity <= 0)
                    Items.Remove(item);
                else
                    item.Quantity = quantity;
            }
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.Price * i.Quantity);
        }

        public int GetTotalItems()
        {
            return Items.Sum(i => i.Quantity);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}