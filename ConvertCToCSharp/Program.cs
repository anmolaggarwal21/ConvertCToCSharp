// See https://aka.ms/new-console-template for more information
using ConvertCToCSharp;

Console.WriteLine("Hello, World!");
CSharpCodeNew cs = new CSharpCodeNew();
CSharpCodeWithChnages cs1 = new CSharpCodeWithChnages();
Console.WriteLine("Enter the port");
string port = Console.ReadLine();
SerialPortImplementation serialPortImplementation = new SerialPortImplementation(port);
serialPortImplementation.main();
Console.ReadLine();
