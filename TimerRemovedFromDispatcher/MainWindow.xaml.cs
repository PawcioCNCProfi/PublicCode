using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TimerRemovedFromDispatcher {
	public partial class MainWindow : Window {
		private Thread thread;
		private Dispatcher dispatcher;
		private DispatcherTimer timer;
		private event Action INITIALIZED;

		public MainWindow() {
			InitializeComponent();

			//setTimer(() => dispatcher?.InvokeAsync(workOnMyThread)); //A - working
			INITIALIZED += () => setTimer(workOnMyThread); //B - not working, timer removed from dispatcher after first exit from mainLoop()

			thread = new Thread(() => {
				dispatcher = Dispatcher.CurrentDispatcher;
				INITIALIZED?.Invoke();
				dispatcher.Invoke(mainLoop);
				Dispatcher.Run();
			});
			thread.Name = "My thread";
			thread.Start();

		}

		private int c;
		private void mainLoop() {
			c++;
			Thread.Sleep(15);
			dispatcher.InvokeAsync(mainLoop);
		}

		private void setTimer(Action tickHandler) {
			timer = new DispatcherTimer(DispatcherPriority.Input);
			timer.Interval = TimeSpan.FromSeconds(.5);
			timer.Tick += (s, e) => tickHandler();
			timer.Start();
		}

		private void workOnMyThread() {
			Console.WriteLine(c);
		}
	}
}
