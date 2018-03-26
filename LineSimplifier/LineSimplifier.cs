﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSimplifier {
    abstract public class LineSimplifier {
        public List<int> m_keys { get; set; }
        public IList<Point> Points { get; set; }
        public bool bNormalization {get;set;}
        public int NumOfPoints {
            get {
                return Points.Count();
            }
        }
        public IList<int> GetKeys {
            get {
                return m_keys;
            }
        }

        public LineSimplifier(IList<Point> pts, bool bnorm) {
            Points = new List<Point>();
            foreach (var pt in pts) {
                Points.Add(new Point(pt));
            }
            this.bNormalization = bnorm;
            if (this.bNormalization) {
                NormalizePoints();
            }
        }

        public void NormalizePoints() {
            double xmax=0, xmin=0, ymax=0, ymin=0;
            foreach (var pt in Points) {
                xmax = Math.Max(pt.X, xmax);
                xmin = Math.Max(pt.X, xmin);
                ymax = Math.Max(pt.Y, ymax);
                ymin = Math.Min(pt.Y, ymin);
            }
            double dx = xmax - xmin;
            double dy = ymax - ymin;
            if (dx == 0) dx = 1;
            if (dy == 0) dy = 1;
            foreach (var pt in Points) {
                pt.X = (pt.X - xmin) / dx;
                pt.Y = (pt.Y - ymin) / dy;
            }

        }

        public double SqrDist(Point p, Point q) {
            double dx = p.X - q.X;
            double dy = p.Y - q.Y;
            return dx * dx + dy * dy;
        }
        


        public double DotProduct(Point p, Point q) {
            return p.X * q.X + p.Y * q.Y;
        }

        abstract public void Simplify(double tol);

        



    }
}
