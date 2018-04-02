using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LineSimplifier;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Diagnostics;

namespace TrendSimplifierTest {
    class Program {
        static void Main(string[] args) {
            string filename = @"C:\testa.csv";

            //Generate testdata

			//using (StreamWriter sw = new StreamWriter(filename, false)) {
			//    Random rd = new Random();
			//    int numofentries = 1000000;
			//    double a1 = 0.5;
			//    double w1 = 0.01;
			//    double a2 = 0.6;
			//    double w2 = 0.028;
			//    double s = 1000 / (double)numofentries;
			//    for (int i = 0; i <= numofentries; i++) {
			//        double x = (double)i;
			//        double y = 0;
			//        y = a1 * Math.Sin(w1 * s * x) + a2 * Math.Cos(w2 * s * x)
			//            + a1 * Math.Sin(2 * w1 * s * x) + a2 * Math.Cos(2 * w2 * s * x);
			//        y += +rd.NextDouble();
			//        sw.WriteLine("{0},{1}", x, y);
			//    }
			//}

            //var lstPts = ReadCSVLines(filename).Where(x=>x.X<97).ToList();
            Console.WriteLine("Hello");
            var varlstPts = ReadCSVLines(filename);
            //Console.WriteLine("HelloLast");
            //var a = varlstPts.Last();
            //Console.WriteLine("HelloArray");
            //var arrlstpts = varlstPts.ToArray();

            Console.WriteLine("Hello2");
            var lstPts = varlstPts.ToList();

			Console.WriteLine("Please pressure Enter to continue!");
			Console.ReadKey();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            DPHull dphull = new DPHull(lstPts, false);
            Console.WriteLine("Reduced key number = {0}", dphull.GetKeys.Count);
            double tol = 0.95; //0.05
            dphull.Simplify(tol);
            watch.Stop();
            Console.WriteLine("Time elapsed : {0}", watch.Elapsed);

            int k1 = dphull.GetKeys[0];
            //for (int i = 1; i < dphull.GetKeys.Count; i++) {
            //    Point p1 = lstPts[dphull.GetKeys[k1]];
            //    Point p2 = lstPts[dphull.GetKeys[i]];
            //    SHomog line = new SHomog(p1,p2 );
            //    double lensq = p1.SqrDist(p2);
            //    for (int j = k1 + 1; j < i; j++) {
            //        Point p = lstPts[dphull.GetKeys[j]];
            //        if (line.DotProduct_2CH(p) > tol * lensq) {
            //            Console.WriteLine("Failed point x = {0}", lstPts[dphull.GetKeys[j]].X);
            //        }

            //    }
            //}
			//Console.WriteLine("Reduced key number = {0}", dphull.GetKeys.Count);
			//using (StreamWriter sw = new StreamWriter("C:\\simplified_test.csv", false)) {
			//    foreach (int i in dphull.GetKeys) {
			//        sw.WriteLine("{0},{1}", lstPts[i].X, lstPts[i].Y);
			//    }
			//}

			//Console.WriteLine("Reduced key number = {0}", dphull.GetKeys.Count);

            //Console.ReadKey();

        }

        public static IEnumerable<Point> ReadCSVLines(string filepath) {
            using (TextFieldParser parser = new TextFieldParser(filepath)) {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData) {
                    string[] fields = parser.ReadFields();
                    if (fields.Count() < 2) {
                        throw new Exception("Need Time, UpPressure and DownPressure on each line in CSV file");
                    }
                    double x = Convert.ToDouble(fields[0]);
                    double y = Convert.ToDouble(fields[1]);
                    Point tp = new Point(x, y);
                    yield return tp;
                }
            }
        }
    }


}
