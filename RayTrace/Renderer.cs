using ImageProcessorCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RayTrace
{
    public static class Renderer
    {
        public static async Task<byte[]> Render(int imageNo, int imageCount)
        {
            // Configuration
            const int width = 640;// 1600;
            const int height = 480;// 800;
            //const int imageCount = 8;

            // Generate image
            Console.WriteLine("Render started");
            var imageWidth = width / imageCount;
            var image = new Image(imageWidth, height);
            using (var pixelAccessor = image.Lock())
            {
                // Raytracer
                RayTracer rayTracer = new RayTracer(width, height, (int x, int y, ImageProcessorCore.Color color) =>
                {
                    pixelAccessor[x, y] = color;
                });

                // Scene
                Scene scene = _GenerateScene(((double)height)/((double)width));

                // Render
                var start = DateTime.Now;
                rayTracer.RenderPartial(scene, imageNo, imageCount);
                var end = DateTime.Now;

                Console.WriteLine($"Render complete. Time: {end - start}");
            }

            // Save and return
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream);
                return stream.ToArray();
            }
        }

        private static Scene _GenerateScene(double ratio)
        {
            // Things
            List<SceneObject> things = new List<SceneObject>();
            things.Add(new Plane() {
                Norm = Vector.Make(0,1,0),
                Offset = 0,
                Surface = Surfaces.CheckerBoard
            });
            things.AddRange(_CircleOfSpheres(10, 4.0, 1.0));
            things.AddRange(_CircleOfSpheres(16, 8.0, 1.5));

            // Lights
            List<Light> lights = new List<Light>(); 
            lights.Add(new Light() { 
                Pos = Vector.Make(-2,2.5,0), 
                Color = RayTrace.Color.Make(.49,.07,.07) });
            lights.Add(new Light() { 
                Pos = Vector.Make(1.5,2.5,1.5),
                Color = RayTrace.Color.Make(.07,.07,.49) });
            lights.Add(new Light() { 
                Pos = Vector.Make(1.5,2.5,-1.5),
                Color = RayTrace.Color.Make(.07,.49,.071) });
            lights.Add(new Light() { 
                Pos = Vector.Make(0,3.5,0),
                Color = RayTrace.Color.Make(.21,.21,.35) });

            // Camera
            Camera camera = Camera.Create(Vector.Make(8, 6, 10), Vector.Make(-1, .5, 0), ratio);

            // Scene
            return new Scene() {
                Things = things,
                Lights = lights,
                Camera = camera
            };
        }

        private static IEnumerable<Sphere> _CircleOfSpheres(int num, double radius, double sphereRadius)
        {
            for (int i = 0; i < num; i++)
            {
                double x = Math.Sin(Math.PI * 2.0 * (double)i / (double)num) * radius;
                double z = Math.Cos(Math.PI * 2.0 * (double)i / (double)num) * radius;

                yield return new Sphere() {
                    Center = Vector.Make(x, sphereRadius, z),
                    Radius = sphereRadius,
                    Surface = Surfaces.Shiny
                };
            }
        }
    }
}
