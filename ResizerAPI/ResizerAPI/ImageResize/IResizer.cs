using System.Runtime.CompilerServices;

namespace ResizerAPI.ImageResize
{
    public interface IResizer
    {
        public ResizeStatusInfo Resize(int height, int width, string file, [CallerFilePath] string tmpPath = "");
    }
}
