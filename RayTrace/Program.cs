using System.IO;

namespace RayTrace
{
    class Program
    {
        static void Main(string[] args)
        {
            var bytes = Renderer.Render(0, 1).GetAwaiter().GetResult();
            File.WriteAllBytes("out.png", bytes);
        }
    }
}
