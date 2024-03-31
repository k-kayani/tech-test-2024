using System;
using System.Linq;

public class Checkout : ICheckout
{
    private (string service, int quantity)[] cart;
    private readonly (string service, int price, (int quantity, int specialPrice)[] discounts)[] servicePricing;

    public Checkout()
    {
        cart = new (string, int)[0];
        servicePricing = new[]
        {
            ("A", 10, new (int, int)[]{(3, 25)}),
            ("B", 12, new (int, int)[]{(2, 20)}),
            ("C", 15, new (int, int)[0]),
            ("D", 25, new (int, int)[0]),
            ("F", 8, new (int, int)[]{(2, 15)})
        };
    }

    public void Scan(string service)
    {
        var index = Array.FindIndex(cart, item => item.service == service);
        if (index != -1)
        {
            cart[index] = (service, cart[index].quantity + 1);
        }
        else
        {
            Array.Resize(ref cart, cart.Length + 1);
            cart[cart.Length - 1] = (service, 1);
        }
    }

    public (int originalPrice, int finalPrice, int totalDiscount) GetTotalPrice()
    {
        int originalPrice = 0;
        int finalPrice = 0;
        int totalDiscount = 0;

        foreach (var item in cart)
        {
            var (service, quantity) = item;
            var serviceInfo = servicePricing.First(s => s.service == service);
            int price = serviceInfo.price;
            var discounts = serviceInfo.discounts;

            //  original price
            originalPrice += price * quantity;

            // if any discounts 
            foreach (var discount in discounts)
            {
                int discountQuantity = discount.Item1;
                int specialPrice = discount.Item2;

                if (quantity >= discountQuantity)
                {
                    int numDiscounts = quantity / discountQuantity;
                    int remainder = quantity % discountQuantity;
                    int discountPrice = numDiscounts * specialPrice + remainder * price;
                    finalPrice += discountPrice;
                    totalDiscount += originalPrice - discountPrice; 
                    break; 
                }
            }

            if (finalPrice == 0)
            {
                finalPrice += price * quantity; 
            }
        }

        return (originalPrice, finalPrice, totalDiscount);
    }
}

public interface ICheckout
{
    void Scan(string service);
    (int originalPrice, int finalPrice, int totalDiscount) GetTotalPrice();
}

class Program
{
    static void Main(string[] args)
    {
        //  multipurchase Discount Advantage
        ICheckout checkout = new Checkout();
        checkout.Scan("B");
        checkout.Scan("B");
        var totalPrice1 = checkout.GetTotalPrice();
        Console.WriteLine($"example 1");
        Console.WriteLine($"original price for 2 x Service B: £{totalPrice1.originalPrice}");
        Console.WriteLine($"Discount applied: £{totalPrice1.totalDiscount}");
        Console.WriteLine($"Final price: £{totalPrice1.finalPrice}");

        // no Multipurchase Discount
        checkout = new Checkout();
        checkout.Scan("F");
        checkout.Scan("C");
        var totalPrice2 = checkout.GetTotalPrice();
        Console.WriteLine($"\nexample 2");
        Console.WriteLine($"Original price for 1 x Service F and 1 x Service C: £{totalPrice2.originalPrice}");
        Console.WriteLine($"No applicable discount");
        Console.WriteLine($"Final price: £{totalPrice2.finalPrice}");

        // mix of Discounts and No Discount
        checkout = new Checkout();
        checkout.Scan("F");
        checkout.Scan("F");
        checkout.Scan("B");
        var totalPrice3 = checkout.GetTotalPrice();
        Console.WriteLine($"\nexample 3");
        Console.WriteLine($"Original price for 2 x Service F and 1 x Service B: £{totalPrice3.originalPrice}");
        Console.WriteLine($"Discount applied: £{totalPrice3.totalDiscount}");
        Console.WriteLine($"Final price: £{totalPrice3.finalPrice}");
    }
}
