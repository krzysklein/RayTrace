using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RayTrace
{
    public class Plane : SceneObject
    {
        public Vector Norm;
        public double Offset;

        public override Intersection Intersect(Ray ray)
        {
            double denom = Vector.Dot(Norm, ray.Dir);
            if (denom > 0) return null;
            return new Intersection()
            {
                Thing = this,
                Ray = ray,
                Dist = (Vector.Dot(Norm, ray.Start) + Offset) / (-denom)
            };
        }

        public override Vector Normal(Vector pos)
        {
            return Norm;
        }
    }
}
