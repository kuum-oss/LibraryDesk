using System.Text;
using LibraryDesk.Core;

Console.OutputEncoding = Encoding.UTF8;
var z = new Order("A-1001", "Іваненко");
z.AddLine("SKU-1", 3, 250m);
z.AddLine("SKU-2", 12, 90m);
Console.WriteLine(z.CalculateTotal(false));
Console.WriteLine(z.CalculateTotal(true));
Console.WriteLine(z.IsValid());
Console.WriteLine(z.TryChangeStatus(1));
Console.Write(z.BuildReport());
