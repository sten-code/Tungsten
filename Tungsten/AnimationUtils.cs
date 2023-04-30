using System.Windows.Media.Animation;
using System.Windows;
using System;

namespace Tungsten
{
    public static class AnimationUtils
    {
        public static IEasingFunction EaseInOut { get; } = new QuarticEase
        {
            EasingMode = EasingMode.EaseInOut
        };

        public static IEasingFunction EaseOut { get; } = new QuarticEase
        {
            EasingMode = EasingMode.EaseOut
        };

        public static void ObjectShift(DependencyObject obj, Thickness get, Thickness set, IEasingFunction easing, int duration = 500)
        {
            ThicknessAnimation anim = new ThicknessAnimation()
            {
                From = get,
                To = set,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = easing
            };
            Storyboard.SetTarget(anim, obj);
            Storyboard.SetTargetProperty(anim, new PropertyPath(FrameworkElement.MarginProperty));
            Storyboard sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Begin();
            sb.Children.Remove(anim);
        }

        public static void ObjectWidth(DependencyObject obj, double get, double set, IEasingFunction easing, int duration)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = get,
                To = set,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = easing
            };
            Storyboard.SetTarget(anim, obj);
            Storyboard.SetTargetProperty(anim, new PropertyPath(FrameworkElement.WidthProperty));
            Storyboard sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Begin();
            sb.Children.Remove(anim);
        }

    }
}
