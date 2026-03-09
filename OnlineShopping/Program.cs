using OnlineShopping.Interfaces;
using OnlineShopping.Menus;
using OnlineShopping.Services;
using OnlineShopping.Utilities;

var context = new AppDataContext();
SeedData.Initialize(context);

IAuthService authService = new AuthService(context);
IProductService productService = new ProductService(context);
ICartService cartService = new CartService(productService, context);
IPaymentService paymentService = new PaymentService(context);
IOrderService orderService = new OrderService(context, paymentService);
IReportService reportService = new ReportService(context);

var customerMenu = new CustomerMenu(productService, cartService, orderService, paymentService);
var adminMenu = new AdminMenu(productService, orderService, reportService);
var mainMenu = new MainMenu(authService, customerMenu, adminMenu);

Console.Clear();
Console.WriteLine("========================================");
Console.WriteLine(" Online Shopping Backend System (Console)");
Console.WriteLine("========================================");
Console.WriteLine("Seed Accounts:");
Console.WriteLine("- Admin: admin / admin123");
Console.WriteLine("- Customer: customer / cust123");

mainMenu.Run();
