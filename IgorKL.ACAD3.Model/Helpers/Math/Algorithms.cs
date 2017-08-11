using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using IgorKL.ACAD3.Model.Extensions;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace IgorKL.ACAD3.Model.Helpers.Math {
    public static class Algorithms {

        public static bool IsInsidePolygon(IEnumerable<Point2d> polygon, Point3d pt, double tolerencePct = 0.001) {
            int n = polygon.Count();
            double angle = 0;
            Point pt1, pt2;
            double tolerence = System.Math.PI * tolerencePct / 100d;

            for (int i = 0; i < n; i++) {
                pt1.X = polygon.ElementAt(i).X - pt.X;
                pt1.Y = polygon.ElementAt(i).Y - pt.Y;
                pt2.X = polygon.ElementAt((i + 1) % n).X - pt.X;
                pt2.Y = polygon.ElementAt((i + 1) % n).Y - pt.Y;
                angle += Angle2D(pt1.X, pt1.Y, pt2.X, pt2.Y);
            }

            if (System.Math.Abs(angle) - System.Math.PI < -tolerence)
                return false;
            else
                return true;
        }
        private static double Angle2D(double x1, double y1, double x2, double y2) {
            double dtheta, theta1, theta2;

            theta1 = System.Math.Atan2(y1, x1);
            theta2 = System.Math.Atan2(y2, x2);
            dtheta = theta2 - theta1;
            while (dtheta > System.Math.PI)
                dtheta -= (System.Math.PI * 2);
            while (dtheta < -System.Math.PI)
                dtheta += (System.Math.PI * 2);
            return (dtheta);
        }

        /// <summary>
        /// Проверяет находится ли точка внутри полигона
        /// </summary>
        /// <param name="polygon">Исходный контур (полигон)</param>
        /// <param name="pt">Определяемая точка</param>
        /// <param name="tolerencePct">Допустимая погрешность расчета</param>
        /// <returns></returns>
        public static bool IsInsidePolygon(IEnumerable<Point3d> polygon, Point3d pt, double tolerencePct = 0.001) {
            var polygon2d = polygon.Select(p3d => new Point2d(p3d[0], p3d[1]));
            return IsInsidePolygon(polygon2d, pt, tolerencePct);
        }
        private struct Point {
            public double X, Y;
        };

        public class RandomPoint {
            public RandomPoint(IEnumerable<Point3d> polygon) {
                this.ClculateMinMax(polygon);
            }

            double xMin, xMax, yMin, yMax;
            Random rnd = new Random(DateTime.Now.Millisecond);

            public void ClculateMinMax(IEnumerable<Point3d> points) {
                int count = points.Count();
                var xs = points.OrderBy(p => p.X).Where((p, i) => { return i == 0 || i == count - 1; }).Select(p => p.X);
                var ys = points.OrderBy(p => p.Y).Where((p, i) => { return i == 0 || i == count - 1; }).Select(p => p.Y);

                xMax = xs.Max();
                yMax = ys.Max();
                xMin = xs.Min();
                yMin = ys.Min();
            }

            public bool GenPoint(IEnumerable<Point3d> polygon, out Point3d point) {
                double x = xMin + (xMax - xMin) * rnd.NextDouble();
                double y = yMin + (yMax - yMin) * rnd.NextDouble();
                point = new Point3d(x, y, 0);
                // p is your point, p.x is the x coord, p.y is the y coord
                if (point.X < xMin || point.X > xMax || point.Y < yMin || point.Y > yMax) {
                    return false;
                } else if (IsInsidePolygon(polygon, point)) {
                    return true;
                }
                return false;
            }

            [RibbonCommandButton("Случ точка в конт", RibbonPanelCategories.Points_Coordinates)]
            [Autodesk.AutoCAD.Runtime.CommandMethod("iCmd_RndPoint", Autodesk.AutoCAD.Runtime.CommandFlags.UsePickSet)]
            public static void RndPointCmd() {
                var id = ObjectCollector.SelectAllowedClassObject<Polyline>("Укажите полилинию", "Выбранный объект не полилиния");
                if (id == null)
                    return;
                Editor ed = AcadEnvironments.Editor;

                var stepRes = ed.GetDouble(new PromptDoubleOptions("Укажите \'шаг\', м") {
                    AllowNegative = false,
                    DefaultValue = 10,
                    AllowZero = false
                });
                if (stepRes.Status != PromptStatus.OK)
                    return;
                double step = stepRes.Value;

                var kwOpt = new PromptKeywordOptions("Acad | Cogo points?");
                kwOpt.AllowNone = true;
                kwOpt.AppendKeywordsToMessage = true;
                kwOpt.Keywords.Add("AcadPoints");
                kwOpt.Keywords.Add("CogoPoints");
                kwOpt.AllowArbitraryInput = true;

                var kwRes = ed.GetKeywords(kwOpt);

                if (kwRes.Status != PromptStatus.OK)
                    return;

                Tools.StartTransaction((trans, doc) => {
                    Polyline pline = id.GetObject<Polyline>(OpenMode.ForRead);
                    var polygon = pline.GetPoints3d();
                    RandomPoint test = new RandomPoint(polygon);
                    List<Point3d> points = new List<Point3d>();
                    int count = (int)(System.Math.Floor(pline.Area / System.Math.Pow(step, 2))) + 2;
                    if (count > System.Math.Pow(10000,2)) {
                        ed.WriteMessage($"\nСлишком много точек - {count}, Нужно увеличить шаг (текущее значение шага - {step}м)\n");
                        return;
                    }
                    int maxIter = 1000, i = 0;
                    while (points.Count < count && i < maxIter) {
                        if (test.GenPoint(polygon, out Point3d point))
                            points.Add(point);
                        else i++;

                    }
                    if (kwRes.StringResult == "AcadPoints")
                        Tools.AppendEntity(points.Select(p =>  new DBPoint(p) ));
                    else
                        IgorKL.ACAD3.Model.CogoPoints.CogoPointFactory.CreateCogoPoints(points);

                });

            }
        }



    }
}
