using PoultryManager.Models;
using HAlign = Microsoft.Maui.Graphics.HorizontalAlignment;
using VAlign = Microsoft.Maui.Graphics.VerticalAlignment;
using GFont = Microsoft.Maui.Graphics.Font;

namespace PoultryManager.Controls;

/// <summary>
/// Diagramme en barres avec valeurs au-dessus et libellés dessous
/// (mortalité par jour en rouge, taux de ponte par semaine en vert).
/// </summary>
public sealed class BarChartView : GraphicsView
{
    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points), typeof(IReadOnlyList<ChartPoint>), typeof(BarChartView), null,
        propertyChanged: (bindable, _, _) => ((BarChartView)bindable).Invalidate());

    public static readonly BindableProperty BarColorProperty = BindableProperty.Create(
        nameof(BarColor), typeof(Color), typeof(BarChartView), Color.FromArgb("#286B46"),
        propertyChanged: (bindable, _, _) => ((BarChartView)bindable).Invalidate());

    public static readonly BindableProperty HighlightColorProperty = BindableProperty.Create(
        nameof(HighlightColor), typeof(Color), typeof(BarChartView), Color.FromArgb("#174B35"),
        propertyChanged: (bindable, _, _) => ((BarChartView)bindable).Invalidate());

    public BarChartView()
    {
        Drawable = new BarChartDrawable(this);
        HeightRequest = 150;
        BackgroundColor = Colors.Transparent;
    }

    public IReadOnlyList<ChartPoint>? Points
    {
        get => (IReadOnlyList<ChartPoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    public Color HighlightColor
    {
        get => (Color)GetValue(HighlightColorProperty);
        set => SetValue(HighlightColorProperty, value);
    }

    private sealed class BarChartDrawable(BarChartView owner) : IDrawable
    {
        private static readonly Color TextColor = Color.FromArgb("#17392C");
        private static readonly Color Muted = Color.FromArgb("#5C7065");

        public void Draw(ICanvas canvas, RectF rect)
        {
            var points = owner.Points ?? [];
            const float top = 18, bottom = 20;
            var chart = new RectF(rect.X, rect.Y + top, rect.Width, rect.Height - top - bottom);
            canvas.FontSize = 11;

            if (points.Count == 0)
            {
                canvas.Font = GFont.Default;
                canvas.FontColor = Muted;
                canvas.DrawString("Aucune donnée sur la période", rect.X, rect.Y, rect.Width, rect.Height,
                    HAlign.Center, VAlign.Center);
                return;
            }

            var max = Math.Max(points.Max(p => p.Value), 1e-9);
            var slot = chart.Width / points.Count;
            var gap = Math.Min(10f, slot * 0.2f);
            var barWidth = Math.Max(2f, slot - gap);
            var showValues = slot >= 18;
            var labelStep = Math.Max(1, (int)Math.Ceiling(points.Count / 7.0));

            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                var x = chart.Left + i * slot + (slot - barWidth) / 2;
                var height = (float)(p.Value / max * chart.Height);
                if (p.Value > 0)
                {
                    height = Math.Max(height, 6);
                    canvas.FillColor = p.Highlight ? owner.HighlightColor : owner.BarColor;
                    canvas.FillRoundedRectangle(x, chart.Bottom - height, barWidth, height, Math.Min(6, barWidth / 2));
                }

                if (showValues && !string.IsNullOrEmpty(p.ValueLabel))
                {
                    canvas.Font = GFont.DefaultBold;
                    canvas.FontColor = TextColor;
                    canvas.DrawString(p.ValueLabel, x + barWidth / 2, chart.Bottom - height - 5, HAlign.Center);
                }

                if (i % labelStep == 0)
                {
                    canvas.Font = GFont.Default;
                    canvas.FontColor = Muted;
                    canvas.DrawString(p.Label, x + barWidth / 2, rect.Bottom - 4, HAlign.Center);
                }
            }
        }
    }
}
