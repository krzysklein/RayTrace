using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RayTrace
{
    public class RayTracer
    {
		private int screenWidth;
        private int screenHeight;
        private const int MaxDepth = 5;

        public Action<int, int, ImageProcessorCore.Color> setPixel;

        public RayTracer(int screenWidth, int screenHeight, Action<int, int, ImageProcessorCore.Color> setPixel)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.setPixel = setPixel;
        }

        private IEnumerable<Intersection> Intersections(Ray ray, Scene scene)
        {
            return scene.Things
                        .Select(obj => obj.Intersect(ray))
                        .Where(inter => inter != null)
                        .OrderBy(inter => inter.Dist);
        }

        private double TestRay(Ray ray, Scene scene)
        {
            var isects = Intersections(ray, scene);
            var isect = isects.FirstOrDefault();
            if (isect == null)
                return 0;
            return isect.Dist;
        }

        private Color TraceRay(Ray ray, Scene scene, int depth)
        {
            var isects = Intersections(ray, scene);
            var isect = isects.FirstOrDefault();
            if (isect == null)
                return Color.Background;
            return Shade(isect, scene, depth);
        }

        private Color GetNaturalColor(SceneObject thing, Vector pos, Vector norm, Vector rd, Scene scene)
        {
            Color ret = Color.Make(0, 0, 0);
            foreach (Light light in scene.Lights)
            {
                Vector ldis = Vector.Minus(light.Pos, pos);
                Vector livec = Vector.Norm(ldis);
                double neatIsect = TestRay(new Ray() { Start = pos, Dir = livec }, scene);
                bool isInShadow = !((neatIsect > Vector.Mag(ldis)) || (neatIsect == 0));
                if (!isInShadow)
                {
                    double illum = Vector.Dot(livec, norm);
                    Color lcolor = illum > 0 ? Color.Times(illum, light.Color) : Color.Make(0, 0, 0);
                    double specular = Vector.Dot(livec, Vector.Norm(rd));
                    Color scolor = specular > 0 ? Color.Times(Math.Pow(specular, thing.Surface.Roughness), light.Color) : Color.Make(0, 0, 0);
                    ret = Color.Plus(ret, Color.Plus(Color.Times(thing.Surface.Diffuse(pos), lcolor),
                                                     Color.Times(thing.Surface.Specular(pos), scolor)));
                }
            }
            return ret;
        }

        private Color GetReflectionColor(SceneObject thing, Vector pos, Vector norm, Vector rd, Scene scene, int depth)
        {
            return Color.Times(thing.Surface.Reflect(pos), TraceRay(new Ray() { Start = pos, Dir = rd }, scene, depth + 1));
        }

        private Color Shade(Intersection isect, Scene scene, int depth)
        {
            var d = isect.Ray.Dir;
            var pos = Vector.Plus(Vector.Times(isect.Dist, isect.Ray.Dir), isect.Ray.Start);
            var normal = isect.Thing.Normal(pos);
            var reflectDir = Vector.Minus(d, Vector.Times(2 * Vector.Dot(normal, d), normal));
            Color ret = Color.DefaultColor;
            ret = Color.Plus(ret, GetNaturalColor(isect.Thing, pos, normal, reflectDir, scene));
            if (depth >= MaxDepth)
            {
                return Color.Plus(ret, Color.Make(.5, .5, .5));
            }
            return Color.Plus(ret, GetReflectionColor(isect.Thing, Vector.Plus(pos, Vector.Times(.001, reflectDir)), normal, reflectDir, scene, depth));
        }

        private double RecenterX(double x)
        {
            return (x - (screenWidth / 2.0)) / (2.0 * screenWidth);
        }
        private double RecenterY(double y)
        {
            return -(y - (screenHeight / 2.0)) / (2.0 * screenHeight);
        }

        private Vector GetPoint(double x, double y, Camera camera)
        {
            return Vector.Norm(camera.Forward + (RecenterX(x) * camera.Right) + (RecenterY(y) * camera.Up));
        }

        public void Render(Scene scene)
        {
            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    Color color = TraceRay(new Ray() { Start = scene.Camera.Pos, Dir = GetPoint(x, y, scene.Camera) }, scene, 0);
                    setPixel(x, y, color.ToImageProcessorColor());
                }
            }
        }

        public void RenderPartial(Scene scene, int part, int partCount)
        {
            int partWidth = screenWidth / partCount;
            int partStartX = part * partWidth;
            //int partEndX = partStartX + partWidth - 1;

            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < partWidth; x++)
                {
                    Color color = color = TraceRay(new Ray() { Start = scene.Camera.Pos, Dir = GetPoint(x + partStartX, y, scene.Camera) }, scene, 0);
                    setPixel(x, y, color.ToImageProcessorColor());
                }
            }
        }
    }
}
