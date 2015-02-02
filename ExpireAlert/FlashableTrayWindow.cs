using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Hardcodet.Wpf.TaskbarNotification;

namespace ExpireAlert
{
    public class FlashableTrayWindow : Window
    {
        public FlashableTrayWindow()
        {
            this.m_tray = new TaskbarIcon();

            // binding
            var bindingIcon = new Binding("Icon") { Source = this };
            this.m_tray.SetBinding(TaskbarIcon.IconSourceProperty, bindingIcon);

            var bindingTooltip = new Binding("Title") { Source = this };
            this.m_tray.SetBinding(TaskbarIcon.ToolTipTextProperty, bindingTooltip);

            this.m_tray.TrayMouseDoubleClick += TrayMouseDoubleClick;
            this.Closing += MainWindow_Closing;

            // menu
            var ctxMenu = new ContextMenu();
            var miQuit = new MenuItem() { Header = "退出(_Q)" };
            miQuit.Click += MI_Quit_Click;
            ctxMenu.Items.Add(miQuit);
            this.m_tray.ContextMenu = ctxMenu;

            // FlashIcon via animation
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame() {
                KeyTime = KeyTime.FromPercent(0.5),
                Value = null
            });

            this.m_storyboardFlash = new Storyboard();
            this.m_storyboardFlash.Children.Add(animation);
            Storyboard.SetTarget(animation, this.m_tray);
            Storyboard.SetTargetProperty(animation, new PropertyPath(TaskbarIcon.IconSourceProperty));
        }

        public bool FlashIcon
        {
            get { return (bool)this.GetValue(FlashIconProperty); }
            set { this.SetValue(FlashIconProperty, value); }
        }

        private void FlashIconChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Do not apply animation in suppressing time.
                if (DateTime.Now > this.m_tmSuppress + TimeSpan.FromHours(8.0))
                {
                    this.m_storyboardFlash.Begin();
                    this.WindowState = WindowState.Normal;
                    this.Visibility = Visibility.Visible;
                    this.ShowInTaskbar = true;
                }
                else
                    this.FlashIcon = false;
            }
            else this.m_storyboardFlash.Stop();
        }

        public static readonly DependencyProperty FlashIconProperty = DependencyProperty.Register(
            "FlashIcon", typeof(bool), typeof(FlashableTrayWindow),
            new PropertyMetadata(false, (d, e) =>
            {
                var _this = d as FlashableTrayWindow;
                if (_this != null) _this.FlashIconChanged(e);
            }));

        protected TaskbarIcon m_tray;

        private void TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;
                this.Visibility = Visibility.Visible;
            }

            this.Activate();
            this.FlashIcon = false;
            this.m_tmSuppress = DateTime.Now;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!m_bQuit)
            {
                // 仅仅最小化,不实际退出
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void MI_Quit_Click(object sender, RoutedEventArgs e)
        {
            this.m_bQuit = true;
            this.Close();
            this.m_bQuit = false;
        }

        private bool m_bQuit = false;
        private Storyboard m_storyboardFlash;
        private DateTime m_tmSuppress = DateTime.Parse("2000-01-01");
    }
}
