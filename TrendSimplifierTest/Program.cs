using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LineSimplifier;
using Microsoft.VisualBasic.FileIO;

namespace TrendSimplifierTest {
    class Program {
        static void Main(string[] args) {
            string filename = @"D:\test.csv";
            var lstPts = ReadCSVLines(filename).ToList();
            DPHull dphull = new DPHull(lstPts, false);
            double tol = 3;
            dphull.Simplify(tol);
            foreach (int i in dphull.GetKeys) {
                Console.WriteLine("x = {0}", lstPts[i].X);
            }

            Console.ReadKey();

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
