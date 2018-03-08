﻿using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GitHub.Exports;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views
{
    [ExportViewFor(typeof(IActorViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ActorAvatarView : FrameworkElement
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(IActorViewModel),
                typeof(ActorAvatarView),
                new FrameworkPropertyMetadata(HandleViewModelChanged));

        readonly DispatcherTimer invalidateTimer;
        IActorViewModel toRender;
        long updateStart;
        Geometry clip;

        static ActorAvatarView()
        {
            WidthProperty.OverrideMetadata(typeof(ActorAvatarView), new FrameworkPropertyMetadata(30.0));
            HeightProperty.OverrideMetadata(typeof(ActorAvatarView), new FrameworkPropertyMetadata(30.0));
        }

        public ActorAvatarView()
        {
            invalidateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            invalidateTimer.Interval = TimeSpan.FromMilliseconds(10);
            invalidateTimer.Tick += (s, e) =>
            {
                toRender = ViewModel;
                InvalidateVisual();
                invalidateTimer.Stop();
            };
        }

        public IActorViewModel ViewModel
        {
            get { return (IActorViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            clip = null;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (toRender != null)
            {
                if (clip == null)
                {
                    clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight), 3, 3);
                }

                drawingContext.PushClip(clip);
                drawingContext.DrawImage(toRender.Avatar, new Rect(0, 0, ActualWidth, ActualHeight));
                drawingContext.Pop();
            }
        }

        void ViewModelChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldValue = e.OldValue as IActorViewModel;
            var newValue = e.NewValue as IActorViewModel;

            if (oldValue != null)
            {
                oldValue.PropertyChanged -= ViewModelPropertyChanged;
            }

            if (newValue != null)
            {
                newValue.PropertyChanged += ViewModelPropertyChanged;
            }

            updateStart = Environment.TickCount;
            toRender = null;
            invalidateTimer.Start();
            InvalidateVisual();
        }

        void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IActorViewModel.Avatar))
            {
                InvalidateVisual();
            }
        }

        static void HandleViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ActorAvatarView)?.ViewModelChanged(e);
        }
    }
}
