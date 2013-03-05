using EventBus.Deferred;
using EventBus.Infrastructure;
using System;
using System.Windows;

namespace EventBus.WpfApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private int counter = 0;
		private Subscriber1 s1;
		private Subscriber2 s2;
		private Subscriber3 s3;

		private IDisposable ds1;
		private IDisposable ds2;
		private IDisposable ds3;

		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
			this.Closing += MainWindow_Closing;
		}

		void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DefaultSingleton<ActionMessageBus>.Instance.Clear();
			DefaultSingleton<ActionMessageBus>.Instance = null;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			DefaultSingleton<ActionMessageBus>.Instance = new Deferred.ActionMessageBus();
		}

		private void btnAlloc_Click_1(object sender, RoutedEventArgs e)
		{
			switch (counter)
			{
				case 0:
					if (s1 == null)
					{
						counter++;
						s1 = new Subscriber1();
						ds1 = DefaultSingleton<ActionMessageBus>.Instance.Subscribe<Message1>(s1.Handle);
					}
					break;

				case 1:
					counter++;
					s2 = new Subscriber2();
					ds2 = DefaultSingleton<ActionMessageBus>.Instance.Subscribe<Message1>(s2.Handle);
					break;

				case 2:
					if (s3 == null)
					{
						s3 = new Subscriber3();
						ds3 = DefaultSingleton<ActionMessageBus>.Instance.Subscribe<Message1>(s3.Handle);
					}
					break;

				default:
					return;
			}
		}

		private void btnFree_Click_1(object sender, RoutedEventArgs e)
		{
			switch (counter)
			{
				case 0:
					if (s1 != null)
					{
						s1 = null;
						ds1.Dispose();
						ds1 = null;
						GC.Collect();
					}
					break;

				case 1:
					s2 = null;
					ds2.Dispose();
					ds2 = null;
					GC.Collect();
					counter--;
					break;

				case 2:
					s3 = null;
					ds3.Dispose();
					ds3 = null;
					GC.Collect();
					counter--;
					break;

				default:
					return;
			}
		}

		private void btnFire_Click_1(object sender, RoutedEventArgs e)
		{
			DefaultSingleton<ActionMessageBus>.Instance.Publish<Message1>(new Message1() { Data = DateTime.Now.ToString() });
		}
	}

	internal class Subscriber1
	{
		public void Handle(Message1 data)
		{
			System.Diagnostics.Debug.WriteLine(data.Data, "Subscriber1");
		}
	}

	internal class Subscriber2
	{
		public void Handle(Message1 data)
		{
			System.Diagnostics.Debug.WriteLine(data.Data, "Subscriber2");
		}
	}

	internal class Subscriber3
	{
		public void Handle(Message1 data)
		{
			System.Diagnostics.Debug.WriteLine(data.Data, "Subscriber3");
		}
	}

	internal class Message1 : IMessage
	{
		public string Data { get; set; }
	}
}