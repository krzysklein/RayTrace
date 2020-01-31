using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RayTrace
{
    public class Scene
    {
        public List<SceneObject> Things;
        public List<Light> Lights;
        public Camera Camera;

        public IEnumerable<Intersection> Intersect(Ray r)
        {
            return Things
                .Select(t => t.Intersect(r));
        }
    }
}
