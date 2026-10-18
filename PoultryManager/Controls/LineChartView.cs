using PoultryManager.Models;
using HAlign = Microsoft.Maui.Graphics.HorizontalAlignment;
using VAlign = Microsoft.Maui.Graphics.VerticalAlignment;
using GFont = Microsoft.Maui.Graphics.Font;

namespace PoultryManager.Controls;

/// <summary>
/// Courbe avec aplat (production d'œufs par jour) : trait #286B46 sur aplat #DCE8DF,
/// première et dernière valeurs affichées directement, sans légende (DESIGN.md, section 5).
/// </summary>
public sealed class LineChartView : GraphicsView
{
    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points), typeof(IReadOnlyList<ChartPoint>), typeof(LineChartView), null,
        propertyChanged: (bindable, _, _) => ((LineChartView)bindable).Invalidate());

    public LineChartView()
    {
        Drawable = new LineChartDrawable(this);
        HeightRequest = 150;
        BackgroundColor = Colors.Transparent;
    }

    public IReadOnlyList<ChartPoint>? Points
    {
        get => (IReadOnlyList<ChartPoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    private sealed class LineChartDrawable(LineChartView owner) : IDrawable
    {
        private static readonly Color Line = Color.FromArgb("#286B46");
        private static readonly Color Area = Color.FromArgb("#DCE8DF");
        private static readonly Color Grid = Color.FromArgb("#E1EAE3");
        private static readonly Color TextColor = Color.FromArgb("#17392C");
        private static readonly Color Muted = Color.FromArgb("#5C7065");

        public void Draw(ICanvas canvas, RectF rect)
        {
            var points = owner.Points ?? [];
            const float top = 14, bottom = 22, side = 6;
            var chart = new RectF(rect.X + side, rect.Y + top, rect.Width - 2 * side, rect.Height - top - bottom);

            canvas.StrokeColor = Grid;
            canvas.StrokeSize = 1;
            for (var i = 0; i < 3; i++)
            {
                var y = chart.Top + chart.Height * (0.15f + i * 0.35f);
                canvas.DrawLine(chart.Left, y, chart.Right, y);
            }

            canvas.Font = GFont.Default;
            canvas.FontSize = 11;

            if (points.Count == 0)
            {
                canvas.FontColor = Muted;
                canvas.DrawString("Aucune donnée sur la période", rect.X, rect.Y, rect.Width, rect.Height,
                    HAlign.Center, VAlign.Center);
                return;
            }

            var min = points.Min(p => p.Value);
            var max = points.Max(p => p.Value);
            if (Math.Abs(max - min) < 1e-9)
            {
                min -= 1;
                max += 1;
            }

            // Marge haute et basse pour les étiquettes.
            var range = max - min;
            min -= range * 0.35;
            max += range * 0.15;

            float X(int i) => points.Count == 1 ? chart.Center.X : chart.Left + chart.Width * i / (points.Count - 1);
            float Y(double v) => (float)(chart.Bottom - (v - min) / (max - min) * chart.Height);

            // Aplat
            var area = new PathF();
            area.MoveTo(X(0), chart.Bottom);
            for (var i = 0; i < points.Count; i++) area.LineTo(X(i), Y(points[i].Value));
            area.LineTo(X(points.Count - 1), chart.Bottom);
            area.Close();
            canvas.FillColor = Area;
            canvas.FillPath(area);

            // Courbe
            var line = new PathF();
            line.MoveTo(X(0), Y(points[0].Value));
            for (var i = 1; i < points.Count; i++) line.LineTo(X(i), Y(points[i].Value));
            canvas.StrokeColor = Line;
            canvas.StrokeSize = 2.5f;
            canvas.StrokeLineJoin = Microsoft.Maui.Graphics.LineJoin.Round;
            canvas.StrokeLineCap = Microsoft.Maui.Graphics.LineCap.Round;
            canvas.DrawPath(line);

            // Dernier point
            var last = points.Count - 1;
            canvas.FillColor = Colors.White;
            canvas.FillCircle(X(last), Y(points[last].Value), 6);
            canvas.FillColor = Line;
            canvas.FillCircle(X(last), Y(points[last].Value), 4.5f);

            // Valeurs : première et dernière
            canvas.Font = GFont.DefaultBold;
            canvas.FontColor = TextColor;
            canvas.DrawString(points[0].ValueLabel ?? string.Empty, X(0), Y(points[0].Value) + 16, HAlign.Left);
            if (last > 0)
            {
                canvas.DrawString(points[last].ValueLabel ?? string.Empty, X(last) - 10, Y(points[last].Value) + 18, HAlign.Right);
            }

            // Libellés de l'axe (7 au maximum)
            canvas.Font = GFont.Default;
            canvas.FontColor = Muted;
            var step = Math.Max(1, (int)Math.Ceiling(points.Count / 7.0));
            for (var i = 0; i < points.Count; i += step)
            {
                var align = i == 0 ? HAlign.Left : HAlign.Center;
                canvas.DrawString(points[i].Label, X(i), rect.Bottom - 6, align);
            }
        }
    }
}
