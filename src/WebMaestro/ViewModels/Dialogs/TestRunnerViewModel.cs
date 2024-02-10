using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using WebMaestro.Models;
using WebMaestro.Services;

namespace WebMaestro.ViewModels.Dialogs
{
    internal partial class TestRunnerViewModel : ObservableValidator, IModalDialogViewModel
    {
        private readonly EnvironmentModel environment;
        private readonly RequestModel request;
        private readonly HttpRequestService requestService;
        private readonly List<Result> results = new();


        public TestRunnerViewModel(EnvironmentModel environment, RequestModel request)
        {
            this.environment = environment;
            this.request = request;
            this.requestService = new HttpRequestService();

            this.PlotModel = new PlotModel { Title = "Avg Response Time" };
            this.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "ms", Minimum = 0 });
            this.PlotModel.Axes.Add(new TimeSpanAxis() { Position = AxisPosition.Bottom, Title = "Elapsed time", MinorStep = 1 });
        }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        [Required]
        private int threads = 100;

        [ObservableProperty]
        [Required]
        private int rampUpTime = 1;

        [ObservableProperty]
        [Required]
        private int executionTime = 60;

        [ObservableProperty]
        [Required]
        private int delay = 1;

        public PlotModel PlotModel { get; private set; }

        [RelayCommand]
        private void CancelTest()
        {
            this.RunTestCommand.Cancel();
        }

        [RelayCommand]
        private async Task RunTest(CancellationToken cancellationToken)
        {
            this.results.Clear();
            this.PlotModel.Series.Clear();

            List<Task> tasks = new();

            var delay = TimeSpan.FromSeconds(this.Delay);
            var users = this.Threads;

            var rampUpTimer = new PeriodicTimer(TimeSpan.FromSeconds(this.RampUpTime / (double)users));

            var executionCT = new CancellationTokenSource(TimeSpan.FromSeconds(this.ExecutionTime)).Token;

            var combinedCT = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, executionCT).Token;


            while (users > 0 && await rampUpTimer.WaitForNextTickAsync(combinedCT))
            {
                tasks.Add(Execute(delay, combinedCT));
                users--;
            }

            await Task.WhenAll(tasks);

            var groups = this.results.OrderBy(x => x.EndTime).GroupBy(x => ((int)x.EndTime.TotalSeconds)).ToList();

            var series = new LineSeries
            {
                Color = OxyColors.Green
            };

            foreach (var group in groups)
            {
                series.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(group.Key)), group.Average(x => x.Duration.TotalMilliseconds)));
            }

            this.PlotModel.Series.Add(series);
            this.PlotModel.InvalidatePlot(true);

        }

        private async Task Execute(TimeSpan waitAfter, CancellationToken cancellationToken)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = new Result
                    {
                        StartTime = sw.Elapsed
                    };
                    try
                    {
                        var response = await this.requestService.SendAsync(this.environment, this.request, cancellationToken);
                        result.Status = (int)response.Status;
                    }
                    catch
                    {

                    }
                    result.EndTime = sw.Elapsed;
                    result.Duration = result.EndTime - result.StartTime;
                    this.results.Add(result);

                    await Task.Delay(waitAfter, cancellationToken).ConfigureAwait(false);
                }

                sw.Stop();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private class Result
        {
            public TimeSpan StartTime;
            public TimeSpan EndTime;
            public TimeSpan Duration;
            public int Status;
        }
    }
}
