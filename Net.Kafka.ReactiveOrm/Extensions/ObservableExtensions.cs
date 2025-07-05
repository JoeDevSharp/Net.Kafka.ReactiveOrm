using System;
using System.Reactive.Linq;

namespace Net.Kafka.ReactiveOrm.Extensions
{
    /// <summary>
    /// Useful extensions for IObservable<T> to simplify reactive handling.
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Logs each element of the observable sequence using the provided action.
        /// </summary>
        public static IObservable<T> Log<T>(this IObservable<T> source, Action<T> logAction)
        {
            return source.Do(logAction);
        }

        /// <summary>
        /// Catches and logs exceptions, preventing the observable from terminating unexpectedly.
        /// </summary>
        public static IObservable<T> CatchAndLog<T>(this IObservable<T> source, Action<Exception> logError)
        {
            return source.Catch<T, Exception>(ex =>
            {
                logError(ex);
                return Observable.Empty<T>();
            });
        }

        /// <summary>
        /// Converts a synchronous `Action<T>` into an `async void` safe subscription.
        /// </summary>
        public static IDisposable SafeSubscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(
                onNext,
                ex => Console.WriteLine($"[ReactiveError] {ex}"),
                () => Console.WriteLine("[Reactive] Completed")
            );
        }

        /// <summary>
        /// Filters messages with a null check for safety.
        /// </summary>
        public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source) where T : class
        {
            return source.Where(x => x != null)!;
        }
    }
}
