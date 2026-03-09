using OnlineShopping.Interfaces;
using OnlineShopping.Menus;
using OnlineShopping.Repositories;
using OnlineShopping.Services;
using OnlineShopping.Utilities;

var context = new AppDataContext();
SeedData.Initialize(context);

IRepositorySession repositorySession = new RepositorySession(context);
IUserRepository userRepository = new UserRepository(context);
IProductRepository productRepository = new ProductRepository(context);
IOrderRepository orderRepository = new OrderRepository(context);
IPaymentRepository paymentRepository = new PaymentRepository(context);
IUserFactory userFactory = new UserFactory();

IOrderStatusTransitionPolicy orderStatusTransitionPolicy = new OrderStatusTransitionPolicy();
IPaymentStrategy walletPaymentStrategy = new WalletPaymentStrategy(paymentRepository, repositorySession);
IPaymentStrategy codPaymentStrategy = new CashOnDeliveryPaymentStrategy(paymentRepository, repositorySession);
IPaymentStrategyFactory paymentStrategyFactory = new PaymentStrategyFactory([walletPaymentStrategy, codPaymentStrategy]);

IPaymentService paymentService = new PaymentService(paymentRepository, repositorySession);
IAuthService authService = new AuthService(userRepository, userFactory, repositorySession);
IProductService productService = new ProductService(productRepository, orderRepository, repositorySession);
ICartService cartService = new CartService(productService, repositorySession);
IOrderService orderService = new OrderService(orderRepository, userRepository, paymentStrategyFactory, paymentService, repositorySession, orderStatusTransitionPolicy);
IReportService reportService = new ReportService(orderRepository, productRepository);

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
