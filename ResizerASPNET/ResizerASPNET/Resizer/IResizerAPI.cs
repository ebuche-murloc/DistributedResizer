namespace ResizerASPNET.Resizer
{
    public interface IResizerAPI
    {
        ResizeStatusInfo Resize(int height, int width, string file);
    }
}
