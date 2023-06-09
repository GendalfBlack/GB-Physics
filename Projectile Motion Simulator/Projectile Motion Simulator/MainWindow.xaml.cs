﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace Projectile_Motion_Simulator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer plotTimer = new DispatcherTimer();
        double t, x, y0, v0, theta, vx, vy, g, maxHeight, maxDistance;
        Polyline trajectoryPlot = new Polyline();
        Line initialVector = new Line();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            plotTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            plotTimer.Tick += new EventHandler(plotTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Tick += new EventHandler(timer_Tick);
            trajectoryPlot.Stroke = Brushes.Black;
            trajectoryPlot.StrokeThickness = 2;
            trajectoryPlot.StrokeDashArray = new DoubleCollection() { 4, 4 };
            initialVector.Stroke = Brushes.Red;
            initialVector.StrokeThickness = 3;
            mainCanvas.Children.Add(trajectoryPlot);
            mainCanvas.Children.Add(initialVector);
            Setup();
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled) { timer.Stop(); }
            else { timer.Start(); }
            if (plotTimer.IsEnabled) { plotTimer.Stop(); }
            else { plotTimer.Start(); }
        }
        private void angleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Setup();
        }
        private void v0TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Setup();
        }
        private void y0TextBox_TextInput(object sender, TextChangedEventArgs e)
        {
            Setup();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Setup();
        }
        private void ResetProjectile(double dy)
        {
            var p = ProjectileEllipse;
            if (mainCanvas != null)
            {
                double h = mainCanvas.ActualHeight;
                double ph = p.ActualHeight;
                y0 = h - ph - dy;
                if (y0 >= h - ph){ Canvas.SetTop(p, h - ph); }
                else { Canvas.SetTop(p, y0); }
                Canvas.SetLeft(p, 0);
            }
            trajectoryPlot.Points.Clear();
        }
        private void Setup()
        {
            maxHeight = 0;
            maxDistance = 0;
            t = 0;
            x = 0;
            if (y0TextBox == null ){ return; }
            if (angleTextBox == null){ return; }
            if (v0TextBox == null ){ return; }
            if (gravityTextBox == null ){ return; }
            if (!Regex.IsMatch(y0TextBox.Text, @"^-?\d*,?\d+$")){ return; }
            if (!Regex.IsMatch(v0TextBox.Text, @"^-?\d*,?\d+$")){ return; }
            if (!Regex.IsMatch(angleTextBox.Text, @"^-?\d*,?\d+$")){ return; }
            if (!Regex.IsMatch(gravityTextBox.Text, @"^-?\d*,?\d+$")){ return; }
            ResetProjectile(Convert.ToDouble(y0TextBox.Text));
            v0 = Convert.ToDouble(v0TextBox.Text);
            theta = -Convert.ToDouble(angleTextBox.Text) * Math.PI / 180;
            vx = v0 * Math.Cos(theta);
            vy = v0 * Math.Sin(theta);
            g = -Convert.ToDouble(gravityTextBox.Text);
            UpdateStatusBar();
            SetupInitialVector();
        }
        private void SetupInitialVector()
        {
            var p = ProjectileEllipse;
            if (p != null)
            {
                double x = (double)p.GetValue(Canvas.LeftProperty);
                double y = (double)p.GetValue(Canvas.TopProperty);
                initialVector.X1 = x + p.Width / 2;
                initialVector.Y1 = y + p.Height / 2;
                initialVector.X2 = x + v0 * Math.Cos(theta) + p.Width / 2;
                initialVector.Y2 = y + v0 * Math.Sin(theta) + p.Height / 2;
            }           
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Setup();
            timer.Start();
            plotTimer.Start();
        }
        private void plotTimer_Tick(object sender, EventArgs e)
        {
            var p = ProjectileEllipse;
            double x = (double)p.GetValue(Canvas.LeftProperty) + p.Width / 2;
            double y = (double)p.GetValue(Canvas.TopProperty) + p.Width / 2;
            trajectoryPlot.Points.Add(new Point(x, y));
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            var p = ProjectileEllipse;
            double dt = (double)timer.Interval.Milliseconds / 1000;
            t += dt;
            vy = vy - g * dt;
            x = (double)p.GetValue(Canvas.LeftProperty);
            y0 = (double)p.GetValue(Canvas.TopProperty);
            Canvas.SetLeft(p, x + vx * dt);
            Canvas.SetTop(p, y0 + vy * dt);
            if (y0 >= mainCanvas.ActualHeight - p.ActualHeight){timer.Stop(); plotTimer.Stop(); }
            UpdateStatusBar();
        }
        private void UpdateStatusBar()
        {
            var p = ProjectileEllipse;
            double x = (double)p.GetValue(Canvas.LeftProperty);
            double h = mainCanvas.ActualHeight;
            double ph = p.ActualHeight;
            double y = h - ph - (double)p.GetValue(Canvas.TopProperty);
            if (y < 0) { y = 0; }
            maxDistance = x > maxDistance ? x : maxDistance;
            maxHeight = y > maxHeight ? y : maxHeight;
            double v = Math.Sqrt(vx * vx + vy * vy);
            positionLabel.Content = $"Позиція снаряду: {Math.Floor(x)};{Math.Floor(y)}";
            velocityLabel.Content = $"Швидкість снаряду: {Math.Round(v, 2)}м/c";
            timeLabel.Content = $"Час польоту: {Math.Round(t, 2)}c";
            maxHeightLabel.Content = $"Максимальна висота: {Math.Round(maxHeight, 2)}м";
            maxDistanceLabel.Content = $"Максимальна дальність: {Math.Round(maxDistance, 2)}м";
        }
    }
}
