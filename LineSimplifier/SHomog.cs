using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSimplifier {
    public class SHomog {
        public double U { get; set; }
        public double V { get; set; }
        public double W { get; set; }

        public SHomog(Point p, Point q) {
            U = -q.Y + p.Y;
            V = q.X - p.X;
            W = p.X * q.Y + p.Y * q.X;
        }


        public double DotProduct_2CH(Point p) {
            return W + p.X * U + p.Y * V;
        }

        public bool Slope_Sign(Point p, Point q) {
            return ((U * (q.X - p.X) + V * (q.Y - p.Y)) >= 0);
        }

    }
}
