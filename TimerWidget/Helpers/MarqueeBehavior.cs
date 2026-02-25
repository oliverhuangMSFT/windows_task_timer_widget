using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TimerWidget.Helpers
{
    public static class MarqueeBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled", typeof(bool), typeof(MarqueeBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock tb) return;

            if ((bool)e.NewValue)
            {
                tb.RenderTransform = new TranslateTransform();
                tb.Loaded += OnLoaded;
                tb.SizeChanged += OnSizeChanged;
            }
            else
            {
                tb.Loaded -= OnLoaded;
                tb.SizeChanged -= OnSizeChanged;
                StopAnimation(tb);
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var tb = (TextBlock)sender;
            if (VisualTreeHelper.GetParent(tb) is FrameworkElement container)
            {
                container.SizeChanged -= OnContainerSizeChanged;
                container.SizeChanged += OnContainerSizeChanged;
            }
            UpdateAnimation(tb);
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateAnimation((TextBlock)sender);

        private static void OnContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not FrameworkElement container) return;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
            {
                if (VisualTreeHelper.GetChild(container, i) is TextBlock tb && GetIsEnabled(tb))
                {
                    UpdateAnimation(tb);
                    break;
                }
            }
        }

        private static void UpdateAnimation(TextBlock tb)
        {
            var container = VisualTreeHelper.GetParent(tb) as FrameworkElement;
            if (container == null) return;

            double textWidth = tb.ActualWidth;
            double containerWidth = container.ActualWidth;

            if (textWidth <= 0 || containerWidth <= 0) return;

            if (textWidth > containerWidth)
            {
                double overflow = textWidth - containerWidth;
                var transform = tb.RenderTransform as TranslateTransform;
                if (transform == null || transform.IsFrozen)
                {
                    transform = new TranslateTransform();
                    tb.RenderTransform = transform;
                }

                double scrollSeconds = Math.Max(overflow / 50.0, 1.0);

                var animation = new DoubleAnimationUsingKeyFrames
                {
                    RepeatBehavior = RepeatBehavior.Forever
                };

                double t = 0;
                animation.KeyFrames.Add(new LinearDoubleKeyFrame(0,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(t += 2))));
                animation.KeyFrames.Add(new LinearDoubleKeyFrame(-overflow,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(t += scrollSeconds))));
                animation.KeyFrames.Add(new LinearDoubleKeyFrame(-overflow,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(t += 2))));
                animation.KeyFrames.Add(new LinearDoubleKeyFrame(0,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(t += scrollSeconds))));

                transform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else
            {
                StopAnimation(tb);
            }
        }

        private static void StopAnimation(TextBlock tb)
        {
            if (tb.RenderTransform is TranslateTransform transform && !transform.IsFrozen)
            {
                transform.BeginAnimation(TranslateTransform.XProperty, null);
                transform.X = 0;
            }
        }
    }
}
