using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
	class Program
	{
		public static int numThreads = 5;
		public static int threadResult = 100;

		public static object locker = new object();
		static Random rnd = new Random();

		private const char _block = '■';

		static void Main(string[] args)
		{
			Console.CursorVisible = false;
			// задаем ширину чтобы уместились все символы
			Console.WindowWidth = 130;
			var threads = new Thread[numThreads];

			for (int i = 0; i < numThreads; i++)
			{
				threads[i] = new Thread(new ParameterizedThreadStart(CalcProcess));
				// выводим порядковый номер и идентификатор потока
				Console.Write($"{string.Format("{0,2:00}", i)}({string.Format("{0,2:00}", threads[i].ManagedThreadId)})   ");
				// когда стартуем то передаем количество шагов и положение курсора чтобы каждый поток "знал" своё положение
				threads[i].Start( new { threadResult, Console.CursorLeft, Console.CursorTop });
				Console.WriteLine();
			}
			

			foreach (var thread in threads)
			{
				thread.Join();
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.SetCursorPosition(0, numThreads);
			Console.WriteLine("Done!");

			Console.ReadKey(true);
		}



		static void CalcProcess(object state)
		{
			dynamic o = state;
			var threadResult = (int)o.threadResult;

			// заводим специальный таймер для учета времени работы метода,
			// внутри потока так как для каждого пока свой должен быть
			var timer = Stopwatch.StartNew();
			// положение курсора для каждого потока своё
			int cursorLeft = (int)o.CursorLeft;
			int cursorTop = (int)o.CursorTop;
		
			// в начале думал что нужно нужно синхронизировать значение результата где то в главном потоке и уже его выводить,
			// но когда погуглил догадался что нужно просто взять обьект для блокировки (locker) и выводить в консоль сразу результат на текущий момент

			int result = 0;
			int errResult = 0;
			// просто цикл для ожидания количества шагов,
			// так как незнал нужно выводить шаги или проценты сделал так
			// если нужно будет прогресс бар переделать в проценты то легко это поменять
			while (result < threadResult)
			{
				// случайная пауза, имитация вычислений, конечно в реальности вычисления должны быть в try/catch
				Thread.Sleep(rnd.Next(0, 3) * 100);

				try
				{
					// имитация исключения
					var rndvalue = rnd.Next(0, 100);
					if (rndvalue < 10)
					{
						throw new Exception();
					}

					result++;
					lock (locker)
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.SetCursorPosition(cursorLeft, cursorTop);
						Console.Write(_block);
						Console.SetCursorPosition(cursorLeft + 1, cursorTop);
						Console.Write(string.Format(" {0,2:00}", result));
					}
					cursorLeft++;
				}
				catch (Exception ex)
				{
					// прибавляем так как это шаг хоть и неудачный
					result++;
					lock (locker)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.SetCursorPosition(cursorLeft, cursorTop);
						Console.Write(_block);

						Console.ForegroundColor = ConsoleColor.White;
						Console.SetCursorPosition(cursorLeft + 1, cursorTop);
						Console.Write(string.Format(" {0,2:00}", result));
					}
					cursorLeft++;
					errResult++;
					continue;
				}
			}

			timer.Stop();
			var resultTime = timer.Elapsed;

			// строка, которая будет содержать значение затраченного времени
			string elapsedTime = String.Format("{0:00}:{1:00}.{2:000}",
				resultTime.Minutes,
				resultTime.Seconds,
				resultTime.Milliseconds);


			lock (locker)
			{
				// выводим количество ошибочных шагов и время выполнения
				Console.ForegroundColor = ConsoleColor.Red;
				Console.SetCursorPosition(cursorLeft + 1, cursorTop);
				Console.Write($"{string.Format("{0,2:00}!", errResult)} {elapsedTime}\r\n");
			}
			
		}


	}
}
