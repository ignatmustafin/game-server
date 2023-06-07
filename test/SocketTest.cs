// using EngineIOSharp.Common.Enum;
// using Newtonsoft.Json.Linq;
// using Xunit;
// using Xunit.Abstractions;
// using SocketIOSharp.Client;
// using SocketIOSharp.Server.Client;
//
// public class SocketTest
// {
//     private readonly ITestOutputHelper output;
     // private SocketIOClient _socket1;
     // private SocketIOClient _socket2;
//
//     public SocketTest(ITestOutputHelper output)
//     {
//         this.output = output;
//     }
//     
//     [Fact]
//     public void TestConnection()
//     {
         // _socket1 = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "127.0.0.1", 3000));
         // _socket1.Connect();
         //
         // _socket1.On("connect", (JToken[] data) =>
         // {
         //     output.WriteLine($"Connected to server! {data[0]}");
         //     _socket1.Emit("test", 1);
         // });
//         //
//         // _socket1.On("disconnect", () =>
//         // {
//         //     output.WriteLine("Disconnected from server!");
//         // });
//         output.WriteLine("Test 1 called");
//
//     }
//
//     [Fact]
//     public void TestConnectionSocket2()
//     {
//         // _socket2 = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "127.0.0.1", 3000));
//         // _socket2.Connect();
//         //
//         // _socket2.On("connect", (JToken[] data) =>
//         // {
//         //     output.WriteLine($"Connected to server! {data[0]}");
//         //     _socket2.Emit("test", 3);
//         // });
//         //
//         // _socket2.On("disconnect", () =>
//         // {
//         //     output.WriteLine("Disconnected from server!");
//         // });
//         output.WriteLine("Test 2 called");
//     }
//
//     [Fact]
//     public void Disconnect()
//     {
//         // output.WriteLine($"socket 1 {_socket1}");
//         // _socket1.Close();
//         // _socket2.Close();
//         output.WriteLine("Test 3 called");
//     }
// }