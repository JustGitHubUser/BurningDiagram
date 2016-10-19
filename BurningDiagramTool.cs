﻿using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DevExpress.Diagram.Core;
using DevExpress.Diagram.Core.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Diagram;
using Utils.Helpers;

namespace BurningDiagram {
    static class BurningDiagramTool {
        public static void Run(string sourceFile, string targetFile) {
            var inputLines = File.ReadAllLines(sourceFile);
            var startDay = ParseDate(inputLines[0]);
            var finishDay = startDay.AddDays(14);
            var allDays = LinqExtensions.Unfold(startDay, x => x.AddDays(1), x => x >= finishDay).ToArray();
            var estimates = inputLines.Skip(1).Select(decimal.Parse).ToArray();
            var diagram = FillDiagram(allDays, estimates);
            diagram.SaveDocument(targetFile);
            //diagram.ExportDiagram(targetFile);
        }
        static DateTime ParseDate(string dateString) {
            var parts = dateString.Split('/');
            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var day = int.Parse(parts[2]);
            return new DateTime(year, month, day);
        }
        static DiagramControl FillDiagram(IEnumerable<DateTime> days, IEnumerable<decimal> estimates) {
            var diagram = new DiagramControl();
            var size = PaperSizeCalculator.GetPixelSize(PaperKind.A4);
            diagram.PageSize = new Size(size.Height, size.Width);
            double lineWidth, x0, y0, x1, y1;
            AddAxes(diagram, days.Count(), estimates.First(), out lineWidth, out x0, out y0, out x1, out y1);
            AddIdealLine(diagram, days, estimates.First(), lineWidth, x0, y0, x1, y1);
            AddRealLine(diagram, days, estimates, lineWidth, x0, y0, x1, y1);
            return diagram;
        }
        static void AddIdealLine(DiagramControl diagram, IEnumerable<DateTime> days, decimal maximum, double lineWidth, double x0, double y0, double x1, double y1) {
            var workDaysCount = days.Count(x => !IsHolyday(x));
            var dayValue = (double)maximum * y1 / workDaysCount;
            var points = days.Aggregate(((double)maximum * y1).Yield(), (r, x) => r.Concat((IsHolyday(x) ? r.Last() : r.Last() - dayValue).Yield())).ToArray();
            var lines = points.Take(points.Length - 1).Zip(points.Skip(1), (x, y) => new { start = (double)x, end = (double)y }).ToArray();
            var shapes = lines.Select((x, i) =>
                new DiagramShape() {
                    Background = Brushes.Honeydew,
                    Height = lineWidth,
                    Width = Math.Sqrt((x.start - x.end) * (x.start - x.end) + x1 * x1),
                    Angle = Math.Atan2(x.end - x.start, x1) * 180.0 / Math.PI,
                    Position = new Point(x0 + i * x1 + x1 / 2 - Math.Sqrt((x.start - x.end) * (x.start - x.end) + x1 * x1) / 2, y0 - (x.start + x.end) / 2 - lineWidth / 2)
                }).ToArray();
            shapes.ForEach(x => diagram.Items.Add(x));
        }
        static void AddRealLine(DiagramControl diagram, IEnumerable<DateTime> days, IEnumerable<decimal> estimates, double lineWidth, double x0, double y0, double x1, double y1) {
            var points = estimates.Select(x => (double)x * y1).ToArray();
            var lines = points.Take(points.Length - 1).Zip(points.Skip(1), (x, y) => new { start = (double)x, end = (double)y }).ToArray();
            var shapes = lines.Select((x, i) =>
                new DiagramShape() {
                    Background = Brushes.Green,
                    Height = lineWidth,
                    Width = Math.Sqrt((x.start - x.end) * (x.start - x.end) + x1 * x1),
                    Angle = Math.Atan2(x.end - x.start, x1) * 180.0 / Math.PI,
                    Position = new Point(x0 + i * x1 + x1 / 2 - Math.Sqrt((x.start - x.end) * (x.start - x.end) + x1 * x1) / 2, y0 - (x.start + x.end) / 2 - lineWidth / 2)
                }).ToArray();
            shapes.ForEach(x => diagram.Items.Add(x));
        }
        static bool IsHolyday(DateTime day) {
            return day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
        }
        static void AddAxes(DiagramControl diagram, int daysCount, decimal maximum, out double lineWidth, out double x0, out double y0, out double x1, out double y1) {
            var day = Math.Floor(diagram.PageSize.Width * 0.8 / daysCount);
            var storyPoint = Math.Floor(diagram.PageSize.Height * 0.8 / (double)maximum);
            var areaX = day * daysCount;
            var areaY = storyPoint * (double)maximum;
            x0 = (diagram.PageSize.Width - areaX) / 2.0 - 10.0;
            y0 = diagram.PageSize.Height - (diagram.PageSize.Height - areaY) / 2.0 + 10.0;
            lineWidth = 10.0;
            var axisX = new DiagramShape() { Shape = ArrowShapes.NotchedArrow, Position = new Point(x0, y0), Width = areaX + 60.0, Height = 2 * lineWidth };
            var axisY = new DiagramShape() { Shape = ArrowShapes.NotchedArrow, Position = new Point(x0 - areaY / 2 - 30.0, y0 - areaY / 2 - 30.0), Width = areaY + 60.0, Angle = 90.0, Height = 2 * lineWidth };
            diagram.Items.Add(axisX);
            diagram.Items.Add(axisY);
            x1 = day;
            y1 = storyPoint;
        }
    }
}