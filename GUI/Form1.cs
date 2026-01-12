using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace GUI
{
    public partial class Form1 : Form
    {
        private List<PointF> _points = new();
        private float _xMin = -10f, _xMax = 10f, _yMin = -1f, _yMax = 1f;

        // Cached formats to avoid re-allocations during painting
        private static readonly StringFormat CenterFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
        private static readonly StringFormat RightFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

        public Form1()
        {
            InitializeComponent();
            this.plotArea.Paint += plotArea_Paint;
            this.plotArea.Resize += (s, e) => this.plotArea.Invalidate();
            this.DoubleBuffered = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string input = inputbox.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                ErrorBox1.Text = "enter the expression";
                Resultbox.Clear();
                return;
            }
            else
            {
                string result = Mexer.evaluateexpression(input);
                if (result != null && result.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorBox1.Text = result;
                    Resultbox.Clear();
                }
                else
                {
                    Resultbox.Text = result ?? string.Empty;
                    ErrorBox1.Clear();
                }
            }
        }

        private void plotButton_Click(object? sender, EventArgs e)
        {
            try
            {
                string expr = inputbox.Text.Trim();
                if (string.IsNullOrWhiteSpace(expr))
                {
                    ErrorBox1.Text = "Enter an expression in terms of x (e.g., a*x + b).";
                    return;
                }

                if (!float.TryParse(textXMin.Text, out _xMin)) _xMin = -10f;
                if (!float.TryParse(textXMax.Text, out _xMax)) _xMax = 10f;
                if (!float.TryParse(textStep.Text, out float dx)) dx = 0.1f;

                if (_xMax <= _xMin)
                {
                    ErrorBox1.Text = "x max must be greater than x min";
                    return;
                }
                if (dx <= 0f)
                {
                    ErrorBox1.Text = "dx must be > 0";
                    return;
                }

                // Sample points efficiently: parse once and evaluate for many x values in F#
                var pts = new List<PointF>();
                double ymin = double.PositiveInfinity, ymax = double.NegativeInfinity;

                int n = (int)Math.Ceiling((_xMax - _xMin) / dx) + 1;
                if (n <= 1) n = 2;
                var xs = new double[n];
                for (int i = 0; i < n; i++) xs[i] = _xMin + i * dx;
                xs[n - 1] = _xMax; // ensure last equals max exactly

                var ys = Mexer.evaluateManyD(expr, xs);
                for (int i = 0; i < n; i++)
                {
                    double x = xs[i];
                    double y = ys[i];
                    if (double.IsNaN(y) || double.IsInfinity(y)) continue;
                    ymin = Math.Min(ymin, y);
                    ymax = Math.Max(ymax, y);
                    pts.Add(new PointF((float)x, (float)y));
                }

                if (pts.Count < 2)
                {
                    ErrorBox1.Text = "Could not sample enough points to plot. Check expression.";
                    _points.Clear();
                    plotArea.Invalidate();
                    return;
                }

                // Expand y range slightly for padding
                if (double.IsInfinity(ymin) || double.IsInfinity(ymax))
                {
                    ymin = -1; ymax = 1;
                }
                if (Math.Abs(ymax - ymin) < 1e-6) { ymax = ymin + 1; }

                _yMin = (float)(ymin - 0.05 * (ymax - ymin));
                _yMax = (float)(ymax + 0.05 * (ymax - ymin));

                _points = pts;
                ErrorBox1.Clear();
                plotArea.Invalidate();
            }
            catch (Exception ex)
            {
                ErrorBox1.Text = $"Error: {ex.Message}";
            }
        }

        private void plotArea_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);

            Rectangle client = plotArea.ClientRectangle;
            if (client.Width <= 2 || client.Height <= 2)
                return;

            // Draw ticks, grid and axis numbers first
            DrawAxesAndTicks(g, client);

            // draw border
            using (var borderPen = new Pen(Color.Silver, 1))
            {
                g.DrawRectangle(borderPen, client.Left, client.Top, client.Width - 1, client.Height - 1);
            }

            if (_points.Count >= 2)
            {
                using var curvePen = new Pen(Color.DarkBlue, 2);
                var screenPts = new Point[_points.Count];
                for (int i = 0; i < _points.Count; i++)
                {
                    var p = _points[i];
                    screenPts[i] = new Point(WorldToScreenX(p.X, client), WorldToScreenY(p.Y, client));
                }
                // clip to panel
                var oldClip = g.Clip;
                g.SetClip(client);
                g.DrawLines(curvePen, screenPts);
                g.SetClip(oldClip, System.Drawing.Drawing2D.CombineMode.Replace);
            }
        }

        private void DrawAxesAndTicks(Graphics g, Rectangle client)
        {
            // visual constants
            const int tickSize = 6;
            const int padding = 4;
            var gridColor = Color.FromArgb(235, 235, 235);
            var axisColor = Color.LightGray;
            using var gridPen = new Pen(gridColor, 1);
            using var axisPen = new Pen(axisColor, 1);
            using var textBrush = new SolidBrush(Color.Gray);
            using var zeroPen = new Pen(Color.Gray, 1);

            // Compute nice tick steps based on pixel density (~70 px target spacing)
            double xStep = ComputeNiceStep(_xMin, _xMax, Math.Max(1, client.Width / 70));
            double yStep = ComputeNiceStep(_yMin, _yMax, Math.Max(1, client.Height / 50));

            // Determine reference axis positions in screen space
            int yZeroPx = (_yMin < 0 && _yMax > 0) ? WorldToScreenY(0, client) : client.Bottom - 20; // near bottom if 0 not visible
            int xZeroPx = (_xMin < 0 && _xMax > 0) ? WorldToScreenX(0, client) : client.Left + 30;   // near left if 0 not visible

            // X ticks and labels
            double xStart = Math.Ceiling(_xMin / xStep) * xStep;
            for (double x = xStart; x <= _xMax + 1e-9; x += xStep)
            {
                int sx = WorldToScreenX((float)x, client);
                // grid line
                g.DrawLine(gridPen, sx, client.Top, sx, client.Bottom);
                // tick
                g.DrawLine(axisPen, sx, yZeroPx - tickSize / 2, sx, yZeroPx + tickSize / 2);
                // label below axis
                string label = FormatTick(x);
                g.DrawString(label, plotArea.Font, textBrush, sx, Math.Min(client.Bottom - 15, yZeroPx + tickSize / 2 + padding), CenterFormat);
            }

            // Y ticks and labels
            double yStart = Math.Ceiling(_yMin / yStep) * yStep;
            for (double y = yStart; y <= _yMax + 1e-9; y += yStep)
            {
                int sy = WorldToScreenY((float)y, client);
                // grid line
                g.DrawLine(gridPen, client.Left, sy, client.Right, sy);
                // tick
                g.DrawLine(axisPen, xZeroPx - tickSize / 2, sy, xZeroPx + tickSize / 2, sy);
                // label to the left of axis
                string label = FormatTick(y);
                g.DrawString(label, plotArea.Font, textBrush, Math.Max(client.Left + 2, xZeroPx - tickSize / 2 - padding), sy, RightFormat);
            }

            // Draw main axes if within view
            if (_yMin < 0 && _yMax > 0)
            {
                int y0 = WorldToScreenY(0, client);
                g.DrawLine(zeroPen, client.Left, y0, client.Right, y0);
            }
            if (_xMin < 0 && _xMax > 0)
            {
                int x0 = WorldToScreenX(0, client);
                g.DrawLine(zeroPen, x0, client.Top, x0, client.Bottom);
            }
        }

        private static double ComputeNiceStep(double vmin, double vmax, int approxTicks)
        {
            double range = Math.Max(1e-12, vmax - vmin);
            double rough = range / Math.Max(1, approxTicks);
            double pow10 = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            double[] bases = { 1, 2, 5 };
            double best = bases[0] * pow10;
            double minDiff = Math.Abs(best - rough);
            for (int i = 0; i < bases.Length; i++)
            {
                double step = bases[i] * pow10;
                double diff = Math.Abs(step - rough);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    best = step;
                }
            }
            // If too few ticks, try doubling
            if (range / best < approxTicks / 2.0)
                best /= 2.0;
            return best;
        }

        private static string FormatTick(double val)
        {
            double abs = Math.Abs(val);
            if (abs >= 1e6 || (abs > 0 && abs < 1e-3))
                return val.ToString("0.###e+0", CultureInfo.InvariantCulture);
            if (Math.Abs(val - Math.Round(val)) < 1e-9)
                return Math.Round(val).ToString(CultureInfo.InvariantCulture);
            return val.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private int WorldToScreenX(float x, Rectangle client)
        {
            double t = (x - _xMin) / (_xMax - _xMin);
            return client.Left + (int)Math.Round(t * (client.Width - 1));
        }

        private int WorldToScreenY(float y, Rectangle client)
        {
            double t = (y - _yMin) / (_yMax - _yMin);
            // invert Y for screen
            return client.Bottom - 1 - (int)Math.Round(t * (client.Height - 1));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
