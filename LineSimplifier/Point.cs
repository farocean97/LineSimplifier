using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSimplifier {
    public class Point {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public Point(Point p) {
            this.X = p.X;
            this.Y = p.Y;
        }
    }
}
