using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Maximus.Views
{
    public partial class DynamicBackground : UserControl
    {
        public DynamicBackground()
        {
            InitializeComponent();
            Loaded += (_, _) => { ApplyAccentColor(); StartAnimations(); };
            SizeChanged += (_, _) => StartAnimations();
        }

        private void ApplyAccentColor()
        {
            var accent = ResolveAccent();

            if (RootCanvas.Background is DrawingBrush db)
                db.Opacity = 0.06;

            Orb1.Fill = MakeOrb(accent, alpha: 0x38); 
            Orb2.Fill = MakeOrb(accent, alpha: 0x26);
            Orb3.Fill = MakeOrb(accent, alpha: 0x1A);
        }

        private Color ResolveAccent()
        {
            var res = TryFindResource("AccentColor");
            if (res is Color c)         return c;
            if (res is SolidColorBrush b) return b.Color;
            return Color.FromRgb(0, 150, 255); // fallback на случай если ресурс не найден
        }

        private static RadialGradientBrush MakeOrb(Color accent, byte alpha)
        {
            var inner = Color.FromArgb(alpha, accent.R, accent.G, accent.B);
            var outer = Color.FromArgb(0,     accent.R, accent.G, accent.B);
            return new RadialGradientBrush(inner, outer);
        }

        private void StartAnimations()
        {
            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            AnimateOrb(Orb1,
                fromX: w * -0.10, toX: w * 0.40,
                fromY: h * -0.10, toY: h * 0.40,
                dX: 38, dY: 51, dOp: 18,
                opMin: 0.45, opMax: 0.75);

            AnimateOrb(Orb2,
                fromX: w * 0.55, toX: w * 0.05,
                fromY: h * 0.40, toY: h * 0.65,
                dX: 47, dY: 33, dOp: 23,
                opMin: 0.30, opMax: 0.55);

            AnimateOrb(Orb3,
                fromX: w * 0.20, toX: w * 0.65,
                fromY: h * 0.55, toY: h * 0.05,
                dX: 43, dY: 58, dOp: 27,
                opMin: 0.20, opMax: 0.45);
        }

        private static void AnimateOrb(
            UIElement orb,
            double fromX, double toX,
            double fromY, double toY,
            double dX, double dY, double dOp,
            double opMin, double opMax)
        {
            var sine = new SineEase { EasingMode = EasingMode.EaseInOut };

            orb.BeginAnimation(Canvas.LeftProperty, Loop(fromX, toX, dX, sine));
            orb.BeginAnimation(Canvas.TopProperty,  Loop(fromY, toY, dY, sine));
            orb.BeginAnimation(OpacityProperty,     Loop(opMin, opMax, dOp, sine));
        }

        private static DoubleAnimation Loop(double from, double to, double sec, IEasingFunction ease) =>
            new(from, to, new Duration(TimeSpan.FromSeconds(sec)))
            {
                AutoReverse        = true,
                RepeatBehavior     = RepeatBehavior.Forever,
                EasingFunction     = ease
            };
    }
}