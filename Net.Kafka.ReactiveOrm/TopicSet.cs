using System.Reactive.Linq;

namespace Net.Kafka.ReactiveOrm
{
    /// <summary>
    /// Represents a strongly typed Kafka topic set that supports reactive subscription and publishing.
    /// </summary>
    /// <typeparam name="T">The type of messages in the Kafka topic.</typeparam>
    public class TopicSet<T>
    {
        private readonly IKafkaBus _bus;
        private readonly string _topicName;
        private readonly string? _consumerGroup;
        private readonly IObservable<T> _observableStream;

        internal TopicSet(IKafkaBus bus, string topicName, string? consumerGroup = null)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _topicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            _consumerGroup = consumerGroup;

            // Configura el stream reactivo con protección contra nulls y errores
            _observableStream = Reactive.ReactiveSubscription.Observe<T>(_bus, _topicName, _consumerGroup);
        }

        /// <summary>
        /// Publica un mensaje en el topic.
        /// </summary>
        /// <param name="entity">El mensaje a publicar.</param>
        public Task Publish(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return Reactive.ReactivePublisher.PublishAsync(_bus, _topicName, entity);
        }

        /// <summary>
        /// Expone el stream observable para suscribirse y filtrar.
        /// </summary>
        public IObservable<T> AsObservable()
        {
            return _observableStream;
        }

        /// <summary>
        /// Permite encadenar filtros LINQ para suscripciones reactivas.
        /// </summary>
        /// <param name="predicate">Filtro para los mensajes.</param>
        public IObservable<T> Where(Func<T, bool> predicate)
        {
            return _observableStream.Where(predicate);
        }

        /// <summary>
        /// Permite transformar los mensajes con Select (proyección).
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado proyectado.</typeparam>
        /// <param name="selector">Función de transformación.</param>
        public IObservable<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            return _observableStream.Select(selector);
        }

        /// <summary>
        /// Se suscribe a mensajes con un manejador síncrono.
        /// </summary>
        /// <param name="onNext">Acción a ejecutar por mensaje.</param>
        public IDisposable Subscribe(Action<T> onNext)
        {
            return _observableStream.Subscribe(onNext);
        }

        /// <summary>
        /// Se suscribe a mensajes con manejo de error y finalización.
        /// </summary>
        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            return _observableStream.Subscribe(onNext, onError, onCompleted);
        }
    }
}
