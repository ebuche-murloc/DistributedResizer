namespace ResizerASPNET.Rabbit
{
    public interface IRpcClient 
    {
        public Task<string> SendAsync(string message);
    }
}