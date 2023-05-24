using EngineIOSharp.Common.Enum;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using SocketIOSharp.Client;

public class SocketTest
{
    private readonly ITestOutputHelper output;

    public SocketTest(ITestOutputHelper output)
    {
        this.output = output;
    }
    
    [Fact]
    public void TestConnection()
    {
        var socket = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "127.0.0.1", 3000));
        socket.Connect();

        socket.On("connect", () =>
        {
            output.WriteLine("Connected to server!");
        });

        socket.On("disconnect", () =>
        {
            output.WriteLine("Disconnected from server!");
        });

        socket.On("rasdeq", (JToken[] data) => 
        {
            output.WriteLine($"Server sent a response: {data}");
        });
        
        // Wait for a bit before disconnecting
        Thread.Sleep(5000);
        socket.Close();
    }
}