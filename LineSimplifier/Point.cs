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

		public double SqrDist(Point q) {
			double dx = X - q.X;
			double dy = Y - q.Y;
			return dx * dx + dy * dy;
		}

		public double DotProduct(Point q) {
			return X * q.X + Y * q.Y;
		}
    }
}
