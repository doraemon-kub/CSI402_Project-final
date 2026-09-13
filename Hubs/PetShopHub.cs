using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CSI402_Project_final.Hubs
{
    public class PetShopHub : Hub
    {
        // แจ้งเตือนเมื่อมีคนเพิ่มสินค้าลงตะกร้า (Req 8: Social Proof Real-Time)
        public async Task BroadcastCartAdd(int productId, int currentCartCount)
        {
            await Clients.All.SendAsync("ReceiveCartCountUpdate", productId, currentCartCount);
        }

        // แจ้งเตือนออเดอร์ใหม่ให้กับ Order Staff & Back-Office
        public async Task BroadcastNewOrder(string orderId, string customerName, decimal grandTotal)
        {
            await Clients.All.SendAsync("ReceiveNewOrderAlert", orderId, customerName, grandTotal);
        }

        // แจ้งเตือนสินค้าสต็อกต่ำกว่าเกณฑ์ให้กับ Inventory Staff (Req 18)
        public async Task BroadcastLowStock(string productName, int currentStock)
        {
            await Clients.All.SendAsync("ReceiveLowStockAlert", productName, currentStock);
        }

        // แจ้งเตือนสถานะพัสดุจัดส่งแล้วให้กับลูกค้า (Req 4, 16)
        public async Task BroadcastOrderShipped(string orderId, string trackingNumber, string courier)
        {
            await Clients.All.SendAsync("ReceiveOrderShippedAlert", orderId, trackingNumber, courier);
        }
    }
}
